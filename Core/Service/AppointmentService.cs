using AutoMapper;
using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceAbstraction;
using Shared;
using Shared.DTOs.Appointment;
using Shared.Errors;
using Shared.Settings;
namespace Service
{
    public sealed class AppointmentService(
        IUnitOfWork unitOfWork,
        IBookingLockProvider bookingLockProvider,
        IMapper mapper,
        TimeProvider timeProvider,
        IOptions<BookingSettings> settings)
        : IAppointmentService
    {
        private readonly BookingSettings _settings = settings.Value;

        public async Task<Result<AppointmentResponse>> CreateAppointmentAsync(
            CreateAppointmentRequest request,
            CancellationToken cancellationToken = default)
        {
            var startAt = request.StartAt.ToUniversalTime();
            var endAt = startAt.AddMinutes(request.DurationInMinutes);

            if (startAt <= timeProvider.GetUtcNow())
                return Result<AppointmentResponse>.Failure(AppointmentErrors.BookingInPast);

            var localStart = TimeZoneInfo.ConvertTime(
                startAt, _settings.TimeZone);

            var localEnd = TimeZoneInfo.ConvertTime(
                endAt, _settings.TimeZone);

            if (DateOnly.FromDateTime(localStart.DateTime) !=
                DateOnly.FromDateTime(localEnd.DateTime))
                return Result<AppointmentResponse>.Failure(AppointmentErrors.OutsideWorkingHours);

            var workStart = localStart.Date.Add(_settings.WorkingStart.ToTimeSpan());
            var workEnd = localStart.Date.Add(_settings.WorkingEnd.ToTimeSpan());

            if (localStart.DateTime < workStart || localEnd.DateTime > workEnd)
                return Result<AppointmentResponse>.Failure(AppointmentErrors.OutsideWorkingHours);

            var appointmentDate = DateOnly.FromDateTime(localStart.DateTime);

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var lockAcquired = await bookingLockProvider.AcquireAsync(
                    request.EmployeeId,
                    appointmentDate,
                    cancellationToken
                    );

                if (!lockAcquired)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<AppointmentResponse>.Failure(AppointmentErrors.BookingLockUnavailable);
                }

                var employeeRepository = unitOfWork.GetRepository<Employee, int>();

                var appointmentRepository = unitOfWork.GetRepository<Appointment, long>();

                var employee = await employeeRepository.GetFirstOrDefaultAsync(x => x.Id == request.EmployeeId);

                if (employee is null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<AppointmentResponse>.Failure(EmployeeErrors.NotFound);
                }

                if (!employee.IsActive)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<AppointmentResponse>.Failure(EmployeeErrors.Inactive);
                }

                var hasOverlap = await appointmentRepository
                    .GetAllQueryable()
                    .AnyAsync(x =>
                        x.EmployeeId == request.EmployeeId &&
                        x.Status == AppointmentStatus.Scheduled &&
                        x.StartAt < endAt &&
                        x.EndAt > startAt, cancellationToken);

                if (hasOverlap)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<AppointmentResponse>.Failure(AppointmentErrors.TimeSlotUnavailable);
                }

                var appointmentResult = Appointment.Create(
                    request.CustomerName,
                    request.PhoneNumber,
                    request.EmployeeId,
                    startAt,
                    endAt);

                if (appointmentResult.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<AppointmentResponse>.Failure(appointmentResult.Error!);
                }

                var appointment = appointmentResult.Value;

                await appointmentRepository.AddAsync(appointment);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                return mapper.Map<AppointmentResponse>(appointment);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Result<IReadOnlyList<AvailableSlotResponse>>> GetAvailableSlotsAsync(
            int employeeId,
            DateOnly date,
            int durationInMinutes,
            CancellationToken cancellationToken = default)
        {
            if (durationInMinutes is < 15 or > 120)
                return Result<IReadOnlyList<AvailableSlotResponse>>.Failure(AppointmentErrors.InvalidDuration);

            var employeeRepository =
                unitOfWork.GetRepository<Employee, int>();

            var appointmentRepository =
                unitOfWork.GetRepository<Appointment, long>();

            var employee = await employeeRepository.GetFirstOrDefaultAsync(
                x => x.Id == employeeId, cancellationToken);

            if (employee is null)
                return Result<IReadOnlyList<AvailableSlotResponse>>.Failure(EmployeeErrors.NotFound);

            if (!employee.IsActive)
                return Result<IReadOnlyList<AvailableSlotResponse>>.Failure(EmployeeErrors.Inactive);

            var localStart = date.ToDateTime(
                _settings.WorkingStart,
                DateTimeKind.Unspecified);

            var localEnd = date.ToDateTime(
                _settings.WorkingEnd,
                DateTimeKind.Unspecified);

            var startAt = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    localStart, _settings.TimeZone));

            var endAt = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    localEnd, _settings.TimeZone));

            var appointments = await appointmentRepository
                .GetAllQueryable()
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    x.Status == AppointmentStatus.Scheduled &&
                    x.StartAt < endAt &&
                    x.EndAt > startAt)
                .OrderBy(x => x.StartAt)
                .ToListAsync(cancellationToken);

            var slots = new List<AvailableSlotResponse>();
            var now = timeProvider.GetUtcNow();

            var slotStart = startAt;

            while (slotStart.AddMinutes(durationInMinutes) <= endAt)
            {
                var slotEnd = slotStart.AddMinutes(durationInMinutes);

                var overlaps = appointments.Any(x =>
                    x.StartAt < slotEnd && x.EndAt > slotStart);

                if (!overlaps && slotStart > now)
                {
                    slots.Add(new AvailableSlotResponse(
                        slotStart,
                        slotEnd));
                }

                slotStart = slotStart.AddMinutes(
                    _settings.SlotIntervalMinutes);
            }

            return slots;
        }

        public async Task<Result<PagedResult<EmployeeAppointmentResponse>>> GetEmployeeScheduleAsync(
            int employeeId,
            DateOnly date,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0 || pageSize <= 0 || pageSize > 100)
                return Result<PagedResult<EmployeeAppointmentResponse>>.Failure(AppointmentErrors.InvalidPagination);

            var employeeRepository =
                unitOfWork.GetRepository<Employee, int>();

            var appointmentRepository =
                unitOfWork.GetRepository<Appointment, long>();

            var employeeExists = await employeeRepository.GetFirstOrDefaultAsync(
                x => x.Id == employeeId, cancellationToken);

            if (employeeExists is null)
                return Result<PagedResult<EmployeeAppointmentResponse>>.Failure(EmployeeErrors.NotFound);

            var localStart = date.ToDateTime(
                _settings.WorkingStart,
                DateTimeKind.Unspecified);

            var localEnd = date.ToDateTime(
                _settings.WorkingEnd,
                DateTimeKind.Unspecified);

            var startAt = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    localStart, _settings.TimeZone));

            var endAt = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    localEnd, _settings.TimeZone));

            var query = appointmentRepository
                .GetAllQueryable()
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    x.Status == AppointmentStatus.Scheduled &&
                    x.StartAt >= startAt &&
                    x.StartAt < endAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.StartAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var mappedItems = mapper.Map<IReadOnlyList<EmployeeAppointmentResponse>>(items);

            return Result<PagedResult<EmployeeAppointmentResponse>>.Success(
                new PagedResult<EmployeeAppointmentResponse>(
                    mappedItems,
                    totalCount));
        }

        public async Task<Result> CancelAppointmentAsync(
            long appointmentId,
            CancellationToken cancellationToken = default)
        {
            var appointmentRepository = unitOfWork.GetRepository<Appointment, long>();

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var appointment = await appointmentRepository.GetFirstOrDefaultAsync(x => x.Id == appointmentId);

                if (appointment is null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure(AppointmentErrors.NotFound);
                }

                var localStart = TimeZoneInfo.ConvertTime(
                    appointment.StartAt, _settings.TimeZone);

                var appointmentDate = DateOnly.FromDateTime(
                    localStart.DateTime);

                var lockAcquired = await bookingLockProvider.AcquireAsync(
                    appointment.EmployeeId,
                    appointmentDate,
                    cancellationToken);

                if (!lockAcquired)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure(AppointmentErrors.BookingLockUnavailable);
                }

                // Re-read after acquiring the lock.
                appointment = await appointmentRepository.GetFirstOrDefaultAsync(x => x.Id == appointmentId, cancellationToken);

                if (appointment is null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure(AppointmentErrors.NotFound);
                }

                var cancelResult = appointment.Cancel(timeProvider.GetUtcNow());

                if (cancelResult.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure(cancelResult.Error!);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}

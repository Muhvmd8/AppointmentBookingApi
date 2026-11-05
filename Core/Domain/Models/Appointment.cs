using Shared;
using Shared.Errors;

namespace Domain.Models
{
    public class Appointment : BaseEntity<long>
    {
        public string CustomerName { get; private set; } = null!;
        public string PhoneNumber { get; private set; } = null!;
        public int EmployeeId { get; private set; }
        public Employee Employee { get; private set; } = null!;
        public DateTimeOffset StartAt { get; private set; }
        public DateTimeOffset EndAt { get; private set; }
        public AppointmentStatus Status { get; private set; }
        public DateTimeOffset? CancelledAt { get; private set; }
        public int DurationInMinutes => (int)(EndAt - StartAt).TotalMinutes;

        private Appointment()
        {
        }

        private Appointment(
            string customerName,
            string phoneNumber,
            int employeeId,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
        {
            CustomerName = customerName;
            PhoneNumber = phoneNumber;
            EmployeeId = employeeId;
            StartAt = startAt.ToUniversalTime();
            EndAt = endAt.ToUniversalTime();
            Status = AppointmentStatus.Scheduled;
        }

        public static Result<Appointment> Create(
            string customerName,
            string phoneNumber,
            int employeeId,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                return Result<Appointment>.Failure(AppointmentErrors.CustomerNameRequired);

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return Result<Appointment>.Failure(AppointmentErrors.PhoneNumberRequired);

            if (employeeId <= 0)
                return Result<Appointment>.Failure(AppointmentErrors.InvalidEmployeeId);

            if (endAt <= startAt)
                return Result<Appointment>.Failure(AppointmentErrors.InvalidTimeRange);

            var duration = (endAt - startAt).TotalMinutes;

            if (duration < 15 || duration > 120)
                return Result<Appointment>.Failure(AppointmentErrors.InvalidDuration);

            var appointment = new Appointment(
                customerName.Trim(),
                phoneNumber.Trim(),
                employeeId,
                startAt,
                endAt);

            return Result<Appointment>.Success(appointment);
        }

        public Result Cancel(DateTimeOffset cancelledAt)
        {
            if (Status == AppointmentStatus.Cancelled)
                return Result.Failure(AppointmentErrors.AlreadyCancelled);

            Status = AppointmentStatus.Cancelled;
            CancelledAt = cancelledAt.ToUniversalTime();

            return Result.Success();
        }

        public bool OverlapsWith(
            DateTimeOffset requestedStart,
            DateTimeOffset requestedEnd)
        {
            return StartAt < requestedEnd &&
                   EndAt > requestedStart;
        }
    }

}

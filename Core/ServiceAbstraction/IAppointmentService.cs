using Shared;
using Shared.DTOs.Appointment;

namespace ServiceAbstraction
{
    public interface IAppointmentService
    {
        Task<Result<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);

        Task<Result<IReadOnlyList<AvailableSlotResponse>>> GetAvailableSlotsAsync(
            int employeeId,
            DateOnly date,
            int durationInMinutes,
            CancellationToken cancellationToken = default);

        Task<Result<PagedResult<EmployeeAppointmentResponse>>> GetEmployeeScheduleAsync(
            int employeeId,
            DateOnly date,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<Result> CancelAppointmentAsync(long appointmentId, CancellationToken cancellationToken = default);
    }
}

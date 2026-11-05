using Shared;
using Shared.DTOs.Appointment;

namespace ServiceAbstraction
{
    public interface IAppointmentService
    {
        Task<Result<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request);

        Task<Result<IReadOnlyList<AvailableSlotResponse>>> GetAvailableSlotsAsync(
            int employeeId,
            DateOnly date,
            int durationInMinutes);

        Task<Result<PagedResult<EmployeeAppointmentResponse>>> GetEmployeeScheduleAsync(
            int employeeId,
            DateOnly date,
            int pageNumber,
            int pageSize);

        Task<Result> CancelAppointmentAsync(long appointmentId);
    }
}

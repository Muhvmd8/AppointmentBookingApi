namespace Shared.DTOs.Appointment
{
    public sealed record CreateAppointmentRequest(
        int EmployeeId,
        string CustomerName,
        string PhoneNumber,
        DateTimeOffset StartAt,
        int DurationInMinutes
    );
}

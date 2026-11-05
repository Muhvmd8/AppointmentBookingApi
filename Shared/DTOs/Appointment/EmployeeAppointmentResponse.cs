namespace Shared.DTOs.Appointment
{
    public sealed record EmployeeAppointmentResponse(
        int Id,
        string CustomerName,
        string PhoneNumber,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt);
}

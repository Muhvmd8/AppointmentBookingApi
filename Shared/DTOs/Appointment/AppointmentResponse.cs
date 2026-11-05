namespace Shared.DTOs.Appointment
{
    public sealed record AppointmentResponse(
        int Id,
        int EmployeeId,
        string EmployeeName,
        string CustomerName,
        string PhoneNumber,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        int DurationInMinutes,
        string Status
    );
}

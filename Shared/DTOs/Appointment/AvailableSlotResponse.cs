namespace Shared.DTOs.Appointment
{
    public sealed record AvailableSlotResponse(
        DateTimeOffset StartAt,
        DateTimeOffset EndAt);
}

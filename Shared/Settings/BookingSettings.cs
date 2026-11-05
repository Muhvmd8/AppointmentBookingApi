namespace Shared.Settings
{
    public sealed class BookingSettings
    {
        public TimeOnly WorkingStart { get; init; } = new(9, 0);
        public TimeOnly WorkingEnd { get; init; } = new(17, 0);
        public int SlotIntervalMinutes { get; init; } = 15;
        public TimeZoneInfo TimeZone { get; init; } =
            TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
    }
}

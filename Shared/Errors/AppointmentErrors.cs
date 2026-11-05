namespace Shared.Errors
{
    public static class AppointmentErrors
    {
        public static readonly Error NotFound =
            Error.NotFound(
                "Appointment.NotFound",
                "The appointment was not found.");

        public static readonly Error CustomerNameRequired =
            Error.Validation(
                "Appointment.CustomerNameRequired",
                "Customer name is required.");

        public static readonly Error PhoneNumberRequired =
            Error.Validation(
                "Appointment.PhoneNumberRequired",
                "Customer phone number is required.");

        public static readonly Error InvalidEmployeeId =
            Error.Validation(
                "Appointment.InvalidEmployeeId",
                "The employee ID must be valid.");

        public static readonly Error InvalidTimeRange =
            Error.Validation(
                "Appointment.InvalidTimeRange",
                "The appointment end time must be greater than its start time.");

        public static readonly Error InvalidDuration =
            Error.Validation(
                "Appointment.InvalidDuration",
                "Appointment duration must be between 15 and 120 minutes.");

        public static readonly Error BookingInPast =
            Error.Validation(
                "Appointment.BookingInPast",
                "Appointments cannot be booked in the past.");

        public static readonly Error OutsideWorkingHours =
            Error.Validation(
                "Appointment.OutsideWorkingHours",
                "The requested appointment is outside working hours.");

        public static readonly Error TimeSlotUnavailable =
            Error.Conflict(
                "Appointment.TimeSlotUnavailable",
                "The requested time slot is already booked.");

        public static readonly Error AlreadyCancelled =
            Error.Conflict(
                "Appointment.AlreadyCancelled",
                "The appointment is already cancelled.");
        public static readonly Error BookingLockUnavailable =
            Error.Conflict(
                "Appointment.BookingLockUnavailable",
                "Unable to acquire booking lock.");

        public static readonly Error InvalidPagination =
            Error.Validation(
                "Appointment.InvalidPagination",
                "Invalid pagination parameters.");
    }
}

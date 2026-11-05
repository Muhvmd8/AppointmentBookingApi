namespace Shared.Errors
{
    public static class EmployeeErrors
    {
        public static readonly Error InvalidName =
            Error.Validation(
                "Employee.InvalidName",
                "The employee name is invalid.");

        public static readonly Error InvalidPhoneNumber =
            Error.Validation(
                "Employee.InvalidPhoneNumber",
                "The employee phone number is invalid.");

        public static readonly Error NotFound =
            Error.NotFound(
                "Employee.NotFound",
                "The employee was not found.");

        public static readonly Error Inactive =
            Error.Validation(
                "Employee.Inactive",
                "The employee is not active and cannot accept appointments.");

        public static readonly Error InvalidPagination
            = Error.Validation(
                "Employee.InvalidPagination",
                "The pagination parameters are invalid.");
    }
}

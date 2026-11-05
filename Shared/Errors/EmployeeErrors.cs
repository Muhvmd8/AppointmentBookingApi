namespace Shared.Errors
{
    public static class EmployeeErrors
    {
        public static readonly Error NotFound =
            Error.NotFound(
                "Employee.NotFound",
                "The employee was not found.");

        public static readonly Error Inactive =
            Error.Validation(
                "Employee.Inactive",
                "The employee is not active and cannot accept appointments.");
    }
}

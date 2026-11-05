namespace Shared.DTOs.Employee
{
    public sealed record EmployeeResponse(
        int Id,
        string Name,
        string PhoneNumber,
        bool IsActive);
}

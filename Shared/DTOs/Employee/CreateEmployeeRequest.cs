namespace Shared.DTOs.Employee
{
    public sealed record CreateEmployeeRequest(
        string Name,
        string PhoneNumber);
}

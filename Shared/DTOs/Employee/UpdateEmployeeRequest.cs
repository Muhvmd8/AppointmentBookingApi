namespace Shared.DTOs.Employee
{
    public sealed record UpdateEmployeeRequest(
        string Name,
        string PhoneNumber);
}

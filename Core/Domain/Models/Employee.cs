using Shared;
using Shared.Errors;

namespace Domain.Models
{
    public class Employee : BaseEntity<int>
    {
        public string Name { get; private set; } = null!;
        public string? PhoneNumber { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Employee()
        {
        }

        private Employee(string name, string? phoneNumber)
        {
            Name = name;
            PhoneNumber = phoneNumber;
            IsActive = true;
        }

        public static Result<Employee> Create(string name, string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Employee>.Failure(EmployeeErrors.InvalidName);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return Result<Employee>.Failure(EmployeeErrors.InvalidPhoneNumber);

            var employee = new Employee(name.Trim(), phoneNumber?.Trim());
            return Result<Employee>.Success(employee);
        }

        public Result<Employee> UpdateInformation(string name, string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Employee>.Failure(EmployeeErrors.InvalidName);
            if (!string.IsNullOrWhiteSpace(phoneNumber))
                return Result<Employee>.Failure(EmployeeErrors.InvalidPhoneNumber);

            Name = name.Trim();
            PhoneNumber = phoneNumber?.Trim();
            return Result<Employee>.Success(this);
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public bool CanAcceptAppointments() => IsActive;
    }
}

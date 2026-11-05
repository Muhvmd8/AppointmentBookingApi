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

        public static Employee Create(string name, string? phoneNumber)
            => new Employee(name.Trim(), phoneNumber?.Trim());

        public void UpdateInformation(string name, string? phoneNumber)
        {
            Name = name.Trim();
            PhoneNumber = phoneNumber?.Trim();
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public bool CanAcceptAppointments() => IsActive;
    }
}

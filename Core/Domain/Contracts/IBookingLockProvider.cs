namespace Domain.Contracts
{
    public interface IBookingLockProvider
    {
        Task<bool> AcquireAsync(int employeeId, DateOnly appointmentDate);
    }
}

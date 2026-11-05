namespace Persistence.Locking
{
    public interface IBookingLockProvider
    {
        Task<bool> AcquireAsync(int employeeId, DateOnly appointmentDate);
    }
}

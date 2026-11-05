namespace Persistence.Locking
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Persistence.Data;

    public sealed class SqlServerBookingLockProvider(
        ApplicationDbContext context)
        : IBookingLockProvider
    {
        public async Task<bool> AcquireAsync(int employeeId, DateOnly appointmentDate)
        {
            var resource = $"AppointmentBooking:{employeeId}:{appointmentDate:yyyyMMdd}";

            var connection = context.Database.GetDbConnection();

            await using var command = connection.CreateCommand();

            command.Transaction = context.Database.CurrentTransaction!.GetDbTransaction();

            command.CommandText = """
            DECLARE @Result INT;

            EXEC @Result = sp_getapplock
                @Resource = @Resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 5000;

            SELECT @Result;
            """;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@Resource";
            parameter.Value = resource;
            command.Parameters.Add(parameter);

            var result = Convert.ToInt32(await command.ExecuteScalarAsync());

            return result >= 0;
        }
    }
}

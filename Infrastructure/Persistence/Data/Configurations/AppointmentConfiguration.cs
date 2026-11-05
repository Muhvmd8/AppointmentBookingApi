using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public sealed class AppointmentConfiguration
        : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments", table =>
            {
                table.HasCheckConstraint(
                    "CK_Appointments_ValidDuration",
                    """
                [EndAt] >= DATEADD(MINUTE, 15, [StartAt])
                AND [EndAt] <= DATEADD(MINUTE, 120, [StartAt])
                """);
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.EmployeeId)
                .IsRequired();

            builder.Property(x => x.StartAt)
                .HasColumnType("datetimeoffset(0)")
                .IsRequired();

            builder.Property(x => x.EndAt)
                .HasColumnType("datetimeoffset(0)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<byte>()
                .HasColumnType("tinyint")
                .IsRequired()
                .HasDefaultValue(AppointmentStatus.Scheduled);

            builder.Property(x => x.CancelledAt)
                .HasColumnType("datetimeoffset(0)");

            builder.Property(x => x.CreatedAt)
                .HasColumnType("datetimeoffset(0)")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("datetimeoffset(0)");

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.EmployeeId,
                x.StartAt
            })
                .HasDatabaseName(
                    "IX_Appointments_EmployeeId_StartAt_Active")
                .HasFilter("[Status] = 1")
                .IncludeProperties(x => new
                {
                    x.EndAt,
                    x.CustomerName,
                    x.PhoneNumber
                });
        }
    }
}

using HRManagement.Employees.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Employees.Api.Infrastructure.Data.Configurations;

public class JobResponsibilityConfiguration : IEntityTypeConfiguration<JobResponsibility>
{
    public void Configure(EntityTypeBuilder<JobResponsibility> builder)
    {
        builder.ToTable("JobResponsibilities");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(j => j.Position)
            .WithMany(p => p.Responsibilities)
            .HasForeignKey(j => j.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(j => !j.IsDeleted);
    }
}

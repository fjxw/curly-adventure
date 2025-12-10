using HRManagement.Payroll.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Payroll.Api.Infrastructure.Data;

public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options)
    {
    }

    public DbSet<StaffingTable> StaffingTables => Set<StaffingTable>();
    public DbSet<StaffingPosition> StaffingPositions => Set<StaffingPosition>();
    public DbSet<LaborNorm> LaborNorms => Set<LaborNorm>();
    public DbSet<TimeSheet> TimeSheets => Set<TimeSheet>();
    public DbSet<SalaryCalculation> SalaryCalculations => Set<SalaryCalculation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // StaffingTable
        modelBuilder.Entity<StaffingTable>(entity =>
        {
            entity.ToTable("StaffingTables");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // StaffingPosition
        modelBuilder.Entity<StaffingPosition>(entity =>
        {
            entity.ToTable("StaffingPositions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PositionName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DepartmentName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Salary).HasPrecision(18, 2);
            entity.HasOne(e => e.StaffingTable)
                .WithMany(s => s.Positions)
                .HasForeignKey(e => e.StaffingTableId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // LaborNorm
        modelBuilder.Entity<LaborNorm>(entity =>
        {
            entity.ToTable("LaborNorms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PositionName).HasMaxLength(200);
            entity.Property(e => e.StandardHoursPerDay).HasPrecision(5, 2);
            entity.Property(e => e.StandardHoursPerWeek).HasPrecision(5, 2);
            entity.Property(e => e.StandardHoursPerMonth).HasPrecision(6, 2);
            entity.Property(e => e.OvertimeMultiplier).HasPrecision(4, 2);
            entity.Property(e => e.NightShiftMultiplier).HasPrecision(4, 2);
            entity.Property(e => e.HolidayMultiplier).HasPrecision(4, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // TimeSheet
        modelBuilder.Entity<TimeSheet>(entity =>
        {
            entity.ToTable("TimeSheets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeName).HasMaxLength(200);
            entity.Property(e => e.WorkedHours).HasPrecision(6, 2);
            entity.Property(e => e.OvertimeHours).HasPrecision(6, 2);
            entity.Property(e => e.NightHours).HasPrecision(6, 2);
            entity.Property(e => e.HolidayHours).HasPrecision(6, 2);
            entity.Property(e => e.SickLeaveHours).HasPrecision(6, 2);
            entity.Property(e => e.VacationHours).HasPrecision(6, 2);
            entity.HasIndex(e => new { e.EmployeeId, e.Month, e.Year }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // SalaryCalculation
        modelBuilder.Entity<SalaryCalculation>(entity =>
        {
            entity.ToTable("SalaryCalculations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeName).HasMaxLength(200);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.OvertimePay).HasPrecision(18, 2);
            entity.Property(e => e.NightShiftPay).HasPrecision(18, 2);
            entity.Property(e => e.HolidayPay).HasPrecision(18, 2);
            entity.Property(e => e.Bonuses).HasPrecision(18, 2);
            entity.Property(e => e.Allowances).HasPrecision(18, 2);
            entity.Property(e => e.GrossSalary).HasPrecision(18, 2);
            entity.Property(e => e.IncomeTax).HasPrecision(18, 2);
            entity.Property(e => e.SocialTax).HasPrecision(18, 2);
            entity.Property(e => e.PensionContribution).HasPrecision(18, 2);
            entity.Property(e => e.OtherDeductions).HasPrecision(18, 2);
            entity.Property(e => e.TotalDeductions).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.EmployeeId, e.Month, e.Year }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Shared.Common.Models.BaseEntity entity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
                        break;
                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

using HRManagement.Recruitment.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Recruitment.Api.Infrastructure.Data;

public class RecruitmentDbContext : DbContext
{
    public RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options) : base(options)
    {
    }

    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<TrainingParticipant> TrainingParticipants => Set<TrainingParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vacancy>(entity =>
        {
            entity.ToTable("Vacancies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.Requirements).HasMaxLength(4000);
            entity.Property(e => e.DepartmentName).HasMaxLength(200);
            entity.Property(e => e.PositionName).HasMaxLength(200);
            entity.Property(e => e.SalaryFrom).HasPrecision(18, 2);
            entity.Property(e => e.SalaryTo).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ToTable("Candidates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ResumeUrl).HasMaxLength(500);
            entity.Property(e => e.CoverLetter).HasMaxLength(4000);
            entity.Property(e => e.Education).HasMaxLength(500);
            entity.Property(e => e.Skills).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.HasOne(e => e.Vacancy)
                .WithMany(v => v.Candidates)
                .HasForeignKey(e => e.VacancyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Interview>(entity =>
        {
            entity.ToTable("Interviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InterviewerName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(4000);
            entity.HasOne(e => e.Candidate)
                .WithMany(c => c.Interviews)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Training>(entity =>
        {
            entity.ToTable("Trainings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.Provider).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.Cost).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<TrainingParticipant>(entity =>
        {
            entity.ToTable("TrainingParticipants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeName).HasMaxLength(200);
            entity.Property(e => e.CertificateNumber).HasMaxLength(100);
            entity.Property(e => e.Feedback).HasMaxLength(2000);
            entity.HasOne(e => e.Training)
                .WithMany(t => t.Participants)
                .HasForeignKey(e => e.TrainingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.TrainingId, e.EmployeeId }).IsUnique();
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

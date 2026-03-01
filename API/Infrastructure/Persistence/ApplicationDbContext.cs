using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolAddress> SchoolAddresses => Set<SchoolAddress>();
    public DbSet<LocalAuthority> LocalAuthorities => Set<LocalAuthority>();
    public DbSet<PhaseOfEducation> PhasesOfEducation => Set<PhaseOfEducation>();
    public DbSet<EstablishmentType> EstablishmentTypes => Set<EstablishmentType>();
    public DbSet<EstablishmentGroup> EstablishmentGroups => Set<EstablishmentGroup>();
    public DbSet<EstablishmentStatus> EstablishmentStatuses => Set<EstablishmentStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // School
        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.URN)
                .IsRequired();

            entity.HasIndex(s => s.URN)
                .IsUnique();

            entity.Property(s => s.EstablishmentNumber)
                .IsRequired();

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(s => s.Version)
                .IsConcurrencyToken();

            // Relationships
            entity.HasOne(s => s.Address)
                .WithOne()
                .HasForeignKey<SchoolAddress>(a => a.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.PhaseOfEducation)
                .WithMany()
                .HasForeignKey(s => s.PhaseOfEducationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.LocalAuthority)
                .WithMany()
                .HasForeignKey(s => s.LocalAuthorityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.EstablishmentType)
                .WithMany()
                .HasForeignKey(s => s.EstablishmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.EstablishmentGroup)
                .WithMany()
                .HasForeignKey(s => s.EstablishmentGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.EstablishmentStatus)
                .WithMany()
                .HasForeignKey(s => s.EstablishmentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SchoolAddress
        modelBuilder.Entity<SchoolAddress>(entity =>
        {
            entity.HasKey(a => a.SchoolId);

            entity.Property(a => a.Town)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.PostCode)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(a => a.Version)
                .IsConcurrencyToken();
        });

        // LocalAuthority
        modelBuilder.Entity<LocalAuthority>(entity =>
        {
            entity.HasKey(la => la.Id);

            entity.HasIndex(la => la.Code)
                .IsUnique();

            entity.Property(la => la.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(la => la.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(la => la.Version)
                .IsConcurrencyToken();
        });

        // PhaseOfEducation
        modelBuilder.Entity<PhaseOfEducation>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasIndex(p => p.Code)
                .IsUnique();

            entity.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(p => p.Version)
                .IsConcurrencyToken();
        });

        // EstablishmentType
        modelBuilder.Entity<EstablishmentType>(entity =>
        {
            entity.HasKey(et => et.Id);

            entity.HasIndex(et => et.Code)
                .IsUnique();

            entity.Property(et => et.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(et => et.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(et => et.Version)
                .IsConcurrencyToken();
        });

        // EstablishmentGroup
        modelBuilder.Entity<EstablishmentGroup>(entity =>
        {
            entity.HasKey(eg => eg.Id);

            entity.HasIndex(eg => eg.Code)
                .IsUnique();

            entity.Property(eg => eg.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(eg => eg.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(eg => eg.Version)
                .IsConcurrencyToken();
        });

        // EstablishmentStatus
        modelBuilder.Entity<EstablishmentStatus>(entity =>
        {
            entity.HasKey(es => es.Id);

            entity.HasIndex(es => es.Code)
                .IsUnique();

            entity.Property(es => es.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(es => es.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(es => es.Version)
                .IsConcurrencyToken();
        });
    }
}
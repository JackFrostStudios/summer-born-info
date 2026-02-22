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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(s => s.Id);
            
            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.URN)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(s => s.URN)
                .IsUnique();

            entity.Property(s => s.Address)
                .HasMaxLength(500);

            entity.Property(s => s.City)
                .HasMaxLength(100);

            entity.Property(s => s.County)
                .HasMaxLength(100);

            entity.Property(s => s.Postcode)
                .HasMaxLength(20);

            entity.Property(s => s.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(s => s.Website)
                .HasMaxLength(500);

            entity.Property(s => s.CreatedAt)
                .HasDefaultValueSql("NOW()");
        });
    }
}
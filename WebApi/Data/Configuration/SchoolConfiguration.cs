using SummerBornInfo.Domain.School;

namespace SummerBornInfo.Data.Configuration;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("school");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired();

        builder.Property(s => s.URN)
            .IsRequired();
        builder.HasIndex(s => s.URN);

        builder.Property(s => s.UKPRN)
            .IsRequired();
        builder.HasIndex(s => s.UKPRN);

        builder.Property(s => s.EstablishmentNumber)
            .IsRequired();
        builder.HasIndex(s => s.EstablishmentNumber);

        builder.Property(s => s.Name)
            .HasMaxLength(2000)
            .IsRequired();
        builder.HasIndex(s => s.Name);


        builder.Property(s => s.Version)
            .IsRowVersion();

        builder
            .HasOne(s => s.Address)
            .WithOne()
            .HasForeignKey<Address>(a => a.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(s => s.Address)
            .AutoInclude()
            .IsRequired();

        builder
            .HasOne(s => s.PhaseOfEducation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(s => s.PhaseOfEducation)
            .AutoInclude();

        builder
            .HasOne(s => s.LocalAuthority)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(s => s.LocalAuthority)
            .AutoInclude();

        builder
            .HasOne(s => s.EstablishmentType)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(s => s.EstablishmentType)
            .AutoInclude();

        builder
            .HasOne(s => s.EstablishmentGroup)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(s => s.EstablishmentGroup)
            .AutoInclude();

        builder
            .HasOne(s => s.EstablishmentStatus)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(s => s.EstablishmentStatus)
            .AutoInclude();
    }
}

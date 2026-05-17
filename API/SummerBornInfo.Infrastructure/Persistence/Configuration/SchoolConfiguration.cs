namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        _ = builder.ToTable("school");

        _ = builder.HasKey(s => s.Id);
        _ = builder.Property(s => s.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(s => s.URN)
            .IsRequired();

        _ = builder.Property(s => s.EstablishmentNumber)
            .IsRequired();

        _ = builder.Property(s => s.Name)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property<uint>("Version")
            .IsRowVersion();

        _ = builder
            .HasOne(s => s.Address)
            .WithOne()
            .HasForeignKey<SchoolAddress>("Id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.Address)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.PhaseOfEducation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.PhaseOfEducation)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.LocalAuthority)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.LocalAuthority)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.EstablishmentType)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.EstablishmentType)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.EstablishmentGroup)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.EstablishmentGroup)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.EstablishmentStatus)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.EstablishmentStatus)
            .AutoInclude();
    }
}

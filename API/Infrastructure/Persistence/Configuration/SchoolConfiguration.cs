namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("school");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired();

        builder.Property(s => s.URN)
            .IsRequired();

        builder.Property(s => s.EstablishmentNumber)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property<uint>("Version")
            .IsRowVersion();

        builder
            .HasOne(s => s.Address)
            .WithOne()
            .HasForeignKey<SchoolAddress>("Id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .Navigation(s => s.Address)
            .AutoInclude();

        builder
            .HasOne(s => s.PhaseOfEducation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .Navigation(s => s.PhaseOfEducation)
            .AutoInclude();

        builder
            .HasOne(s => s.LocalAuthority)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .Navigation(s => s.LocalAuthority)
            .AutoInclude();

        builder
            .HasOne(s => s.EstablishmentType)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .Navigation(s => s.EstablishmentType)
            .AutoInclude();

        builder
            .HasOne(s => s.EstablishmentGroup)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .Navigation(s => s.EstablishmentGroup)
            .AutoInclude();

        builder
            .HasOne(s => s.EstablishmentStatus)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .Navigation(s => s.EstablishmentStatus)
            .AutoInclude();
    }
}

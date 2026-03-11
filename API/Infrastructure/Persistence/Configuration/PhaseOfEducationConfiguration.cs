namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal class PhaseOfEducationConfiguration : IEntityTypeConfiguration<PhaseOfEducation>
{
    public void Configure(EntityTypeBuilder<PhaseOfEducation> builder)
    {
        builder.ToTable("phase_of_education");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.Version)
            .IsRowVersion();

    }
}

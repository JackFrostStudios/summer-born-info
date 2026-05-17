namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class PhaseOfEducationConfiguration : IEntityTypeConfiguration<PhaseOfEducation>
{
    public void Configure(EntityTypeBuilder<PhaseOfEducation> builder)
    {
        _ = builder.ToTable("phase_of_education");

        _ = builder.HasKey(p => p.Id);
        _ = builder.Property(p => p.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(p => p.Code)
            .HasMaxLength(100)
            .IsRequired();

        _ = builder.HasIndex(p => p.Code)
            .IsUnique();

        _ = builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property<uint>("Version")
            .IsRowVersion();

    }
}

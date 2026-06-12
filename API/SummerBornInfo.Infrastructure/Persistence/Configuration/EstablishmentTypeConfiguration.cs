namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
internal sealed class EstablishmentTypeConfiguration : IEntityTypeConfiguration<EstablishmentType>
{
    public void Configure(EntityTypeBuilder<EstablishmentType> builder)
    {
        _ = builder.ToTable("establishment_type");

        _ = builder.HasKey(et => et.Id);
        _ = builder.Property(et => et.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(et => et.Code)
            .HasMaxLength(100)
            .IsRequired();

        _ = builder.HasIndex(et => et.Code)
            .IsUnique();

        _ = builder.Property(et => et.Name)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

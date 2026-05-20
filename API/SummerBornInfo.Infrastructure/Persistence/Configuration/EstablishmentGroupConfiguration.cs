namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class EstablishmentGroupConfiguration : IEntityTypeConfiguration<EstablishmentGroup>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroup> builder)
    {
        _ = builder.ToTable("establishment_group");

        _ = builder.HasKey(eg => eg.Id);
        _ = builder.Property(eg => eg.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(eg => eg.Code)
            .HasMaxLength(100)
            .IsRequired();

        _ = builder.HasIndex(eg => eg.Code)
            .IsUnique();

        _ = builder.Property(eg => eg.Name)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

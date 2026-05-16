namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class EstablishmentGroupConfiguration : IEntityTypeConfiguration<EstablishmentGroup>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroup> builder)
    {
        builder.ToTable("establishment_group");

        builder.HasKey(eg => eg.Id);
        builder.Property(eg => eg.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(eg => eg.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(eg => eg.Code)
            .IsUnique();

        builder.Property(eg => eg.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal class EstablishmentTypeConfiguration : IEntityTypeConfiguration<EstablishmentType>
{
    public void Configure(EntityTypeBuilder<EstablishmentType> builder)
    {
        builder.ToTable("establishment_type");

        builder.HasKey(et => et.Id);
        builder.Property(et => et.Id)
            .IsRequired();

        builder.Property(et => et.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(et => et.Code)
            .IsUnique();

        builder.Property(et => et.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(et => et.Version)
            .IsRowVersion();
    }
}

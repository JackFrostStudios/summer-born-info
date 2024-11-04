namespace SummerBornInfo.Data.Configuration;

public class EstablishmentTypeConfiguration : IEntityTypeConfiguration<EstablishmentType>
{
    public void Configure(EntityTypeBuilder<EstablishmentType> builder)
    {
        builder.ToTable("establishment_type");

        builder.HasKey(et => et.Id);
        builder.Property(et => et.Id)
            .IsRequired();

        builder.Property(et => et.Code)
            .IsRequired();

        builder.HasIndex(et => et.Code)
            .IsUnique();

        builder.Property(et => et.Name)
            .IsRequired();

        builder.Property(et => et.Version)
            .IsRowVersion();
    }
}

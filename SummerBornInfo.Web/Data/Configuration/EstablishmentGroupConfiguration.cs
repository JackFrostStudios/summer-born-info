namespace SummerBornInfo.Web.Data.Configuration;

public class EstablishmentGroupConfiguration : IEntityTypeConfiguration<EstablishmentGroup>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroup> builder)
    {
        builder.ToTable("establishment_group");

        builder.HasKey(eg => eg.Id);
        builder.Property(eg => eg.Id)
            .IsRequired();

        builder.Property(eg => eg.Code)
            .IsRequired();

        builder.HasIndex(eg => eg.Code)
            .IsUnique();

        builder.Property(eg => eg.Name)
            .IsRequired();

        builder.Property(eg => eg.Version)
            .IsRowVersion();
    }
}

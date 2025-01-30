namespace SummerBornInfo.Web.Data.Configuration;

internal class EstablishmentStatusConfiguration : IEntityTypeConfiguration<EstablishmentStatus>
{
    public void Configure(EntityTypeBuilder<EstablishmentStatus> builder)
    {
        builder.ToTable("establishment_status");

        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id)
            .IsRequired();

        builder.Property(es => es.Code)
            .IsRequired();

        builder.HasIndex(es => es.Code)
            .IsUnique();

        builder.Property(es => es.Name)
            .IsRequired();

        builder.Property(es => es.Version)
            .IsRowVersion();
    }
}

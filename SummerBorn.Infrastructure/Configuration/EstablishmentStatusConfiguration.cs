namespace SummerBorn.Infrastructure.Configuration;

public class EstablishmentStatusConfiguration : IEntityTypeConfiguration<EstablishmentStatus>
{
    public void Configure(EntityTypeBuilder<EstablishmentStatus> builder)
    {
        builder.ToTable("establishment_status");

        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id)
            .IsRequired();

        builder.Property(es => es.Version)
            .IsRowVersion();
    }
}

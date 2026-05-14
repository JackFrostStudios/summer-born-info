namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class EstablishmentStatusConfiguration : IEntityTypeConfiguration<EstablishmentStatus>
{
    public void Configure(EntityTypeBuilder<EstablishmentStatus> builder)
    {
        builder.ToTable("establishment_status");

        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(es => es.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(es => es.Code)
            .IsUnique();

        builder.Property(es => es.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

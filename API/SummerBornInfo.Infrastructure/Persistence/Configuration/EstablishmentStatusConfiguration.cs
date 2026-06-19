namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
internal sealed class EstablishmentStatusConfiguration : IEntityTypeConfiguration<EstablishmentStatus>
{
    public void Configure(EntityTypeBuilder<EstablishmentStatus> builder)
    {
        _ = builder.ToTable("establishment_status");

        _ = builder.HasKey(es => es.Id);
        _ = builder.Property(es => es.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(es => es.Code)
            .HasMaxLength(100)
            .IsRequired();

        _ = builder.HasIndex(es => es.Code)
            .IsUnique();

        _ = builder.Property(es => es.Name)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

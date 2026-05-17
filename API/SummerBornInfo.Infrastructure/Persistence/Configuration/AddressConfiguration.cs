namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<SchoolAddress>
{
    public void Configure(EntityTypeBuilder<SchoolAddress> builder)
    {
        _ = builder.ToTable("school");

        _ = builder.Property<Guid>("Id")
            .IsRequired()
            .ValueGeneratedNever();
        _ = builder.HasKey("Id");

        _ = builder.Property(a => a.Street)
            .HasMaxLength(300);

        _ = builder.Property(a => a.Locality)
            .HasMaxLength(300);

        _ = builder.Property(a => a.AddressThree)
            .HasMaxLength(300);

        _ = builder.Property(a => a.Town)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property(a => a.County)
            .HasMaxLength(300);

        _ = builder.Property(a => a.PostCode)
            .HasMaxLength(30)
            .IsRequired();

        _ = builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

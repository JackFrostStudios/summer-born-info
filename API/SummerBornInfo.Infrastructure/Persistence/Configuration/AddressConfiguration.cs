namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<SchoolAddress>
{
    public void Configure(EntityTypeBuilder<SchoolAddress> builder)
    {
        builder.ToTable("school");

        builder.Property<Guid>("Id")
            .IsRequired()
            .ValueGeneratedNever();
        builder.HasKey("Id");

        builder.Property(a => a.Street)
            .HasMaxLength(300);

        builder.Property(a => a.Locality)
            .HasMaxLength(300);

        builder.Property(a => a.AddressThree)
            .HasMaxLength(300);

        builder.Property(a => a.Town)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(a => a.County)
            .HasMaxLength(300);

        builder.Property(a => a.PostCode)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

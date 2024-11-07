using SummerBornInfo.Web.Domain.School;

namespace SummerBornInfo.Web.Data.Configuration;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("school");

        builder.HasKey(a => a.SchoolId);
        builder.Property(a => a.SchoolId)
            .HasColumnName("Id")
            .IsRequired();

        builder.HasIndex(a => a.Street);
        builder.HasIndex(a => a.Locality);
        builder.HasIndex(a => a.AddressThree);
        builder.HasIndex(a => a.Town);
        builder.HasIndex(a => a.County);
        builder.HasIndex(a => a.PostCode);

        builder.Property(a => a.Version)
            .IsRowVersion();
    }
}

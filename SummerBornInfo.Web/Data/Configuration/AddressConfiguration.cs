﻿namespace SummerBornInfo.Web.Data.Configuration;

internal class AddressConfiguration : IEntityTypeConfiguration<SchoolAddress>
{
    public void Configure(EntityTypeBuilder<SchoolAddress> builder)
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

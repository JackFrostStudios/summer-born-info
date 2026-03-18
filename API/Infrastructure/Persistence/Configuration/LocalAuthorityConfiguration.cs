namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal class LocalAuthorityConfiguration : IEntityTypeConfiguration<LocalAuthority>
{
    public void Configure(EntityTypeBuilder<LocalAuthority> builder)
    {
        builder.ToTable("local_authority");

        builder.HasKey(la => la.Id);
        builder.Property(la => la.Id)
            .IsRequired();

        builder.Property(la => la.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(la => la.Code)
            .IsUnique();

        builder.Property(la => la.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

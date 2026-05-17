namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class LocalAuthorityConfiguration : IEntityTypeConfiguration<LocalAuthority>
{
    public void Configure(EntityTypeBuilder<LocalAuthority> builder)
    {
        _ = builder.ToTable("local_authority");

        _ = builder.HasKey(la => la.Id);
        _ = builder.Property(la => la.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(la => la.Code)
            .HasMaxLength(100)
            .IsRequired();

        _ = builder.HasIndex(la => la.Code)
            .IsUnique();

        _ = builder.Property(la => la.Name)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property<uint>("Version")
            .IsRowVersion();
    }
}

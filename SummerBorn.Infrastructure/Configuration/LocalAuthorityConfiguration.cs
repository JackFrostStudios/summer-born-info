namespace SummerBorn.Infrastructure.Configuration;

public class LocalAuthorityConfiguration : IEntityTypeConfiguration<LocalAuthority>
{
    public void Configure(EntityTypeBuilder<LocalAuthority> builder)
    {
        builder.ToTable("local_authority");

        builder.HasKey(la => la.Id);
        builder.Property(la => la.Id)
            .IsRequired();

        builder.Property(la => la.Version)
            .IsRowVersion();
    }
}

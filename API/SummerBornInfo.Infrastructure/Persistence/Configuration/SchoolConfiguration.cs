namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    private const string SearchVectorComputedColumnSql =
        """setweight(to_tsvector('simple', coalesce("Name", '')), 'A') || setweight(to_tsvector('simple', coalesce("Town", '')), 'B') || setweight(to_tsvector('simple', coalesce("PostCode", '')), 'B') || setweight(to_tsvector('simple', coalesce("Street", '')), 'C') || setweight(to_tsvector('simple', coalesce("Locality", '')), 'C') || setweight(to_tsvector('simple', coalesce("AddressThree", '')), 'C') || setweight(to_tsvector('simple', coalesce("County", '')), 'C')""";

    private const string SearchNameNormalizedComputedColumnSql =
        """lower(coalesce("Name", ''))""";

    private const string SearchPostcodeNormalizedComputedColumnSql =
        """replace(lower(coalesce("PostCode", '')), ' ', '')""";

    private const string SearchAddressNormalizedComputedColumnSql =
        """lower(btrim(coalesce("Street", '') || ' ' || coalesce("Locality", '') || ' ' || coalesce("AddressThree", '') || ' ' || coalesce("Town", '') || ' ' || coalesce("County", '') || ' ' || coalesce("PostCode", '')))""";

    public void Configure(EntityTypeBuilder<School> builder)
    {
        _ = builder.ToTable("school");

        _ = builder.HasKey(s => s.Id);
        _ = builder.Property(s => s.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(s => s.URN)
            .IsRequired();

        _ = builder.HasIndex(s => s.URN)
            .HasDatabaseName("ix_school_urn")
            .IsUnique();

        _ = builder.Property(s => s.EstablishmentNumber)
            .IsRequired();

        _ = builder.Property(s => s.Name)
            .HasMaxLength(300)
            .IsRequired();

        _ = builder.Property(s => s.Location)
            .HasColumnType("geography (point, 4326)");

        _ = builder.HasIndex(s => s.Location)
            .HasDatabaseName("ix_school_location")
            .HasMethod("gist");

        _ = builder.Property<NpgsqlTsVector>("SearchVector")
            .HasColumnName("search_vector")
            .HasColumnType("tsvector")
            .HasComputedColumnSql(SearchVectorComputedColumnSql, stored: true);

        _ = builder.Property<string>("SearchNameNormalized")
            .HasColumnName("search_name_normalized")
            .HasComputedColumnSql(SearchNameNormalizedComputedColumnSql, stored: true);

        _ = builder.Property<string>("SearchPostcodeNormalized")
            .HasColumnName("search_postcode_normalized")
            .HasComputedColumnSql(SearchPostcodeNormalizedComputedColumnSql, stored: true);

        _ = builder.Property<string>("SearchAddressNormalized")
            .HasColumnName("search_address_normalized")
            .HasComputedColumnSql(SearchAddressNormalizedComputedColumnSql, stored: true);

        _ = builder.HasIndex("SearchVector")
            .HasDatabaseName("ix_school_search_vector")
            .HasMethod("gin");

        _ = builder.HasIndex("SearchNameNormalized")
            .HasDatabaseName("ix_school_search_name_normalized")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        _ = builder.HasIndex("SearchPostcodeNormalized")
            .HasDatabaseName("ix_school_search_postcode_normalized")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        _ = builder.HasIndex("SearchAddressNormalized")
            .HasDatabaseName("ix_school_search_address_normalized")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        _ = builder.Property<uint>("Version")
            .IsRowVersion();

        _ = builder
            .HasOne(s => s.Address)
            .WithOne()
            .HasForeignKey<SchoolAddress>("Id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.Address)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.PhaseOfEducation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.PhaseOfEducation)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.LocalAuthority)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.LocalAuthority)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.EstablishmentType)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.EstablishmentType)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.EstablishmentGroup)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.EstablishmentGroup)
            .AutoInclude();

        _ = builder
            .HasOne(s => s.EstablishmentStatus)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        _ = builder
            .Navigation(s => s.EstablishmentStatus)
            .AutoInclude();
    }
}

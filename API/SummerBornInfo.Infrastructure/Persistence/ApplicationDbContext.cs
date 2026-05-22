namespace SummerBornInfo.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>(options)
{
    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolAddress> SchoolAddresses => Set<SchoolAddress>();
    public DbSet<LocalAuthority> LocalAuthorities => Set<LocalAuthority>();
    public DbSet<PhaseOfEducation> PhasesOfEducation => Set<PhaseOfEducation>();
    public DbSet<EstablishmentType> EstablishmentTypes => Set<EstablishmentType>();
    public DbSet<EstablishmentGroup> EstablishmentGroups => Set<EstablishmentGroup>();
    public DbSet<EstablishmentStatus> EstablishmentStatuses => Set<EstablishmentStatus>();
    public DbSet<SchoolBulkImportRequest> SchoolBulkImportRequests => Set<SchoolBulkImportRequest>();
    public DbSet<SchoolBulkImportFailure> SchoolBulkImportFailures => Set<SchoolBulkImportFailure>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureIdentityPersistence();
        _ = builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public NpgsqlConnection GetNpgsqlConnection()
    {
        return (NpgsqlConnection)Database.GetDbConnection();
    }
}

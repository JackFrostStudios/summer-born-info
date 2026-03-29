using Npgsql;

namespace SummerBornInfo.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolAddress> SchoolAddresses => Set<SchoolAddress>();
    public DbSet<LocalAuthority> LocalAuthorities => Set<LocalAuthority>();
    public DbSet<PhaseOfEducation> PhasesOfEducation => Set<PhaseOfEducation>();
    public DbSet<EstablishmentType> EstablishmentTypes => Set<EstablishmentType>();
    public DbSet<EstablishmentGroup> EstablishmentGroups => Set<EstablishmentGroup>();
    public DbSet<EstablishmentStatus> EstablishmentStatuses => Set<EstablishmentStatus>();
    public DbSet<SchoolBulkImportRequest> SchoolBulkImportRequests => Set<SchoolBulkImportRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public NpgsqlConnection GetNpgsqlConnection()
    {
        return (NpgsqlConnection)Database.GetDbConnection();
    }
}
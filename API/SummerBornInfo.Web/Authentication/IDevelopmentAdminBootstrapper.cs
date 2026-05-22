namespace SummerBornInfo.Web.Authentication;

internal interface IDevelopmentAdminBootstrapper
{
    Task UpsertAsync(CancellationToken cancellationToken);
}

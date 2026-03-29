using Npgmq;
using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Events;

public sealed class EventEmitter(ApplicationDbContext dbContext) : IEventEmitter
{
    public async Task EmitEventAsync<T>(T message, CancellationToken cancellationToken) where T : class
    {
        var npgmq = new NpgmqClient(dbContext.GetNpgsqlConnection());
        await npgmq.SendAsync<T>(EventQueues.SchoolBulkImport, message, cancellationToken);
    }
}

using BigMission.RedMist.DriveSync.Multicast.Models;
using BigMission.TestHelpers;

namespace BigMission.RedMist.DriveSync.Multicast.DriveSync;

public class SubscriptionTracker(IDateTimeHelper dateTime)
{
    private static readonly TimeSpan timeout = TimeSpan.FromSeconds(15);

    private Dictionary<string, DateTime> Subscriptions { get; } = [];
    private readonly SemaphoreSlim semaphoreSlim = new(1);

    public async Task SubscribeAsync(Command subscription, CancellationToken stoppingToken = default)
    {
        if (subscription.Type == Command.ChannelStatusType)
        {
            await semaphoreSlim.WaitAsync(stoppingToken);
            Subscriptions[subscription.Type] = dateTime.Now;
            semaphoreSlim.Release();
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public async Task<bool> IsSubscribedAsync(string type, CancellationToken stoppingToken = default)
    {
        await semaphoreSlim.WaitAsync(stoppingToken);
        bool result = false;
        if (Subscriptions.TryGetValue(type, out var lastSeen))
        {
            if (dateTime.Now - lastSeen > timeout)
            {
                Subscriptions.Remove(type);
            }
            else
            {
                result = true;
            }
        }

        semaphoreSlim.Release();
        return result;
    }
}

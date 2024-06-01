using BigMission.RedMist.DriveSync.Multicast.Models;
using BigMission.TestHelpers;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System.Diagnostics;

namespace BigMission.RedMist.DriveSync.Multicast.Client;

public class DriveSyncClient : IDisposable
{
    private readonly MulticastComm multicastComm;
    private Task? receiveTask;
    private volatile bool subscribedToStatus;
    private readonly ILogger logger;
    private readonly TimeSpan subscriptionRenewalInterval = TimeSpan.FromSeconds(5);
    public event Func<ChannelValueCollection, Task>? OnReceivedChannelStatus;
    public event Func<ChannelMappingCollection, Task>? OnReceivedChannelMappings;
    private readonly Dictionary<Guid, (ApplicationInfo, DateTime)> activeApplications = [];
    private readonly SemaphoreSlim activeApplicationsSemaphore = new(1);
    private static readonly TimeSpan applicationInfoTimeout = TimeSpan.FromSeconds(3);
    private IDateTimeHelper DateTime { get; }

    public DriveSyncClient(MulticastComm multicastComm, ILoggerFactory loggerFactory, IDateTimeHelper dateTime)
    {
        this.multicastComm = multicastComm;
        DateTime = dateTime;
        logger = loggerFactory.CreateLogger(GetType().Name);
        multicastComm.OnReceived += OnReceived;
    }

    public void SubscribeToStatus(CancellationToken cancellationToken = default)
    {
        if (subscribedToStatus)
        {
            return;
        }
        subscribedToStatus = true;
        var _ = Task.Run(async () => await RenewSubscription(cancellationToken), cancellationToken);
    }

    private async Task RenewSubscription(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && subscribedToStatus)
        {
            var sw = Stopwatch.StartNew();
            using var buff = new MemoryStream();
            var sub = new Command { Type = Command.ChannelStatusType };
            Serializer.Serialize(buff, sub);
            await multicastComm.SendAsync(buff.ToArray(), sub.GetType().ToTypeIndex(), cancellationToken);
            logger.LogTrace($"Sent status subscription renewal in {sw.ElapsedMilliseconds}ms");

            await Task.Delay(subscriptionRenewalInterval, cancellationToken);
        }
    }

    public void UnsubscribeStatus()
    {
        subscribedToStatus = false;
    }

    public async Task RequestChannelMappings()
    {
        var sw = Stopwatch.StartNew();
        using var buff = new MemoryStream();
        var cmd = new Command { Type = Command.SendChannelMappings };
        Serializer.Serialize(buff, cmd);
        await multicastComm.SendAsync(buff.ToArray(), cmd.GetType().ToTypeIndex());
        logger.LogTrace($"Sent channel mappings request in {sw.ElapsedMilliseconds}ms");
    }

    public void StartReceive(CancellationToken cancellationToken)
    {
        if (receiveTask != null)
        {
            return;
        }

        receiveTask = Task.Run(async () => await multicastComm.ReceiveMessagesAsync(cancellationToken), cancellationToken);
    }

    private async Task OnReceived(PayloadHeader payload)
    {
        try
        {
            using var stream = new MemoryStream(payload.Payload);
            switch (payload.PayloadTypeIndex)
            {
                // Application info
                case 1:
                    var appInfo = Serializer.Deserialize<ApplicationInfo>(stream);
                    await ProcessApplicationInfo(payload.Source, appInfo);
                    break;
                // Channel status
                case 3:
                    var status = Serializer.Deserialize<ChannelValueCollection>(stream);
                    if (OnReceivedChannelStatus != null)
                    {
                        await OnReceivedChannelStatus(status);
                    }
                    break;
                // Channel mappings
                case 4:
                    var mappings = Serializer.Deserialize<ChannelMappingCollection>(stream);
                    if (OnReceivedChannelMappings != null)
                    {
                        await OnReceivedChannelMappings(mappings);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing received message");
        }
    }

    private async Task ProcessApplicationInfo(Guid appId, ApplicationInfo appInfo)
    {
        await activeApplicationsSemaphore.WaitAsync();
        try
        {
            activeApplications[appId] = (appInfo, DateTime.Now);
        }
        finally
        {
            activeApplicationsSemaphore.Release();
        }
    }

    public async Task<ApplicationInfo[]> GetActiveApplications()
    {
        ApplicationInfo[] apps;
        await activeApplicationsSemaphore.WaitAsync();
        try
        {
            // Get active applications
            var now = DateTime.Now;
            apps = activeApplications.Values
                .Where(x => (now - x.Item2) <= applicationInfoTimeout)
                .Select(x => x.Item1)
                .ToArray();

            // Remove timed out applications
            foreach (var app in activeApplications.Keys.ToArray())
            {
                if ((now - activeApplications[app].Item2) > applicationInfoTimeout)
                {
                    activeApplications.Remove(app);
                }
            }
        }
        finally
        {
            activeApplicationsSemaphore.Release();
        }
        return apps;
    }

    public void Dispose()
    {
        UnsubscribeStatus();
        receiveTask?.Dispose();
        GC.SuppressFinalize(this);
    }
}

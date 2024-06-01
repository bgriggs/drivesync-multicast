using BigMission.RedMist.DriveSync.Multicast.Models;
using BigMission.TestHelpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace BigMission.RedMist.DriveSync.Multicast.DriveSync;

public class Transmitter : BackgroundService
{
    public IDateTimeHelper DateTime { get; }
    private readonly IApplicationInfoProvider applicationInfoProvider;
    private readonly MulticastComm comm;
    private readonly SubscriptionTracker subscriptions;
    private readonly IChannelProvider channelProvider;
    private readonly ILogger logger;
    private readonly static TimeSpan applicationInfoInterval = TimeSpan.FromMilliseconds(500);
    private DateTime lastApplicationInfoSent = System.DateTime.MinValue;

    public Transmitter(IApplicationInfoProvider applicationInfoProvider, MulticastComm comm,
        SubscriptionTracker subscriptions, IChannelProvider channelProvider, IDateTimeHelper dateTime,
        ILoggerFactory loggerFactory)
    {
        this.applicationInfoProvider = applicationInfoProvider;
        this.comm = comm;
        this.subscriptions = subscriptions;
        this.channelProvider = channelProvider;
        DateTime = dateTime;
        logger = loggerFactory.CreateLogger(GetType().Name);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Multicast transmitter starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Broadcast application info to clients
            if (DateTime.Now - lastApplicationInfoSent > applicationInfoInterval)
            {
                var appInfo = applicationInfoProvider.GetApplicationInfo();
                logger.LogTrace($"Sent application info {appInfo.Type}...");
                using var stream = new MemoryStream();
                Serializer.Serialize(stream, appInfo);
                await comm.SendAsync(stream.ToArray(), 1, stoppingToken);
                lastApplicationInfoSent = DateTime.Now;
            }

            // Check for channel status subscriptions
            bool sendStatus = await subscriptions.IsSubscribedAsync(Command.ChannelStatusType, stoppingToken);
            if (sendStatus)
            {
                // Send status
                logger.LogTrace("Sending channel values...");
                var channels = channelProvider.GetChannelValues();
                using var buff = new MemoryStream();
                Serializer.Serialize(buff, channels);
                await comm.SendAsync(buff.ToArray(), channels.GetType().ToTypeIndex(), stoppingToken);
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}

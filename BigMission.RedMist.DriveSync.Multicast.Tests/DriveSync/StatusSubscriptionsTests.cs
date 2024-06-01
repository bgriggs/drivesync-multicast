using BigMission.RedMist.DriveSync.Multicast.DriveSync;
using BigMission.RedMist.DriveSync.Multicast.Models;
using BigMission.TestHelpers;
using Moq;

namespace BigMission.RedMist.DriveSync.Multicast.Tests.DriveSync;

[TestClass]
public class StatusSubscriptionsTests
{
    [TestMethod]
    public async Task SubscribeAsync_ShouldAddSubscription_WhenCommandIsSendChannels()
    {
        var dateTimeHelper = new Mock<IDateTimeHelper>();
        dateTimeHelper.Setup(d => d.Now).Returns(DateTime.Now);

        var statusSubscriptions = new SubscriptionTracker(dateTimeHelper.Object);
        var subscription = new Command { Type = Command.ChannelStatusType };

        await statusSubscriptions.SubscribeAsync(subscription);

        Assert.IsTrue(await statusSubscriptions.IsSubscribedAsync(Command.ChannelStatusType));
    }

    [TestMethod]
    public async Task SubscribeAsync_ShouldThrowException_WhenCommandIsNotSendChannels()
    {
        var dateTimeHelper = new Mock<IDateTimeHelper>();
        dateTimeHelper.Setup(d => d.Now).Returns(DateTime.Now);

        var statusSubscriptions = new SubscriptionTracker(dateTimeHelper.Object);
        var subscription = new Command { Type = "InvalidCommand" };

        await Assert.ThrowsExceptionAsync<NotImplementedException>(() => statusSubscriptions.SubscribeAsync(subscription));
    }

    [TestMethod]
    public async Task IsSubscribedAsync_ShouldReturnFalse_WhenSubscriptionDoesNotExist()
    {
        var dateTimeHelper = new Mock<IDateTimeHelper>();
        dateTimeHelper.Setup(d => d.Now).Returns(DateTime.Now);

        var statusSubscriptions = new SubscriptionTracker(dateTimeHelper.Object);

        Assert.IsFalse(await statusSubscriptions.IsSubscribedAsync("NonExistentCommand"));
    }

    [TestMethod]
    public async Task IsSubscribedAsync_ShouldReturnFalse_WhenSubscriptionHasExpired()
    {
        var dateTimeHelper = new Mock<IDateTimeHelper>();
        dateTimeHelper.SetupSequence(d => d.Now)
            .Returns(DateTime.Now)
            .Returns(DateTime.Now.AddSeconds(16));

        var statusSubscriptions = new SubscriptionTracker(dateTimeHelper.Object);
        var subscription = new Command { Type = Command.ChannelStatusType };

        await statusSubscriptions.SubscribeAsync(subscription);

        Assert.IsFalse(await statusSubscriptions.IsSubscribedAsync(Command.ChannelStatusType));
    }

    [TestMethod]
    public async Task IsSubscribedAsync_ShouldReturnTrue_WhenSubscriptionIsValidAndNotExpired()
    {
        var dateTimeHelper = new Mock<IDateTimeHelper>();
        dateTimeHelper.SetupSequence(d => d.Now)
            .Returns(DateTime.Now)
            .Returns(DateTime.Now.AddSeconds(10));

        var statusSubscriptions = new SubscriptionTracker(dateTimeHelper.Object);
        var subscription = new Command { Type = Command.ChannelStatusType };

        await statusSubscriptions.SubscribeAsync(subscription);

        Assert.IsTrue(await statusSubscriptions.IsSubscribedAsync(Command.ChannelStatusType));
    }
}

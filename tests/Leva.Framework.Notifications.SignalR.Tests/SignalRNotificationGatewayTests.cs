using Leva.Framework.Fakes;
using Leva.Framework.Notifications;
using Leva.Framework.Notifications.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Xunit;

namespace Leva.Framework.Notifications.SignalR.Tests;

public sealed class SignalRNotificationGatewayTests
{
	[Fact]
	public async Task SendAsync_SendsPayloadToRecipientUser()
	{
		var client = new TestSignalRNotificationClient();
		var hub = new TestHubContext(client);
		var gateway = CreateGateway(hub);

		var notification = TestNotification(channel: SignalRNotificationOptions.DefaultChannel);
		ResultAssert.Success(await gateway.SendAsync(notification));
		Assert.Equal("principal-1", hub.Clients.LastUserId);

		var payload = Assert.Single(client.Received);
		Assert.Equal(notification.Id.Value, payload.Id);
		Assert.Equal(notification.Recipient.Id, payload.RecipientId);

		Assert.Equal(notification.Channel.Value, payload.Channel);
		Assert.Equal(notification.Subject, payload.Subject);
		Assert.Equal(notification.Body, payload.Body);
	}

	[Fact]
	public async Task SendAsync_RejectsUnsupportedChannel()
	{
		var client = new TestSignalRNotificationClient();
		var gateway = CreateGateway(new TestHubContext(client));
		var result = await gateway.SendAsync(TestNotification(channel: new NotificationChannel("email")));

		Assert.True(result.IsFailure);
		Assert.Empty(client.Received);
	}

	[Fact]
	public async Task SendAsync_RejectsMissingRecipientId()
	{
		var client = new TestSignalRNotificationClient();
		var gateway = CreateGateway(new TestHubContext(client));

		var result = await gateway.SendAsync(
			TestNotification(recipientId: " ", channel: SignalRNotificationOptions.DefaultChannel)
		);

		Assert.True(result.IsFailure);
		Assert.Empty(client.Received);
	}

	[Fact]
	public async Task SendAsync_ReturnsFailureWhenClientSendFails()
	{
		var client = new TestSignalRNotificationClient { Error = new InvalidOperationException("Disconnected.") };
		var gateway = CreateGateway(new TestHubContext(client));
		var result = await gateway.SendAsync(TestNotification(channel: SignalRNotificationOptions.DefaultChannel));

		Assert.True(result.IsFailure);
	}

	private static SignalRNotificationGateway CreateGateway(TestHubContext hub)
	{
		return new SignalRNotificationGateway(hub, Options.Create(new SignalRNotificationOptions()));
	}

	private static Notification TestNotification(
		string recipientId = "principal-1",
		NotificationChannel? channel = null
	)
	{
		return new Notification(
			NotificationId.New(),
			new NotificationRecipient(recipientId),
			channel ?? new NotificationChannel("signalr"),
			"Subject",
			"Body",
			DateTimeOffset.UtcNow
		);
	}

	private sealed class TestSignalRNotificationClient : ISignalRNotificationClient
	{
		public List<SignalRNotificationPayload> Received { get; } = [];
		public Exception? Error { get; init; }

		public Task ReceiveNotification(SignalRNotificationPayload notification)
		{
			if (Error is not null)
				throw Error;

			Received.Add(notification);
			return Task.CompletedTask;
		}
	}

	private sealed class TestHubContext(TestSignalRNotificationClient client)
		: IHubContext<SignalRNotificationHub, ISignalRNotificationClient>
	{
		public TestHubClients Clients { get; } = new(client);
		IHubClients<ISignalRNotificationClient> IHubContext<
			SignalRNotificationHub,
			ISignalRNotificationClient
		>.Clients => Clients;
		public IGroupManager Groups { get; } = new TestGroupManager();
	}

	private sealed class TestHubClients(TestSignalRNotificationClient client) : IHubClients<ISignalRNotificationClient>
	{
		public string? LastUserId { get; private set; }
		public ISignalRNotificationClient All => client;

		public ISignalRNotificationClient AllExcept(IReadOnlyList<string> excludedConnectionIds) => client;

		public ISignalRNotificationClient Client(string connectionId) => client;

		public ISignalRNotificationClient Clients(IReadOnlyList<string> connectionIds) => client;

		public ISignalRNotificationClient Group(string groupName) => client;

		public ISignalRNotificationClient GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) =>
			client;

		public ISignalRNotificationClient Groups(IReadOnlyList<string> groupNames) => client;

		public ISignalRNotificationClient User(string userId)
		{
			LastUserId = userId;
			return client;
		}

		public ISignalRNotificationClient Users(IReadOnlyList<string> userIds) => client;
	}

	private sealed class TestGroupManager : IGroupManager
	{
		public Task AddToGroupAsync(
			string connectionId,
			string groupName,
			CancellationToken cancellationToken = default
		) => Task.CompletedTask;

		public Task RemoveFromGroupAsync(
			string connectionId,
			string groupName,
			CancellationToken cancellationToken = default
		) => Task.CompletedTask;
	}
}

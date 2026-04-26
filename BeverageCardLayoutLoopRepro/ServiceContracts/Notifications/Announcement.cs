using System.Text.Json.Serialization;

namespace Beerbox.Service.Contracts.Notifications;

public record Announcement(string Body, string Subtitle, string Title) : INotification
{
	[JsonIgnore]
	public NotificationType Type { get; } = NotificationType.Announcement;
}

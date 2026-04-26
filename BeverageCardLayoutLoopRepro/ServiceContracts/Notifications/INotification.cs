namespace Beerbox.Service.Contracts.Notifications;

public interface INotification
{
	string Body { get; }
	string Subtitle { get; }
	string Title { get; }
	NotificationType Type { get; }
}

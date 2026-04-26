namespace Beerbox.Service.Contracts.Notifications;

public record NewOfferBeverage(
	string Maker,
	string Flavors,
	string ImageUrl,
	string Name,
	string OfferID,
	string Style,
	string Title
) : INotification
{
	public string Body { get; } = Name;
	public string Subtitle { get; } = Style;
	public NotificationType Type { get; } = NotificationType.NewOffer;
}

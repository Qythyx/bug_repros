namespace Beerbox.Service.Contracts.Notifications;

public static class DictionaryParser
{
	public static bool Parse(Func<string, string> fieldGetter, out INotification notification)
	{
		notification = Enum.Parse<NotificationType>(fieldGetter(nameof(INotification.Type))) switch
		{
			NotificationType.Announcement => new Announcement(
				fieldGetter(nameof(Announcement.Title)),
				fieldGetter(nameof(Announcement.Subtitle)),
				fieldGetter(nameof(Announcement.Body))
			),
			NotificationType.NewOffer => new NewOfferBeverage(
				fieldGetter(nameof(NewOfferBeverage.Maker)),
				fieldGetter(nameof(NewOfferBeverage.Flavors)),
				fieldGetter(nameof(NewOfferBeverage.ImageUrl)),
				fieldGetter(nameof(NewOfferBeverage.Name)),
				fieldGetter(nameof(NewOfferBeverage.OfferID)),
				fieldGetter(nameof(NewOfferBeverage.Style)),
				fieldGetter(nameof(NewOfferBeverage.Title))
			),
			_ => null!,
		};
		return notification != null;
	}
}

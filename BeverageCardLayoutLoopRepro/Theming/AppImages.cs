namespace Beerbox.App.Theming;

/// <summary>
/// Centralized image references, replacing scattered ImageSource.FromFile() string literals.
/// </summary>
internal static class AppImages
{
	internal static ImageSource AppLogo { get; } = ImageSource.FromFile("app_logo.png");
	internal static ImageSource ChevronLeft { get; } = ImageSource.FromFile("chevron_left.png");
	internal static ImageSource HamburgerMenu { get; } = ImageSource.FromFile("hamburger_menu.png");
	internal static ImageSource OfferEnding { get; } = ImageSource.FromFile("icon_offer_ending.png");
	internal static ImageSource OfferSale { get; } = ImageSource.FromFile("icon_offer_sale.png");
	internal static ImageSource OfferExclusive { get; } = ImageSource.FromFile("icon_offer_exclusive.png");
	internal static ImageSource OfferRare { get; } = ImageSource.FromFile("icon_offer_rare.png");

	internal static readonly ThemePair<string> Background = new("background_light.jpg", "background_dark.jpg");

	internal static readonly string[] Welcome =
	[
		"welcome_1.png",
		"welcome_2.png",
		"welcome_3.png",
		"welcome_4.png",
		"welcome_5.png",
	];

	public static class Icons
	{
		internal static ImageSource About { get; } = ImageSource.FromFile("icon_about.png");
		internal static ImageSource Account { get; } = ImageSource.FromFile("icon_account.png");
		internal static ImageSource Beerbox { get; } = ImageSource.FromFile("icon_beerbox.png");
		internal static ImageSource ContactUs { get; } = ImageSource.FromFile("icon_contact_us.png");
		internal static ImageSource CreateAccount { get; } = ImageSource.FromFile("icon_create_account.png");
		internal static ImageSource HowItWorks { get; } = ImageSource.FromFile("icon_how_it_works.png");
		internal static ImageSource Login { get; } = ImageSource.FromFile("icon_login.png");
		internal static ImageSource Logout { get; } = ImageSource.FromFile("icon_logout.png");
		internal static ImageSource Offers { get; } = ImageSource.FromFile("icon_offers.png");
		internal static ImageSource OrderHistory { get; } = ImageSource.FromFile("icon_order_history.png");
		internal static ImageSource Settings { get; } = ImageSource.FromFile("icon_settings.png");
	}
}

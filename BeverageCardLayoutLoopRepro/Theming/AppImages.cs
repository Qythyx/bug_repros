namespace Beerbox.App.Theming;

/// <summary>
/// Centralized image references, replacing scattered ImageSource.FromFile() string literals.
/// </summary>
internal static class AppImages
{
	internal static ImageSource AppLogo { get; } = ImageSource.FromFile("app_logo.png");
	internal static ImageSource ChevronLeft { get; } = ImageSource.FromFile("chevron_left.png");
	internal static ImageSource HamburgerMenu { get; } = ImageSource.FromFile("hamburger_menu.png");

	internal static readonly ThemePair<string> Background = new("background_light.jpg", "background_dark.jpg");

	public static class Icons
	{
		internal static ImageSource OrderHistory { get; } = ImageSource.FromFile("icon_order_history.png");
	}
}

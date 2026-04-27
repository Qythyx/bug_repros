using Beerbox.App.Pages;
using Beerbox.App.Theming;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using MauiReactor;
#if DEBUG
using MauiReactor.HotReload;
#endif

namespace Beerbox.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
			.UseMauiReactorApp<MainShell>(app => app.UseTheme<AppTheme>())
#if DEBUG
			.UseMauiReactorHotReload()
#endif
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup()
			.ConfigureFonts(fonts =>
				fonts
					.AddFont("Roboto-Bold.ttf", AppFonts.RobotoBold)
					.AddFont("Roboto-Regular.ttf", AppFonts.Roboto)
					.AddFont("RobotoCondensed-Bold.ttf", AppFonts.RobotoCondensedBold)
					.AddFont("RobotoCondensed-Regular.ttf", AppFonts.RobotoCondensed)
					.AddFont("RobotoMono-Bold.ttf", AppFonts.RobotoMonoBold)
					.AddFont("RobotoMono-Regular.ttf", AppFonts.RobotoMono)
			);

#if DEBUG
		Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions.AddDebug(builder.Logging);
#endif

		return builder.Build();
	}
}

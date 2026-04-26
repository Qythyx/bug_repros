using Beerbox.App.Core;
using Beerbox.App.Core.Services;
using Beerbox.App.Pages;
using Beerbox.App.Services;
using Beerbox.App.Theming;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using MauiReactor;
using Microsoft.Maui.LifecycleEvents;
using PanCardView;
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
			.UseCardsView()
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

		builder.Services.AddSingleton<AppSettings>(new BeerboxAppSettings(DefaultLanguageProvider.DefaultLanguage));
		builder.Services.AddSingleton<IBackendConfiguration>(sp => new BackendConfiguration(
			sp.GetRequiredService<AppSettings>(),
			DeviceInfo.Current.Platform == DevicePlatform.Android
		));
		builder.Services.AddSingleton<IPlatformContext, PlatformContext>();
		builder.Services.AddSingleton<IVibrator, Vibrator>();
		builder.Services.AddSingleton<LoggerBase, Logger>();

		builder.Services.AddSingleton<IConnectivityService, MockConnectivityService>();
		var mockDataManager = new MockDataManager();
		builder.Services.AddSingleton<DataManager>(mockDataManager);
		builder.Services.AddSingleton<BeerboxTelemetryService>(mockDataManager.MockTelemetry);
		builder.Services.AddSingleton<IExceptionHandler>(mockDataManager.MockTelemetry);

		builder.Services.AddSingleton<AppLifecycleService>();

		builder.ConfigureLifecycleEvents(events =>
			events.AddiOS(ios =>
			{
				ios.OnActivated(_ => AppLifecycleService.OnResume());
				ios.OnResignActivation(_ => AppLifecycleService.OnSleep());
			})
		);

		Platforms.iOS.Handlers.ShellHandlerExtensions.Register(builder);
		Platforms.iOS.Handlers.DatePickerHandlerExtensions.Register();
		Platforms.iOS.Handlers.EditorHandlerExtensions.Register();
		Platforms.iOS.Handlers.EntryHandlerExtensions.Register();
		Platforms.iOS.Handlers.PickerHandlerExtensions.Register();
		Platforms.iOS.Handlers.RefreshViewHandlerExtensions.Register();

		return builder.Build();
	}
}

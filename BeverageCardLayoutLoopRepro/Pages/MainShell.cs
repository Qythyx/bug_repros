using Beerbox.App.Services;
using Beerbox.App.Theming;
using MauiReactor;
using AIs = Beerbox.App.Theming.AppImages.Icons;
using ARs = Beerbox.App.Resources.Localization.AppResources;
using IDs = Beerbox.App.AutomationIds.Shell;
using MauiControls = Microsoft.Maui.Controls;
using R = Beerbox.App.Pages.MainShell.Routes;

namespace Beerbox.App.Pages;

public sealed class MainShell : Component<MainShell.MyState>
{
	public sealed class MyState
	{
		public bool IsReady { get; set; }
		public bool FlyoutIsPresented { get; set; }
	}

	protected override void OnMounted()
	{
		base.OnMounted();

		Services.GetRequiredService<AppLifecycleService>().Initialize();

		// Defer Shell rendering to the second pass — iOS Shell flyout crashes if
		// rendered during the initial component mount before the platform is ready.
		SetState(s => s.IsReady = true);
	}

	private static MauiReactor.ShellContent Item(
		string title,
		ImageSource icon,
		Func<VisualNode> renderer,
		string route,
		AutoId<MauiReactor.ShellContent> automationID
	) => ShellContent(title).FlyoutIcon(icon).RenderContent(renderer).Route(route).ID(automationID);

	public override VisualNode Render()
	{
		if (!State.IsReady)
		{
			return ContentPage();
		}

		IPlatformApplication.Current?.Services.GetService<IStatusBarStyleManager>()?.SetColor(AppColors.ControlArea.Resolve);

		return Shell(
				Item(ARs.OffersTitle, AIs.Offers, () => new Offers(), R.Offers, IDs.Offers),
				Item(ARs.OrderHistoryTitle, AIs.OrderHistory, () => new OrderHistory(), R.OrderHistory, IDs.OrderHistory)
			)
			.FlyoutIsPresented(State.FlyoutIsPresented)
			.FlyoutWidth(AppStyles.FlyoutMenuWidth)
			.FlyoutBackgroundImage(AppImages.Background.Resolve)
			.FlyoutBackgroundImageAspect(Aspect.AspectFill)
			.FlyoutIcon(new FontImageSource { Glyph = "", Size = 0 })
			.FlyoutHeader(RenderFlyoutHeader())
			.ItemTemplate(RenderItemTemplate)
			.OnNavigating(HandleNavigating)
			.OnNavigated(HandleNavigated);
	}

	private static MauiReactor.Grid RenderFlyoutHeader() =>
		Grid(
#if ANDROID
				Image(AppImages.Background.Resolve).Scale(4.4),
#endif
				Image()
					.Source(AppImages.AppLogo)
					.Aspect(Aspect.AspectFit)
					.Margin(0, -AppStyles.SpacingMedium)
					.HeightRequest(AppStyles.LogoSize)
			)
			.HeightRequest(AppStyles.LogoSize)
			.Margin(0, 0, 0, 0.01)
			.IsClippedToBounds(true);

	private static VisualNode RenderItemTemplate(MauiControls.BaseShellItem item) =>
		Grid(
				"*",
				"Auto,*",
				Image().Source(item.FlyoutIcon).VCenter().HeightRequest(AppFonts.FontSizeHuge).WidthRequest(AppFonts.FontSizeHuge),
				Label(item.Title).VCenter().GridColumn(1)
			)
			.Padding(7, AppStyles.Spacing)
			.BackgroundColor(AppColors.SemiTransparent.Resolve)
			.AutomationId(item.AutomationId)
			.Set(AutomationProperties.IsInAccessibleTreeProperty, true);

	private void HandleNavigating(ShellNavigatingEventArgs args)
	{
		if (
			DeviceInfo.Current.Platform == DevicePlatform.Android
			&& args.Source == ShellNavigationSource.Pop
			&& args.Target.Location.ToString()?.Length == 0
		)
		{
			if (MauiControls.Shell.Current?.FlyoutIsPresented == true)
			{
				SetState(s => s.FlyoutIsPresented = false);
				_ = args.Cancel();
				return;
			}

			if (CurrentShellPage != ShellPage.Offers)
			{
				_ = args.Cancel();
				_ = GoToAsync(ShellPage.Offers);
			}
		}
	}

	private void HandleNavigated(ShellNavigatedEventArgs args)
	{
		if (State.FlyoutIsPresented)
		{
			SetState(s => s.FlyoutIsPresented = false);
		}

		MauiControls.Shell.Current?.ApplyShellColors();
	}

	public static ShellPage CurrentShellPage
	{
		get
		{
			var location = MauiControls.Shell.Current?.CurrentState?.Location?.OriginalString;
			return location is null
				? ShellPage.Offers
				: location switch
				{
					_ when location.Contains(R.Offers) => ShellPage.Offers,
					_ when location.Contains(R.OrderHistory) => ShellPage.OrderHistory,
					_ => ShellPage.Offers,
				};
		}
	}

	public static async Task GoToAsync(ShellPage page, bool animate = false, string parameters = "")
	{
		var route = page switch
		{
			ShellPage.Offers => R.Offers,
			ShellPage.OrderHistory => R.OrderHistory,
			_ => R.Offers,
		};
		await MauiControls.Shell.Current.GoToAsync($"//{route}{parameters}", animate);
	}

	internal static class Routes
	{
		public const string Offers = "offers";
		public const string OrderHistory = "orderhistory";
	}
}

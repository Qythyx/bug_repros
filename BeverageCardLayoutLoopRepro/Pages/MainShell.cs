using Beerbox.App.Theming;
using MauiReactor;
using AIs = Beerbox.App.Theming.AppImages.Icons;
using ARs = Beerbox.App.Resources.Localization.AppResources;
using IDs = Beerbox.App.AutomationIds.Shell;
using MauiControls = Microsoft.Maui.Controls;

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

		// Defer Shell rendering to the second pass — iOS Shell flyout crashes if
		// rendered during the initial component mount before the platform is ready.
		SetState(s => s.IsReady = true);
	}

	public override VisualNode Render()
	{
		if (!State.IsReady)
		{
			return ContentPage();
		}

		IPlatformApplication.Current?.Services.GetService<IStatusBarStyleManager>()?.SetColor(AppColors.ControlArea.Resolve);

		return Shell(
				ShellContent(ARs.OrderHistoryTitle)
					.FlyoutIcon(AIs.OrderHistory)
					.RenderContent(() => new OrderHistory())
					.Route("orderhistory")
					.ID(IDs.OrderHistory)
			)
			.FlyoutIsPresented(State.FlyoutIsPresented)
			.FlyoutWidth(AppStyles.FlyoutMenuWidth)
			.FlyoutBackgroundImage(AppImages.Background.Resolve)
			.FlyoutBackgroundImageAspect(Aspect.AspectFill)
			.FlyoutIcon(new FontImageSource { Glyph = "", Size = 0 })
			.FlyoutHeader(RenderFlyoutHeader())
			.ItemTemplate(RenderItemTemplate)
			.OnNavigated(HandleNavigated);
	}

	private static MauiReactor.Grid RenderFlyoutHeader() =>
		Grid(
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

	private void HandleNavigated(ShellNavigatedEventArgs args)
	{
		if (State.FlyoutIsPresented)
		{
			SetState(s => s.FlyoutIsPresented = false);
		}

		MauiControls.Shell.Current?.ApplyShellColors();
	}
}

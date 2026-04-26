using Beerbox.App.Components;
using Beerbox.App.Core.Services;
using Beerbox.App.Services;
using Beerbox.App.Theming;
using MauiReactor;

namespace Beerbox.App.Pages;

public abstract partial class Base<TState, TProps> : Component<TState, TProps>
	where TState : class, new()
	where TProps : class, new()
{
#pragma warning disable IDE0032
	[Inject]
	private readonly IPlatformContext _platformContext;

	[Inject]
	private readonly BeerboxTelemetryService _telemetry;

	[Inject]
	private readonly AppSettings _settings;

	[Inject]
	private readonly DataManager _dataManager;
#pragma warning restore IDE0032

	private bool _showBody;

	protected BeerboxTelemetryService Telemetry => _telemetry;
	protected AppSettings Settings => _settings;
	protected DataManager DataManager => _dataManager;
	protected IPlatformContext PlatformContext => _platformContext;

	protected abstract string PageTitle { get; }
	protected virtual bool ShowBackButton => false;

	protected virtual ShellPage? TitleBarPage => null;

	protected abstract VisualNode RenderContent();

	protected virtual void OnPageAppearing() { }

	protected virtual void OnPageMounted() { }

	protected virtual void OnPageWillUnmount() { }

	protected sealed override void OnMounted()
	{
		base.OnMounted();
		_showBody = true;
		OnPageMounted();
	}

	protected sealed override void OnWillUnmount()
	{
		OnPageWillUnmount();
		base.OnWillUnmount();
	}

	public sealed override VisualNode Render()
	{
		var content = RenderContent();
		var body = Border(content).StrokeThickness(0).BackgroundColor(AppColors.PageBackground.Resolve);
		var titleBar = RenderTitleBar();
		var connectivityBanner = new ConnectivityBanner();

		var page = _platformContext.IsAndroid
			? ContentPage(
					Grid("Auto,*", "*", connectivityBanner.GridRow(0), body.GridRow(1))
						.RowSpacing(0)
						.Set(Layout.SafeAreaEdgesProperty, AppStyles.SafeAreaEdgesTopOnly)
						.IsVisible(_showBody)
				)
				.TitleView(titleBar)
				.OnLoaded(() =>
				{
					if (!_showBody)
					{
						_showBody = true;
						Invalidate();
					}
				})
				.OnDisappearing(() => _showBody = false)
			: ContentPage(
					Grid("Auto,Auto,*", "*", titleBar.GridRow(0), connectivityBanner.GridRow(1), body.GridRow(2))
						.RowSpacing(0)
						.BackgroundColor(AppColors.ControlArea.Resolve)
						.Set(Layout.SafeAreaEdgesProperty, AppStyles.SafeAreaEdgesTopOnly)
				)
				.Set(Microsoft.Maui.Controls.Shell.NavBarIsVisibleProperty, false);

		return page.BackButtonBehavior(new() { IsVisible = false })
			.OnAppearing(() =>
			{
				_telemetry.TrackPageAppearing(GetType().Name);
				OnPageAppearing();
			});
	}

	private MauiReactor.Grid RenderTitleBar() =>
		Grid(
				"*,Auto",
				"Auto,Auto,*,Auto,Auto",
				Image()
					.ID(AutomationIds.TitleView.Back)
					.Source(AppImages.ChevronLeft)
					.VCenter()
					.IsVisible(ShowBackButton)
					.OnTapped(async () => await _platformContext.GoToAsync("..", true))
					.GridColumn(0),
				Image()
					.Source(AppImages.HamburgerMenu)
					.VCenter()
					.ID(AutomationIds.TitleView.HamburgerMenu)
					.OnTapped(() => _platformContext.SetFlyoutPresented(true))
					.GridColumn(1),
				Label(PageTitle)
					.HCenter()
					.VCenter()
					.LineBreakMode(LineBreakMode.TailTruncation)
					.GridColumn(2)
					.GridRowSpan(2)
			)
			.HeightRequest(AppStyles.ChipIconSize + AppStyles.UnderlineHeight)
			.ColumnSpacing(0)
			.RowSpacing(0)
			.Margin(
				AppStyles.Spacing / 2,
				0,
				AppStyles.Spacing / 2,
				_platformContext.IsAndroid ? 0 : AppStyles.Spacing / 2
			)
			.BackgroundColor(AppColors.ControlArea.Resolve);
}

public abstract class Base<TState> : Base<TState, EmptyProps>
	where TState : class, new();

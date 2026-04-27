using Beerbox.App.Theming;
using MauiReactor;
using MauiControls = Microsoft.Maui.Controls;

namespace Beerbox.App.Pages;

public abstract partial class Base<TState, TProps> : Component<TState, TProps>
	where TState : class, new()
	where TProps : class, new()
{
	private bool _showBody;

	protected abstract string PageTitle { get; }
	protected virtual bool ShowBackButton => false;

	protected abstract VisualNode RenderContent();

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

		var page =
			DeviceInfo.Current.Platform == DevicePlatform.Android
				? ContentPage(
						Grid("*", "*", body)
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
						Grid("Auto,*", "*", titleBar.GridRow(0), body.GridRow(1))
							.RowSpacing(0)
							.BackgroundColor(AppColors.ControlArea.Resolve)
							.Set(Layout.SafeAreaEdgesProperty, AppStyles.SafeAreaEdgesTopOnly)
					)
					.Set(MauiControls.Shell.NavBarIsVisibleProperty, false);

		return page.BackButtonBehavior(new() { IsVisible = false });
	}

	private MauiReactor.Grid RenderTitleBar() =>
		Grid(
				"*,Auto",
				"Auto,Auto,*,Auto,Auto",
				Image()
					.Source(AppImages.ChevronLeft)
					.VCenter()
					.IsVisible(ShowBackButton)
					.OnTapped(async () => await MauiControls.Shell.Current.GoToAsync("..", true))
					.GridColumn(0),
				Image()
					.Source(AppImages.HamburgerMenu)
					.VCenter()
					.OnTapped(() => MauiControls.Shell.Current.FlyoutIsPresented = true)
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
				DeviceInfo.Current.Platform == DevicePlatform.Android ? 0 : AppStyles.Spacing / 2
			)
			.BackgroundColor(AppColors.ControlArea.Resolve);
}

public abstract class Base<TState> : Base<TState, EmptyProps>
	where TState : class, new();

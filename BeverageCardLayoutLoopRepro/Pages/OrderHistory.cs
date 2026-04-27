using Beerbox.App.Resources.Localization;
using MauiReactor;
using MauiControls = Microsoft.Maui.Controls;

namespace Beerbox.App.Pages;

public sealed class OrderHistory : Base<OrderHistory.MyState>
{
	public sealed class MyState;

	protected override string PageTitle => AppResources.OrderHistoryTitle;

	protected override VisualNode RenderContent() => Button("show card").OnClicked(HandleShowCard).VCenter().HCenter();

	private static async void HandleShowCard() =>
		_ = await MauiControls.Shell.Current.Navigation.PushAsync<OrderEntry>();
}

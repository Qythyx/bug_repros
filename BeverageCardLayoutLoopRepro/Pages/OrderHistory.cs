using Beerbox.App.Resources.Localization;
using MauiReactor;
using ModelEntry = Beerbox.App.Core.Models.OrderEntry;

namespace Beerbox.App.Pages;

public sealed class OrderHistory : Base<OrderHistory.MyState>
{
	public sealed class MyState;

	protected override string PageTitle => AppResources.OrderHistoryTitle;

	protected override VisualNode RenderContent() =>
		Button("Show Grey Goose").OnClicked(HandleShowGreyGoose).VCenter().HCenter();

	private async void HandleShowGreyGoose()
	{
		var greyGoose = DataManager.Offers.First(o => o.Beverage.ID == "grey-goose");
		var entry = new ModelEntry(greyGoose, 1);
		await PlatformContext.PushAsync<OrderEntry, OrderEntry.MyProps>(props => props.Entry = entry);
	}
}

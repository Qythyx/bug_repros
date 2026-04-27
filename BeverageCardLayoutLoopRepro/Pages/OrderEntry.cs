using Beerbox.App.Components;
using Beerbox.App.Resources.Localization;
using MauiReactor;

namespace Beerbox.App.Pages;

public sealed class OrderEntry : Base<OrderEntry.MyState>
{
	public sealed class MyState;

	protected override string PageTitle => AppResources.BeverageDetailsTitle;
	protected override bool ShowBackButton => true;

	protected override VisualNode RenderContent() => new BeverageCard();
}

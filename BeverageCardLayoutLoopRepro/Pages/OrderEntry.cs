using Beerbox.App.Components;
using MauiReactor;

namespace Beerbox.App.Pages;

public sealed class OrderEntry : Base<OrderEntry.MyState>
{
	public sealed class MyState;

	protected override string PageTitle => "Details";
	protected override bool ShowBackButton => true;

	protected override VisualNode RenderContent() => new BeverageCard();
}

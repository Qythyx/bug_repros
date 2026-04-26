using Beerbox.App.Components;
using Beerbox.App.Resources.Localization;
using MauiReactor;

namespace Beerbox.App.Pages;

public sealed class OrderEntry : Base<OrderEntry.MyState, OrderEntry.MyProps>
{
	public sealed class MyState;

	public sealed class MyProps
	{
		public Core.Models.OrderEntry Entry { get; set; } = null!;
		public ShellPage? TitleBarPage { get; set; }
	}

	protected override string PageTitle => AppResources.BeverageDetailsTitle;
	protected override bool ShowBackButton => true;
	protected override ShellPage? TitleBarPage => Props.TitleBarPage;

	protected override VisualNode RenderContent() => new BeverageCard().Offer(Props.Entry.Offer);
}

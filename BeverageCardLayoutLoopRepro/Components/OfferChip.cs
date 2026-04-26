using Beerbox.App.Theming;
using MauiReactor;

namespace Beerbox.App.Components;

public sealed class OfferChip : Component<OfferChip.MyState>
{
	public sealed record MyState
	{
		public bool ShowText { get; set; }
	}

	private ImageSource? _chipImage;
	private string _chipText = "";

	public OfferChip ChipImage(ImageSource image)
	{
		_chipImage = image;
		return this;
	}

	public OfferChip ChipText(string text)
	{
		_chipText = text;
		return this;
	}

	public override VisualNode Render() =>
		Border(
				State.ShowText || _chipImage == null
					? (VisualNode)
						Border(
								Label(_chipText)
									.Margin(new Thickness(AppStyles.Spacing, 0))
									.VCenter()
									.ThemeHeader()
									.TextColor(AppColors.Primary.Resolve)
							)
							.BackgroundColor(Colors.Transparent)
							.HeightRequest(AppStyles.ChipIconSize)
					: Image()
						.Source(_chipImage)
						.HeightRequest(AppStyles.ChipIconSize)
						.WidthRequest(AppStyles.ChipIconSize)
			)
			.ThemeSemiTransparent()
			.HEnd()
			.VStart()
			.OnTapped(() => SetState(s => s.ShowText = !s.ShowText));
}

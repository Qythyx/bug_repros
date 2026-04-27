using System.Globalization;
using Beerbox.App.Theming;
using MauiReactor;

namespace Beerbox.App.Components;

public sealed partial class BeverageCard : Component
{
	public override VisualNode Render()
	{
		Console.WriteLine($"*** TRACE Render entry @ {DateTime.Now:HH:mm:ss.fff}");

		return Grid(
				"*,Auto",
				"*",
				Border()
					.GridRow(0)
					.OnLoaded(() => Console.WriteLine($"*** LOADED border @ {DateTime.Now:HH:mm:ss.fff}"))
					.OnSizeChanged(
						(sender, _) =>
						{
							if (sender is VisualElement v)
							{
								Console.WriteLine(
									$"*** SIZECHG border {v.Width:G17}x{v.Height:G17} @ {DateTime.Now:HH:mm:ss.fff}"
								);
							}
						}
					)
					.OnPropertyChanged(
						(sender, e) =>
						{
							if (e.PropertyName is "Height" or "Y" or "Width" or "X" && sender is VisualElement v)
							{
								var val = e.PropertyName switch
								{
									"Height" => v.Height,
									"Width" => v.Width,
									"X" => v.X,
									"Y" => v.Y,
									_ => double.NaN,
								};
								Console.WriteLine($"*** PROPCHG border {e.PropertyName}={val:G17}");
							}
						}
					),
				Border(Label("label").Margin(AppStyles.Spacing, AppStyles.Spacing / 2).VCenter().ThemeHeader())
					.ThemeSemiTransparent()
					.HCenter()
					.VEnd()
					.Margin(AppStyles.Spacing / 2)
					.StrokeCornerRadius(AppStyles.ChipIconSize)
					.GridRow(0)
					.InputTransparent(true),
				VStack(
						Label("line 1").ThemeBeverageName(),
						Label("line 2").ThemeMaker(),
						FlexLayout(
								Label($"foo   •   bar").LineBreakMode(LineBreakMode.TailTruncation),
								Label($"baz").LineBreakMode(LineBreakMode.NoWrap)
							)
							.Wrap(Microsoft.Maui.Layouts.FlexWrap.Wrap)
							.AlignItems(Microsoft.Maui.Layouts.FlexAlignItems.Start),
						Label("line 3"),
						Label("live 4"),
						RenderStarRating("community", 3.4)
					)
					.Margin(AppStyles.MarginLR)
					.InputTransparent(true)
					.CascadeInputTransparent(false)
					.GridRow(1)
					.OnLoaded(() => Console.WriteLine($"*** LOADED vstack @ {DateTime.Now:HH:mm:ss.fff}"))
					.OnSizeChanged(
						(sender, _) =>
						{
							if (sender is VisualElement v)
							{
								Console.WriteLine(
									$"*** SIZECHG vstack {v.Width:G17}x{v.Height:G17} @ {DateTime.Now:HH:mm:ss.fff}"
								);
							}
						}
					)
					.OnPropertyChanged(
						(sender, e) =>
						{
							if (e.PropertyName is "Height" or "Y" or "Width" or "X" && sender is VisualElement v)
							{
								var val = e.PropertyName switch
								{
									"Height" => v.Height,
									"Width" => v.Width,
									"X" => v.X,
									"Y" => v.Y,
									_ => double.NaN,
								};
								Console.WriteLine($"*** PROPCHG vstack {e.PropertyName}={val:G17}");
							}
						}
					)
			)
			.OnLoaded(() => Console.WriteLine($"*** LOADED grid @ {DateTime.Now:HH:mm:ss.fff}"))
			.OnSizeChanged(
				(sender, _) =>
				{
					if (sender is VisualElement v)
					{
						Console.WriteLine(
							$"*** SIZECHG grid {v.Width:G17}x{v.Height:G17} @ {DateTime.Now:HH:mm:ss.fff}"
						);
					}
				}
			);
	}

	private static MauiReactor.Grid RenderStarRating(string label, double rating)
	{
		var starFile = $"rating_star_{Math.Round(rating * 4) * 0.25:0.00}".Replace(".", "_") + ".png";
		const double starHeight = AppFonts.FontSizeMedium;
		const double starWidth = starHeight * 5.0;

		return Grid(
				"Auto",
				"Auto,Auto,*",
				Label(label).TextColor(AppStyles.StarRatingColor).LineBreakMode(LineBreakMode.NoWrap).GridColumn(0),
				Image(starFile).Aspect(Aspect.Fill).HeightRequest(starHeight).WidthRequest(starWidth).GridColumn(1),
				Label(rating.ToString("0.##", CultureInfo.InvariantCulture))
					.TextColor(AppStyles.StarRatingColor)
					.LineBreakMode(LineBreakMode.NoWrap)
					.GridColumn(2)
			)
			.HStart();
	}
}

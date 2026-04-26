using MauiReactor;
using Microsoft.Maui.Controls.Shapes;
using MauiControls = Microsoft.Maui.Controls;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace Beerbox.App.Components;

public sealed class FAB : Component
{
	private double _size;
	private Brush _fill = Brush.Black;
	private Color _color = Colors.White;
	private MauiReactor.Shadow _shadow = [];
	private Symbols _symbol;
	private Action? _onClicked;

	public FAB Size(double size)
	{
		_size = size;
		return this;
	}

	public FAB Fill(Brush fill)
	{
		_fill = fill;
		return this;
	}

	public FAB Color(Color color)
	{
		_color = color;
		return this;
	}

	public FAB Symbol(Symbols symbol)
	{
		_symbol = symbol;
		return this;
	}

	public FAB OnClicked(Action onClicked)
	{
		_onClicked = onClicked;
		return this;
	}

	public FAB Shadow(MauiReactor.Shadow shadow)
	{
		_shadow = shadow;
		return this;
	}

	public override VisualNode Render() =>
		Grid(
				Border()
					.StrokeThickness(0)
					.BackgroundColor(Colors.Transparent)
					.Set(MauiControls.Border.StrokeShapeProperty, new Ellipse())
					.Set(MauiControls.Border.ContentProperty, new Ellipse { Fill = _fill, StrokeThickness = 0 })
					.Shadow(_shadow),
				ContentView().Set(MauiControls.ContentView.ContentProperty, BuildSymbolPath()),
				Button().BackgroundColor(Colors.Transparent).BorderWidth(0).OnClicked(_onClicked)
			)
			.HeightRequest(_size)
			.WidthRequest(_size);

	private Path BuildSymbolPath()
	{
		var path = new Path { StrokeThickness = 0, Fill = new SolidColorBrush(_color) };

		switch (_symbol)
		{
			case Symbols.Close:
				path.Data = new PathGeometry { Figures = [GetPlus(_size, 0, 0, 0.08, 0.2)] };
				path.Rotation = 45;
				break;
			case Symbols.Minus:
				path.Data = new PathGeometry { Figures = [GetMinus(_size, 0, 0, 0.08, 0.2)] };
				break;
			case Symbols.Plus:
				path.Data = new PathGeometry { Figures = [GetPlus(_size, 0, 0, 0.08, 0.2)] };
				break;
			case Symbols.PlusMinus:
				path.Data = new PathGeometry
				{
					Figures =
					[
						GetMinus(_size / 2, 0.25 * _size, 0.45 * _size, 0.08, 0.15),
						GetPlus(_size / 2, 0.25 * _size, 0.15 * _size, 0.08, 0.15),
					],
				};
				break;
		}

		return path;
	}

	private static PathFigure GetMinus(double size, double xOffset, double yOffset, double thickness, double padding)
	{
		Point point(double x, double y) => new(xOffset + (size * x), yOffset + (size * y));
		LineSegment seg(double x, double y) => new(point(x, y));

		return new PathFigure
		{
			IsClosed = true,
			StartPoint = point(padding, 0.5 - thickness),
			Segments =
			[
				seg(1 - padding, 0.5 - thickness),
				seg(1 - padding, 0.5 + thickness),
				seg(padding, 0.5 + thickness),
			],
		};
	}

	private static PathFigure GetPlus(double size, double xOffset, double yOffset, double thickness, double padding)
	{
		Point point(double x, double y) => new(xOffset + (size * x), yOffset + (size * y));
		LineSegment seg(double x, double y) => new(point(x, y));

		return new PathFigure
		{
			IsClosed = true,
			StartPoint = point(0.5 - thickness, padding),
			Segments =
			[
				seg(0.5 + thickness, padding),
				seg(0.5 + thickness, 0.5 - thickness),
				seg(1 - padding, 0.5 - thickness),
				seg(1 - padding, 0.5 + thickness),
				seg(0.5 + thickness, 0.5 + thickness),
				seg(0.5 + thickness, 1 - padding),
				seg(0.5 - thickness, 1 - padding),
				seg(0.5 - thickness, 0.5 + thickness),
				seg(padding, 0.5 + thickness),
				seg(padding, 0.5 - thickness),
				seg(0.5 - thickness, 0.5 - thickness),
			],
		};
	}

	public enum Symbols
	{
		Close = 0,
		Minus = 1,
		Plus = 2,
		PlusMinus = 3,
	}
}

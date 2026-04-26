using MauiReactor;

namespace Beerbox.App.Components;

/// <summary>
/// Custom carousel indicator dots that support tapping to jump to any position.
/// Replaces MAUI's IndicatorView which only advances +-1 per tap on iOS (dotnet/maui#27007).
/// When there are too many dots to fit in the available width, shows a sliding window
/// with edge dots progressively smaller to hint at more items beyond.
/// </summary>
public sealed partial class CarouselIndicator : Component<CarouselIndicator.MyState>
{
	/// <summary>The percentage of the available width to show full-sized dots.
	/// </summary>
	private const double FullSizeArea = 0.5;
	private const double ReducedSizeArea = (1 - FullSizeArea) / 2;
	private const double MinDotScale = ReducedSizeArea / (ReducedSizeArea + FullSizeArea);

	public sealed class MyState
	{
		public double AvailableWidth { get; set; }
	}

	/// <summary>
	/// Total number of indicator dots to display.
	/// </summary>
	[Prop]
	private int _itemCount;

	/// <summary>
	/// Zero-based index of the currently selected dot.
	/// </summary>
	[Prop]
	private int _selectedIndex;

	/// <summary>
	/// Diameter of each indicator dot in device-independent pixels.
	/// </summary>
	[Prop]
	private double _size;

	/// <summary>
	/// Fill color of the selected dot.
	/// </summary>
	[Prop]
	private Color _selectedColor = Colors.Black;

	/// <summary>
	/// Fill color of unselected dots.
	/// </summary>
	[Prop]
	private Color _unselectedColor = Colors.Gray;

	/// <summary>
	/// Horizontal spacing between dots in device-independent pixels.
	/// </summary>
	[Prop]
	private double _dotSpacing;

	/// <summary>
	/// Called when the user taps a dot. The argument is the zero-based index of the tapped dot.
	/// </summary>
	[Prop]
	private Action<int>? _onPositionSelected;

	public override VisualNode Render()
	{
		var maxVisible = CalculateMaxVisible();
		var allFit = _itemCount <= maxVisible;

		int windowStart;
		int windowEnd;

		if (allFit)
		{
			windowStart = 0;
			windowEnd = _itemCount - 1;
		}
		else
		{
			var halfWindow = (maxVisible - 1) / 2;
			windowStart = Math.Clamp(_selectedIndex - halfWindow, 0, _itemCount - maxVisible);
			windowEnd = windowStart + maxVisible - 1;
		}

		var maxEdge = (int)Math.Ceiling(maxVisible * ReducedSizeArea);
		var leftEdge = allFit ? 0 : Math.Min(maxEdge, windowStart);
		var rightEdge = allFit ? 0 : Math.Min(maxEdge, _itemCount - 1 - windowEnd);

		return Grid(
				HStack(
						Enumerable
							.Range(windowStart, windowEnd - windowStart + 1)
							.Select(index =>
							{
								var posInWindow = index - windowStart;
								var scale = GetDotScale(posInWindow, maxVisible, maxEdge, leftEdge, rightEdge);
								return RenderDot(index, scale);
							})
					)
					.Spacing(_dotSpacing)
					.HeightRequest(_size)
					.HCenter()
					.VCenter()
			)
			.HFill()
			.OnSizeChanged(HandleSizeChanged);
	}

	private void HandleSizeChanged(object? sender, EventArgs e)
	{
		if (
			sender is Microsoft.Maui.Controls.Grid grid
			&& grid.Width > 0
			&& Math.Abs(grid.Width - State.AvailableWidth) > 1
		)
		{
			SetState(s => s.AvailableWidth = grid.Width);
		}
	}

	private int CalculateMaxVisible()
	{
		if (State.AvailableWidth <= 0 || _size <= 0)
		{
			return _itemCount;
		}

		var dotSlot = _size + _dotSpacing;
		var maxVisible = (int)Math.Floor((State.AvailableWidth + _dotSpacing) / dotSlot);
		return Math.Min(Math.Max(maxVisible, 1), _itemCount);
	}

	private static double GetDotScale(int posInWindow, int maxVisible, int maxEdge, int leftEdge, int rightEdge)
	{
		// Left edge: interpolate over maxEdge, starting from the inner portion
		// when fewer items are hidden. E.g., 1 hidden → almost full, many → smallest.
		if (posInWindow < leftEdge)
		{
			var offset = maxEdge - leftEdge;
			return MinDotScale + ((1.0 - MinDotScale) * ((double)(offset + posInWindow) / maxEdge));
		}

		// Right edge: mirror of left
		var distFromEnd = maxVisible - 1 - posInWindow;
		if (distFromEnd < rightEdge)
		{
			var offset = maxEdge - rightEdge;
			return MinDotScale + ((1.0 - MinDotScale) * ((double)(offset + distFromEnd) / maxEdge));
		}

		return 1.0;
	}

	private MauiReactor.BoxView RenderDot(int index, double scale)
	{
		var dotSize = _size * scale;
		return BoxView()
			.CornerRadius(dotSize / 2)
			.Color(index == _selectedIndex ? _selectedColor : _unselectedColor)
			.WidthRequest(dotSize)
			.HeightRequest(dotSize)
			.VCenter()
			.OnTapped(() => _onPositionSelected?.Invoke(index));
	}
}

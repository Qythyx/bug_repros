namespace Beerbox.App.Theming;

/// <summary>
/// A light/dark theme pair for a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the themed value.</typeparam>
/// <param name="Light">The value for light theme.</param>
/// <param name="Dark">The value for dark theme.</param>
internal readonly record struct ThemePair<T>(T Light, T Dark)
	where T : notnull
{
	/// <summary>
	/// Resolves a <see cref="ThemePair{T}"/> to the value matching the current app theme.
	/// Use in non-component code (services, effects, handlers) where MauiReactor's OnApply() is not available.
	/// </summary>
	public T Resolve =>
		Application.Current?.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? Dark : Light;
}

/// <summary>
/// All application color constants, organized as Light/Dark <see cref="ThemePair{T}"/> values.
/// </summary>
internal static class AppColors
{
	/// <summary>Page Background.</summary>
	internal static readonly ThemePair<Color> PageBackground = new(Colors.White, Color.FromArgb("#222222"));

	/// <summary>Primary (accent).</summary>
	internal static readonly ThemePair<Color> Primary = new(Color.FromArgb("#FFB300"), Color.FromArgb("#FFB300"));

	/// <summary>Primary Dimmed.</summary>
	internal static readonly ThemePair<Color> PrimaryDimmed = new(
		Color.FromArgb("#44FFB300"),
		Color.FromArgb("#44FFB300")
	);

	/// <summary>Body Text.</summary>
	internal static readonly ThemePair<Color> BodyText = new(Colors.Black, Colors.White);

	/// <summary>Body Text Dimmed.</summary>
	internal static readonly ThemePair<Color> BodyTextDimmed = new(
		Color.FromArgb("#44000000"),
		Color.FromArgb("#66FFFFFF")
	);

	/// <summary>Text Alert.</summary>
	internal static readonly ThemePair<Color> TextAlert = new(Color.FromArgb("#993300"), Color.FromArgb("#FF8888"));

	/// <summary>Semi-Transparent.</summary>
	internal static readonly ThemePair<Color> SemiTransparent = new(
		Color.FromArgb("#99FFFFFF"),
		Color.FromArgb("#99222222")
	);

	/// <summary>Mid.</summary>
	internal static readonly ThemePair<Color> Mid = new(Color.FromArgb("#939598"), Color.FromArgb("#939598"));

	/// <summary>FAB Text.</summary>
	internal static readonly ThemePair<Color> FABText = new(Colors.White, Colors.White);

	/// <summary>Control Area.</summary>
	internal static readonly ThemePair<Color> ControlArea = new(Color.FromArgb("#EEEEEE"), Color.FromArgb("#2B2B2B"));

	/// <summary>Control Selected.</summary>
	internal static readonly ThemePair<Color> ControlSelected = new(
		Color.FromArgb("#999999"),
		Color.FromArgb("#888888")
	);

	/// <summary>Valid Input (checkmark green).</summary>
	internal static readonly Color ValidInput = Color.FromArgb("#44DD00");

	/// <summary>Star Rating (gold).</summary>
	internal static readonly Color StarRating = Color.FromArgb("#FFC000");

	#region Brushes

	internal static readonly Brush Gradient1 = new LinearGradientBrush(
		[
			new(Color.FromArgb("#86D3EF"), 0f),
			new(Color.FromArgb("#F1EAA7"), 0.2f),
			new(Color.FromArgb("#F1EAA7"), 0.5f),
			new(Color.FromArgb("#85D2EE"), 1f),
		],
		new Point(0.5, 0),
		new Point(0.5, 1)
	);

	internal static readonly Brush Gradient2 = new LinearGradientBrush(
		[
			new(Colors.Transparent, 0f),
			new(Color.FromArgb("#AAE38BA6"), 0.5f),
			new(Color.FromArgb("#AAE38BA6"), 0.8f),
			new(Colors.Transparent, 1f),
		],
		new Point(0.2, 0.2),
		new Point(0.8, 1)
	);

	internal static readonly ThemePair<Brush> Shadow = new(
		new SolidColorBrush(Color.FromArgb("#77000000")),
		new SolidColorBrush(Color.FromArgb("#AA000000"))
	);

	#endregion Brushes
}

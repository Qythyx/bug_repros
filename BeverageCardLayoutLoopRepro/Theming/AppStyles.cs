namespace Beerbox.App.Theming;

/// <summary>
/// Application-wide spacing constants, thickness values, and size constants.
/// </summary>
internal static class AppStyles
{
	#region Spacing / Thickness

	internal const double Spacing = 10;
	internal const double SpacingMedium = 15;
	internal const double SpacingExtra = 20;

	internal static readonly Thickness Margin = new(Spacing);
	internal static readonly Thickness MarginR = new(0, 0, Spacing, 0);
	internal static readonly Thickness MarginT = new(0, Spacing, 0, 0);
	internal static readonly Thickness MarginTExtra = new(0, SpacingExtra, 0, 0);
	internal static readonly Thickness MarginLR = new(Spacing, 0, Spacing, 0);
	internal static readonly Thickness MarginTB = new(0, Spacing, 0, Spacing);

	#endregion Spacing / Thickness

	#region Sizes

	internal const double ChipIconSize = 40;
	internal const double CornerRadiusLarge = 16;
	internal const double CornerRadiusXL = 20;
	internal const double FabCloseSize = 25;
	internal const double FabSize = 54;
	internal const double FlyoutMenuWidth = 250;
	internal const double LogoSize = 200;
	internal const double OverlayOpacity = 0.8;
	internal const double QuantityButtonFontSize = 30;
	internal const double UnderlineHeight = 2;

	/// <summary>
	/// Keyboard for text input: no auto-capitalization, with spellcheck.
	/// https://github.com/xamarin/docs-archive/tree/master/Recipes/xamarin-forms/Controls/choose-keyboard-for-entry
	/// </summary>
	internal static Keyboard KeyboardText => Keyboard.Create(KeyboardFlags.CapitalizeNone | KeyboardFlags.Spellcheck);

	internal static readonly Color StarRatingColor = AppColors.StarRating;

	internal static readonly SafeAreaEdges SafeAreaEdgesTopOnly = new(
		SafeAreaRegions.None,
		SafeAreaRegions.All,
		SafeAreaRegions.None,
		SafeAreaRegions.None
	);

	#endregion Sizes

	/// <summary>
	/// Creates a themed <see cref="Shadow"/> with platform-appropriate offset and radius.
	/// </summary>
	internal static Shadow CreateShadow()
	{
		var shadow = new Shadow { Offset = new Point(3, 3), Radius = 5f };
		shadow.SetAppTheme(Shadow.BrushProperty, AppColors.Shadow.Light, AppColors.Shadow.Dark);
		return shadow;
	}
}

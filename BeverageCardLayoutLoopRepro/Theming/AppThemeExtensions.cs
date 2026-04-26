using MauiReactor;

namespace Beerbox.App.Theming;

internal static class AppThemeExtensions
{
	internal static MauiReactor.BoxView ThemeDivider(this MauiReactor.BoxView boxView) =>
		boxView.HeightRequest(1.0).Color(AppColors.Primary.Resolve).Margin(new Thickness(0, AppStyles.Spacing / 2));

	internal static MauiReactor.Border ThemeSemiTransparent(this MauiReactor.Border border) =>
		border
			.Margin(Thickness.Zero)
			.Padding(Thickness.Zero)
			.VerticalOptions(LayoutOptions.Fill)
			.BackgroundColor(AppColors.SemiTransparent.Resolve)
			.Stroke(Colors.Transparent)
			.StrokeCornerRadius(4);

	internal static MauiReactor.Label ThemeAlert(this MauiReactor.Label label) =>
		label
			.FontFamily(AppFonts.RobotoCondensed)
			.FontSize(AppFonts.FontSizeLarge)
			.TextColor(AppColors.TextAlert.Resolve)
			.VerticalTextAlignment(TextAlignment.End)
			.TextTransform(TextTransform.Uppercase)
			.VerticalOptions(LayoutOptions.Center);

	internal static MauiReactor.Label ThemeHeader(this MauiReactor.Label label) =>
		label
			.FontFamily(AppFonts.RobotoCondensedBold)
			.FontSize(AppFonts.FontSizeMedium)
			.TextColor(AppColors.BodyText.Resolve)
			.VerticalTextAlignment(TextAlignment.End)
			.TextTransform(TextTransform.Uppercase)
			.VerticalOptions(LayoutOptions.Center);

	internal static MauiReactor.Label ThemeBeverageName(this MauiReactor.Label label) =>
		label
			.FontFamily(AppFonts.RobotoBold)
			.FontSize(AppFonts.FontSizeMedium)
			.TextColor(AppColors.BodyText.Resolve)
			.VerticalTextAlignment(TextAlignment.End)
			.TextTransform(TextTransform.Uppercase)
			.VerticalOptions(LayoutOptions.Center);

	internal static MauiReactor.Label ThemeMaker(this MauiReactor.Label label) =>
		label
			.FontFamily(AppFonts.Roboto)
			.FontSize(AppFonts.FontSizeMedium)
			.TextColor(AppColors.BodyText.Resolve)
			.VerticalTextAlignment(TextAlignment.End)
			.TextTransform(TextTransform.Uppercase)
			.VerticalOptions(LayoutOptions.Center);

	internal static MauiReactor.Label ThemeTip(this MauiReactor.Label label) =>
		label
			.FontFamily(AppFonts.RobotoCondensed)
			.FontSize(AppFonts.FontSizeSmall)
			.TextColor(AppColors.BodyText.Resolve)
			.VerticalTextAlignment(TextAlignment.End)
			.TextTransform(TextTransform.Uppercase)
			.VerticalOptions(LayoutOptions.Center);

	internal static MauiReactor.ActivityIndicator ThemeHighContrast(this MauiReactor.ActivityIndicator indicator) =>
		indicator.Scale(1.0).Color(AppColors.BodyText.Resolve).Margin(AppStyles.Margin);

	internal static MauiReactor.Entry ThemeBase(this MauiReactor.Entry entry) =>
		entry
			.TextColor(AppColors.BodyText.Resolve)
			.BackgroundColor(AppColors.PageBackground.Resolve)
			.ClearButtonVisibility(ClearButtonVisibility.WhileEditing);

	internal static MauiReactor.Entry ThemeText(this MauiReactor.Entry entry) =>
		entry
			.TextColor(AppColors.BodyText.Resolve)
			.BackgroundColor(AppColors.PageBackground.Resolve)
			.ClearButtonVisibility(ClearButtonVisibility.WhileEditing)
			.Keyboard(AppStyles.KeyboardText);

	internal static MauiReactor.Entry ThemeEmail(this MauiReactor.Entry entry) =>
		entry
			.TextColor(AppColors.BodyText.Resolve)
			.BackgroundColor(AppColors.PageBackground.Resolve)
			.ClearButtonVisibility(ClearButtonVisibility.WhileEditing)
			.Keyboard(Keyboard.Email);

	internal static MauiReactor.Entry ThemeNumeric(this MauiReactor.Entry entry) =>
		entry
			.TextColor(AppColors.BodyText.Resolve)
			.BackgroundColor(AppColors.PageBackground.Resolve)
			.ClearButtonVisibility(ClearButtonVisibility.WhileEditing)
			.Keyboard(Keyboard.Numeric);

	internal static MauiReactor.Grid ThemeMargin(this MauiReactor.Grid grid) =>
		grid.HorizontalOptions(LayoutOptions.Fill)
			.ColumnSpacing(AppStyles.Spacing)
			.RowSpacing(AppStyles.Spacing)
			.Margin(AppStyles.Margin);

	internal static MauiReactor.Image ThemeBackground(this MauiReactor.Image image) =>
		image.Aspect(Aspect.AspectFill).Source(ImageSource.FromFile(AppTheme.ResolveString(AppImages.Background)));

	internal static VStack ThemeCollectionViewTemplate(this VStack vstack) =>
		vstack
			.Spacing(AppStyles.Spacing)
			.Padding(Thickness.Zero)
			.Margin(Thickness.Zero)
			.HorizontalOptions(LayoutOptions.Fill);

	/// <summary>
	/// Workaround for MAUI bug where Shell colors don't update unless set twice
	/// (first to a dummy value, then to the real value).
	/// </summary>
	/// <param name="shell">The Shell to apply the colors to.</param>
	internal static void ApplyShellColors(this Microsoft.Maui.Controls.Shell shell)
	{
		Microsoft.Maui.Controls.Shell.SetBackgroundColor(shell, Colors.White);
		Microsoft.Maui.Controls.Shell.SetBackgroundColor(shell, AppColors.ControlArea.Resolve);
		Microsoft.Maui.Controls.Shell.SetForegroundColor(shell, Colors.White);
		Microsoft.Maui.Controls.Shell.SetForegroundColor(shell, AppColors.Primary.Resolve);
	}

	/// <summary>
	/// Sets AbsoluteLayout bounds and flags to fill the parent.
	/// Shorthand for <c>.AbsoluteLayoutBounds(new Rect(0, 0, 1, 1)).AbsoluteLayoutFlags(All)</c>.
	/// </summary>
	/// <typeparam name="T">The VisualNode type.</typeparam>
	/// <param name="node">The node to configure.</param>
	internal static T AbsoluteLayoutFill<T>(this T node)
		where T : VisualNode =>
		node.AbsoluteLayoutBounds(new Rect(0, 0, 1, 1))
			.AbsoluteLayoutFlags(Microsoft.Maui.Layouts.AbsoluteLayoutFlags.All);
}

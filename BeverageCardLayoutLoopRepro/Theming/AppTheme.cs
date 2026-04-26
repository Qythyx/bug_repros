using MauiReactor;
using MauiControls = Microsoft.Maui.Controls;

namespace Beerbox.App.Theming;

public class AppTheme : Theme
{
	internal static Color Resolve(ThemePair<Color> pair) => IsDarkTheme ? pair.Dark : pair.Light;

	internal static string ResolveString(ThemePair<string> pair) => IsDarkTheme ? pair.Dark : pair.Light;

	protected override void OnApply()
	{
		#region Implicit styles (defaults)

		ButtonStyles.Default = _ =>
			_.BackgroundColor(Resolve(AppColors.Primary))
				.BorderColor(Resolve(AppColors.Primary))
				.BorderWidth(1)
				.CornerRadius(10)
				.FontFamily(AppFonts.Roboto)
				.FontSize(AppFonts.FontSizeMedium)
				.HorizontalOptions(LayoutOptions.Fill)
				.Padding(AppStyles.Margin)
				.TextColor(Resolve(AppColors.BodyText))
				.TextTransform(TextTransform.Uppercase)
				.VisualState("CommonStates", "Disabled", VisualElement.OpacityProperty, 0.5);

		CheckBoxStyles.Default = _ => _.Color(Resolve(AppColors.BodyText));

		ImageButtonStyles.Default = _ =>
			_.Opacity(1.0)
				.BorderColor(Colors.Transparent)
				.BorderWidth(0.0)
				.CornerRadius(0)
				.VisualState("CommonStates", "Disabled", VisualElement.OpacityProperty, 0.5);

		PickerStyles.Default = _ =>
			_.TextColor(Resolve(AppColors.BodyText))
				.BackgroundColor(Resolve(AppColors.PageBackground))
				.FontFamily(AppFonts.RobotoCondensed)
				.FontSize(AppFonts.FontSizeMedium);

		ProgressBarStyles.Default = _ => _.ProgressColor(Resolve(AppColors.Primary));

		RadioButtonStyles.Default = _ =>
			_.BackgroundColor(Colors.Transparent)
				.Padding(new Thickness(6, 0, 0, 0))
				.TextColor(Resolve(AppColors.BodyText))
				.TextTransform(TextTransform.Uppercase)
				.FontFamily(AppFonts.RobotoCondensed)
				.FontSize(AppFonts.FontSizeMedium);

		RefreshViewStyles.Default = _ => _.RefreshColor(Resolve(AppColors.Primary));

		SearchBarStyles.Default = _ =>
			_.TextColor(Resolve(AppColors.BodyText))
				.PlaceholderColor(Resolve(AppColors.BodyTextDimmed))
				.CancelButtonColor(Resolve(AppColors.Mid))
				.BackgroundColor(Colors.Transparent)
				.FontFamily(AppFonts.RobotoCondensed)
				.FontSize(AppFonts.FontSizeMedium);

		ShellStyles.Default = _ =>
			_.Set(MauiControls.Shell.BackgroundColorProperty, Resolve(AppColors.ControlArea))
				.Set(MauiControls.Shell.ForegroundColorProperty, Resolve(AppColors.BodyText))
				.Set(MauiControls.Shell.TitleColorProperty, Resolve(AppColors.BodyText))
				.Set(MauiControls.Shell.DisabledColorProperty, Resolve(AppColors.BodyTextDimmed))
				.Set(MauiControls.Shell.UnselectedColorProperty, Resolve(AppColors.Mid))
				.Set(MauiControls.Shell.NavBarHasShadowProperty, false)
				.Set(MauiControls.Shell.TabBarBackgroundColorProperty, Resolve(AppColors.PageBackground))
				.Set(MauiControls.Shell.TabBarForegroundColorProperty, Resolve(AppColors.Primary))
				.Set(MauiControls.Shell.TabBarTitleColorProperty, Resolve(AppColors.Primary))
				.Set(MauiControls.Shell.TabBarUnselectedColorProperty, Resolve(AppColors.Mid));

		SliderStyles.Default = _ =>
			_.MinimumTrackColor(Resolve(AppColors.Primary))
				.MaximumTrackColor(Resolve(AppColors.Mid))
				.ThumbColor(Resolve(AppColors.Primary));

		SwitchStyles.Default = _ => _.OnColor(Resolve(AppColors.PrimaryDimmed)).ThumbColor(Resolve(AppColors.Primary));

		GridStyles.Default = _ =>
			_.HorizontalOptions(LayoutOptions.Fill)
				.ColumnSpacing(AppStyles.Spacing)
				.RowSpacing(AppStyles.Spacing)
				.Margin(Thickness.Zero);

		LabelStyles.Default = _ =>
			_.FontFamily(AppFonts.RobotoCondensed)
				.FontSize(AppFonts.FontSizeMedium)
				.TextColor(Resolve(AppColors.BodyText))
				.VerticalTextAlignment(TextAlignment.End)
				.TextTransform(TextTransform.Uppercase)
				.VerticalOptions(LayoutOptions.Center);

		VerticalStackLayoutStyles.Default = _ =>
			_.Spacing(AppStyles.Spacing)
				.Padding(Thickness.Zero)
				.Margin(Thickness.Zero)
				.HorizontalOptions(LayoutOptions.Fill);

		HorizontalStackLayoutStyles.Default = _ =>
			_.Spacing(AppStyles.Spacing)
				.Padding(Thickness.Zero)
				.Margin(Thickness.Zero)
				.HorizontalOptions(LayoutOptions.Fill);

		#endregion Implicit styles (defaults)

		ActivityIndicatorStyles.Default = _ => _.Scale(2.0).Color(Resolve(AppColors.Primary)).Margin(new Thickness(30));
	}
}

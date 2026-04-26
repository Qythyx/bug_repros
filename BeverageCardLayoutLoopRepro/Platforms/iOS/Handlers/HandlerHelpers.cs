using Beerbox.App.Theming;
using CoreAnimation;
using Microsoft.Maui.Platform;

namespace Beerbox.App.Platforms.iOS.Handlers;

internal static class HandlerHelpers
{
	internal static void ApplyThemedBorder(CALayer layer)
	{
		var color = AppColors.Primary.Resolve.ToPlatform();
		layer.BorderColor = color.CGColor;
		layer.BorderWidth = 1;
		layer.CornerRadius = 4;
		layer.MasksToBounds = true;
	}
}

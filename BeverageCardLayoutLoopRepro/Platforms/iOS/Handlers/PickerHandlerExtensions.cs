using Microsoft.Maui.Handlers;

namespace Beerbox.App.Platforms.iOS.Handlers;

public static class PickerHandlerExtensions
{
	private static void MapBorderStyle(IPickerHandler handler, IPicker picker)
	{
		if (handler.PlatformView is UIKit.UITextField nativePicker)
		{
			HandlerHelpers.ApplyThemedBorder(nativePicker.Layer);
		}
	}

	public static void Register() => PickerHandler.Mapper.AppendToMapping("SetBorderStyle", MapBorderStyle);
}

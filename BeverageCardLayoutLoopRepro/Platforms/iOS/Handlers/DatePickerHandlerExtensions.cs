using Microsoft.Maui.Handlers;

namespace Beerbox.App.Platforms.iOS.Handlers;

public static class DatePickerHandlerExtensions
{
	private static void MapBorderStyle(IDatePickerHandler handler, IDatePicker datePicker)
	{
		if (
			datePicker is DatePicker mauiDatePicker
			&& mauiDatePicker.Handler?.PlatformView is UIKit.UITextField textField
		)
		{
			HandlerHelpers.ApplyThemedBorder(textField.Layer);
		}
	}

	public static void Register() => DatePickerHandler.Mapper.AppendToMapping("SetBorderStyle", MapBorderStyle);
}

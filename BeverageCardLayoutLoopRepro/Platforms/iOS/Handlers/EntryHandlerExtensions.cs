using Microsoft.Maui.Handlers;

namespace Beerbox.App.Platforms.iOS.Handlers;

public static class EntryHandlerExtensions
{
	private static void MapBorderStyle(IEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is UIKit.UITextField nativeEntry)
		{
			HandlerHelpers.ApplyThemedBorder(nativeEntry.Layer);
		}
	}

	public static void Register() => EntryHandler.Mapper.AppendToMapping("SetBorderStyle", MapBorderStyle);
}

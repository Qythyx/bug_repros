using Microsoft.Maui.Handlers;

namespace Beerbox.App.Platforms.iOS.Handlers;

public static class EditorHandlerExtensions
{
	private static void MapBorderStyle(IEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is UIKit.UITextView nativeEditor)
		{
			HandlerHelpers.ApplyThemedBorder(nativeEditor.Layer);
		}
	}

	public static void Register() => EditorHandler.Mapper.AppendToMapping("SetBorderStyle", MapBorderStyle);
}

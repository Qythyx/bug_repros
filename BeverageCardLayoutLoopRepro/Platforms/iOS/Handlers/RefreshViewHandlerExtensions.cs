using Microsoft.Maui.Handlers;

namespace Beerbox.App.Platforms.iOS.Handlers;

/// <summary>
/// Enables directional lock on the scroll view underlying a RefreshView, constraining
/// pull-to-refresh to vertical-only (prevents diagonal pulling).
/// </summary>
public static class RefreshViewHandlerExtensions
{
	private static void MapDirectionalLock(IRefreshViewHandler handler, IRefreshView refreshView)
	{
		if (handler.PlatformView is UIKit.UIView view)
		{
			EnableDirectionalLock(view);
		}
	}

	private static void EnableDirectionalLock(UIKit.UIView view)
	{
		if (view is UIKit.UIScrollView scrollView)
		{
			scrollView.DirectionalLockEnabled = true;
			return;
		}

		foreach (var subview in view.Subviews)
		{
			EnableDirectionalLock(subview);
		}
	}

	public static void Register() => RefreshViewHandler.Mapper.AppendToMapping("DirectionalLock", MapDirectionalLock);
}

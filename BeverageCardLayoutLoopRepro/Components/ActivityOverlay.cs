using Beerbox.App.Resources.Localization;
using Beerbox.App.Theming;
using CommunityToolkit.Maui.Extensions;
using MauiReactor;
using MauiControls = Microsoft.Maui.Controls;

namespace Beerbox.App.Components;

/// <summary>
/// Shows a modal activity overlay popup on the current page while an async action runs.
/// Content is built with MauiReactor and rendered into a native view via <see cref="TemplateHost"/>.
/// </summary>
internal static class ActivityOverlay
{
	private static ITemplateHost? _templateHost;

	/// <summary>
	/// Shows the activity overlay, runs <paramref name="action"/>, then hides the overlay.
	/// Returns the action's result, or <c>null</c> if the user cancelled.
	/// </summary>
	/// <param name="page">The page to show the overlay on.</param>
	/// <param name="action">The async work to perform while the overlay is shown.</param>
	/// <param name="cancellable">Whether to show a cancel button.</param>
	/// <typeparam name="T">The result type of the async action.</typeparam>
	internal static async Task<T?> ShowWhile<T>(MauiControls.Page? page, Func<Task<T>> action, bool cancellable)
		where T : class
	{
		if (_templateHost != null)
		{
			return null;
		}

		if (page?.Window == null)
		{
			return await action();
		}

		using var cts = cancellable ? new CancellationTokenSource() : null;

		var content = new VStack
		{
			new MauiReactor.ActivityIndicator().IsRunning(true).ID(AutomationIds.ActivityOverlay.Indicator),
			cts != null ? new MauiReactor.Button(AppResources.ButtonCancel).OnClicked(cts.Cancel) : null,
		}
			.Spacing(0)
			.BackgroundColor(AppColors.PageBackground.Resolve)
			.ID(AutomationIds.ActivityOverlay.Overlay);

		_templateHost = TemplateHost.Create(content);

		page.ShowPopup(
			(View)_templateHost.NativeElement!,
			new CommunityToolkit.Maui.PopupOptions
			{
				CanBeDismissedByTappingOutsideOfPopup = false,
				PageOverlayColor = AppColors.SemiTransparent.Resolve,
				Shadow = AppStyles.CreateShadow(),
				Shape = new MauiControls.Shapes.RoundRectangle
				{
					CornerRadius = new CornerRadius(AppStyles.CornerRadiusLarge),
				},
			}
		);

		T? result;
		try
		{
			var actionTask = action();
			if (cts != null)
			{
				_ = await Task.WhenAny(actionTask, Task.Delay(Timeout.Infinite, cts.Token));
				result = actionTask.IsCompleted ? await actionTask : null;
			}
			else
			{
				result = await actionTask;
			}
		}
		finally
		{
			_templateHost = null;
			_ = await page.ClosePopupAsync();
		}

		return result;
	}
}

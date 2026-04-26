using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace Beerbox.App.Platforms.iOS.Handlers;

/// <summary>
/// Registers a custom Shell renderer that disables shell item transition animations on iOS.
/// MAUI ignores the animate parameter in GoToAsync and always runs a 0.5s fade via
/// <see cref="IShellItemTransition"/>, plus iOS adds a view controller containment
/// slide-up animation. This handler suppresses both.
/// </summary>
public static class ShellHandlerExtensions
{
	public static void Register(MauiAppBuilder builder) =>
		builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Shell, CustomShellRenderer>());
}

internal sealed class CustomShellRenderer : ShellRenderer
{
	/// <inheritdoc/>
	/// <remarks>
	/// Disables all UIKit animations during the shell item switch to suppress the iOS
	/// view controller containment transition (slide-up animation) that MAUI does not
	/// expose a parameter for. Also returns a no-op <see cref="IShellItemTransition"/>
	/// to eliminate the 0.5s fade that MAUI applies on top.
	/// </remarks>
	protected override async Task OnCurrentItemChangedAsync()
	{
		UIView.AnimationsEnabled = false;
		try
		{
			await base.OnCurrentItemChangedAsync();
		}
		finally
		{
			UIView.AnimationsEnabled = true;
		}
	}

	/// <inheritdoc/>
	protected override IShellItemTransition CreateShellItemTransition() => new NoAnimationShellItemTransition();
}

/// <summary>
/// Shell item transition that swaps views instantly without the default 0.5s fade.
/// </summary>
internal sealed class NoAnimationShellItemTransition : IShellItemTransition
{
	public Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer)
	{
		var oldView = oldRenderer.ViewController.View;
		var newView = newRenderer.ViewController.View;

		oldView!.Layer.RemoveAllAnimations();
		newView!.Alpha = 1;
		oldView.Superview!.InsertSubviewAbove(newView, oldView);

		return Task.CompletedTask;
	}
}

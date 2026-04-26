using MauiReactor;

namespace Beerbox.App.Components;

// CardsView.Maui is used here as a workaround for multiple MAUI CarouselView
// bugs that made the built-in control unusable in production:
//   - dotnet/maui#21480 — programmatic Position set fires spurious
//     PositionChanged events and bounces back
//   - dotnet/maui#23023 — ItemsSource reload clobbers the current Position
//   - dotnet/maui#27007 — iOS IndicatorView dot tap only advances ±1
// Upstream reproduction tests for the first two were filed as
// https://github.com/dotnet/maui/pull/35010 and the third as
// https://github.com/dotnet/maui/pull/35015. When those fixes ship in a
// 10.0.x servicing release, revisit this file and remove CardsView.Maui in
// favor of MAUI's built-in CarouselView + IndicatorView.
#pragma warning disable CA1010 // Generic interface should also be implemented
[Scaffold("PanCardView.CardsView", implementItemTemplate: true)]
public partial class CardsView;

[Scaffold("PanCardView.CarouselView", implementItemTemplate: true)]
public partial class PanCarouselView;

/// <summary>
/// Workaround for a MauiReactor scaffold bug (https://github.com/adospace/reactorui-maui/issues/370)
/// in the headless test host. The generated
/// <c>OnUpdate</c> crashes when setting native control properties (CardsView internal
/// processing fails without MAUI handlers), preventing <c>base.OnUpdate()</c> from running.
/// This means AutomationId is never applied to the native control (it's queued in
/// <c>_propertiesToSet</c> but only applied by <c>base.OnUpdate</c>), and
/// <c>_loadedForciblyChildren</c> stays null.
/// <para/>
/// Fix: <c>OnBeginUpdate</c> pre-applies the AutomationId from the property queue so
/// <c>Find</c> can locate the control even when <c>base.OnUpdate</c> never runs.
/// <c>OnEndUpdate</c> initializes <c>_loadedForciblyChildren</c> when <c>OnUpdate</c>
/// completes normally.
/// </summary>
/// <typeparam name="T">The native MAUI control this extends, <see cref="PanCardView.CardsView"/>.</typeparam>
public partial class CardsView<T>
	where T : PanCardView.CardsView, new()
{
	partial void OnBeginUpdate()
	{
		if (
			NativeControl is Element element
			&& string.IsNullOrEmpty(element.AutomationId)
			&& _propertiesToSet?.TryGetValue(Element.AutomationIdProperty, out var id) == true
			&& id is string automationId
		)
		{
			element.AutomationId = automationId;
		}
	}

	partial void OnEndUpdate() => _loadedForciblyChildren ??= [];
}
#pragma warning restore CA1010 // Generic interface should also be implemented

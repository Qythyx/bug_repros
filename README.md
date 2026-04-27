# MAUI iOS — UriImageSource(CachingEnabled=false) + WordWrap labels triggers UIKit `layoutSubviews()` invalidation loop

**Filed upstream:** [dotnet/maui#35142](https://github.com/dotnet/maui/issues/35142).

## Summary

On iOS, when a MauiReactor `Image` uses `UriImageSource { CachingEnabled = false }`
inside a layout that also contains `LineBreakMode.WordWrap` `Label`s on a narrow
screen, the iOS image handler invalidates layout on every `layoutSubviews()` pass
while the image is loading. Each pass re-sets `Image.Height` and `Image.Y` even
when the values don't change, and MAUI's `VisualElement.UpdateBoundsComponents`
fires `SizeChanged` even when the new frame equals the old one. The combination
piles invalidations onto the UI thread faster than they can be drained, and the
app becomes permanently stuck in a `-[UIView layoutSubviews]` recursion. No taps
register, no further frames render.

Reproduces on **iPhone 16e** (375 pt screen width). The same project on
**iPhone 17 Pro** (~440 pt) does not freeze — on the wider screen the WordWrap
labels fit on fewer lines, so each layout pass is fast enough that
invalidations drain before the next one is queued.

## Root cause vs. amplifier

There are two pieces. Either one alone is harmless; both together freeze the
UI thread.

- **Root cause (the actual bug).** MAUI's iOS image handler invalidates layout
  on every `layoutSubviews()` pass while a `UriImageSource` with
  `CachingEnabled = false` is loading, not just when bytes arrive. The default
  (`CachingEnabled = true`) masks this because the cached path resolves in one
  shot. There also appears to be a missing equality check in
  `VisualElement.UpdateBoundsComponents` — it fires `SizeChanged` even when the
  frame is identical (see the trace below: `370.00000000 × 149.99999936`
  repeats unchanged for hundreds of cycles).

- **Amplifier (in our app).** A `VerticalStackLayout` whose two
  `LineBreakMode.WordWrap` children have *mixed* `HorizontalOptions` —
  `HorizontalOptions.Fill` (default) for one label and `HorizontalOptions.Start`
  for the other — is a slow layout pattern on iOS. Each pass takes long enough
  on a narrow screen that overlapping invalidations stack up. Aligning both to
  the same `HorizontalOptions` makes each pass fast enough that the
  image-handler invalidation storm doesn't pile up.

## The repro project

`BeverageCardLayoutLoopRepro/` is a stripped-down MauiReactor app whose only
reachable navigation path is:

```text
Launch → Order History (landing) → tap "Show Grey Goose"
```

That tap pushes a page that renders a single `BeverageCard`, which wraps the
problematic `Image` + WordWrap-`Label` combination. The freeze happens
immediately on that page.

## Prerequisites

- macOS host with the iOS workload installed
- .NET 10 SDK (`global.json` pins `10.0.100` — adjust if you have a different patch)
- iPhone 16e simulator booted (or any narrow-width iPhone simulator). The UUID
  shown below is mine — use your own.

## Build

```bash
dotnet build BeverageCardLayoutLoopRepro/BeverageCardLayoutLoopRepro.csproj \
  -c Debug -f net10.0-ios
```

There is no real backend in this branch; the repro renders one hardcoded
beverage card with no data layer at all — no `DataManager`, no DI registrations,
no mock JSON. Everything the card displays is a `const` literal inside
`BeverageCard.cs`.

## Deploy and run

```bash
DEVICE=$(xcrun simctl list devices | awk '/iPhone 16e.*Booted/ {gsub(/[()]/,"",$NF); print $NF; exit}')
xcrun simctl terminate "$DEVICE" jp.beercats.beerbox 2>/dev/null
xcrun simctl install "$DEVICE" \
  BeverageCardLayoutLoopRepro/bin/Debug/net10.0-ios/iossimulator-arm64/beerbox.app
xcrun simctl launch "$DEVICE" jp.beercats.beerbox
```

## Reproduction steps

1. Launch the app. It lands on **Order History** (the only Shell tab — a single button).
2. Tap **Show Grey Goose**.
3. The `BeverageCard` for Grey Goose begins rendering and the app freezes within
   ~1 second. Taps no longer register; no further frames render. The UI thread
   never becomes responsive again.

### Expected

The `BeverageCard` finishes rendering, the image loads, and the page is responsive.

### Actual

The UI thread is stuck recursing through `-[UIView layoutSubviews]` because the
image handler's per-pass layout invalidations are amplified by the two
`LineBreakMode.WordWrap` labels (the tags label with `HorizontalOptions.Fill`
and the size+price label with `HorizontalOptions.Start`) wrapping to two lines
each on a 375 pt screen.

## Trigger line

The exact code that triggers the cascade is in
[`BeverageCardLayoutLoopRepro/Components/BeverageCard.cs`](BeverageCardLayoutLoopRepro/Components/BeverageCard.cs):

```csharp
private static MauiReactor.Image RenderBeverageImage(string imageUrl) =>
    Image()
        .Source(
            new UriImageSource
            {
                Uri = new Uri(imageUrl),
                CacheValidity = TimeSpan.FromDays(28),
                CachingEnabled = false,   // ← the trigger
            }
        )
        .Aspect(Aspect.AspectFill)
        .InputTransparent(true);
```

Either of these workarounds, applied alone, makes the freeze go away on
iPhone 16e:

- Set `CachingEnabled = true` (or omit it — `true` is the default).
- Drop the `.HStart()` from the size/price label so both `WordWrap` labels
  share `HorizontalOptions.Fill`. Layout converges in one pass and the
  image-handler invalidation storm doesn't pile up. **This is the recommended
  product-side workaround** while waiting for the upstream MAUI fix — it
  doesn't require giving up cache-bypass for cases where it's needed.

## Captured per-cycle log pattern

When attached to the simulator with `xcrun simctl launch --console-pty` and
the diagnostic instrumentation referenced below, each freeze cycle produces
this exact sequence:

```text
*** PROPCHG image Height
*** SIZECHG image 390.0x556.0
*** PROPCHG image Y
*** PROPCHG image Height
*** SIZECHG vstack 370.00000000x149.99999936
```

The VStack frame `370.00000000 × 149.99999936` is **stable across iterations**
— it repeats unchanged for hundreds of cycles. This is *not* measurement
oscillation. It's `VisualElement.UpdateBoundsComponents` firing `SizeChanged`
on every `Frame` set even when the new frame equals the old one, combined with
the image handler re-setting `Image.Height` / `Image.Y` on every
`layoutSubviews()` pass.

## Captured stack at the breakpoint

Setting a breakpoint inside the `OnSizeChanged` handler captures this stack
on every cycle:

```
MauiReactor.SyncEventCommand<EventArgs>.Execute()
MauiReactor.VisualElement<VerticalStackLayout>.NativeControl_SizeChanged
Microsoft.Maui.Controls.VisualElement.UpdateBoundsComponents()
Microsoft.Maui.Controls.VisualElement.Frame.set()
Microsoft.Maui.Controls.VisualElement.ArrangeOverride()
Microsoft.Maui.Controls.VisualElement.IView.Arrange()
Microsoft.Maui.Layouts.GridLayoutManager.ArrangeChildren()
Microsoft.Maui.Platform.MauiView.LayoutSubviews()      ← UIKit -[UIView layoutSubviews]
... UIKit ...
```

The driver of the loop is in native UIKit code, so a "pause from the
toolbar" while the app is frozen typically lands at a quiet moment and shows
only `Program.Main`. A breakpoint inside `OnSizeChanged` is what captures the
recursion path — see "Reproducing the trace yourself" below.

## Ruled out (so you don't have to)

These were investigated and confirmed *not* to be the cause:

- **The Newtonsoft → STJ migration in our app.** The symptom is a layout-thread
  freeze, not a data-thread one.
- **MauiReactor 4.0.17.** Pinning back to 4.0.16 still reproduces. The plausible
  4.0.17 suspect was [PR #369 ("Fix SetState/Invalidate during layout cycle
  silently losing invalidation")](https://github.com/adospace/reactorui-maui/pull/369);
  it isn't the cause.
- **Per-render `new UriImageSource(...)` allocation.** MauiReactor's
  [`CompareUtils.AreEquals`](https://github.com/adospace/reactorui-maui/blob/main/src/MauiReactor/Internals/CompareUtils.cs)
  special-cases `UriImageSource` and compares by `Uri`, so allocating a fresh
  `UriImageSource` on every render does **not** trigger a native re-fetch.
  Caching the instance does nothing.
- **Render-side exceptions in `BeverageCard.Render()`.** Interactive debugging
  confirmed `Render()` returns successfully every cycle. (MauiReactor does
  silently swallow exceptions in `Component.Render`/`OnMounted`/`OnWillUnmount`
  — but that's not what's happening here.)

## Bottom-up minimal repros that *failed* to reproduce

Two from-scratch projects mirroring the layout 1:1 were tried before stripping
down the real app:

- `BeverageCardFreezeRepro` — XAML
- `BeverageCardFreezeReproReactor` — MauiReactor

Neither freezes on iPhone 16e. Empirically, removing any of the following from
this stripped repro also makes the freeze go away — they appear to be part of
the trigger surface:

- `Shell` + flyout
- The `Pages/Base.cs` `ContentPage` wrapper (with the safe-area edges set)
- The `Platforms/iOS/Handlers/` shims (status bar, shell, picker, refresh, etc.)

That guidance is what informed which pieces to keep when stripping the app
down to this repro.

## Reproducing the trace yourself

To produce the trace shown above, instrument the inner `VStack` and `Image`
in [`Components/BeverageCard.cs`][bc] with `.OnSizeChanged(...)` /
`.OnPropertyChanged(...)` handlers that `Debug.WriteLine` the new value, then
re-run with `xcrun simctl launch --console-pty`. The repro project itself
ships *without* the diagnostic instrumentation so the trigger surface is as
small as possible.

To capture the stack: in your `launch.json` set `"justMyCode": false` so MAUI
and MauiReactor frames aren't collapsed into `[External Code]`, set a
breakpoint inside the inline `.OnSizeChanged` handler, and let the loop hit
it. Pausing from the toolbar instead won't work — UIKit drives the loop and
the pause typically lands at a quiet moment.

[bc]: BeverageCardLayoutLoopRepro/Components/BeverageCard.cs

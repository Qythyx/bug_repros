# MAUI iOS — UriImageSource + WordWrap labels triggers UIKit `layoutSubviews()` invalidation loop

## Summary

On iOS, when a MauiReactor `Image` uses `UriImageSource { CachingEnabled = false }`
inside a layout that also contains `LineBreakMode.WordWrap` `Label`s, the iOS image
handler invalidates layout on every `layoutSubviews()` pass while the image is loading.
Once the layout becomes complex enough that each pass takes more than a frame on
narrow screens, invalidations pile up faster than they can be drained, and the UI
thread becomes permanently stuck in a `layoutSubviews()` recursion. The app freezes —
no taps register, no further frames render.

This is reproducible on **iPhone 16e** (375 pt screen width). The same project on
**iPhone 17 Pro** (~440 pt) does not freeze, because the wider screen lets the
WordWrap labels fit on fewer lines so each layout pass is fast enough that
invalidations drain in time.

The repro is a stripped-down MauiReactor app whose only reachable navigation path is:

```
Launch → Offers (landing) → tap hamburger → tap "Order History" → tap "Show Grey Goose"
```

That last tap pushes a page that renders a single `BeverageCard`, which wraps the
problematic `Image` + WordWrap-`Label` combination. The freeze happens immediately on
that page.

## Prerequisites

- macOS host with the iOS workload installed
- .NET 10 SDK (`global.json` pins `10.0.100` — adjust if you have a different patch)
- iPhone 16e simulator booted (or any narrow-width iPhone simulator). UUID shown
  below is mine — use your own.

## Build

````bash
dotnet build BeverageCardLayoutLoopRepro/BeverageCardLayoutLoopRepro.csproj \
  -c Debug -f net10.0-ios```

The repro always runs in mock mode — an in-memory data manager synthesizes the
Grey Goose offer that the OrderHistory stub navigates to. There is no real backend
in this branch.

## Deploy and run

```bash
# Pick a booted iPhone 16e simulator
DEVICE=$(xcrun simctl list devices | awk '/iPhone 16e.*Booted/ {gsub(/[()]/,"",$NF); print $NF; exit}')
xcrun simctl terminate "$DEVICE" jp.beercats.beerbox 2>/dev/null
xcrun simctl install "$DEVICE" \
  BeverageCardLayoutLoopRepro/bin/Debug/net10.0-ios/iossimulator-arm64/beerbox.app
xcrun simctl launch "$DEVICE" jp.beercats.beerbox
````

## Reproduction steps

1. App lands on **Offers** (showing Pliny the Elder).
2. Tap the hamburger menu icon (top-left of the title bar).
3. Tap **Order History** in the flyout.
4. Tap the **Show Grey Goose** button.
5. The `BeverageCard` for Grey Goose begins rendering and the app freezes within
   ~1 second. Taps do not register; no further frames render. The UI thread
   never becomes responsive again.

### Expected

The `BeverageCard` finishes rendering, the image loads, and the page is responsive.

### Actual

The UI thread is stuck recursing through `-[UIView layoutSubviews]` because the
image handler's layout invalidations on each pass are amplified by the
`LineBreakMode.WordWrap` labels (`Label(details.Name)` with `HorizontalOptions.Fill`,
plus `Label("size 510ml • price ¥800")` with `HorizontalOptions.Start`) wrapping
to two lines each on a 375 pt screen.

## Trigger line

The exact code that triggers the cascade is in
[`BeverageCardLayoutLoopRepro/Components/BeverageCard.cs`][bc] around line 119:

```csharp
private static MauiReactor.Image RenderBeverageImage(string? imageUrl) =>
    string.IsNullOrEmpty(imageUrl)
        ? Image().InputTransparent(true)
        : Image()
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

Setting `CachingEnabled = true` (or omitting it — `true` is the default) makes the
freeze go away on iPhone 16e. Removing the `LineBreakMode.WordWrap` `Label`s while
keeping `CachingEnabled = false` also avoids the freeze. Both pieces are needed.

[bc]: BeverageCardLayoutLoopRepro/Components/BeverageCard.cs

## Captured per-cycle log pattern

When attached to the simulator with `xcrun simctl launch --console-pty`, the trace
shows a continuous stream of:

```
[Image] handler: setting Source -> invalidates layout
-[UIView layoutSubviews]
  -[Label measure]   ← WordWrap path is the slow leg
  -[Image layoutSubviews]
[Image] handler: image loading completes / progresses -> invalidates layout (again)
-[UIView layoutSubviews]
...
```

…repeating until the watchdog or the user terminates the app. The stack at any
sampled moment is dominated by `-[MauiCALayerImpl layoutSublayers]` →
`-[UIView layoutSubviews]` → `-[MauiLabel measure]` → `-[NSAttributedString ...]`.

## Bottom-up minimal repros that **failed** to reproduce

I tried two from-scratch projects before stripping down the real app:

- `BeverageCardFreezeRepro` — XAML, mirrors the layout 1:1.
- `BeverageCardFreezeReproReactor` — MauiReactor, mirrors the layout 1:1.

Neither freezes on iPhone 16e. The difference is something in the surrounding
render tree of the real app that this stripped repro preserves: Shell + flyout +
`Base.cs` `ContentPage` wrapper + the safe-area / status-bar handlers in
`Platforms/iOS/Handlers/`. Trimming any of those removes the freeze; that was the
empirical guidance for what to keep in this strip.

## How to file the upstream issue

The reachable nav graph is now: `MainShell → Offers → OrderHistory → OrderEntry`,
with `OrderHistory` a single-button stub. Compare bin sizes / file counts:

- ~133 .cs files in the repro project
- One project, single-target `net10.0-ios`, only iOS Platforms folder remains
- Always-mock; no `MOCK` conditional or backend code paths

When opening the bug at `dotnet/maui`, link this branch and reference the trigger
line above.

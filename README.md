# MAUI iOS — UriImageSource(CachingEnabled=false) inside a `Grid("*,Auto", ...)` triggers a non-converging layout loop

**Filed upstream:** [dotnet/maui#35142](https://github.com/dotnet/maui/issues/35142) (issue body
predates this analysis — needs an update with the findings below).

## Summary

On iOS, when a MauiReactor `Image` uses `UriImageSource { CachingEnabled = false }` and is placed
inside a 2-row `Grid` whose first row is `*` (the image) and second row is `Auto` (containing a
sibling `VerticalStackLayout`), the iOS image handler invalidates layout on every
`layoutSubviews()` pass while the image is loading. That alone wouldn't be fatal — but combined
with ULP-level non-determinism in MAUI's measure pass, the layout settles into an **infinite
oscillation between two near-identical heights** that never converges. The UI thread becomes
permanently stuck. No taps register, no further frames render.

Reproduces on **iPhone 16e** (375 pt screen width). Does **not** reproduce on
**iPhone 17 Pro** (~440 pt). The repro path is `Launch → tap "show card" → freeze`.

## Captured per-cycle log pattern (G17 precision)

With the diagnostic instrumentation in
[`Components/BeverageCard.cs`](BeverageCardLayoutLoopRepro/Components/BeverageCard.cs) that logs
sizes at `G17` precision and captured under
`xcrun simctl launch --console-pty jp.beercats.beerbox`, each freeze cycle (~1.5 ms apart, tens of
thousands of cycles per second) produces this exact pattern:

```text
*** PROPCHG image Height=556.00000063578295
*** SIZECHG image  390x556.00000063578295
*** PROPCHG vstack Y=566.00000063578295
*** PROPCHG vstack Height=149.99999936421713
*** SIZECHG vstack 370x149.99999936421713

*** PROPCHG image Height=556.00000063578273
*** SIZECHG image  390x556.00000063578273
*** PROPCHG vstack Y=566.00000063578273
*** PROPCHG vstack Height=149.99999936421725
*** SIZECHG vstack 370x149.99999936421725
```

The two heights toggle indefinitely:

| Element | Value A                | Value B                | ΔULP    |
|---------|------------------------|------------------------|---------|
| image   | `556.00000063578295`   | `556.00000063578273`   | ~22 ULP |
| vstack  | `149.99999936421713`   | `149.99999936421725`   | ~12 ULP |
| sums    | `706.00000000000008`   | `705.99999999999998`   | ~1 ULP  |

Notice the sums: the total grid height is 750 pt; image + vstack + spacing always lands at ≈706.
The two halves trade fractional bits with each other on every measure pass, but neither side ever
settles. A single 26,033-line trace was captured during one freeze before the simulator was
killed.

## Root cause

1. iOS image handler invalidates layout on every `-[UIView layoutSubviews]` pass while a
   `UriImageSource` with `CachingEnabled = false` is loading. (With `CachingEnabled = true` —
   the default — the cached path resolves in one shot and the loop never starts.)
2. MAUI's measure pass produces ULP-level jitter in the `VerticalStackLayout`'s measured height
   (`149.99999936421713` vs `149.99999936421725` here).
3. The Grid `*` row computes the image's effective height as `total - vstackHeight - rowSpacing`.
   The vstack's ULP jitter propagates straight into the image's height
   (`556.00000063578295` vs `556.00000063578273`).
4. Setting the image's `Height` fires `PropertyChanged`. Even though the new value is only ~22
   ULP different from the old one, the equality check is bit-exact, so the change is observed.
5. `PropertyChanged` invalidates layout. UIKit schedules another `layoutSubviews()`.
6. Goto 2. Vstack now measures to the *other* of the two values (the layout subsystem appears to
   alternate). Image height flips to the matching companion value. Repeat forever.

The crucial property is that *neither side reaches a fixed point.* Each measure pass inputs
`height_n` and outputs `height_{n+1}`; the function has a 2-cycle, not a fixed point.

## What is *not* the cause

- **`LineBreakMode.WordWrap` labels.** An earlier version of this README claimed the
  `WordWrap` labels were a layout amplifier. They are not. **Removing every WordWrap modifier
  from the labels does not stop the freeze.** On iPhone 16e the label text doesn't actually wrap
  to a second line either, so wrapping was never happening to begin with.
- **Mixed `HorizontalOptions` (`.Fill` vs `.HStart()`) on sibling labels.** Same — the freeze
  reproduces with all labels at default `.Fill`.
- **Pile-up of invalidations on the UI thread.** Earlier framing was wrong. The trace shows the
  loop running at ~1.5 ms per cycle for as long as you let it. It's not that invalidations
  arrive faster than they can be drained — it's that *each pass produces a different value than
  the last*, so layout literally never converges.
- **The Newtonsoft → STJ migration in our app.** The symptom is layout-thread, not
  data-thread.
- **MauiReactor 4.0.17.** Pinning back to 4.0.16 still reproduces.
- **Per-render `new UriImageSource(...)` allocation.** MauiReactor's
  [`CompareUtils.AreEquals`](https://github.com/adospace/reactorui-maui/blob/main/src/MauiReactor/Internals/CompareUtils.cs)
  special-cases `UriImageSource` and compares by `Uri`, so allocating a fresh instance on every
  render does **not** trigger a native re-fetch. Caching the instance does nothing.
- **Render-side exceptions in `BeverageCard.Render()`.** `Render()` returns successfully every
  cycle.

## Why iPhone 17 Pro doesn't reproduce

Unknown. The wider screen produces different ratios for image and vstack height; presumably the
specific values that come out of the measure pass converge to a fixed point on the wider device
where they fall into a 2-cycle on the narrower one. The bug is fundamentally a numerical
non-determinism story, not a "narrow screen is too slow" story.

## Workaround at the call site

Switch the Grid rows from `"*,Auto"` to `"Auto,Auto"` and explicitly compute the image's
`HeightRequest` from captured grid and vstack heights, gating the update with a tolerance much
larger than ULP jitter (e.g. 0.5 px). The image height is then no longer derived from the
vstack's per-measure height — it only updates when there's a real layout change. The full
workaround as applied in the production app is in the beerbox repo's `BeverageCard.cs`; the
short version is:

```csharp
var imageHeightRequest =
    State.GridHeight is double gh && State.VStackHeight is double vh && gh > vh + AppStyles.Spacing
        ? gh - vh - AppStyles.Spacing
        : -1;

return Grid("Auto,Auto", "*",
    Image()...
        .HeightRequest(imageHeightRequest)
        .GridRow(0),
    VStack(...)
        .OnSizeChanged((s, _) => {
            if (s is VisualElement v && Math.Abs((State.VStackHeight ?? -1) - v.Height) > 0.5)
                SetState(st => st.VStackHeight = v.Height);
        })
        .GridRow(1)
);
```

A separate, simpler workaround if your app can tolerate it: set `CachingEnabled = true` (the
default). The cached image-load path doesn't trigger the per-pass layout invalidation, so the
2-cycle never starts.

## The repro project

`BeverageCardLayoutLoopRepro/` is a stripped-down MauiReactor app whose only reachable navigation
path is `Launch → Order History → tap "show card"`. That tap pushes a page that renders a single
`BeverageCard` with all data (name, image URL, etc.) hardcoded as `const` literals. There is no
data layer, no DI, no mock JSON — just the page and the card. The card includes
`Console.WriteLine`-based instrumentation on grid/image/vstack so the per-cycle pattern is
visible in `xcrun simctl launch --console-pty` output.

### Prerequisites

- macOS host with the iOS workload installed
- .NET 10 SDK (`global.json` pins `10.0.100` — adjust if you have a different patch)
- iPhone 16e simulator booted (or any narrow-width iPhone simulator). The UUID shown below is
  mine — use your own.

### Build

```bash
dotnet build BeverageCardLayoutLoopRepro/BeverageCardLayoutLoopRepro.csproj \
  -c Debug -f net10.0-ios
```

### Deploy and run

```bash
DEVICE=$(xcrun simctl list devices | awk '/iPhone 16e.*Booted/ {gsub(/[()]/,"",$NF); print $NF; exit}')
xcrun simctl terminate "$DEVICE" jp.beercats.beerbox 2>/dev/null
xcrun simctl install "$DEVICE" \
  BeverageCardLayoutLoopRepro/bin/Debug/net10.0-ios/iossimulator-arm64/beerbox.app
xcrun simctl launch --console-pty "$DEVICE" jp.beercats.beerbox
```

Pipe to a file (`>/tmp/repro-trace.log 2>&1 &`) if you want to inspect the trace at leisure;
the freeze produces ~26,000 lines per second.

### Reproduction steps

1. Launch the app. It lands on **Order History** (one Shell tab — a single button).
2. Tap **show card**.
3. The `BeverageCard` for Grey Goose begins rendering and the app freezes within ~1 second. Taps
   no longer register; no further frames render. The UI thread never becomes responsive again.
4. The console-pty stream shows the alternating-values pattern above, repeating until the
   simulator is killed.

## Trigger line

The `UriImageSource` in
[`BeverageCardLayoutLoopRepro/Components/BeverageCard.cs`](BeverageCardLayoutLoopRepro/Components/BeverageCard.cs):

```csharp
private static MauiReactor.Image RenderBeverageImage(string imageUrl) =>
    Image()
        .Source(
            new UriImageSource
            {
                Uri = new Uri(imageUrl),
                CacheValidity = TimeSpan.FromDays(28),
                CachingEnabled = false,   // ← required for the freeze
            }
        )
        .Aspect(Aspect.AspectFill)
        .InputTransparent(true);
```

…combined with the parent `Grid("*,Auto", "*", ...)` containing the image (in the `*` row) and
the `VStack` (in the `Auto` row).

## Bottom-up minimal repros that *failed* to reproduce

Two from-scratch projects mirroring the layout 1:1 were tried before stripping down the real
app:

- `BeverageCardFreezeRepro` — XAML
- `BeverageCardFreezeReproReactor` — MauiReactor

Neither freezes on iPhone 16e. Empirically, removing the `Shell` + flyout or the `Base.cs`
`ContentPage` wrapper from this stripped repro also makes the freeze go away — they appear to be
part of the trigger surface in some way that bottom-up minimal projects don't reproduce. (Why is
unclear; the surrounding container presumably affects the exact layout dimensions enough to push
the measure pass into the 2-cycle.) Earlier passes also kept the `Platforms/iOS/Handlers/`
shims for the same reason; a later pass deleted them and the freeze still reproduces, so they're
not actually load-bearing.

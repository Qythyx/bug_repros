# MAUI iOS — `Grid("*,Auto")` over a `VerticalStackLayout` enters a non-converging measurement 2-cycle on iPhone 16e

**Filed upstream:** [dotnet/maui#35142](https://github.com/dotnet/maui/issues/35142) (issue body
predates this analysis — needs an update with the findings below).

## Summary

On iPhone 16e, a `Grid("*,Auto", "*", ...)` whose `*` row contains any view with a height-property
notification and whose `Auto` row contains a `VerticalStackLayout` of a few labels enters an
**infinite oscillation between two ULP-different heights** (ULP = unit in the last place — the
bit-level gap between consecutive `double`s) that never converges. The vstack's measure pass is
non-deterministic at the ULP level; the Grid's `*` row recomputes its content's height as
`gridHeight - vstackHeight - rowSpacing`, so the vstack's jitter feeds straight into the `*` row's
content height; the content's `Height` PropertyChanged fires (because the bit-level new value
differs from the old); that invalidates layout; UIKit schedules another `layoutSubviews()`; vstack
measures to the _other_ of two near-identical values; goto top. The UI thread becomes permanently
stuck. No taps register, no further frames render.

Reproduces on **iPhone 16e** simulator (375 pt screen width), but does **not** reproduce on **iPhone
17 Pro** simulator (~440 pt). The repro path is `Launch → tap "show card" → freeze`.

Also, if you adjust the actual rendered content, like removing (or commenting out) any of the VStack
labels, then the problem does not repro. This, along with the simulator difference mentioned above,
strongly suggest the problem is a very subtle measuring issue that requires very specific
conditions.

## Captured per-cycle log pattern (G17 precision)

With the diagnostic instrumentation in
[`Components/BeverageCard.cs`](BeverageCardLayoutLoopRepro/Components/BeverageCard.cs) that logs
sizes at `G17` precision and captured under `xcrun simctl launch --console-pty`, each freeze cycle
produces this exact pattern:

```text
*** PROPCHG border Height=556.00000063578295
*** SIZECHG border 390x556.00000063578295
*** PROPCHG vstack Y=566.00000063578295
*** PROPCHG vstack Height=149.99999936421713
*** SIZECHG vstack 370x149.99999936421713

*** PROPCHG border Height=556.00000063578273
*** SIZECHG border 390x556.00000063578273
*** PROPCHG vstack Y=566.00000063578273
*** PROPCHG vstack Height=149.99999936421725
*** SIZECHG vstack 370x149.99999936421725
```

The two heights toggle indefinitely:

| Element | Value A              | Value B              | ΔULP    |
| ------- | -------------------- | -------------------- | ------- |
| border  | `556.00000063578295` | `556.00000063578273` | ~22 ULP |
| vstack  | `149.99999936421713` | `149.99999936421725` | ~12 ULP |
| sums    | `706.00000000000008` | `705.99999999999998` | ~1 ULP  |

Notice the sums: the total grid height is 750 pt; border + vstack + spacing always lands at ≈706.
The two halves trade fractional bits with each other on every measure pass, but neither side ever
settles.

## Root cause

1. The `VerticalStackLayout`'s measure pass produces ULP-level non-determinism in its measured
   height — `149.99999936421713` vs `149.99999936421725` (and occasionally `…736`) here.
2. The Grid `*` row computes the content's effective height as
   `gridHeight - vstackHeight - rowSpacing`. The vstack's ULP jitter propagates straight into the
   `*` row's content height (`556.00000063578295` vs `556.00000063578273`).
3. Setting that view's `Height` fires `PropertyChanged`. The new value is only ~22 ULP different
   from the old one, but the equality check is bit-exact, so the change is observed.
4. `PropertyChanged` invalidates layout. UIKit schedules another `layoutSubviews()`.
5. Goto 1. Vstack now measures to the _other_ of the two values (the layout subsystem alternates).
   The `*` row content's height flips to the matching companion value. Repeat forever.

The crucial property: **neither side ever reaches a fixed point.** Each measure pass takes
`height_n` as input and produces `height_{n+1}`; the function has a 2-cycle, not a fixed point.

## What is _not_ the cause

- **MauiReactor.** The trace fires from native UIKit `layoutSubviews()` → MAUI Grid /
  VerticalStackLayout managers; MauiReactor is the rendering frontend that exposes the bug, not the
  bug itself. Having said that, I have not tried reproducing this is pure MAUI without MauiReactor.

## Why iPhone 17 Pro doesn't reproduce

The problem requires very specific layout to repro, and the different device size means the layout
happens differently and therefore does not repro. Other devices may also repro, but I haven't tested
them exahoustively.

## Workaround

I have not yet found a reliable workaround. I'm playing with multi-render phases, starting with
`"Auto,Auto"` in the Grid and then manually setting the height of the first row after that, but this
isn't quite working reliably yet.

## The repro project

`BeverageCardLayoutLoopRepro/` is a stripped-down MauiReactor app whose only reachable navigation
path is `Launch → Order History → tap "show card"`. That tap pushes a page that renders the smallest
reproducer found so far — a `Grid("*,Auto", "*", ...)` containing an empty `Border()` in the `*` row
and a `VStack` of a few labels in the `Auto` row. There is no data layer, no DI, no images loaded
over the network. The card includes `Console.WriteLine`-based instrumentation on grid/border/vstack
so the per-cycle pattern is visible in `xcrun simctl launch --console-pty` output.

### Prerequisites

- macOS host with the iOS workload installed
- .NET 10 SDK (`global.json` pins `10.0.100` — adjust if you have a different patch)
- iPhone 16e simulator booted (or any narrow-width iPhone simulator). The UUID shown below is mine —
  use your own.

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

Pipe to a file (`>/tmp/repro-trace.log 2>&1 &`) if you want to inspect the trace at leisure; note
that when it freezes it produces many lines per second.

### Reproduction steps

1. Launch the app. It lands on **Order History** (one Shell tab — a single button).
2. Tap **show card**.
3. The page begins rendering and the app freezes within ~1 second. Taps no longer register; no
   further frames render. The UI thread never becomes responsive again.
4. The console-pty stream shows the alternating-values pattern above, repeating until the simulator
   is killed.

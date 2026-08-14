# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this
repository.

## What this is

Helios is a C# API that unifies Thermo Fisher's IAPI across Exploris and Tribrid ("Fusion")
instrument platforms into one interface, so applications can connect to either instrument family
from a single code base. It ships two ways to consume it:

- **In-process (net48)**: link `Helios.dll` directly (`Helios/Helios.csproj`) and call
  `Helios.Interfaces.InstrumentAccessContainerFactory.Create()`. Helios talks to IAPI in the same
  process. `ScanInjector` uses this path.
- **Over a gRPC bridge (net8)**: IAPI only runs on Framework 4.8, so a .NET 8 app can't link
  `Helios.dll` directly. `Bridge/Helios.Bridge.Host` (net48) wraps IAPI the same way the in-process
  path does and exposes it over gRPC; `Bridge/Helios.Client` (net8) is the client-side library,
  whose public API mirrors Helios's own interfaces so Core 8 application code reads like Helios
  application code. `ScanSpy` uses this path.

`HISTORY.md` is an append-only log of completed work on this repo (newest first) — add an entry
there when you finish, fix, or verify a component; don't rewrite past entries.

## Commands

Build everything (from `Helios/`, where the solution lives):
```
dotnet build Helios.sln -c Release -p:Platform=x64
```
`ScanInjector` builds clean, but needs a real `nuget.exe` restore on a fresh clone (see "A
fresh-clone gotcha" below) — `dotnet build`/`dotnet restore` don't drive packages.config-style
restore at all (unlike PackageReference, which they handle natively), so there's no way around
fetching the classic CLI for this one.

Manually starting the host is no longer required for local dev/testing — `HeliosClient.ConnectAsync`
auto-launches it if nothing is listening on `127.0.0.1:50100` yet (see "Auto-launch and
self-managed lifetime" below), so `dotnet run --project Bridge/Helios.Client.Demo -c Release` (or
running `ScanSpy`) on its own is enough. Starting it manually still works and is useful when you
want to watch its own console output directly:
```
Bridge/Helios.Bridge.Host/bin/x64/Release/net48/Helios.Bridge.Host.exe
dotnet run --project Bridge/Helios.Client.Demo -c Release
```
`Helios.Bridge.Host`'s `App.config` (`InstrumentFamily` key) controls which backend it uses:
- `Auto` (default) — probe Fusion, then Exploris, then Corona (VMS); fall back to `Simulated` only
  if none answered.
- `Real` — same probe, but throws instead of falling back if nothing answered.
- `Simulated` — always use the built-in synthetic generator (`SimulatedScanIntervalMs` controls
  its rate; `1` = 1000 scans/sec, used for pipeline stress-testing).

**Before rebuilding `Helios.Bridge.Host`, stop any running instance of it first** — MSBuild can't
overwrite a locked `.exe`, and a background/orphaned process from a previous run is a common cause
of `MSB3021`/`MSB3027` ("being used by another process") during a rebuild.

### A fresh-clone gotcha: `packages.config`-style projects (`Nova`, `ScanInjector`)

`Helios.csproj` (for `Nova`) and `ScanInjector.csproj` (for ScottPlot/SkiaSharp/HarfBuzzSharp/...)
both reference dependencies via `packages.config`, not `PackageReference` — `dotnet build`/`dotnet
restore` only drive PackageReference-style restore, so packages.config projects come up with
nothing in `Helios/packages/` on a fresh clone and fail with "missing NuGet package(s)"-style
errors. Two ways to fix it, depending on the package:

- **`Nova`** has no public feed; extract the matching `Nova.<version>.nupkg` (a local feed lives at
  `D:\Software\SchweppeLab\NuGet` on this machine) into `Helios/packages/Nova.<version>/`, matching
  the layout restore would produce (`lib/net48/Nova.dll` under that folder).
- **`ScanInjector`'s packages** are all public. Fetch the real NuGet CLI
  (`https://dist.nuget.org/win-x86-commandline/latest/nuget.exe`) and run
  `nuget.exe restore ScanInjector\packages.config -PackagesDirectory Helios\packages` from the repo
  root — this is the one restore mechanism actually designed for packages.config and reproduces the
  exact folder layout the project's `HintPath`s expect. Don't try to fix a packages.config project by
  converting it to `PackageReference` instead: attempted once for `ScanInjector` and abandoned —
  legacy (non-SDK-style, `TargetFrameworkVersion`-based) projects don't reliably get automatic
  compile-time wiring for PackageReference-resolved transitive dependencies or package-provided
  build targets the way SDK-style projects do (concretely, `System.Resources.Extensions`'s own
  auto-wiring `.targets` file no-ops because it's gated on `$(TargetFramework)`, an SDK-style-only
  property this project never sets) — converting would mean either hand-wiring every transitive
  `HintPath` via `$(NuGetPackageRoot)` (fragile, breaks again whenever the dependency graph shifts)
  or a full SDK-style project-format rewrite (a much bigger, riskier change than "fix the build").

Separately, on any SDK version new enough (seen on the .NET 9 SDK's bundled MSBuild), a project
with non-string (image/icon) `.resx` resources needs
`<GenerateResourceUsePreserializedResources>true</GenerateResourceUsePreserializedResources>` plus
a `System.Resources.Extensions` reference or it fails with `MSB3822`/`MSB3823` — unrelated to NuGet
restore entirely, a toolchain-version quirk that would hit even a from-scratch legacy WinForms
project with no packages.config involved at all. `ScanInjector.csproj` hits this; its
`System.Resources.Extensions` reference is HintPath'd like everything else in that file (restored
the same `nuget.exe restore` way above) rather than added as a `PackageReference`, for the same
auto-wiring reason.

## Architecture

### The net48 core (`Helios/`)

`Helios/Interfaces/` is not just interface definitions — most files pair an IAPI-mirroring
interface (`IInstrumentAccess`, `IControl`, `IScans`, `IMsScan`, ...) with concrete
`internal class Helios<Thing><Family>` implementations for each instrument family: `Exploris`,
`Fusion` (Tribrid), and `VMS` (Corona, over named pipes — see below).
`Helios.Interfaces.InstrumentAccessContainerFactory.Create()` (`Interfaces/Interfaces.cs`) tries
Fusion, then Exploris, then VMS, and returns whichever connects, or `null` if none did.

Fusion and Exploris IAPI assemblies redeclare the same `Thermo.Interfaces.InstrumentAccess_V1.*`
namespace as physically distinct types, so both are referenced with `<Aliases>` in
`Helios.csproj` (`fusion`, `exploris`) and pulled in with `extern alias fusion;`/`extern alias
exploris;` at the top of files that need them.

IAPI DLL references use relative `HintPath`s (`..\..\iapi\lib\...`, pointing at the sibling `iapi`
repo) rather than being vendored.

`Helios.csproj` targets `net48`, `x64` only for Release (the configuration that matters — see
memory: Debug is unmaintained for sibling repos in this workspace).

### `Nova.IPC.Pipes` / the `VMS` (Corona) backend

`HeliosInstrumentAccessContainerVMS`
(`Helios/Interfaces/InstrumentAccess/IInstrumentAccessContainer.cs`) is a named-pipes client
(`Nova.IPC.Pipes.PipesClient`) that connects to an external process named "Corona" and receives
scan/acquisition events over pipes. It's the only backend that isn't backed by real IAPI hardware
communication, and it's Corona (a separate consumer application, not part of this repo) that drives
it — Helios's own design isn't shaped around Corona specifically.

**`HeliosMsScanVMS` (`Interfaces/InstrumentAccess/MsScanContainer/IMsScan.cs`) has several
never-assigned fields**, found and worked around while getting real Corona scan data flowing
through the bridge (see `HISTORY.md`'s 2026-08-13 entries for the live-debugging story): both
constructors (`Spectrum` and `SpectrumEx` overloads) never set `StatusLog`, `TuneData`, or
`DetectorName`, so they're `null` for every VMS scan — unlike Exploris/Fusion, which always wrap a
real (if possibly empty) source. Additionally, the generic `Centroid` class
(`Interfaces/SpectrumFormat/ICentroid.cs`), used by both Fusion and VMS, stubs
`IsExceptional`/`IsReferenced`/`IsMerged`/`IsFragmented`/`Profile`/`ChargeEnvelopeIndex`/
`IsClusterTop` with `throw new NotImplementedException()` — only `Mz`/`Intensity`/`Charge`/
`Resolution` are real. None of this has been fixed in `Helios.dll` itself (per the bug-fixing
policy below); `Bridge/Helios.Bridge.Host/Instruments/HeliosInstrumentGateway.cs` works around all
of it on the consuming side (null-checks, a fallback detector name, and a try/catch defaulting the
four stubbed centroid flags to `false`). Any other code that reads `IMsScan` fields from a VMS
scan should expect the same gaps.

### The bridge (`Bridge/`)

Five projects, three runtime boundaries:

- **`Helios.Bridge.Contracts`** (netstandard2.0) — the `.proto` contract, referenced by both the
  host and the client. Wire format highlights: columnar (structure-of-arrays) `CentroidBlock` for
  scan centroid data (`repeated double mz`, not `repeated Centroid`) — the single biggest lever on
  wire/alloc cost on the hottest, highest-frequency path in the system; preserve this shape through
  any change that touches scan data.
- **`Helios.Bridge.Host`** (net48 console) — the gRPC server.
  - `Instruments/HeliosInstrumentGateway.cs` wraps `Helios.dll`'s own public interfaces
    (`IInstrumentAccessContainer`/`IInstrumentAccess`/`IControl`/`IAcquisition`/`IScans`/...)
    rather than re-implementing IAPI access — one adapter suffices for both Fusion and Exploris
    since Helios's interfaces already unify them (single `SystemMode`/`InstrumentState` enums, not
    one per family). It takes an already-probed `IInstrumentAccessContainer` (see `Program.cs`'s
    `CreateGateway`) rather than probing itself, so the host can fall back to
    `Instruments/Simulated/SimulatedInstrumentGateway.cs` (a self-contained synthetic generator,
    no hardware/license/Helios.dll dependency at all) when nothing real answered.
  - Every Helios/IAPI event subscription in `HeliosInstrumentGateway.cs` goes through
    `CallbackGuard.Run(name, action)` (try/catch + `Console.Error`) rather than invoking the
    forwarded event directly. This isn't defensive boilerplate: for VMS specifically, an unhandled
    exception in *any* mapping callback kills Corona's entire pipe dispatch thread, not just that
    one event — confirmed live (see `HISTORY.md`) when a `StatusLog` null reference silently ended
    an entire session's worth of scan/acquisition events after the very first scan. `CallbackGuard`
    turns a bug anywhere in this file into a logged, recoverable no-op instead of a dead connection.
  - `Services/*ServiceImpl.cs` are the five gRPC service implementations. Every server-streaming
    RPC follows the same shape: subscribe a handler that writes into an unbounded
    `System.Threading.Channels.Channel<T>`, then drain that channel into the gRPC
    `IServerStreamWriter<T>` (`Services/GrpcStreaming.cs`'s `PumpAsync` for single-event-source
    RPCs; hand-inlined for RPCs that fan in multiple events onto one stream). Keep any new
    subscribe/unsubscribe state **local to the call**, never a field on a `*ServiceImpl` class —
    those are constructed once and shared across concurrent client calls.
- **`Helios.Client`** (net8 library) — the .NET 8-facing API. Mirrors Helios's interface names and
  read/event surface (`IInstrumentAccess`, `IControl`, `IAcquisition`, `IScans`, `IMsScan`, ...),
  but isn't a byte-for-byte reproduction — three deliberate, documented divergences (see the doc
  comment at the top of `Interfaces.cs`): synchronous properties/events stay synchronous, backed by
  a cache kept current by a background gRPC stream; genuine round-trip calls that shouldn't block a
  caller are `Task`-returning instead of exactly-synchronous; `IMsScan.Centroids` stays the
  columnar `CentroidBlock`, not `IEnumerable<ICentroid>`. Neither this project's public surface nor
  the host's `IInstrumentGateway` ever expose a generated proto type — `Mapping.cs` (client-side)
  and each `Services/*ServiceImpl.cs` (host-side) are the only places proto types appear.
- **`Helios.Client.Demo`** (net8 console) — end-to-end showcase: connects, streams scans with a
  rolling latency report, submits a demo custom scan, toggles acquisition on/off.

Loopback-only, no TLS, by design (`ServerCredentials.Insecure` host-side;
`AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true)`
client-side, set once in `HeliosClient`'s static constructor — required for the channel to connect
at all, easy to forget when extending this pattern elsewhere).

### Auto-launch and self-managed lifetime

`HeliosClient.ConnectAsync` no longer requires the caller to have started `Helios.Bridge.Host`
manually. Real deployments have no single build-time-known install path for it — a NuGet-only
consumer app never built this repo, and `Helios.Bridge.Host` itself is expected to arrive via an
installer or a zip file the user unpacks wherever they like — so discovery and lifetime both have
to be resolved at runtime rather than assumed:

- **Discovery** (`Helios.Client/BridgeHostLocator.cs`), first match wins: an explicit
  `hostExecutablePath` argument to `ConnectAsync` → the `HELIOS_BRIDGE_HOST_PATH` environment
  variable → `HKCU\Software\SchweppeLab\Helios\BridgeHostPath` in the registry → `Helios.Bridge.Host.exe`
  found on `PATH`. Only consulted if nothing is already listening on `127.0.0.1:50100` — if another
  app already has a host running (and possibly connected to real hardware), `ConnectAsync` reuses
  it rather than starting a redundant instance.
- **Self-registration** (`Helios.Bridge.Host/Program.cs`'s `RegisterInstallLocation`): the host
  writes its own `MainModule.FileName` to that same registry key on every successful startup — not
  just once, so a reinstalled/moved host keeps the key accurate without a separate
  uninstall/unregister step. For zip-file installs with no installer to do this, run
  `Helios.Bridge.Host.exe --register` once to write the key and exit without starting the server.
  `BridgeHostLocator` self-heals a stale entry (host deleted without re-registering) by deleting it
  the first time it notices the target file no longer exists, rather than leaving it to keep
  misfiring.
- **Single-instance detection**: `Program.cs` calls `server.Ports.Add(...)` directly (not the
  `Ports = { ... }` collection-initializer shorthand) specifically to check its `int` return value —
  Grpc.Core doesn't throw for "address already in use" the way a raw socket bind would, so `0` is
  the only signal a second `Helios.Bridge.Host` has that another instance already owns the port. If
  two `ConnectAsync` callers race and both decide to launch, both host processes start, but only one
  wins the bind; the loser exits immediately, and both callers' listening-poll loop converges on
  whichever one won. No separate single-instance mutex is needed.
- **Idle auto-shutdown** (`Helios.Bridge.Host/Services/ConnectionWatchdog.cs`): every
  `Helios.Client` connection keeps exactly one `StreamServiceEvents` call open for its whole
  lifetime (opened right after `Connect`, in `HeliosClient.PumpServiceEventsAsync`), which makes
  that call's start/end a reliable, crash-safe proxy for "a client is connected" — when a client
  process dies, its socket closes immediately (even without a graceful `DisposeAsync`), which ends
  the call the same way a clean disconnect would. `InstrumentServiceImpl.StreamServiceEvents` calls
  `ConnectionWatchdog.ConnectionOpened`/`ConnectionClosed` around its body; the watchdog only arms
  its idle timer on a transition from `>0` connections back to `0` (never on startup with zero
  clients, since `ConnectionClosed` can't fire without a prior `ConnectionOpened`), and cancels the
  timer if a new connection arrives before it elapses. Configurable via `App.config`'s
  `IdleShutdownSeconds` (default 5s; `<= 0` disables auto-shutdown entirely, Ctrl+C-only as before
  this existed).

An auto-launched host runs with no console window: `HeliosClient.LaunchHost` runs it via
`cmd.exe /c "... > logfile 2>&1"` rather than launching it directly. This is deliberate, not just
cosmetic — a naive `ProcessStartInfo.RedirectStandardOutput` would need *this* client to keep
draining the pipe for as long as the host runs, but the host is designed to outlive whichever
client happened to launch it (auto-shutdown is driven by total connection count, not by the
launching caller specifically); once that client exited and stopped reading, the host's own
`Console.WriteLine` calls would eventually block on a full pipe buffer and hang. `cmd`'s own file
redirection hands the host a plain file handle instead, so no reader is required, and no console is
ever allocated for the host process either. Output lands in
`%LocalAppData%\SchweppeLab\Helios\Helios.Bridge.Host.<port>.log`, overwritten on each launch. A
manually-started host is unaffected and keeps its normal visible console.

Live-verified end to end (single client: auto-launch → connect → force-kill the client to simulate
a crash → watchdog detects the dropped stream → idle timer fires → host exits cleanly; two clients:
second client reuses the first's already-running host rather than launching a duplicate, host
survives one client disconnecting while the other stays connected, and only shuts down once both
are gone; hidden-launch path re-verified after adding the cmd.exe wrapper: host ran with no visible
window, its startup/status lines appeared in the per-port log file, and auto-shutdown still fired
normally afterward).

### `ScanSpy` (net8.0-windows, ported from the original net48 in-process app)

Retargeted in place to `net8.0-windows` + `Helios.Client` (was net48 + `Helios.dll` directly).
Notable adaptations, useful context if touching it again:
- The old two-step connect (`InstrumentAccessContainerFactory.Create()` →
  `ServiceConnectionChanged` → `Get(1)`) collapsed into one `await HeliosClient.ConnectAsync()`,
  which doesn't return until the instrument is actually reachable.
- `IMsScan.TryHeader`/`TryTrailer` (in-process only) became plain `Header`/`Trailer` dictionary
  lookups — safe because `HeliosMsScanChannelAdapter` (host-side) already resolves Helios's
  canonical ("universal dictionary") header/trailer IDs into those dictionaries regardless of which
  instrument family answered (see `CanonicalHeaderIds`/`CanonicalTrailerIds` in
  `HeliosInstrumentGateway.cs` — reproduced by hand from `HeliosDictionary`'s static constructor
  since that class is `internal` to `Helios.dll`).
- Event handlers now marshal onto the UI thread via `Invoke`/`BeginInvoke` where they touch
  controls. `Helios.Client`'s events fire from a background `Task` draining a gRPC stream, same as
  the in-process events did from an IAPI callback thread — but the original had some unguarded
  cross-thread UI writes that happened to not matter until now.

## Bug-fixing policy for this repo

Helios is stable and in active use today. If something looks wrong in existing Helios/IAPI code —
**report it and wait for explicit go-ahead before fixing it**, unless the fix is scoped strictly to
code being newly written in the current effort (e.g. working around a Helios.dll gap on the
*consuming* side, as `HeliosInstrumentGateway.cs` does for the VMS gaps above, is in scope; editing
`Helios.dll`'s `HeliosMsScanVMS`/`Centroid` classes themselves is not, without asking first). Don't
opportunistically "fix while passing by" in code outside that scope.

## Conventions specific to the bridge code (`Bridge/`, `ScanSpy`)

- Columnar (structure-of-arrays) wire format for scan centroid data — preserve this shape through
  any change that touches scan data.
- No LINQ on the scan-streaming hot path (mapping layers, request/response translation) —
  explicit loops only, to keep that path allocation-light and predictable.
- Guard every `ConnectAsync`-equivalent against being called twice — connection is client-driven
  (the host starts and waits; a client's `Connect` RPC triggers `IInstrumentGateway.ConnectAsync`),
  and calling it twice would double-subscribe every event.
- Wrap Helios/IAPI event callbacks in `CallbackGuard.Run` (host-side) rather than invoking the
  forwarded event directly — see the VMS/Corona note above for why this matters beyond style.
- Existing Helios source files (outside `Bridge/`) are inconsistent between tabs
  (`ExplorisConnection.cs`) and 2-space indent (most files under `Interfaces/`); don't do a
  drive-by reformat either way — match whatever file you're editing. Code under `Bridge/` and the
  ported `ScanSpy` uses 2-space indent throughout.

## Known gaps (tracked, not blocking)

- `IMsScan.NoiseBand` isn't on the wire (`Helios.Bridge.Contracts`' `CentroidBlock` carries
  centroids only) — Helios's real `ISpectrum.NoiseBand` has data, it's just not yet in the proto.
- `IScans` exclusive re-acquisition mid-session isn't wired through the bridge — Helios's own
  `IControl.GetScans(bool exclusiveAccess)` supports it, but `HeliosScanControlAdapter`/`GrpcScans`
  always acquire non-exclusively at connect time; `GetPossibleScanParametersRequest.ExclusiveAccess`
  is accepted but not honored end-to-end.
- `Helios.Interfaces...HeliosInstrumentValues.Get(string|ulong)` is stubbed to `return null;` in
  `Helios.dll` for both Fusion and Exploris — instrument-value reads aren't possible through
  Helios's public surface at all today. The bridge's `GetInstrumentValues`/`GetInstrumentValuesAsync`
  both return an empty dictionary and say why in a comment, rather than working around it further.
- No automated tests exercise the bridge's Fusion/Exploris path against real hardware (compiles
  clean, type-checked against real IAPI DLLs, but only Simulated and VMS/Corona have been run
  live in this environment).

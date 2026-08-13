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
`ScanInjector` currently fails to build on a fresh clone independent of anything in `Bridge/` —
its ScottPlot/HarfBuzzSharp NuGet packages were never restored (packages.config-style; `dotnet
restore`/`build` don't drive that restore path). Not fixed as part of the bridge work since
`ScanInjector` was out of scope for it; see the `Nova` package note below for how the same class of
problem was worked around for `Helios.csproj` itself.

Run the bridge host (net48) and a .NET 8 client (`Helios.Client.Demo` or `ScanSpy`) in separate
processes — the host defaults to `Auto` (see below), so no hardware/license is needed to see it
work:
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

### A fresh-clone gotcha: the `Nova` package

`Helios.csproj` references `Nova` via `packages.config` (not `PackageReference`), which `dotnet
build`/`dotnet restore` don't restore automatically. If `Nova, Version=...` fails to resolve,
extract the matching `Nova.<version>.nupkg` (a local feed lives at
`D:\Software\SchweppeLab\NuGet` on this machine) into `Helios/packages/Nova.<version>/`, matching
the layout `dotnet build` expects (`lib/net48/Nova.dll` under that folder).

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

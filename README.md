# Helios

A unified API for real-time MS data analysis and instrument control. Helios currently wraps [Thermo Fisher Scientific IAPI](https://github.com/thermofisherlsms/iapi)
for both Exploris and Tribrid instrument platforms into a single interface. Applications developed with Helios are capable of connecting to both Exploris and Tribrid
instruments from a single code base — and, as of the bridge described below, from either .NET Framework 4.8 or .NET 8.

📦 **Want to try in-progress features?** Development builds are published automatically from the `Dev` branch — no NuGet account, feed setup, or building this
repo yourself needed, just download and go:
- [Latest dev build](https://github.com/SchweppeLab/Helios/releases/tag/dev-latest) — always the most recent successful build: `Helios.Client-dev-latest.nupkg` /
  `Helios.Bridge.Contracts-dev-latest.nupkg` (the NuGet packages for the bridge's .NET 8 client side) and `Helios.Bridge.Host-dev-latest.zip` (the net48 bridge
  host you actually run — see "Over the bridge" below).
- [All dev builds](https://github.com/SchweppeLab/Helios/releases) — every past build, individually tagged, if you need to pin to or re-download a specific one.

Download both `.nupkg` files into one local folder and add it as a NuGet source (`dotnet nuget add source <folder> -n HeliosDev`) so `Helios.Client`'s dependency
on `Helios.Bridge.Contracts` resolves locally. Unzip `Helios.Bridge.Host-dev-latest.zip` wherever you like and run `Helios.Bridge.Host.exe --register` once (see
"Locating and managing Helios.Bridge.Host" below) — **it does not include Thermo Fisher's IAPI DLLs, which Helios can't redistribute**; the zip's own
`README-IAPI.txt` lists exactly which files to copy in from your own licensed IAPI install to talk to real Fusion/Exploris hardware. Without them, it still runs
fine against the built-in simulator or a Corona/VMS connection. These are pre-release, unsupported builds — expect the API to shift without notice.

## Two ways to use Helios

**In-process (.NET Framework 4.8).** Link `Helios.dll` directly and call `Helios.Interfaces.InstrumentAccessContainerFactory.Create()`. Helios talks to IAPI in the
same process. This is the original, still-supported way to use Helios, and the only option available to Framework 4.8 applications (`ScanInjector` uses it).

**Over the bridge (.NET 8).** IAPI only runs on Framework 4.8, so a .NET 8 application can't link `Helios.dll` directly. Instead, have your .NET 8 application
reference `Helios.Client`, which talks to `Helios.Bridge.Host` (a small Framework 4.8 process that wraps IAPI via `Helios.dll`, exactly like the in-process case)
over a local gRPC connection. `Helios.Client`'s public API mirrors Helios's own interfaces (`IInstrumentAccess`, `IControl`, `IAcquisition`, `IScans`, `IMsScan`,
...), so Core 8 application code reads like Helios application code (`ScanSpy` uses this path).

You don't need to start `Helios.Bridge.Host` yourself: `HeliosClient.ConnectAsync` auto-launches it if nothing is already listening (and reuses an already-running
one — from another app, possibly connected to real hardware — instead of starting a redundant instance), and the host shuts itself back down a short while after
its last connected client disconnects, including a crash (see "Locating and managing Helios.Bridge.Host" below). An auto-launched host runs with no console window
and its output going to a log file instead (see below); starting it manually still works too, opens its usual visible console, and is useful when you want to watch
its output directly.

`Helios.Bridge.Host` picks its backend automatically at startup (`Auto` in its `App.config`, the default): it probes for a real Fusion or Exploris instrument, then a
Corona (VMS) connection, and falls back to a built-in synthetic scan generator (`Simulated`) only if none of those answered — the same behavior applications got for
free when linking `Helios.dll` in-process. Set `InstrumentFamily` to `Real` to require real hardware/Corona and fail if none is found, or `Simulated` to always use
the synthetic generator regardless of what's attached (useful for development or load testing without an instrument).

### Locating and managing Helios.Bridge.Host

`Helios.Client` finds an installed `Helios.Bridge.Host.exe` at runtime, in this order:

1. An explicit `hostExecutablePath` argument to `ConnectAsync`.
2. The `HELIOS_BRIDGE_HOST_PATH` environment variable.
3. The Windows registry (`HKCU\Software\SchweppeLab\Helios\BridgeHostPath`) — written automatically by `Helios.Bridge.Host` itself every time it starts
   successfully. If you installed it via an installer, this is already set for you. If you unpacked a zip file instead, run `Helios.Bridge.Host.exe --register`
   once (it writes the key and exits) so later auto-launches can find it.
4. `Helios.Bridge.Host.exe` on your `PATH`.

If none of those resolve, `ConnectAsync` throws an error explaining all four options. `Helios.Bridge.Host` shuts itself down `IdleShutdownSeconds` (5s by default,
configurable in its `App.config`) after its last connected client disconnects — including an ungraceful one, since it's watching for the dropped connection itself
rather than waiting for a goodbye message — so a crashed or closed app doesn't leave it running forever, but a brief gap between two client sessions doesn't tear
down (and, for real hardware, reconnect) it either. Set `IdleShutdownSeconds` to `0` or a negative number to disable auto-shutdown entirely.

An auto-launched host writes its console output to `%LocalAppData%\SchweppeLab\Helios\Helios.Bridge.Host.<port>.log` instead of a window (overwritten on each
launch) — check there if something needs troubleshooting. A manually-started host is unaffected and keeps using its normal console.

## Repository Contents

Helios is developed in C# and contains C# solutions that were developed in Visual Studio 2022 (`Helios/Helios.sln`).

- **`Helios/`** — the Helios API library (net48). Wraps IAPI in-process for both instrument families.
- **`ScanInjector/`** — a demonstration application (net48) for real-time instrument control. Links `Helios.dll` directly, in-process.
- **`ScanSpy/`** — a demonstration application (net8.0-windows) for real-time data monitoring. Talks to an instrument over the bridge, via `Helios.Client`.
- **`Bridge/`** — the Framework 4.8 ↔ .NET 8 bridge:
  - `Helios.Bridge.Contracts` (netstandard2.0) — the shared gRPC `.proto` contract between host and client.
  - `Helios.Bridge.Host` (net48 console) — the gRPC server; wraps IAPI via `Helios.dll` (or generates synthetic data) and exposes it over gRPC.
  - `Helios.Client` (net8 library) — the .NET 8-facing client, mirroring Helios's own interfaces.
  - `Helios.Client.Demo` (net8 console) — an end-to-end showcase: connects, streams scans with a rolling latency report, submits a demo custom scan, toggles
    acquisition on/off.

### Software Requirements
* The Helios API library requires [Thermo Fisher Scientific IAPI](https://github.com/thermofisherlsms/iapi) and [Nova](https://github.com/SchweppeLab/Nova).
* The Helios demonstration applications (ScanSpy and ScanInjector) also require [ScottPlot](https://github.com/ScottPlot/ScottPlot), however, 
we recommend using NuGet in Visual Studio to manage ScottPlot and its requirements.
* Building or running anything under `Bridge/`, or `ScanSpy`, additionally requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

### Additional Hints:
* `Helios`, `Helios.Bridge.Host`, and `ScanInjector` MUST be Framework 4.8 to maintain compatibility with Thermo Fisher Scientific IAPI.
* All projects that Helios depends on (e.g., Nova) must be compiled in Framework 4.8.
* All projects that link `Helios.dll` directly must also be Framework 4.8 — Core 8 applications should use `Helios.Client` and `Helios.Bridge.Host` instead (see
  above).
* Helios applications will only connect to instruments with a valid IAPI license.
* Make sure your instrument Tune software is in sync with the IAPI releases.
* `Helios.Bridge.Host` and its .NET 8 client are loopback-only (no TLS) by design, since both are expected to run on the same instrument-control PC.

### Cite:
Hoopmann, M. R., McGann, C. D., Canterbury, J. D., von Haller, P. D., and Schweppe, D. K. (2025). “Real-Time Instrument Control across Multiple Orbitrap Platforms through a Single Software Interface.” *J. Proteome Res.*, 24(10). DOI: 10.1021/acs.jproteome.5c00269. [Link](https://pubmed.ncbi.nlm.nih.gov/40939636/)

#### Authors
[Michael Hoopmann](https://github.com/mhoopmann), University of Washington

Helios is Copyright Schweppe Lab 2025

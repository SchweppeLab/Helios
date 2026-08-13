# Helios

A unified API for real-time MS data analysis and instrument control. Helios currently wraps [Thermo Fisher Scientific IAPI](https://github.com/thermofisherlsms/iapi)
for both Exploris and Tribrid instrument platforms into a single interface. Applications developed with Helios are capable of connecting to both Exploris and Tribrid
instruments from a single code base — and, as of the bridge described below, from either .NET Framework 4.8 or .NET 8.

## Two ways to use Helios

**In-process (.NET Framework 4.8).** Link `Helios.dll` directly and call `Helios.Interfaces.InstrumentAccessContainerFactory.Create()`. Helios talks to IAPI in the
same process. This is the original, still-supported way to use Helios, and the only option available to Framework 4.8 applications (`ScanInjector` uses it).

**Over the bridge (.NET 8).** IAPI only runs on Framework 4.8, so a .NET 8 application can't link `Helios.dll` directly. Instead, run `Helios.Bridge.Host` (a small
Framework 4.8 process that wraps IAPI via `Helios.dll`, exactly like the in-process case) and have your .NET 8 application reference `Helios.Client`, which talks to
the host over a local gRPC connection. `Helios.Client`'s public API mirrors Helios's own interfaces (`IInstrumentAccess`, `IControl`, `IAcquisition`, `IScans`,
`IMsScan`, ...), so Core 8 application code reads like Helios application code (`ScanSpy` uses this path).

`Helios.Bridge.Host` picks its backend automatically at startup (`Auto` in its `App.config`, the default): it probes for a real Fusion or Exploris instrument, then a
Corona (VMS) connection, and falls back to a built-in synthetic scan generator (`Simulated`) only if none of those answered — the same behavior applications got for
free when linking `Helios.dll` in-process. Set `InstrumentFamily` to `Real` to require real hardware/Corona and fail if none is found, or `Simulated` to always use
the synthetic generator regardless of what's attached (useful for development or load testing without an instrument).

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

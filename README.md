# Helios
A unified API for real-time MS data analysis and instrument control. Helios currently wraps [Thermo Fisher Scientific IAPI](https://github.com/thermofisherlsms/iapi)
for both Exploris and Tribrid instrument platforms into a single interface. Applications developed with Helios are capable of connecting to both Exploris and Tribrid
instruments from a single code base.

## Repository Contents
Helios is developed in C# and contains C# solutions that were developed in Visual Studio 2022. The solution contains three projects: the Helios API library, ScanSpy: a
demonstration application for real-time data monitoring, and ScanInjector: a demonstration application for real-time instrument control. Additionally, example data collected
from use of ScanInjector are provided. Windows builds are also provided.

### Software Requirements
* The Helios API library requires [Thermo Fisher Scientific IAPI](https://github.com/thermofisherlsms/iapi) and [Nova](https://github.com/SchweppeLab/Nova).
* The Helios demonstration applications (ScanSpy and ScanInjector) also require [ScottPlot](https://github.com/ScottPlot/ScottPlot), however, 
we recommend using NuGet in Visual Studio to manage ScottPlot and its requirements.

### Additional Hints:
* Helios MUST be Framework 4.8 to maintain compatibility with Thermo Fisher Scientific IAPI.
* All projects that Helios depend on (e.g., Nova) must be compiled in Framework 4.8.
* All projects that link Helios.dll must also be Framework 4.8.
* Helios applications will only connect to instruments with a valid IAPI license.
* Make sure your instrument Tune software is in sync with the IAPI releases.

#### Authors
[Michael Hoopmann](https://github.com/mhoopmann), University of Washington

Helios is Copyright Schweppe Lab 2025
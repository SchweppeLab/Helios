---
layout: default
title: Helios
description: The Source Code Repository.
---

## Repository Contents

The Helios source code can be obtained from [its GitHub repository](https://github.com/SchweppeLab/Helios).

Helios is developed in C# and contains a C# solution that was developed in Visual Studio 2022. 
The solution contains three projects: 

1. the Helios API library
2. ScanSpy: a demonstration application for real-time data monitoring
3. ScanInjector: a demonstration application for real-time instrument control. 

Note that Helios must remain in .NET Framework 4.8 to be compatible with IAPI and connect to mass spectrometers.
Do not upgrade the project files in the Helios source tree. 

When building Helios and the demonstration applications from the source solution file, you will have
to fix the project references to dll files obtained externally to Helios. Follow the instructions below
to obtain and correctly reference these supporting dll files.

## Software Requirements

* The Helios API library requires [Thermo Fisher Scientific IAPI](https://github.com/thermofisherlsms/iapi) and [Nova](https://schweppelab.github.io/Nova).
* The Helios demonstration applications (ScanSpy and ScanInjector) also require [ScottPlot](https://github.com/ScottPlot/ScottPlot). We recommend using NuGet in Visual Studio to manage ScottPlot and its requirements.

## Setting Up Your Helios Solution

1. Open the Helios\Helios.sln solution file.
2. In the Helios project references, add Nova. Note that a [Nova NuGet package](https://schweppelab.github.io/Nova/download/) is available and can be installed locally on your computer.
3. In the Helios project references, add the IAPI dll files:
	* API-2.0.dll: in the file properties, leave the alias as "global"
	* Spectrum-1.0.dll: in the file properties, leave the alias as "global"
	* Fusion.API-1.0.dll: in the file properties, set the alias as "fusion"
	* Thermo.TNG.Factory.dll: in the file properties, set the alias as "fusion"
	* Thermo.API.NetStd-1.0: in the file properties, set the alias as "exploris"
	* Thermo.API.Exploris.NetStd-1.0: in the file properties, set the alias as "exploris"
	* Thermo.API.Spectrum.NetStd-1.0: in the file properties, set the alias as "exploris"
5. In the ScanSpy and ScanInjector projects, refresh the NuGet package manager to assure all dependencies are established.
6. Build the solution.	

Additional note: If Nova and IAPI references exist in a broken state, try deleting them from the reference list and readding them using steps 2 and 3 above.
	
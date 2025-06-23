---
layout: default
title: Helios
description: Getting started.
---

## Software Requirements

* The Helios API library requires [Thermo Fisher Scientific IAPI](https://github.com/thermofisherlsms/iapi) and [Nova](https://schweppelab.github.io/Nova).

## Setting Up Your Application

1. Create a new .NET Framework project. **Applications using Helios MUST be .NET Framework 4.8** to maintain compatibility with Thermo Fisher Scientific IAPI.
2. In the project dependencies, add Helios. You can use the Helios NuGet package obtained from this web site.
3. In the project dependencies, add Nova. Note that a [Nova NuGet package](https://schweppelab.github.io/Nova/download/) is available and can be installed locally on your computer.
4. In the project references, add the IAPI dll files:
	* API-2.0.dll: in the file properties, leave the alias as "global"
	* Spectrum-1.0.dll: in the file properties, leave the alias as "global"
	* Fusion.API-1.0.dll: in the file properties, set the alias as "fusion"
	* Thermo.TNG.Factory.dll: in the file properties, set the alias as "fusion"
	* Thermo.API.NetStd-1.0.dll: in the file properties, set the alias as "exploris"
	* Thermo.API.Exploris.NetStd-1.0.dll: in the file properties, set the alias as "exploris"
	* Thermo.API.Spectrum.NetStd-1.0.dll: in the file properties, set the alias as "exploris"
	
## Quick Test Application

1. Follow steps 1-4 from "Setting Up Your Application" above.
2. Add a button to the project application form. Add the following code to the button click event
```csharp
private void button1_Click(object sender, EventArgs e)
{
    try
    {

        IInstrumentAccessContainer _container = InstrumentAccessContainerFactory.Create();
        if (_container == null) {
            MessageBox.Show("No instrument or virtual MS found.");
        }
        else
        {
            MessageBox.Show("Instrument ID: " + _container.InstrumentType());
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Helios create InstrumentAccessContainer failed: " + ex.Message);
    }
}
```
3. Build and run the application. 
	* When run on a computer with an active instrument or virtual MS, the instrument ID will appear when the button is clicked.
	* If there is no instrument or virtual MS, a message will appear indicating neither was found.
	* If the project dependencies are missing or the IAPI dll files are not properly referenced with the correct aliases, a failure message will appear. 
	Return to steps 2-4 in "Setting Up Your Application" above, and double-check that all steps have been completed properly.

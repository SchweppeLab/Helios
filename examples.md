---
layout: default
title: Helios
description: Example code snippets.
---

## Example #1: Connecting to an instrument and receiving scan data.

This example demonstrates how to use Helios to create an application that connects to an instrument, establishes a scan listening function,
and receives real-time mass spectra. 

### Step #1: Create a new Visual Studio project

You can name your project whatever you want, but for the purposes of this example, the project will be named **ReadScan**.

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
	
### Step #2: Import the Helios namespaces

At the top of the ReadScan.cs file, add the following:

```csharp
using Helios.Interfaces;
using Helios.Interfaces.InstrumentAccess;
using Helios.Interfaces.InstrumentAccess.MsScanContainer;
```

### Step #3: Declare the global variables

IAPI has many classes for accessing software and instrumentation required for real-time mass spectrometry (RTMS). These classes need to
interact with each other at various points in the operation of an RTMS application. To ensure the application has access to these classes
when necessary, global declarations are made to Helios objects that wrap around the IAPI classes.
In the ReadScan class declaration in ReadScan.cs, add the following:

```csharp
namespace ReadScan
{
  public partial class ReadScan: Form
  {
    private IInstrumentAccessContainer msContainer;
    private IInstrumentAccess msAccess;
    private IMsScanContainer msScans;
	
```

### Step #4: Connect to the instrument and capture events


Add a button to the ReadScan application form. Add the following code to the button click event:

```csharp
private void button1_Click(object sender, EventArgs e)
{
  try
  {

    msContainer = InstrumentAccessContainerFactory.Create();
    if (msContainer == null) {
      MessageBox.Show("No instrument or virtual MS found.");
    }
    else
    {
	  msContainer.ServiceConnectionChanged += ServiceConnectionChanged;
	  msContainer.MessagesArrived += MessagesArrived;
	  msContainer.StartOnlineAccess();
    }
  }
  catch (Exception ex)
  {
    MessageBox.Show("Helios create InstrumentAccessContainer failed: " + ex.Message);
  }
}
```

Most importantly, the StartOnlineAccess() function must be called to use IAPI, but it should only be called if you 
have a valid msContainer object. Furthermore, notice that two events are managed by the msContainer object, which
receives messages from the IAPI. The ServiceConnectionChanged event is important, because once raised the application
can connect to the instrument. Add the following code to ReadScan.cs to process these events:

```csharp
void ServiceConnectionChanged(object sender, EventArgs e)
{
  if (msContainer != null)
  {
    if (msContainer.ServiceConnected)
    {
      try
      {
        //Get the instrument access object from the instrument access container. 
        //Always use a value of '1' for the parameter.
        msAccess = msContainer.Get(1);
        msAccess.AcquisitionErrorsArrived += AcquisitionErrorsArrived;
        msAccess.ConnectionChanged += ConnectionChanged;
        msAccess.ContactClosureChanged += ContactClosureChanged;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to get instrument access: " + ex.Message);
      }
    }
  }
}

void MessagesArrived(object sender, MessagesArrivedEventArgs e)
{
  //add your own code to process these messages, perhaps to a log.
}
```

Notice that the msAccess class also has events that can be captured. Functions should be added to process these events:

```csharp
void AcquisitionErrorsArrived(object sender, AcquisitionErrorsArrivedEventArgs e)
{
  //add your own code to process these messages, perhaps to a log.
}

void ConnectionChanged(object sender, EventArgs e)
{
  //add your own code to process these messages, perhaps to a log.
}

void ContactClosureChanged(object sender, ContactClosureEventArgs e)
{
  //this event may be particularly useful, if processing needs to be done after contact closure.
}
```

### Step #5: Listen for real-time spectral broadcasts

Once you have access to the instrument, the next step is to establish a connection to the data storage
class so that you can inspect spectra as they are acquired. In ReadScan, we want to do this starting
at the moment we connect to the instrument. Return to your ServiceConnectionChaged() function, and add the new code:

```csharp
void ServiceConnectionChanged(object sender, EventArgs e)
{
  if (msContainer != null)
  {
    if (msContainer.ServiceConnected)
    {
      try
      {
        //Get the instrument access object from the instrument access container. 
        //Always use a value of '1' for the parameter.
        msAccess = msContainer.Get(1);
        msAccess.AcquisitionErrorsArrived += AcquisitionErrorsArrived;
        msAccess.ConnectionChanged += ConnectionChanged;
        msAccess.ContactClosureChanged += ContactClosureChanged;
		
        //Add your new code here!!!
        msScans = msAccess.GetMsScanContainer(0);
        msScans.MsScanArrived += MsScanArrived;
		
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to get instrument access: " + ex.Message);
      }
    }
  }
}
```

The msScans object contains one last event that triggers whenever the instrument acquires a new spectrum.
It is here that you can interrogate the spectra in real-time. Add this next function to the ReadScan.cs:

```csharp
void MsScanArrived(object sender, MsScanEventArgs e)
{
  //these scan arrive fast and use memory. So process and discard as
  //quickly as possible.
  using (IMsScan msScan = e.GetScan())
  {
    /* add your code here for real-time spectrum processing, e.g.,
    foreach (var centroid in msScan.Centroids)
    {
      //do something with the peak data points
    }
    */  
  }
}
```

The IMsScan object contains the spectrum meta information and peak data. It is a convenient structure for
accessing the real-time spectral information that arrived in the spectral acquisition event.


# More Examples Coming Soon!

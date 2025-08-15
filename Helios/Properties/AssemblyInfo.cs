using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Helios")]
[assembly: AssemblyDescription("Universal API for real-time mass spectrometer control.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("University of Washington, Schweppe Lab")]
[assembly: AssemblyProduct("Helios")]
[assembly: AssemblyCopyright("Copyright © 2025, Michael Hoopmann, Devin Schweppe")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7b4c1e72-ab3c-4c84-a4e7-e50009b567c1")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.7")]
[assembly: AssemblyFileVersion("1.0.0.7")]

//Suggested for packing from command line:
//nuget pack .\Helios.csproj -Prop Configuration=Release

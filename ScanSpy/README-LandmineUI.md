# ScanSpy's optional LandmineUI window

ScanSpy can run inside a themed, frameless `LandmineUI.WinForms.SharpWindow`
instead of a plain WinForms `Form`. This is off by default and entirely
machine-local — a fresh clone of this repo builds and runs the plain `Form`
version with no extra setup.

To turn it on for your own clone:

1. Point NuGet at the local LandmineUI feed. Create `NuGet.Config` at the repo
   root (gitignored, next to `Helios.sln`):

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
       <add key="LandmineUI-local" value="D:\Software\NuGet\LandmineUI" />
     </packageSources>
   </configuration>
   ```

   Adjust the `LandmineUI-local` path to wherever your `LandmineUI.*.nupkg`
   files actually live.

2. Create an empty `ScanSpy/LandmineUI.local.props` (gitignored). Its mere
   existence is what `ScanSpy.csproj` checks for — content doesn't matter.

3. Build normally. `ScanSpy.csproj` picks up the `LandmineUI.WinForms`
   package reference and the `USE_LANDMINE_UI` compile symbol automatically
   once both of the above exist.

To go back to the plain WinForms window, delete
`ScanSpy/LandmineUI.local.props` (or just don't create it).

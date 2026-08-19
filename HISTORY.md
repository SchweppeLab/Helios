# History

An append-only log of completed work on the `Core8` branch. Newest entries at the top. Each entry
records what was finished, verified, or fixed. Update this file whenever a component reaches
"done" (built, builds clean, and -- where applicable -- verified live), rather than rewriting or
deleting past entries.

---

## 2026-08-19 -- Temporary move to a Nova dev build (`1.1.0-dev.8`)

**Temporary.** Nova's `Dev` branch has moved its core package (the `Data/`+`IPC/Pipes` code Helios
actually links) from a .NET Framework 4.8-only build to `netstandard2.0` -- no `net48` build of
Nova exists there anymore. Pinned Helios's `Dev` branch to that dev build (`1.1.0-dev.8`) ahead of
Nova's next real stable release, so Helios doesn't sit on the now-unmaintained `1.0.0.18` in the
meantime. Expect this pin to be replaced once Nova ships a real release -- not meant to stick
around.

Changed: `packages.config` and `Helios.csproj`'s `Nova` reference (version + `HintPath`, now
pointing at `lib/netstandard2.0/` instead of `lib/net48/`; also added an explicit
`<Reference Include="netstandard" />`, since the old-style `packages.config`+`HintPath` setup here
doesn't go through a real `nuget install` and so doesn't automatically pick up the netstandard
compat facade the way a `PackageReference`-based restore would), `Helios.nuspec`'s Nova dependency
pin, and the `Dev` workflow's "Fetch Nova into the local packages layout" step (now pulls
`Nova.1.1.0-dev.8.nupkg` directly from Nova's `dev-latest` release rather than a tagged release --
pinned to today's specific build, not a moving target, since Nova's `dev-latest` assets carry
per-run versioned filenames).

Verified locally: `Helios.sln` builds clean (`Release|x64`) -- `Helios.dll` itself and everything
that depends on it (`Helios.Bridge.Host`, `Helios.Client`, `ScanSpy`, `Helios.Client.Demo`) compiled
against the new Nova build with no new warnings or errors. `ScanInjector` failed to restore, but on
a pre-existing, unrelated NuGet RuntimeIdentifier issue (a ScottPlot/SkiaSharp native-asset
restore problem) -- not touched by, or related to, this change.

## 2026-08-18 -- Dev-branch workflow: fixed the original Helios package's first real CI failure

**Status: root cause found and fixed, verified via a properly isolated local reproduction; not yet
confirmed by an actual CI run.** The first live run of the previous entry's "original Helios package"
addition failed: `Pack the original Helios package` errored with `NU5012: Unable to find
'Nova.1.0.0.18.nupkg'. Make sure the project has been built.`, cascading into a confusing secondary
`Copy-Item` failure on the next line (the script had no exit-code check, so it kept going after
`nuget.exe` failed). The other 12 annotations on that run were pre-existing compiler warnings already
present in `Helios/`'s own code (XML doc gaps, unused VMS-only events, one unused exception variable)
-- unrelated to this change, confirmed by checking the job's step list directly: `Build
Helios.Bridge.Host` (the step that actually compiles that code) succeeded; `Pack the original Helios
package` was the one that failed, with everything after it cascade-skipped.

**Root cause: `nuget.exe pack`'s `packages.config`-driven dependency resolution needs the actual
`Nova.1.0.0.18.nupkg` file sitting inside `Helios/packages/Nova.1.0.0.18/`, not just that package's
extracted contents** (which is all the "Fetch Nova" step had ever provided -- `Expand-Archive` unpacks
a nupkg's *contents* into a folder, it doesn't also leave the original archive file sitting there).
This had never been caught locally because this dev machine's global NuGet cache
(`~/.nuget/packages/nova/*`, populated by years of unrelated work on other projects) and a
machine-level `NuGet.Config` (a `"Schweppe Lab"` source pointing at a local feed) both independently
let `nuget.exe pack` resolve Nova anyway, silently masking the gap -- a fresh CI runner has neither.
Worse, the very first successful local pack test (run before this was understood) caused `nuget.exe`
to write a resolved copy of `Nova.1.0.0.18.nupkg` into that same local `packages/` folder as a side
effect, which then made every subsequent "isolated" local test pass too, for the wrong reason (cheating
off that leftover file, not off the actual fix) -- caught only by explicitly deleting that file and
re-testing with both the global cache (`NUGET_PACKAGES` env var redirected to an empty folder) and
config sources (`-ConfigFile` pointed at a `<clear/>`-only config) neutralized at once, which properly
reproduced `NU5012` locally for the first time. Re-adding just the raw `.nupkg` file in that same fully
isolated environment then resolved it, confirming the real fix rather than another false positive.

**Fix:** `Fetch Nova into the local packages layout` now also does a plain `Copy-Item` of the raw
`Nova.1.0.0.18.nupkg` into `Helios/packages/Nova.1.0.0.18/Nova.1.0.0.18.nupkg` after extracting its
contents. Also added an explicit `$LASTEXITCODE` check right after the `nuget.exe pack` call, so any
future failure there surfaces as its own clear error instead of cascading into an unrelated-looking
`Copy-Item` failure on the next line.

**Verified:** reproduced `NU5012` for real in a fully isolated local environment (global packages
cache and configured sources both neutralized), confirmed the fix resolves it in that same
environment, confirmed `Expand-Archive`'s existing (unmodified) extraction line already succeeds on
the real CI runner regardless of a `.nupkg`-extension quirk that only affects this local machine's
Windows PowerShell 5.1 test tooling (GitHub's `windows-latest` runners default `run:` steps to `pwsh`
7+, which doesn't have that restriction -- and this exact step already succeeded in the prior live CI
run before this fix, independent confirmation it works there). **Not yet verified:** an actual CI run
of this fix -- needs another push to `Dev`.

---

## 2026-08-17 -- Dev-branch workflow: the original Helios package too

**Status: pack verified locally against a real build; live CI run not yet done.** Added a third
package to the dev release: the original in-process Helios library (`Helios.dll`), for legacy net48
consumers who link Helios directly instead of going through the bridge, and for anyone who wants to
build their own net48 app that way. Packed inside `build-bridge-host` (which already builds
`Helios.dll` as a side effect of building `Helios.Bridge.Host`, and already has `iapi`/Nova fetched),
via the classic `nuget.exe` CLI against `Helios.csproj`/its pre-existing `Helios.nuspec` --
`Helios.csproj` is old-style (`ToolsVersion="15.0"`, confirmed by checking its header directly), so
`dotnet pack` doesn't apply here the way it does for `Helios.Client`/`Helios.Bridge.Contracts`.

**Fixed a real staleness bug in `Helios.nuspec` while wiring this up** (asked first, per this repo's
bug-fixing policy, since it's pre-existing content -- got a yes): its `<dependencies>` block hardcoded
`Nova >= 1.0.0.11`, stale against `packages.config`'s `1.0.0.18` sitting right next to it. Harmless in
practice (the open range still resolves against a `1.0.0.18` local feed) but a real inconsistency,
never caught before because nothing had ever actually packed from this file until now. Synced to
`1.0.0.18` and reverified the pack picks it up correctly.

**Versioning, agreed in discussion before implementing:** unlike `Helios.Client`/
`Helios.Bridge.Contracts` (never published before, clean slate at `0.1.0-dev.*`), the original Helios
package already has a real version lineage -- `AssemblyInfo.cs` currently says `1.0.0.16` (matching
the `v1.0.0.16` git tag). The dev package uses that same base version plus a `-dev.<run number>`
suffix (`1.0.0.16-dev.<run>`), read out of `AssemblyInfo.cs` at pack time rather than hardcoded in the
workflow, so it stays correct whenever Helios's own version is bumped, and never collides with a bare,
un-suffixed version a real future release might use. Correcting the record here: an earlier
in-conversation summary of this plan said `1.0.0.18` as "Helios's current version" -- that was wrong,
an unverified assumption (conflated with Nova's version, which really is `1.0.0.18`), caught only once
actually reading `AssemblyInfo.cs` directly while implementing this. Long-term plan (once `Dev` merges
to a real release): all three packages unify onto `2.0.0`-based SemVer, version-locked together.

**Verified:** `nuget.exe pack Helios/Helios.csproj -Properties Configuration=Release;Platform=x64
-Version <override>` run locally against a real build, twice (before and after the nuspec fix) --
confirmed the resulting package contains only `lib/net48/Helios.dll` + `Helios.xml` + `LICENSE.txt`,
correctly ignoring the IAPI/Nova DLLs CopyLocal'd into that same `bin` output folder (nuget.exe's
convention-based packing only bundles a project's own primary output, not everything sitting in its
bin directory) -- so no stripping step is needed here the way `Helios.Bridge.Host`'s zip needs one.
Also confirmed `-Version` override and the `AssemblyVersion` regex extraction both work as intended.
**Not yet verified:** whether `nuget.exe`'s MSBuild auto-detection behaves the same on
`windows-latest`'s bundled Visual Studio as it did against this local machine's -- needs a live CI run
to confirm, same as every other not-yet-verified item in this file's recent entries.

---

## 2026-08-17 -- Dev-branch workflow: combined "everything in one zip" bundle

**Status: verified locally.** After the first live run of `.github/workflows/dev-nuget.yml` (below)
succeeded and published all three assets correctly, added a fourth: a single combined
`Helios[.<version>|-dev-latest].zip` with the two NuGet packages under `NuGet/` and the (already
IAPI-stripped) `Helios.Bridge.Host` build under `Helios.Bridge.Host/`, plus its own `README.txt`
covering both setup steps -- so grabbing everything needed no longer means finding and downloading
three separately-named assets. The three component-specific assets stay published too, for anyone
who only wants one piece (e.g. just the NuGet packages, because they already have a
`Helios.Bridge.Host` running elsewhere).

Building this combined zip needs both jobs' output on one runner, but `pack_nuget` (ubuntu-latest)
and `build-bridge-host` (windows-latest) are separate machines -- `pack_nuget` now also uploads its
`.nupkg`s as workflow artifacts (`actions/upload-artifact`, one for the versioned dated-release set,
one for the version-free dev-latest set), which `build-bridge-host` downloads
(`actions/download-artifact`) before assembling both bundles.

Caught one real bug before it could hit CI a second time: the combined-bundle README was first
written as a PowerShell here-string (`@"..."@`) nested inside a function body, with its closing
`"@` indented to match. Windows PowerShell 5.1 requires that closing delimiter at column 0 with no
leading whitespace at all -- indenting it is a parse error. (GitHub's `windows-latest` runners
default `run:` steps to `pwsh` 7+, which relaxed this rule, so it likely would have worked anyway --
but not worth relying on that.) Rewrote it as a plain `$readmeLines = @(...)` array joined by
`Out-File`, which has no delimiter-placement rule to get wrong. The *other* here-string already in
this workflow (`README-IAPI.txt`, added in the previous entry) was checked byte-for-byte and
confirmed already correctly formed -- its delimiters share the run-block's baseline indentation
exactly, so YAML's block-scalar dedent already lands them at column 0.

**Verified:** ran the exact bundle-assembly PowerShell locally against a real `Helios.Bridge.Host`
build and real packed `.nupkg` files, inspected the resulting zip's entry list (correct
`NuGet/`/`Helios.Bridge.Host/` layout, both nupkgs present) and extracted `README.txt` (clean output,
no stray indentation from the array-based approach). The workflow's first live run (NuGet packages +
Bridge.Host, pre-dating this combined-bundle addition) already succeeded end-to-end on push, publicly
confirming: Nova's `1.0.0.18` release nupkg resolves to the correct file (verified by downloading the
actually-published asset and checking `Nova.dll`'s embedded `FileVersion`/`ProductVersion`/
`AssemblyVersion` all read `1.0.0.18`, matching size/timestamp exactly), and the rest of the pipeline
(iapi clone, Bridge.Host build, IAPI-file stripping, both releases, the `dev-latest` tag-move)
functioned as designed. **Not yet verified:** this specific combined-bundle addition hasn't had a
live CI run yet -- needs another push to `Dev`.

---

## 2026-08-17 -- Dev-branch build, published as GitHub Releases (no feed): NuGet packages + Bridge.Host

**Status: every piece verified locally (pack, and a full real Helios.Bridge.Host build against live
iapi/Nova); the GitHub Actions workflow itself not yet run in CI (needs a real push to `Dev` to fire).**

`Helios.Client` already had NuGet pack metadata staged (`PackageId`/`Description`/license file, unused
by any build step); `Helios.Bridge.Contracts` (netstandard2.0, referenced by `Helios.Client` via
`ProjectReference`) got the matching metadata added, mirroring `Helios.nuspec`'s values, so it packs
as a proper dependency rather than needing to be vendored/merged in.

Chose GitHub Releases over GitHub Packages for distribution: GitHub Packages' NuGet registry requires
an authenticated PAT for every download, even on a public repo, which is unnecessary friction for
external testers on a project that doesn't need private distribution. `.github/workflows/dev-nuget.yml`
publishes two GitHub Releases on every push to a new `Dev` branch, both marked pre-release: a dated one
(`dev-<run number>-<short sha>`, kept forever, for pinning to a specific past build) and a rolling
`dev-latest` one, whose git tag and release assets both get force-moved/overwritten each run so it
always points at the newest build. `dev-latest`'s asset filenames are version-free
(`Helios.Client-dev-latest.nupkg`) specifically so its download URLs never change between builds.
README links to both from the top of the page.

**Initially this only covered `Helios.Client`/`Helios.Bridge.Contracts`** (packed at
`0.1.0-dev.<run number>` -- unrelated to `Helios.dll`'s own `v1.x` tag scheme, since neither package
had ever been published before), packed directly from those two `.csproj` files rather than
`Helios.sln`, needing nothing beyond the .NET 8 SDK. Flagged as an incomplete answer to "can testers
get everything they need from one place": `Helios.Bridge.Host` -- the process that actually has to run
for `Helios.Client` to do anything -- wasn't built or published anywhere, and `Helios.Bridge.Host.csproj`
references `Helios.csproj`, which needs the sibling `iapi` repo (private `HintPath`s) and Nova (no
public NuGet feed) to build, neither ordinarily available in CI.

**Closed that gap without a self-hosted runner or IAPI redistribution**, once both blockers turned out
to be avoidable: `thermofisherlsms/iapi` is itself a public, MIT-licensed GitHub repo whose `lib/`
layout matches every `HintPath` in `Helios.csproj` file-for-file (confirmed against the real repo tree,
not assumed), and Nova -- a separate, SchweppeLab-owned Apache-2.0 library, not part of IAPI -- ships
its exact pinned version (`1.0.0.18`) as a pre-built nupkg on its own GitHub Releases (confirmed by
downloading and unzipping it: `lib/net48/Nova.dll`, matching the manual-placement layout this file's
"fresh-clone gotcha" section already documented). A second job (`build-bridge-host`, `windows-latest`,
net48 needs a real Windows/MSBuild toolchain) clones `iapi` as a build-time-only sibling and fetches
Nova the same way, builds `Helios.Bridge.Host.csproj`, then strips the specific Thermo IAPI DLLs
`Helios.csproj`'s `HintPath`s pull into the output (`Helios.csproj` itself is untouched -- flipping its
`Private`/CopyLocal behavior at the source would also change `ScanInjector`'s existing in-process build,
out of scope per this repo's bug-fixing policy) before zipping it, dropping in a `README-IAPI.txt`
listing exactly which files testers must supply from their own licensed install. `Helios.dll` and
`Nova.dll` themselves stay in the zip -- only Thermo's own binaries are excluded. `ScanInjector`/
`ScanSpy` remain unpublished (demo apps, not something a consuming application needs).

**Verified:** `dotnet pack` on both NuGet projects locally with an explicit `-p:PackageVersion`
override, confirming `Helios.Client`'s generated dependency on `Helios.Bridge.Contracts` resolves to
the matching version automatically. Separately, a full real `dotnet build` of
`Bridge/Helios.Bridge.Host/Helios.Bridge.Host.csproj -c Release -p:Platform=x64` against this machine's
existing local `iapi`/Nova setup succeeded clean (0 warnings, 0 errors), and the IAPI-file-stripping
PowerShell logic was tested against that real output -- confirmed all 13 Thermo files (7 DLLs + 6 `.xml`
docs) removed, `Helios.dll`/`Nova.dll`/everything else intact. **Not yet verified:** the GitHub Actions
workflow itself end-to-end (release creation, the `dev-latest` tag-move step, asset overwrite behavior,
and whether `windows-latest`'s toolchain behaves identically to this local machine's) -- needs `Dev`
pushed to a remote to actually run.

---

## 2026-08-15 -- ScanSpy: status LEDs replace plain colored-square Buttons

**Status: done.** The five connection/listener status indicators
(`connectionIndicator`, `disconnectionIndicator`, `listenIndicatorOn`, `listenIndicatorWait`,
`listenIndicatorOff`) were plain `System.Windows.Forms.Button` controls repurposed as 16x16 colored
squares -- `BackColor` was the only thing ever touched on them, and three had empty `Click`
no-op handlers left over from the designer. Replaced with a new `LedIndicator : Control`
(`ScanSpy/LedIndicator.cs`), custom-painted with GDI+: a glossy round LED with a soft multi-ring
glow when lit (fill color not grayscale) and a flatter dim dot when off, instead of Button's native
chrome (3D bevel, focus rectangle, hover highlight) reading as a clickable control rather than a
status light.

`LedIndicator` has no LandmineUI dependency of its own -- it's plain `System.Drawing`/
`System.Windows.Forms` -- so it behaves identically in both build configurations with no `#if`
needed around the Designer.cs declarations, just a straight `Button` -> `LedIndicator` swap
(`BackColor` calls became `LedColor`, `UseVisualStyleBackColor` dropped since it's Button-only).
Under `USE_LANDMINE_UI` specifically, `OnPaint` pulls its backdrop and "off" ring color from
`ThemeManager.Current` (`Background`/`Border`) instead of hardcoded stock-WinForms grays, so it
blends into a `SharpGroupBox` instead of sitting on a mismatched default-gray square. Green/yellow/
red semantics unchanged -- callers still assign `Color.Lime`/`Color.Yellow`/`Color.Red`/`Color.Gray`
the same way, just to a `LedColor` property instead of `BackColor`.

**Verified**: both build paths (`LandmineUI.local.props` present and absent) compile clean with 0
errors. User confirmed the LandmineUI build visually: "Looks great."

---

## 2026-08-15 -- ScanSpy: real scan filter text, header tab scroll preservation

**Status: done.** Two follow-on ScanSpy fixes to the Header tab, both traced through
`D:\Software\Claude\RawFileReader` and `D:\Software\Claude\Nova` for ground truth rather than
guessed at.

**Scan filter string fixed.** `ProcessScanHeader`'s filter line was always rendering as a static-
looking `"FTMS + c ESI?"` -- root cause: `HeliosMsScanVMS` (Corona/VMS-sourced scans) never
populates `IonizationMode` in its header at all, so every VMS-driven scan fell into the old code's
`else` branch and appended a literal `"ESI?"` placeholder regardless of the actual scan. Real fix:
Corona plays back genuine `.raw` files through RawFileReader, and Nova's `ThermoRawReader.cs`
already computes the authentic Thermo filter string (`Spectrum.ScanFilter =
RawFile.GetFilterForScanNumber(...).ToString()`), carried through into the scan header under the
raw `"Filter"` key. `ProcessScanHeader` now uses that verbatim when present. Live Exploris/Fusion
acquisition via IAPI has no equivalent (confirmed absent from `HeliosDictionary`'s canonical
mapping -- the real-time interface never exposes a filter string), so those connections still fall
back to a reconstruction from the fields IAPI actually provides: mass analyzer, polarity,
centroid/profile, ionization mode (used as-is now, no forced placeholder), scan mode, a proper
`ms`/`ms2`/`ms3` suffix derived from `MSOrder` (`MsOrderSuffix`, handling the "MS"/"1"/"MS2"/"2"/"3"
spelling inconsistencies already dealt with elsewhere in the file), and the mass range. No
precursor-mass/activation-energy segment is fabricated for the fallback case -- that data isn't
surfaced anywhere in the live header path.

**Header tab scroll position preserved.** `rtbHeader.Text` gets replaced wholesale on every scan
refresh (~10Hz), which reset the scroll position to the top on every update -- scrolling down to
read a field was pointless. Plain `RichTextBox` build: fixed via `EM_GETFIRSTVISIBLELINE`/
`EM_LINESCROLL`, capturing the line position before the `Text` set and restoring it after. First
pass restored by the pre-change absolute line number, assuming `WM_SETTEXT` always resets the view
to line 0; when that assumption was off by even one line, the shortfall compounded every refresh
into a slow drift back to the top ("ratchets back to the top, one line at a time" -- reported
directly). Fixed by re-reading the actual post-`Text`-set position and scrolling by the *delta* to
the target instead, matching the same delta-based pattern LandmineUI's own scrollbar sync uses
internally.

LandmineUI build: `SharpTextArea`/`SharpTextBox` had no scroll or selection accessors exposed at
all as of v1.1.0 -- filed upstream as a wish
(`D:\Software\Claude\Wishes\LandmineUI\text-control-scroll-position.md`, per that repo's documented
external-wish workflow). Granted same day: v1.2.0 shipped `FirstVisibleLine` (get/set `int`,
handle-not-yet-created-safe) and `SetTextPreservingScroll(string?)` (a thin wrapper over the same
capture/replace/restore approach), built on the same `EM_GETFIRSTVISIBLELINE`/`EM_LINESCROLL` pair
already used internally for `SharpTextBox`'s own themed scrollbar. Bumped the package reference to
1.2.0 and switched `SetHeaderText`'s LandmineUI branch to call it directly. Wish file deleted after
user confirmed both builds worked, per that workflow's own rule that only the requester closes a
wish.

Also fixed a self-inflicted regression during this work: after test-building the plain-WinForms
path with `LandmineUI.local.props` temporarily moved aside, the marker was restored but the project
wasn't rebuilt, so the running `ScanSpy.exe` was a stale plain-WinForms binary -- reported as
"ScanSpy no longer uses LandmineUI." Rebuilding with the marker in place fixed it; confirmed via
`ScanSpy.deps.json` referencing `LandmineUI.WinForms/1.2.0`.

**Verified**: both build paths compile clean with 0 errors. User confirmed live: "It all works
great now."

---

## 2026-08-15 -- ScanSpy: graceful disconnect, spectrum plot cleanup, optional LandmineUI window

**Status: done.** Three independent ScanSpy-side fixes/features, none touching the host/bridge
performance work in the entries below.

**Disconnect crash fixed.** `Grpc.Net.Client` surfaces a locally-cancelled streaming call as
`RpcException(StatusCode.Cancelled)`, not `OperationCanceledException` -- every background "pump"
loop in `Helios.Client` (`HeliosClient.cs`, `Acquisition.cs`, `Scans.cs`, `MsScan.cs`,
`Peripherals.cs`) only caught the latter. `GrpcInstrumentAccess.PumpServiceEventsAsync` is the one
that mattered: it's `await`ed directly inside `DisposeAsync()`, which ScanSpy's `buttonConnect_Click`
(`async void`) awaited with no try/catch, so the escaping exception crashed the app on every
disconnect. Fixed by also catching `RpcException` where `StatusCode == Cancelled` in all six pump
loops, not just the one that was provably crashing -- the other five had the identical latent bug,
just not yet wired to something that awaited them.

**Spectrum plot decluttered.** Compared against Corona's ScottPlot spectrum view (read-only
reference, no changes made there) -- turned out Corona uses essentially the same centroid
zero/peak/zero triple-point trick ScanSpy already had, so nothing to port. Instead: removed
per-vertex markers (`MarkerSize` 1 -> 0), which were leaving a haze of dots along the baseline for
centroid data -- most of what "doesn't represent centroid data well" was actually about; added MS-
level-based trace coloring (MS1/MS2/MS3); fixed a real dropped-value bug where `ProcessScanHeader`
computed the scan filter string but `lblScanFilter.Text` was never assigned it.

**Optional LandmineUI-themed window added.** `ScanSpy` conditionally derives from
`LandmineUI.WinForms.SharpWindow` (frameless, custom-drawn title bar) instead of plain `Form`,
gated behind a `USE_LANDMINE_UI` compile symbol that only turns on when a gitignored
`ScanSpy/LandmineUI.local.props` marker file exists, paired with a gitignored repo-root
`NuGet.Config` pointing at the local LandmineUI feed (`D:\Software\NuGet\LandmineUI`). Neither file
is tracked, so a fresh clone builds/runs the plain WinForms `Form` with zero extra setup; usage
documented in `ScanSpy/README-LandmineUI.md`.

First integration pass wrapped only the window chrome and left every interior control stock,
producing black-on-black text (`SharpWindow.ContentArea` themes `BackColor` but not `ForeColor` --
confirmed via a throwaway reflection probe against the compiled DLL, since the ambient-inheritance
default meant already-dark-background stock children kept default-black text), mismatched
buttons/toolbar/tabs, and an unthemed plot. Fixed by swapping `buttonConnect`/`buttonListen` ->
`SharpButton`, `cbOnAcquisition` -> `SharpCheckBox`, `splitContainer1/2/3` -> `SharpSplitContainer`,
`statusStrip1` -> `SharpStatusBar` (via new `SetStatusLeft`/`SetStatusRight` helpers, since
`SharpStatusBar` exposes flat `LeftText`/`RightText` strings rather than `ToolStripStatusLabel`
items), `tabControl1` -> `SharpTabControl` (`AddTab(title, tag) -> Panel` instead of `TabPages`),
`rtbLog`/`rtbHeader` -> `SharpTextArea` (`Log()`'s `AppendText` call became `Text +=`, since
`SharpTextArea` has no append API); set `ContentArea.ForeColor` explicitly from the active theme;
hand-matched ScottPlot's `FigureBackground`/`DataBackground`/axis colors to the theme (ScottPlot has
no LandmineUI awareness at all, so this can never be automatic).

The three `GroupBox`es (`groupBox1/2/3`) were initially left as stock -- no LandmineUI equivalent
existed, and hand-composing one via `SharpCard` + a manual header label risked breaking the
Designer.cs pixel layout GroupBox's automatic caption-space reservation had been calibrated
against. Filed as an upstream wish (`D:\Software\Claude\Wishes\LandmineUI\groupbox-equivalent.md`,
per that repo's documented external-wish workflow) rather than worked around locally. Granted same
day: `LandmineUI.WinForms` v1.1.0 shipped `SharpGroupBox : Panel` (content goes straight to
`.Controls`, no nested content panel, specifically so `GroupBox` -> `SharpGroupBox` is a clean
class-name swap with no `Location` rework). Bumped the package reference to 1.1.0 and completed the
swap (`.Text` -> `.HeaderText`, everything else unchanged). Wish file deleted after user confirmed
the result visually, per that workflow's own rule that only the requester closes a wish.

**Verified**: both build paths (`LandmineUI.local.props` present and absent) compile clean with 0
errors on every pass across this work. User confirmed the LandmineUI build visually, including
after the final `SharpGroupBox` swap -- "looks good, works well."

---

## 2026-08-14 -- Core8Speed branch: collapsed the host's double-copy of scan data

**Status: done, confirmed live on the real Fusion instrument -- the fix holds.** New branch
(`Core8Speed`, off `Core8`) opened specifically for further performance work after the centroid-
exception fix (previous entry) resolved the user's reported real-Fusion falling-behind symptom.
Continuing the bottleneck review: found that every scan's centroid arrays and all four dictionaries
(`Header`/`Trailer`/`StatusLog`/`TuneData`) were being built twice on the host -- once into
`Instruments.MsScanSnapshot`/`CentroidBlock` (a host-local DTO that exists purely so
`IInstrumentGateway` stays proto-free), then copied again into the proto `MsScanData` message in
`Services/ScanStreamServiceImpl.ToProto`. Not the exception-storm class of bug (no exceptions
involved, just allocation/copy work), but real, unavoidable-looking GC pressure on the
highest-frequency path in the system, paid twice per scan.

Presented as an explicit architecture tradeoff before touching anything: collapsing it means
`IMsScanChannel` (part of `IInstrumentGateway`'s public surface) has to carry `Contracts.MsScanData`
directly instead of a host-local DTO, breaking this namespace's otherwise-consistent "no proto type
crosses this boundary" rule for that one interface member. User's explicit direction: "make the
tradeoff... skip the intermediate steps."

Implemented:
- `Models.cs`: removed `MsScanSnapshot`/`CentroidBlock` entirely; `MsScanEventArgs` now carries
  `Contracts.MsScanData` directly.
- `IInstrumentGateway.cs`: `IMsScanChannel.GetLastMsScan()` returns `Contracts.MsScanData?`.
- `HeliosInstrumentGateway.cs`: `HeliosMsScanChannelAdapter.ToSnapshot` renamed to `ToProto`, now
  builds `Contracts.MsScanData`/`Contracts.CentroidBlock` directly from raw `IMsScan` data in a
  single pass -- `RepeatedField<T>.Capacity` pre-sized per field (same benefit the old fixed-size
  arrays gave for free, without needing them as a separate allocation), `.Add()`'d per peak; Header/
  Trailer/StatusLog/TuneData written straight into `MapField<string,string>` via the same
  `CopyInto`/`ResolveCanonicalTerms` logic, just retargeted. The centroid-flag fast path from the
  previous entry carried over unchanged in spirit, with one real correctness fix along the way:
  `RepeatedField<T>` (unlike a fixed-size `bool[]`) doesn't auto-default unwritten slots to `false`,
  so skipping the four flag adds entirely once known-unsupported would have left those fields
  shorter than `Mz`/`Intensity`/etc., breaking the parallel-arrays wire contract -- a `wroteFlags`
  flag now ensures exactly one entry (real or `false`) is always added per field per peak.
- `SimulatedInstrumentGateway.cs`: `EmitScan`/`SimulatedMsScanChannel` retargeted to build
  `Contracts.MsScanData`/`CentroidBlock` directly too (kept close to its original structure --
  Simulated isn't the hot path being optimized, just needs to satisfy the same interface).
- `Services/ScanStreamServiceImpl.cs`: collapsed to a pure pass-through (no `ToProto`/`CopyInto` of
  its own left at all) now that `IMsScanChannel` already hands back wire-ready data.

Deliberately NOT changed: `Helios.Client`'s own public surface and its proto-to-client-DTO copy
(`Mapping.cs`) -- a separate, consumer-facing API-contract boundary, not part of what was flagged or
agreed to here. Also not changed per explicit instruction: the host's scan-stream channel stays
unbounded with no drop policy (user: memory isn't growing at normal instrument speeds anymore, and
"even if it were, do not skip scans... not ready to throw away data"), and `GrpcStreaming.PumpAsync`'s
`SingleWriter: false` setting (left alone for now).

**Verified**: `Helios.Bridge.Host.csproj` builds clean on the first attempt (0 warnings/errors) --
the `Contracts`-alias-vs-unqualified-`using` collision risk (this namespace already defines its own
`SystemMode`/`InstrumentState`, which would collide with `Contracts.SystemMode`/`InstrumentState`
under a blanket `using Helios.Bridge.Contracts;`) was caught during planning and avoided via
`using Contracts = Helios.Bridge.Contracts;` specifically in the `Instruments` namespace's files
(`Services/*.cs` already used the unqualified form safely, since that namespace has no colliding
names). Full solution builds clean (0 errors). Regression-tested live: connected via
`Helios.Client.Demo` against local VMS/Corona (control-plane operations clean, no exceptions;
VirtualMS wasn't actively streaming scans during the test). Then temporarily switched the deployed
(not source) `App.config` to `Simulated` at its existing `SimulatedScanIntervalMs=1` (1000 scans/sec)
stress-test setting and ran the same demo: sustained ~1000 scans/sec with 0.5-2.7ms latency,
matching the original pre-refactor baseline (~1000 scans/sec, ~1.3ms avg) from when this backend was
first verified -- header/centroid data confirmed flowing correctly end-to-end (the demo's own
scans/sec and latency figures are computed from real header timestamps and arrival times, so a
wrong/malformed `Header` or `Centroids` would have shown up as broken output, not just a crash).
Deployed config reverted to match source (`Auto`) via a clean rebuild afterward. User then confirmed
live on the real Fusion instrument: the fix holds, no regression from the double-copy collapse.

---

## 2026-08-14 -- Host: stopped throwing a .NET exception per centroid peak on Fusion (falling-behind bug, part 2 of 2, host side)

**Status: fixed, confirmed live against real Fusion hardware -- Helios.Bridge.Host now keeps up with
the instrument's real scan rate.** Continuation of the ScanSpy falling-behind investigation -- user
confirmed via
Task Manager that `Helios.Bridge.Host.exe`'s memory climbs steadily during a long run against a real
(Fusion) instrument, separately from the ScanSpy UI-thread fix in the previous entry. User is
testing against a Fusion specifically; per the comments already in this file (Exploris has its own
`Centroid` implementation, not the generic stub), this fix is Fusion/VMS-specific and doesn't touch
the Exploris path at all.

User proposed a structural theory (Header/Trailer/StatusLog/TuneData copying plus canonical-term
resolution plus "thousands of property marshals per MS2" for the four centroid flags) attributing
the host-side cost to real IAPI marshaling that's simply absent in Simulated. Reviewed and largely
agreed, with two corrections: (1) Header/Trailer + canonical-term resolution is cheap -- already-
materialized local dictionaries, translated via Helios's own internal `HeliosDictionary`, no IAPI
round-trip -- so it was dropped from the theory; (2) crucially, the centroid-flag cost isn't
"marshals" at all. `HeliosMsScanChannelAdapter.ToSnapshot`'s per-peak try/catch (added earlier this
session, see the 2026-08-13 VMS/Corona entries) reads `IsExceptional` first; Helios.dll's generic
`Centroid` class (used by Fusion and VMS -- Exploris has its own) stubs it with `throw new
NotImplementedException()`, so every single peak was throwing and catching a real .NET exception --
`IsFragmented`/`IsMerged`/`IsReferenced` never even executed, short-circuited by the throw. .NET
exceptions are dramatically more expensive than a marshal call (stack capture/unwind), so at real
Fusion centroid counts (thousands per scan) and real scan rates, this alone was plausibly enough to
throttle `ToSnapshot` below real-time on its own -- the single most severe item found, not just a
compounding one. Also corrected: this isn't an MS2-specific cost, it scales with centroid count,
which a full-profile MS1 can have as much of as any MS2.

Fixed: added a `static bool? _centroidFlagsSupported` cache to `HeliosMsScanChannelAdapter`, probed
once (on the very first peak that reads these properties) and shared across every channel adapter
instance for the process's lifetime (safe, since one `Helios.Bridge.Host` process talks to one
instrument family for its whole life -- see `Program.cs`'s `CreateGateway`). Once known unsupported,
every later peak skips the try/catch (and the exception behind it) entirely instead of paying for it
on every single peak of every single scan -- turns O(centroids x scans) exceptions into O(1) for the
process's whole lifetime. Deliberately left unsynchronized (a race during the first few overlapping
calls just means a handful of redundant probes, not a correctness bug).

**Verified**: `Helios.Bridge.Host.csproj` builds clean, and the full solution builds clean (0
errors). Regression-tested the modified path via `Helios.Client.Demo` against the local VMS/Corona
backend (which also hits the generic `Centroid` stub, so exercises the same fast-path logic) --
connects, submits a custom scan, toggles acquisition, no regression. User then confirmed live against
the real Fusion instrument: `Helios.Bridge.Host` now keeps up with the instrument's real scan rate,
resolving the falling-behind symptom this and the previous entry were chasing together. The host-side
unbounded channel (`GrpcStreaming.PumpAsync`) remains a separate, deferred issue per the user's
explicit instruction -- not addressed in this entry, and evidently not the dominant factor after all
given the centroid-exception fix alone resolved the symptom.

---

## 2026-08-14 -- ScanSpy: stopped queuing a UI update for every scan (falling-behind bug, part 1 of 2)

**Status: done, builds clean.** User reported `ScanSpy` progressively falling further behind the
live scan stream from a real instrument the longer it ran. Code-review diagnosis (no access to the
instrument PC, so no live profiling) identified two independent contributors: `ScanSpy`'s own
UI-thread queuing, and unbounded buffering in `Helios.Bridge.Host`'s gRPC streaming (`GrpcStreaming.
PumpAsync`'s `Channel.CreateUnbounded<T>`). User confirmed via Task Manager that `Helios.Bridge.
Host.exe`'s memory does climb over a long run, confirming the second issue is real -- deferred for
now, to be addressed separately. This entry covers only the `ScanSpy` fix.

Root cause: `MsScanArrived` ([ScanSpy.cs](../ScanSpy/ScanSpy.cs)) called `UiInvoke` (->
`Control.BeginInvoke`) for *every* scan, not just the ones passing the existing `refreshNow`
throttle (~10Hz) that already gated the plot redraw. The invoked block unconditionally set
`labelScanSpeed.Text` and called `RefreshStats()` (-> `labelStats.Text`) regardless of `refreshNow`.
`BeginInvoke` doesn't block the calling (background, gRPC-stream-reading) thread, but it queues onto
the UI thread's own message loop, which is exactly as unbounded and un-droppable as every other hop
in this pipeline -- at real instrument scan rates well above 10Hz (e.g. DDA with many MS2 events per
MS1), invoking on every scan queued UI work faster than the UI thread could drain it, so every
displayed value (including the Hz readout meant to show the *current* rate) fell progressively
further behind rather than settling at the intended ~10Hz refresh rate.

Fix: moved the entire `UiInvoke(...)` call inside the existing `if (refreshNow)` block, so a UI
update is only queued for scans that pass the throttle -- matching what the throttle was clearly
already meant to gate.

**Verified**: `ScanSpy.csproj` builds clean, 0 errors. Not live-tested against a real instrument
(none available in this environment) or visually verified (WinForms UI, no way to see it render from
here) -- a straightforward code-motion change (no logic altered, just what's gated by the existing
`refreshNow` check), reviewed carefully rather than run. User will verify on the instrument PC.

---

## 2026-08-14 -- Fixed a real-hardware connect race and made Connect failures diagnosable

**Status: fixed, regression-tested against VMS/Corona; pending user retest on real Fusion/Exploris
hardware.** User reported `ScanSpy` failing to connect through the bridge on a separate PC with a
real instrument attached, with only `Status(StatusCode="Unknown", Detail="Exception was thrown by
handler.")` to go on -- no access to that machine's console/log to see the real exception. Diagnosed
remotely from the error message and code alone (the specific gRPC status itself confirmed
auto-launch/discovery worked correctly -- the RPC reached the host's handler and started executing).

Root cause: `HeliosInstrumentGateway.ConnectAsync` called `_container.Get(1)` immediately after
`_container.StartOnlineAccess()`, with no wait for the service to actually come online first. Real
Fusion/Exploris hardware brings the service online asynchronously after `StartOnlineAccess()`;
Simulated and VMS/Corona don't have this handshake at all, which is why this race never surfaced
until the first real-hardware test. `CLAUDE.md` had already documented that the original in-process
pattern required waiting for `ServiceConnectionChanged` before `Get(1)` -- the bridge's port of that
pattern dropped the wait.

Fixed:
- `HeliosInstrumentGateway.ConnectAsync`/new `WaitForServiceConnectedAsync`: waits for
  `ServiceConnectionChanged` to report `ServiceConnected == true` (15s timeout) before calling
  `Get(1)`, mirroring the original three-step sequence. Also fixed an adjacent latent bug found
  while touching this method: the `_connected` guard was set *before* the connect sequence, not
  after it succeeded, so a failed connect left every future `Connect` RPC silently short-circuiting
  forever (returning as if already connected while `_access` stayed `null`) -- now resets to `0` on
  failure so a retry actually retries.
- `InstrumentServiceImpl.Connect`: logs the real exception to the host's console/log file and
  re-throws as `RpcException(StatusCode.Internal, ex.Message)` instead of letting Grpc.Core's
  generic message reach the client -- the specific message now surfaces in the calling app's own
  error log too (e.g. `ScanSpy`'s `Log("HeliosClient.ConnectAsync() " + ex.Message)`), without
  needing access to the host machine to diagnose a failed connect next time.

**Verified**: `Helios.Bridge.Host` builds clean. Regression-tested the modified connect path via
`Helios.Client.Demo` against the local VMS/Corona backend (the only real, non-Simulated backend
reachable in this environment) -- connects, submits a custom scan, toggles acquisition, all as
before; no regression. The actual race only manifests against real Fusion/Exploris hardware, which
isn't available here, so the fix itself is unverified against the reported scenario until the user
retests on the instrument PC.

---

## 2026-08-14 -- Fixed ScanInjector's fresh-clone build failure (out of Core8 scope, fixed on request)

**Status: done.** `ScanInjector` was previously documented as a known, unfixed fresh-clone build
gap (packages.config-style restore, same class of problem as `Nova`, just never worked around).
Fixed on explicit request, per the bug-fixing policy.

Tried converting `ScanInjector.csproj` to `PackageReference` first (drop OpenTK/OpenTK.GLControl,
which turned out to be unused in source, same as the already-removed reference from ScanSpy's
port; let ScottPlot.WinForms pull in SkiaSharp/HarfBuzzSharp transitively) -- abandoned after
hitting `MSB3822`/`MSB3823` (non-string `.resx` resources need
`GenerateResourceUsePreserializedResources` + a `System.Resources.Extensions` reference) and
discovering the package's own auto-wiring `.targets` file no-ops for this project: it's gated on
`$(TargetFramework)`, an SDK-style-only property a legacy `TargetFrameworkVersion`-based project
like this never sets. Reverted the csproj back to its original `packages.config`/`Reference`
+`HintPath` structure entirely (`git checkout`) rather than accumulate hand-wired `HintPath`s for
an ever-shifting transitive graph.

Actual fix: downloaded the real NuGet CLI
(`https://dist.nuget.org/win-x86-commandline/latest/nuget.exe` -- not present on this machine) and
ran `nuget.exe restore ScanInjector\packages.config -PackagesDirectory Helios\packages`, the
restore mechanism packages.config was actually designed for. All 21 packages landed in the exact
`Helios/packages/<id>.<version>/lib/...` layout the project's existing `HintPath`s already expect --
this was always just a "never restored on this clone" problem, same root cause as the `Nova`
package, not a version-compatibility one. Added `System.Resources.Extensions` to the same
`packages.config`/`HintPath` structure to clear the separate, unrelated `MSB3822`/`MSB3823`
toolchain quirk (would hit any legacy WinForms project with image/icon resx resources on this SDK
version, nothing to do with NuGet).

**Verified**: `ScanInjector.csproj` builds clean standalone, and a full `Helios.sln -c Release
-p:Platform=x64` build succeeds end to end (7 projects, 0 errors) -- including alongside the
user's own live `ScanSpy.exe`/`Helios.Bridge.Host.exe` test session, which was left untouched.
`ScanInjector.exe` itself wasn't launched/tested live, to avoid a second in-process connection
racing the user's already-connected bridge session against the same instrument/Corona.

---

## 2026-08-14 -- Auto-launched Helios.Bridge.Host runs with no console window

**Status: done, verified live.** `IdleShutdownSeconds` default lowered from 20s to 5s (`App.config`),
per request. Separately, `HeliosClient.LaunchHost` now launches the host via
`cmd.exe /c "<exe> > logfile 2>&1"` instead of running it directly, so an auto-launched host no
longer pops up a console window. Deliberately not implemented via `ProcessStartInfo.RedirectStandardOutput`:
that would require the launching client itself to keep draining the pipe for as long as the host
runs, but the host is designed to outlive whichever client happened to launch it (auto-shutdown is
driven by total connection count, not the specific caller) -- once that client exited, the host's
`Console.WriteLine` calls would eventually block on a full, undrained pipe buffer and hang. `cmd`'s
own redirection instead hands the host a plain file handle, needing no reader; output lands in
`%LocalAppData%\SchweppeLab\Helios\Helios.Bridge.Host.<port>.log`, overwritten per launch. A
manually-started host is unaffected and keeps its usual visible console.

**Live-verified**: rebuilt `Helios.Client`/`Helios.Client.Demo`, ran `Helios.Client.Demo` cold —
host auto-launched (confirmed running via `tasklist`, no window), its startup/status lines appeared
correctly in the per-port log file, and killing the client still triggered normal idle
auto-shutdown afterward.

---

## 2026-08-13 -- Helios.Bridge.Host auto-launch and self-managed idle shutdown

**Status: done, verified live.** `HeliosClient.ConnectAsync` no longer requires the caller to have
started `Helios.Bridge.Host` manually, and the host no longer needs to be manually stopped either:

- **Discovery** (`Helios.Client/BridgeHostLocator.cs`): explicit `hostExecutablePath` argument ->
  `HELIOS_BRIDGE_HOST_PATH` env var -> `HKCU\Software\SchweppeLab\Helios\BridgeHostPath` registry
  key -> `PATH` search, in that order, only consulted if nothing is already listening on
  `127.0.0.1:50100` (an already-running host, possibly another app's connection to real hardware,
  is reused rather than duplicated). Chosen over build-time output-copying after realizing most
  consumers will build against `Helios.Client` from NuGet, with `Helios.Bridge.Host` installed
  wherever an installer or a manually-unpacked zip put it -- there is no build-time-known path to
  copy in that scenario.
- **Self-registration** (`Helios.Bridge.Host/Program.cs`): writes its own exe path to that registry
  key on every successful startup (not just once, so a moved/reinstalled host self-heals the key
  without an explicit uninstall step), plus a `--register`-only mode for zip-file installs with no
  installer to do this automatically. `BridgeHostLocator` deletes a stale entry (target file gone)
  the first time it notices, rather than leaving it to keep misfiring.
- **Single-instance detection**: switched `Program.cs` from the `Ports = { new ServerPort(...) }`
  collection-initializer shorthand to a direct `server.Ports.Add(...)` call so its `int` return
  value (0 on bind failure) can be checked -- Grpc.Core doesn't throw for "address already in use".
  This is also what makes two racing auto-launches safe with no separate mutex: both host processes
  start, only one wins the bind, the loser exits immediately, and both callers' listening-poll loop
  converges on the winner.
- **Idle auto-shutdown** (`Helios.Bridge.Host/Services/ConnectionWatchdog.cs`): every
  `Helios.Client` connection keeps one `StreamServiceEvents` call open for its whole lifetime,
  which doubles as a crash-safe "client is connected" signal -- a dead client's socket closes
  immediately even without a graceful disconnect. `InstrumentServiceImpl` increments/decrements a
  counter around that call; the watchdog arms an idle timer only on a `>0` -> `0` transition (never
  on startup with zero clients) and cancels it if a new connection arrives first. Configurable via
  `App.config`'s new `IdleShutdownSeconds` (default 20s; `<=0` disables it, Ctrl+C-only as before).

**Live-verified** (killed the still-running `Helios.Bridge.Host.exe` from the prior test session
first, per user confirmation, since it held a file lock blocking the rebuild):
- Registered a built `Helios.Bridge.Host.exe` via `--register`, confirmed the registry key.
- Ran `Helios.Client.Demo` with no host running: it located the host via the registry, launched it,
  waited for it to start listening, and connected -- the host's own `Auto` probe found the
  user's already-running Corona session (VMS/"Corona VirtualMS") rather than falling back to
  Simulated, and normal operations (custom scan submit, acquisition mode toggle) worked over the
  auto-launched connection.
- Force-killed the client (simulating a crash, not a graceful `DisposeAsync`): the watchdog
  detected the dropped stream, waited out `IdleShutdownSeconds`, and the host shut itself down
  cleanly.
- Ran two `Helios.Client.Demo` instances concurrently: the second reused the first's already-running
  host (confirmed exactly one `Helios.Bridge.Host.exe` process). Killing one client left the host
  running past the idle grace period since the other was still connected; killing the second
  triggered auto-shutdown as expected.

---

## 2026-08-13 -- Documentation pass: README, CLAUDE.md, and NuGet packaging metadata

**Status: done.** With the bridge merge landed and verified live (ScanSpy <-> Corona confirmed
working end to end), brought the docs up to date with what actually exists now rather than what
was planned:

- **`README.md`** (root) — added the "Two ways to use Helios" section (in-process net48 vs. the
  gRPC bridge for net8), documented `Helios.Bridge.Host`'s `Auto`/`Real`/`Simulated`
  `InstrumentFamily` config, listed `Bridge/`'s four projects under Repository Contents, and noted
  the .NET 8 SDK requirement.
- **`CLAUDE.md`** (root) — rewritten from a forward-looking plan into a description of the
  landed architecture: a `Commands` section (build/run, the `Nova` package fresh-clone gotcha), the
  full `Bridge/` project breakdown, the `HeliosMsScanVMS`/`Centroid` gaps found while getting
  Corona scans flowing (so the next person touching VMS code doesn't rediscover them the hard way),
  `CallbackGuard`'s rationale, `ScanSpy`'s porting notes, and an updated known-gaps list.
- **`Helios.nuspec`** reviewed, left unchanged — `Helios.dll`'s public contract didn't change, so
  its packaging story hasn't either. Added NuGet metadata (`PackageId`, `Description`, license,
  etc.) directly to `Helios.Client.csproj` instead of writing it a separate `.nuspec` -- SDK-style
  projects don't need one, `dotnet pack` generates it from csproj properties. Packing isn't wired
  into any build step; this only makes `dotnet pack` produce something sensible if `Helios.Client`
  is ever published.

---

## 2026-08-13 -- Fourth live-testing fix: HeliosMsScanVMS.DetectorName is also never assigned

**Status: confirmed live.** Spectra now flow end-to-end: Corona -> Helios.dll (VMS) ->
Helios.Bridge.Host -> gRPC -> Helios.Client -> ScanSpy, including correct
AcquisitionStreamOpening/Closing across multiple runs. This closes out the four-fix chain below
(Auto fallback, StatusLog/TuneData null, Centroid stubs, DetectorName null) -- all four were
required together for real Corona scan data to reach ScanSpy at all.

Same family of bug as the StatusLog/TuneData one two entries down: `HeliosMsScanVMS`'s
constructors (Helios.dll, both the `Spectrum` and `SpectrumEx` overloads) never assign
`DetectorName`, so it's `null` for every VMS/Corona scan. Unlike a plain C# property,
`Helios.Bridge.Contracts.MsScanData.DetectorName` is a protobuf string field, which throws
`ArgumentNullException` on a null assignment rather than silently accepting it -- `CallbackGuard`
caught it (connection survived), but every scan still failed to reach the wire.

`HeliosMsScanChannelAdapter.ToSnapshot` now takes the already-known `DetectorClass` (from
`IMsScanContainer.DetectorClass`, itself hardcoded `"VMS DetectorClass"` in Helios.dll for VMS) as
a fallback and uses `scan.DetectorName ?? fallbackDetectorName` instead of passing the null
through. Same non-fix-Helios.dll-directly approach as every other gap found this session.

If there's a fifth null/stub gap in `HeliosMsScanVMS` waiting after this one, the pattern so far
suggests checking there first before looking anywhere else.

---

## 2026-08-13 -- Third live-testing fix: Helios.dll's stub Centroid properties were killing every scan

**Status: confirmed progress, superseded by the DetectorName issue above** -- this fix is
confirmed working (no more `NotImplementedException` in the log after it), but scan data still
didn't reach ScanSpy because of the separate DetectorName gap above, found immediately after.

With the previous fix's `CallbackGuard` in place, Corona's dispatch loop kept running, but the
host's console showed a `NotImplementedException` on literally every single scan:

```
System.NotImplementedException: The method or operation is not implemented.
   at Helios.Interfaces.SpectrumFormat.Centroid.get_IsExceptional()
```

This one is different from the previous two: it's **existing Helios.dll code, not this branch's**.
`Helios.Interfaces.SpectrumFormat.Centroid` -- the generic `ICentroid` implementation used by both
Fusion and VMS/Corona (Exploris has its own) -- stubs several properties with
`throw new NotImplementedException()`: `IsExceptional`, `IsReferenced`, `IsMerged`,
`IsFragmented`, `Profile`, `ChargeEnvelopeIndex`, `IsClusterTop`. `Mz`/`Intensity`/`Charge`/
`Resolution` are real (constructor-backed). `HeliosMsScanChannelAdapter.ToSnapshot`'s centroid
loop read all four boolean flags unconditionally, so every scan's conversion failed before it
could reach the wire, `CallbackGuard` just kept it from taking the connection down with it.

Per the branch's bug-reporting policy this is reported here rather than silently patched in
Helios.dll itself. Worked around on the bridge side only: the four flag reads are now wrapped in a
single try/catch defaulting all four to `false` on `NotImplementedException`, so a scan's real
data (Mz/Intensity/Charge/Resolution) still makes it through -- those four quality flags just read
as `false` for centroids sourced from this `Centroid` implementation until/unless Helios.dll's own
class is finished.

---

## 2026-08-13 -- Two live-testing-driven fixes: Auto instrument-family fallback, and a VMS/Corona crash-the-pipe null reference

**Status: both verified live against a running Corona.** Found through actual use against Corona
(not by inspection) -- exactly the kind of thing this branch's plan flagged real hardware/Corona
testing would surface.

### 1. No fallback chain when the Simulated backend was turned off

Turning off `Helios.Bridge.Host`'s Simulated backend left ScanSpy with nothing to connect to at
all. The old in-process ScanSpy never had this problem: it called
`InstrumentAccessContainerFactory.Create()` directly, which auto-probes Fusion, then Exploris,
then VMS/Corona, and returns whichever answers. That auto-probe got lost in the port -- the host's
`Program.cs` picked one gateway at startup from a static `Simulated`/`Real` config value, with no
detection and no fallback.

Fixed: `HeliosInstrumentGateway` now takes an already-probed `IInstrumentAccessContainer` instead
of calling `Create()` itself inside `ConnectAsync` -- `Program.cs` does that probe once at host
startup (cheap `Check()` calls only, no online access started, so it doesn't violate "connection
is client-driven"). A new `Auto` mode (the new default) tries Fusion/Exploris/VMS first and falls
back to `SimulatedInstrumentGateway` only if none answered, restoring the original in-process
behavior. `Real` still exists for forcing hardware-only testing (fails loudly instead of silently
falling back); `Simulated` still forces the synthetic generator. Also fixed a mislabeling bug
found in the same code: a VMS/Corona connection was being reported as `InstrumentFamily =
"Simulated"` (copy-paste artifact from before the two backends were distinct) -- now reports
`"VMS"`.

### 2. VMS/Corona scans never reached ScanSpy, and every event after the first scan went dark

Symptom: `AcquisitionStreamOpening` logged fine, but no scan data ever arrived, and neither
`AcquisitionStreamClosing` nor the next run's `AcquisitionStreamOpening` fired either.

Root cause: `HeliosMsScanVMS`'s constructor (Helios.dll, VMS-specific) never assigns `StatusLog`
or `TuneData` -- both stay `null` for every scan Corona sends (only `Header`/`Trailer` get set;
Exploris/Fusion always wrap a real, if possibly-empty, source for all four).
`HeliosMsScanChannelAdapter.ToSnapshot` (written earlier this branch) called
`ToDictionary(scan.StatusLog)` unconditionally with no null check, so it threw a
`NullReferenceException` on literally the first scan -- synchronously, inside Corona's own
pipe-message dispatch chain (`PipesClient` read loop -> `HeliosMsScanContainerVMS.ReceiveScan` ->
`MsScanArrived` -> this method). That silently killed the dispatch loop for the rest of the
connection, which is why nothing after the first scan -- not just scan data -- worked anymore.

Fixed in two parts:
- `ToDictionary` now null-checks its `IInformationSourceAccess` argument.
- Every Helios/IAPI event subscription in `HeliosInstrumentGateway.cs` now runs through a new
  `CallbackGuard.Run(name, action)` wrapper (try/catch + `Console.Error`) rather than invoking the
  forwarded event directly. This isn't just for the bug just found -- for VMS specifically, an
  unhandled exception in *any* mapping callback kills Corona's entire pipe dispatch, not just that
  one event, so a single unnoticed bug anywhere in this file could silently end a whole session.
  Now it logs and the connection keeps running.

---

## 2026-08-13 -- ScanSpy ported to Core 8; a real bridge gap found and fixed along the way

**Status: builds clean, launches and runs live against Helios.Bridge.Host's Simulated backend
without crashing.** Full interactive click-through (Connect, Activate, watching the plot update)
was left to manual testing rather than claimed here -- there's no way to drive a WinForms UI or
capture what it renders from this session, and the request for a screenshot was declined in favor
of hands-on testing.

- `ScanSpy.csproj` retargeted in place from a net48 old-style project to SDK-style
  `net8.0-windows` (`UseWindowsForms`, `PackageReference` for `ScottPlot.WinForms` instead of the
  old packages.config + manually-imported native-asset `.targets` files). `ProjectReference`
  swapped from `Helios.csproj` to `Bridge/Helios.Client/Helios.Client.csproj`. Removed now-unused
  net48-only files (`Properties/AssemblyInfo.cs`, `Resources.*`, `Settings.*`, `packages.config`,
  `OpenTK.dll.config`, `App.config` -- none had real content beyond boilerplate; confirmed by
  grep before deleting).
- `ScanSpy.cs` rewritten against `Helios.Client`'s interfaces: the old two-step
  `InstrumentAccessContainerFactory.Create()` -> `ServiceConnectionChanged` -> `Get(1)` connect
  flow collapsed into a single `await HeliosClient.ConnectAsync()` (the client's factory doesn't
  return until the instrument is actually reachable). `IMsScan.TryHeader`/`TryTrailer` calls
  became `Header`/`Trailer` dictionary lookups (see the gap below for why that's safe). Centroid
  iteration changed from `foreach (ICentroid c in scan.Centroids)` to indexing the columnar
  `CentroidBlock` arrays directly. Event handlers now marshal onto the UI thread via `Invoke`/
  `BeginInvoke` where they touch controls -- `Helios.Client`'s events always fire from a background
  `Task` draining a gRPC stream, same as the direct-IAPI events ScanSpy used to subscribe to did,
  but that made the pre-existing unguarded UI touches (e.g. `labelScanSpeed.Text` set with no
  `Invoke` at all) a real risk for a live test rather than something to carry over unfixed.
- `ScanSpy.Designer.cs` and `ScanSpy.resx` needed no changes -- ordinary WinForms controls, TFM-
  agnostic.
- `AcquisitionErrorsArrived` isn't in `Helios.Client`'s `IInstrumentAccess` (no wire support for it
  in `Helios.Bridge.Contracts` -- ProjectA's proto never carried it either), so that subscription
  and handler were dropped rather than ported.

### Real gap found and fixed (in code from this branch, not pre-existing Helios/ScanSpy code)

`HeliosMsScanChannelAdapter` (written earlier this branch) was raw-copying `IMsScan.Header`/
`Trailer` into the wire snapshot. That's not equivalent to what `IMsScan.TryHeader`/`TryTrailer`
give a caller in-process: Helios's internal `HeliosDictionary` resolves a family-independent
canonical ID (e.g. `"FirstMass"`) to whichever raw key spelling the connected instrument family
actually uses (Exploris spells it `"LowMass"`; Fusion and the canonical form both say
`"FirstMass"`). A raw copy loses that resolution -- `ScanSpy.MsScanArrived`'s
`TryHeader("FirstMass", ...)` calls would have silently returned nothing on real Exploris
hardware, and returned nothing on this branch's own Simulated backend regardless of family, since
the simulator's synthetic `Header` never had `"StartTime"`/`"FirstMass"`/`"BasePeakIntensity"`/
etc. at all -- confirmed by tracing through what ScanSpy's port would actually receive before
writing a single line of its UI code, not discovered by trial and error live.

Fixed in `HeliosInstrumentGateway.cs`: `HeliosMsScanChannelAdapter.ToSnapshot` now also resolves a
hardcoded list of Helios's known canonical header/trailer IDs (reproduced from
`HeliosDictionary`'s static constructor -- the class itself is `internal`, not reachable from
`Helios.Bridge.Host`) via `scan.TryHeader`/`TryTrailer`, merging them into the wire dictionaries
under their canonical name alongside the raw copy. `SimulatedInstrumentGateway.EmitScan` also
gained a realistic set of canonical header values (`StartTime`, `FirstMass`/`LastMass`,
`BasePeakIntensity`, `ScanData`, `MassAnalyzer`, `Polarity`, `ScanMode`, `IonizationMode`,
`InjectTime`, `TIC`) so a live test against the Simulated backend actually exercises this path
instead of silently no-op'ing the way the original bare `{Scan, MSOrder}` header would have.

---

## 2026-08-13 -- Bridge merge: Contracts, Host, Client, Demo all landed and verified end-to-end

**Status: done and verified live** (all four new projects build clean against real Helios.dll and
real IAPI DLLs; a live host+demo run over the Simulated backend streamed ~1000 scans/sec at
~1.3ms average latency, toggled acquisition mode, and submitted a custom scan successfully).

Brought ProjectA's gRPC bridge prototype (`../ProjectA`) into this repo as `Bridge/`, per the plan
recorded in this repo's `CLAUDE.md`. Rather than a mechanical copy-and-rename, the host-side
Fusion/Exploris gateway was rewritten from scratch to wrap `Helios.dll`'s own public interfaces
(`InstrumentAccessContainerFactory`, `IInstrumentAccess`, `IControl`, `IAcquisition`, `IScans`,
...) instead of re-implementing IAPI access a second time -- everything else (proto contracts, the
gRPC service layer, the Simulated backend, streaming plumbing) ported with only namespace renames,
since none of it duplicated anything Helios already had.

### Components delivered

- **`Bridge/Helios.Bridge.Contracts`** (netstandard2.0) -- the `.proto` contract, ported from
  ProjectA.Contracts with only the package/namespace renamed (`projecta.v1` ->
  `helios.bridge.v1`, `ProjectA.Contracts` -> `Helios.Bridge.Contracts`). Wire shapes (columnar
  `CentroidBlock`, the five services) unchanged.
- **`Bridge/Helios.Bridge.Host`** (net48 console) -- the gRPC server.
  - `Instruments/HeliosInstrumentGateway.cs` -- new. Wraps `Helios.dll`'s real interfaces for
    Fusion/Exploris; one adapter suffices where ProjectA needed two (`FusionInstrumentGateway`,
    `ExplorisInstrumentGateway`) because Helios's interfaces already unify the two families
    (single `SystemMode`/`InstrumentState` enums, not one per family). No `extern alias` needed in
    this project at all -- only Helios.dll's own public surface is touched.
  - `Instruments/IInstrumentGateway.cs`, `Instruments/Models.cs`, `Instruments/Simulated/*`,
    `Services/*`, `Program.cs`, `App.config` -- ported from ProjectA with namespace renames only.
  - `InstrumentFamily` (Fusion/Exploris) is no longer an explicit `App.config` choice for real
    hardware: `Helios.Interfaces.InstrumentAccessContainerFactory.Create()` auto-probes (tries
    Fusion, then Exploris, then VMS) and there's no way to force a family from outside Helios.dll
    (its per-family container classes are `internal`, no `InternalsVisibleTo`). `App.config`'s
    `InstrumentFamily` now only chooses Simulated vs. Real.
- **`Bridge/Helios.Client`** (net8 library) -- the .NET 8-facing API, redesigned to mirror
  Helios's own interface names/shapes (`IInstrumentAccess`, `IControl`, `IAcquisition`, `IScans`,
  `IMsScan`, `ISyringePumpControl`, `IAnalogTraceContainer`) per the branch's design goal, rather
  than ProjectA.Client's own idiomatic-but-independent DTO shape. Three disclosed, deliberate
  divergences from a byte-for-byte mirror -- see the doc comment at the top of `Interfaces.cs`:
  synchronous properties/events stay synchronous (cached, kept current by a background gRPC
  stream); genuine round-trip calls are `Task`-returning instead of blocking; `IMsScan.Centroids`
  stays the columnar `CentroidBlock`, not `IEnumerable<ICentroid>`.
  `IControl.SyringePumpControl` is `null` for Exploris, matching Helios.dll's own nullability
  exactly (not ProjectA's always-non-null-plus-a-flag-to-check shape).
- **`Bridge/Helios.Client.Demo`** (net8 console) -- ported from ProjectA.Demo onto the new client
  API; same latency-tracking scan stream, custom scan submission, and acquisition toggling demo.
- **`Helios.sln`** -- gained a `Bridge` solution folder containing all four new projects.
- **`Helios.csproj`** itself -- fixed stale IAPI `HintPath`s (were pointing at a
  `D:\Software\Other\IAPI` that doesn't exist on this machine; now point at
  `D:\Software\Claude\iapi`, redirected per go-ahead). One pre-existing extra `..\` typo on the
  `Fusion.API-2.0` reference (inconsistent depth vs. every other IAPI reference in the same file)
  fixed at the same time, since it was the exact line already being edited.

### Environment setup notes (not code changes)

- This fresh clone had no restored `Nova` NuGet package (`packages.config`-style; `dotnet
  restore`/`dotnet build` don't drive that restore path). Extracted `Nova.1.0.0.18.nupkg` from the
  local feed at `D:\Software\SchweppeLab\NuGet` directly into `Helios/packages/` to unblock the
  build.
- `ScanSpy`/`ScanInjector` still fail to build in this environment (missing `HarfBuzzSharp`/
  ScottPlot native-asset packages, restored the same packages.config way) -- pre-existing, present
  before this branch's changes, and not fixed here since neither project was in scope for this
  pass (ScanSpy's Core 8 port is separately planned; ScanInjector is intentionally untouched).

### Confirmed gap, reported not silently fixed

`Helios.Interfaces.InstrumentAccess.Control.InstrumentValues.HeliosInstrumentValues.Get(string)`
and `Get(ulong)` are both stubbed to `return null;` in current `Helios.dll`, for both Exploris and
Fusion -- reading instrument values by name isn't possible through Helios's public surface at all
today. `HeliosInstrumentGateway.GetInstrumentValues` and `Helios.Client`'s
`GetInstrumentValuesAsync` both return an empty dictionary and say why in a code comment, rather
than working around it further. Fixing `HeliosInstrumentValues` itself is out of scope for this
pass pending direction.

### Known gaps carried over from ProjectA (still open, not blocking)

- `IMsScan.NoiseBand` isn't on the wire (centroids only) -- unlike ProjectA's own gap list, this
  one is now provably just a proto-contract limitation: Helios's real `ISpectrum.NoiseBand`
  exists and has data, it's simply not yet in `scans.proto`/`CentroidBlock`.
- No automated tests yet exercise `HeliosInstrumentGateway` against real Fusion/Exploris hardware
  (compiles clean against the real IAPI DLLs, type-checked, but unverified live -- same caveat
  ProjectA itself carried).
- Helios.dll's own `IControl.GetScans(bool exclusiveAccess)` does support exclusive re-acquisition
  (unlike the raw-IAPI gateway this replaced), but `HeliosScanControlAdapter`/`GrpcScans` don't
  wire a re-acquire path through yet -- `GetPossibleScanParametersRequest.ExclusiveAccess` is
  still not honored end-to-end.

---

## 2026-08-13 -- Branch created

`Core8` branch created off `main` to add a Framework 4.8 <-> .NET 8 communication layer to Helios,
per the plan recorded in this repo's `CLAUDE.md`.

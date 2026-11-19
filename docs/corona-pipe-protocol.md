# Corona pipe protocol (the VMS backend)

Developer reference for the named-pipe contract between Helios's VMS backend and Corona. Both
sides must change together; keep this file current with any protocol change.

Helios's VMS backend is a `Nova.IPC.Pipes.PipesClient` to Corona, which runs a `PipesServer`
named `Corona`. Helios selects this backend only when a process named `Corona` is running. The
backend presents the same `IInstrumentAccessContainer` / `IInstrumentAccess` / `IAcquisition`
surface as the IAPI families, with this mapping:

- `ServiceConnected` is the pipe link: true once the pipe connects (with
  `ServiceConnectionChanged`), false when it drops, true again when Nova reconnects.
- `IAcquisition.State`, `CanPause` and `CanResume` are what Corona last reported (message `6`).
  A pipe drop sets Mode=`Disconnected`, State=`ServerFailed`; a reconnect leaves that until
  Corona reports again, which Corona does on every client connect.
- `Connected` and `ConnectionChanged` follow the mode: false while Mode is `Disconnected`.
- Corona builds that never send message `6`: Helios assumes On/ReadyForRun on connect, Running
  on message `2` and ReadyForRun on message `3`.

## Messages

Messages are `PipeMessage`s (`MsgCode` plus `MsgData`). Text payloads are
`PipeMessage.EncodeString` text holding `Key=Value` lines (newline-separated, the first `=`
splits, values must not contain a newline). Set `MsgCode` after `EncodeString`, which resets it
to `'0'`.

Corona to Helios:

| Code | Meaning | Payload |
|------|---------|---------|
| `0` | Text | free text, written to the console |
| `1` | Scan | serialized spectrum (see `HeliosMsScanContainerVMS.ReceiveScan`) |
| `2` | Acquisition stream opening | raw file name |
| `3` | Acquisition stream closing | ignored |
| `4` | Scan, extended form | serialized spectrum (see `HeliosMsScanContainerVMS.ReceiveScanEx`) |
| `5` | The next custom scan can be accepted | ignored |
| `6` | State report | `Mode=<SystemMode name>`, `State=<InstrumentState name>`, `CanPause=<bool>`, `CanResume=<bool>`; a key left out leaves that value unchanged |

Helios to Corona:

| Code | Meaning | Payload |
|------|---------|---------|
| `A` | Custom scan | binary: `Int32` count, then (`string` key, `string` value) pairs, `BinaryWriter` with UTF-16 |
| `M` | Set mode | `Mode=On` or `Mode=Off` |
| `S` | Start acquisition | `Kind=Permanent`, `LimitedByCount`, `LimitedByTime` or `MethodRun`; `RawFileName`, `SampleName`, `Comment`, `Continuation`, `SingleProcessingDelay`, `WaitForContactClosure`; by kind also `NumberOfScans`, `Duration` (`TimeSpan` "c" format), `MethodName` |
| `C` | Cancel acquisition | empty |

Mode and state names are those of the IAPI `SystemMode` and `InstrumentState` enumerations
(Helios mirrors them in `Helios.Interfaces.InstrumentAccess.Control.Acquisition`), matched
without regard to case.

## Where it lives in Helios

- `Helios/Interfaces/InstrumentAccess/IInstrumentAccessContainer.cs`:
  `HeliosInstrumentAccessContainerVMS` (pipe client, link watcher, message dispatch).
- `Helios/Interfaces/InstrumentAccess/Control/Acquisition/IAcquisition.cs`:
  `HeliosAcquisitionVMS` (state report parsing, outbound `M`/`S`/`C`).
- `Helios/Interfaces/InstrumentAccess/Control/Scans/IScans.cs`: `HeliosScansVMS` (outbound `A`).
- `Helios/Interfaces/InstrumentAccess/MsScanContainer/IMsScanContainer.cs`:
  `HeliosMsScanContainerVMS` (inbound `1`/`4`).

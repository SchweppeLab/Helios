using System;
using System.IO;
using Microsoft.Win32;

namespace Helios.Client
{
  // Finds an installed Helios.Bridge.Host.exe so HeliosClient.ConnectAsync can launch it when it
  // isn't already running. Consumers reach this three different ways -- built from this repo
  // (dev), a NuGet-only app pointed at a Helios.Bridge.Host an installer placed somewhere, or a
  // NuGet-only app pointed at a zip-file install the user unpacked wherever they liked -- so there
  // is no single build-time-known location; this has to be resolved at runtime instead.
  //
  // Checked in order, first match wins:
  //   1. hostExecutablePath passed to ConnectAsync
  //   2. HELIOS_BRIDGE_HOST_PATH environment variable
  //   3. HKCU\Software\SchweppeLab\Helios\BridgeHostPath -- written by Helios.Bridge.Host itself
  //      on every successful startup (see Program.RegisterInstallLocation), or by an installer, or
  //      by a one-time `Helios.Bridge.Host.exe --register` for zip-file installs. Self-healing: if
  //      the registered file no longer exists (moved/uninstalled without re-registering), the
  //      stale entry is deleted here rather than left to keep misfiring.
  //   4. Helios.Bridge.Host.exe found on PATH
  internal static class BridgeHostLocator
  {
    private const string ExeName = "Helios.Bridge.Host.exe";
    private const string EnvVarName = "HELIOS_BRIDGE_HOST_PATH";
    private const string RegistrySubKey = @"Software\SchweppeLab\Helios";
    private const string RegistryValueName = "BridgeHostPath";

    public static string Locate(string? explicitPath)
    {
      if (TryUseExplicit(explicitPath, "hostExecutablePath", out var fromArg)) return fromArg;

      var envPath = Environment.GetEnvironmentVariable(EnvVarName);
      if (TryUseExplicit(envPath, $"the {EnvVarName} environment variable", out var fromEnv)) return fromEnv;

      if (TryUseRegistry(out var fromRegistry)) return fromRegistry;

      if (TryUsePath(out var fromPath)) return fromPath;

      throw new FileNotFoundException(
        $"Could not locate {ExeName} to auto-launch it. Pass hostExecutablePath to ConnectAsync, set the " +
        $"{EnvVarName} environment variable, register its location (run '{ExeName} --register' once), add " +
        $"its folder to PATH, or start it manually before connecting.");
    }

    // A caller-supplied path (explicit param or env var) is trusted enough to be an error, not a
    // silent fallthrough, if it points at nothing -- unlike registry/PATH entries this repo didn't
    // write itself, a wrong explicit path is almost certainly a typo worth surfacing immediately.
    private static bool TryUseExplicit(string? path, string source, out string result)
    {
      result = string.Empty;
      if (string.IsNullOrWhiteSpace(path)) return false;
      if (!File.Exists(path)) throw new FileNotFoundException($"{source} points to '{path}', but no file exists there.");
      result = path;
      return true;
    }

    private static bool TryUseRegistry(out string result)
    {
      result = string.Empty;
      // Helios.Bridge.Host is net48/Windows-only, so this path never matters off Windows --
      // guarding on IsWindows() (rather than a Windows-only TargetFramework or a try/catch for
      // PlatformNotSupportedException) keeps Helios.Client itself a plain, portable net8.0 library.
      if (!OperatingSystem.IsWindows()) return false;

      using var key = Registry.CurrentUser.OpenSubKey(RegistrySubKey, writable: true);
      if (key?.GetValue(RegistryValueName) is not string value || string.IsNullOrWhiteSpace(value)) return false;

      if (!File.Exists(value))
      {
        try { key.DeleteValue(RegistryValueName, throwOnMissingValue: false); } catch { /* best effort */ }
        return false;
      }

      result = value;
      return true;
    }

    private static bool TryUsePath(out string result)
    {
      result = string.Empty;
      var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
      foreach (var dir in pathVar.Split(Path.PathSeparator))
      {
        if (string.IsNullOrWhiteSpace(dir)) continue;

        string candidate;
        try { candidate = Path.Combine(dir, ExeName); }
        catch (ArgumentException) { continue; } // malformed PATH entry -- skip rather than fail discovery outright.

        if (File.Exists(candidate)) { result = candidate; return true; }
      }
      return false;
    }
  }
}

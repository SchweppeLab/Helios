// .NET Framework 4.8 doesn't ship this marker type, but the C# 9+ compiler requires it to be
// present in order to compile `init`-only property accessors. This is the standard shim.
namespace System.Runtime.CompilerServices
{
  internal static class IsExternalInit
  {
  }
}

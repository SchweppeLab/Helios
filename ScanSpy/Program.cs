using System;
using System.Windows.Forms;

namespace ScanSpy
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
#if USE_LANDMINE_UI
      LandmineUI.WinForms.Theming.ThemeManager.SetTheme(LandmineUI.WinForms.Theming.NocturneTheme.Dark);
#endif
      Application.Run(new ScanSpy());
    }
  }
}

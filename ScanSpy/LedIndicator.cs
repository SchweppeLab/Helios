using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ScanSpy
{
  // A small round status light, replacing the plain colored-square Buttons the connection/listener
  // indicators used to be (BackColor was the only thing ever touched on them -- Click handlers were
  // empty no-ops). Button's default chrome (3D bevel, focus rectangle, hover highlight) reads as a
  // clickable control, not a status light, and its flat square fill never actually matched a themed
  // dark panel it happened to sit on. This is a plain custom-painted Control with no LandmineUI
  // dependency of its own, so it behaves identically in both build configurations; under
  // USE_LANDMINE_UI it additionally reads its backdrop/ring color from the active theme so it blends
  // into a SharpGroupBox instead of sitting on a mismatched default-gray square.
  internal sealed class LedIndicator : Control
  {
    private Color _ledColor = Color.Gray;

    public LedIndicator()
    {
      SetStyle(
          ControlStyles.AllPaintingInWmPaint |
          ControlStyles.UserPaint |
          ControlStyles.OptimizedDoubleBuffer |
          ControlStyles.ResizeRedraw |
          ControlStyles.SupportsTransparentBackColor,
          true);
      TabStop = false;
      Cursor = Cursors.Default;
      Size = new Size(16, 16);
    }

    // Kept as the semantic on/off/color signal, same role BackColor played before -- callers just
    // swap which property they assign (Color.Lime/Yellow/Red for active, Color.Gray for off, same
    // as always) and the control decides how to render that.
    public Color LedColor
    {
      get => _ledColor;
      set
      {
        if (_ledColor == value) return;
        _ledColor = value;
        Invalidate();
      }
    }

    // Every call site in ScanSpy.cs uses Color.Gray for "off" and a saturated Lime/Yellow/Red for
    // "on" -- this is what decides whether OnPaint draws a lit glow/bevel or a flat dim dot,
    // without either side needing an explicit separate boolean.
    private static bool IsGrayscale(Color c) =>
        Math.Abs(c.R - c.G) < 8 && Math.Abs(c.G - c.B) < 8 && Math.Abs(c.R - c.B) < 8;

    protected override void OnPaint(PaintEventArgs e)
    {
      var g = e.Graphics;
      g.SmoothingMode = SmoothingMode.AntiAlias;

#if USE_LANDMINE_UI
      var theme = LandmineUI.WinForms.Theming.ThemeManager.Current;
      var backdrop = Parent?.BackColor ?? theme.Background;
      var offRing = theme.Border;
#else
      var backdrop = Parent?.BackColor ?? SystemColors.Control;
      var offRing = Color.FromArgb(120, 120, 120);
#endif
      // Control's SupportsTransparentBackColor doesn't give real transparency -- paint the actual
      // parent background first so the dot sits on the right surface instead of a stale default.
      using (var backdropBrush = new SolidBrush(backdrop))
        g.FillRectangle(backdropBrush, ClientRectangle);

      var rect = ClientRectangle;
      rect.Inflate(-3, -3);
      if (rect.Width <= 0 || rect.Height <= 0) return;

      bool lit = !IsGrayscale(_ledColor);

      if (lit)
      {
        // A few concentric, progressively fainter rings behind the dot read as "glowing" -- a flat
        // fill alone just reads as a colored square with rounded corners.
        for (int i = 3; i >= 1; i--)
        {
          var glowRect = rect;
          glowRect.Inflate(i, i);
          using var glowBrush = new SolidBrush(Color.FromArgb(28 / i, _ledColor));
          g.FillEllipse(glowBrush, glowRect);
        }
      }

      // Glossy bevel: lighter toward the top, base color at the bottom -- gives the dot a rounded,
      // lit look instead of a flat tile of color. Off state gets the same shape, just dimmer, so
      // the five indicators keep a consistent silhouette whether lit or not.
      Color top = lit ? Lighten(_ledColor, 0.55f) : Lighten(_ledColor, 0.12f);
      Color bottom = lit ? _ledColor : Darken(_ledColor, 0.25f);
      using (var fillBrush = new LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical))
        g.FillEllipse(fillBrush, rect);

      using (var pen = new Pen(lit ? Darken(_ledColor, 0.35f) : offRing, 1.25f))
        g.DrawEllipse(pen, rect);
    }

    private static Color Lighten(Color c, float amount)
    {
      amount = Math.Clamp(amount, 0f, 1f);
      return Color.FromArgb(
          c.A,
          (int)(c.R + (255 - c.R) * amount),
          (int)(c.G + (255 - c.G) * amount),
          (int)(c.B + (255 - c.B) * amount));
    }

    private static Color Darken(Color c, float amount)
    {
      amount = Math.Clamp(amount, 0f, 1f);
      return Color.FromArgb(c.A, (int)(c.R * (1 - amount)), (int)(c.G * (1 - amount)), (int)(c.B * (1 - amount)));
    }
  }
}

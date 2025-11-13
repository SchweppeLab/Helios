using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScanInjector
{
  public partial class TargetMZ : Form
  {
    public string value = null;
    public TargetMZ()
    {
      InitializeComponent();
      numericUpDown1.Controls[0].Visible = false;
      numericUpDown1.Select(0, numericUpDown1.Text.Length);
    }

    private void butOK_Click(object sender, EventArgs e)
    {
      value = numericUpDown1.Value.ToString();
    }
  }
}

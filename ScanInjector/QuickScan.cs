using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanInjector
{
  public class QuickScan
  {
    public List<double> mz = new List<double>();
    public List<double> intensity = new List<double>();
    public double firstMass = 0;
    public double lastMass = 0;
    public double maxIntensity = 0;
    public int scanNumber = 0;

    public QuickScan(int scan = 0)
    {
      scanNumber = scan;
    }

    public void Add(double m, double i)
    {
      mz.Add(m);
      intensity.Add(i);
      if (i > maxIntensity) maxIntensity = i;
    }
  }
}

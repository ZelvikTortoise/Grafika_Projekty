using System;
using System.Collections.Generic;
using System.Drawing;
using MathSupport;
using Utilities;

namespace Modules
{
  /// <summary>
  /// ModuleFormulaInternal class - template for internal image filter
  /// defined pixel-by-pixel using lambda functions.
  /// </summary>
  public class ModuleFormulaInternal : ModuleFormula
  {
    /// <summary>
    /// Mandatory plain constructor.
    /// </summary>
    public ModuleFormulaInternal ()
    {
      // TODO: Change to fast.
      param = "slow,create,wid=640,hei=480,freq=5000,bg=[0.0;0.0;0.0],fg=[1.0;1.0;1.0],angle=45,antialias=0,sample=2";
    }

    /// <summary>
    /// Author's full name (SurnameFirstname).
    /// </summary>
    public override string Author => "MacekLukas";

    /// <summary>
    /// Name of the module (short enough to fit inside a list-boxes, etc.).
    /// </summary>
    public override string Name => "FormuleInternal";

    /// <summary>
    /// Tooltip for Param (text parameters).
    /// </summary>
    public override string Tooltip =>
      "fast/slow .. fast/slow bitmap access\r" +
      "create .. new image\r" +
      "wid=<int> .. width of image\r" +
      "hei=<int> .. height of image\r" + 
      "freq=<float> .. frequency for image generation\r" + 
      "bg=[R;G;B] .. bacground color\r" +
      "fg=[R;G;B] .. foreground color\r" +
      "angle=<float> .. pattern angle in degrees\r" +
      "antialias=<int> .. antialiasing: 0=off,1=on\r" +
      "sample=<int> .. supersampling: sample x sample";

    //====================================================
    //--- Formula defined directly in this source file ---

    /// <summary>
    /// Defines implicit formulas if available.
    /// </summary>
    /// <returns>null if formulas sould be read from a script.</returns>
    protected override Formula GetFormula ()
    {
      Formula f = new Formula();

      // Text params -> script context.
      // Any global pre-processing is allowed here.
      f.contextCreate = (in Bitmap input, in string param) =>
      {
        if (string.IsNullOrEmpty(param))
          return null;

        Dictionary<string, string> p = Util.ParseKeyValueList(param);            

        // freq=<float>
        float freq = 500.0f;
        if (Util.TryParse(p, "freq", ref freq))
          freq = Util.Clamp(freq, 1.0f, 100000.0f);

        // angle=<float>
        double angle = 0.0f;
        Util.TryParse(p, "angle", ref angle);
        angle = Arith.DegreeToRadian(angle);

        // bg=[R;G;B]
        float R = 0.0f, G = 0.0f, B = 0.0f;
        float[] bgColor = new float[] { R, G, B };
        if (Util.TryParse(p, "bg", ref R, ref G, ref B, ';'))
          bgColor = new float[] { R, G, B };

        // fg=[R;G;B]
        R = G = B = 1.0f;
        float[] fgColor = new float[] { R, G, B };
        if (Util.TryParse(p, "fg", ref R, ref G, ref B, ';'))
          fgColor = new float[] { R, G, B };        

        // antialias=<int>
        int antialias = 0;
        if (Util.TryParse(p, "antialias", ref antialias))
        {
          if (antialias > 0)
          {
            antialias = 1;
          }
          else
          {
            antialias = 0;
          }          
        }

        // sample=<int>
        int sample = 2;
        if (Util.TryParse(p, "sample", ref sample))
        {
          if (sample < 2)
          {
            sample = 2;
          }          
        }

        Dictionary<string, object> sc = new Dictionary<string, object>();
        sc["freq"] = freq;
        sc["vv"] = Math.Cos(angle);
        sc["uv"] = Math.Sin(angle);
        sc["bg"] = bgColor;
        sc["fg"] = fgColor;
        sc["antialias"] = antialias;
        sc["sample"] = sample;
        sc["tooltip"] = "freq=<float> .. frequency for image generation (default=500)\r" +
                        "bg=[R;G;B] .. bacground color (default=black)\r" +
                        "fg=[R;G;B] .. foreground color (default=white)\r" +
                        "angle=<float> .. pattern angle in degrees (default=0)\r" +
                        "antialias=<int> .. antialiasing: 0=off,1=on (default=0)\r" +
                        "sample=<int> .. supersampling: sample x sample (default=2)";

        return sc;
      };

      f.pixelCreate = (
        in ImageContext ic,
        out float R,
        out float G,
        out float B) =>
      {

        // Custom frequency:
        float frequency = 500.0f;
        Util.TryParse(ic.context, "freq", ref frequency);

        // Do we want antialiasing:
        int antialiasing = 0;
        Util.TryParse(ic.context, "antialias", ref antialiasing);

        // Sample size of the super sampling:
        int n = 2;
        Util.TryParse(ic.context, "sample", ref n);
        if (antialiasing == 0)
          n = 1;

        int width = n * ic.width;
        double mul = frequency / ic.width;
        long ord = 0L;
        double u, v, vv, uv;
        int colorNum1 = 0, colorNum2 = 0;
        float[] color1, color2;
        float r, g, b;
        int x, y;


        if (ic.context.TryGetValue("fg", out object fgo) &&
                fgo is float[] fg)
        {
          r = fg[0];
          g = fg[1];
          b = fg[2];
        }
        else
          r = g = b = 1.0f;
        color1 = new float[] { r, g, b };

        if (ic.context.TryGetValue("bg", out object bgo) &&
                  bgo is float[] bg)
        {
          r = bg[0];
          g = bg[1];
          b = bg[2];
        }
        else
          r = g = b = 0.0f;
        color2 = new float[] { r, g, b };

        vv = 1.0;
        Util.TryParse(ic.context, "vv", ref vv);

        uv = 0.0;
        Util.TryParse(ic.context, "uv", ref uv);

        for (int i = 0; i < n; i++)
        {
          y = n * ic.y + i;
          v = frequency / ic.y;

          for (int j = 0; j < n; j++)
          {
            x = n * ic.x + j;

            if (y > 0)
            {
              u = mul * (x - width / 2) / y;

              ord = (long)(Math.Round(vv * v + uv * u) + Math.Round(uv * v - vv * u));
            }

            // Output color for one pixel of super sampling n x n.
            if ((ord & 1L) == 0)
            {
              colorNum1++;
            }
            else
            {
              colorNum2++;
            }
          }
        }

        // RGB of [x, y]:  
        R = (1.0f * colorNum1 / (n * n)) * color1[0] + (1.0f * colorNum2 / (n * n)) * color2[0];
        G = (1.0f * colorNum1 / (n * n)) * color1[1] + (1.0f * colorNum2 / (n * n)) * color2[1];
        B = (1.0f * colorNum1 / (n * n)) * color1[2] + (1.0f * colorNum2 / (n * n)) * color2[2];
      };
      

      return f;
    }
  }
}

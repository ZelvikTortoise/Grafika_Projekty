using System;
using System.Collections.Generic;
using System.Drawing;
using Utilities;
using MathSupport;

namespace Modules
{
    /// <summary>
    /// ModuleFormulaInternal class - template for internal image filter
    /// defined pixel-by-pixel using lambda functions.
    /// </summary>
    public class ModuleFormulaHSV : ModuleFormula
    {
        /// <summary>
        /// Mandatory plain constructor.
        /// </summary>
        public ModuleFormulaHSV()
        {
            param = "fast";
        }

        /// <summary>
        /// 
        /// 's full name (SurnameFirstname).
        /// </summary>
        public override string Author => "LukasMacek";

        /// <summary>
        /// Name of the module (short enough to fit inside a list-boxes, etc.).
        /// </summary>
        public override string Name => "FormuleHSV";

        /// <summary>
        /// Tooltip for Param (text parameters).
        /// </summary>
        public override string Tooltip =>
          "fast/slow .. fast/slow bitmap access\r" +
          "create .. new image\r" +
          "wid=<width>, hei=<height>";

        //====================================================
        //--- Formula defined directly in this source file ---

        /// <summary>
        /// Defines implicit formulas if available.
        /// </summary>
        /// <returns>null if formulas sould be read from a script.</returns>
        protected override Formula GetFormula()
        {
            Formula f = new Formula();

            // Text params -> script context.
            // Any global pre-processing is allowed here.
            f.contextCreate = (in Bitmap input, in string param) =>
            {
                if (string.IsNullOrEmpty(param))
                    return null;

                Dictionary<string, string> p = Util.ParseKeyValueList(param);

                float dH = 0.0f;

                // coeff=<float>
                if (Util.TryParse(p, "coeff", ref dH))
                    dH = Util.Saturate(dH);


                Dictionary<string, object> sc = new Dictionary<string, object>();
                sc["dH"] = dH;
                sc["tooltip"] = "dH=<float> .. changes the color of each pixel by rotating H for dH degrees in HSV representation\r"; //+
                                                                                                            //"freq=<float> .. density frequency for image generation (default=12)";

                return sc;
            };


            f.pixelTransform0 = (
                in ImageContext ic,
                ref float R,
                ref float G,
                ref float B) =>
            {
                float dH = 0.0f;
                Util.TryParse(ic.context, "dH", ref dH);

                double H, S, V;
                double r, g, b;

                // Conversion to HSV.
                Arith.ColorToHSV(Color.FromArgb((int)R, (int)G, (int)B), out H, out S, out V);
                // 0 <= H <= 360, 0 <= S <= 1, 0 <= V <= 1

                // HSV transform.
                if (true)           // TODO CONDITIONS!
                {
                    H = H + dH;
                    S = Util.Saturate(S);
                    V = Util.Saturate(V);
                }                
                // TODO: Gradual changes according to the accuracy!

                // Conversion back to RGB.
                Arith.HSVtoRGB(H, S, V, out r, out g, out b);
                R = (float)r;
                G = (float)g;
                B = (float)b;

                return true;
            };

            return f;
        }
    }
}

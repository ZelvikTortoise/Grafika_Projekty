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
            param = "fast, create";
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

                float coeff = 1.0f;

                // coeff=<float>
                if (Util.TryParse(p, "coeff", ref coeff))
                    coeff = Util.Saturate(coeff);


                Dictionary<string, object> sc = new Dictionary<string, object>();
                sc["coeff"] = coeff;
                sc["tooltip"] = "coeff=<float> .. swap coefficient (0.0 - no swap, 1.0 - complete swap)\r"; //+
                                                                                                            //"freq=<float> .. density frequency for image generation (default=12)";

                return sc;
            };


            f.pixelTransform0 = (
                in ImageContext ic,
                ref float R,
                ref float G,
                ref float B) =>
            {
                float coeff = 0.0f;
                Util.TryParse(ic.context, "coeff", ref coeff);


                float dH = 0.0f;
                double H, S, V;
                double r, g, b;

                // Conversion to HSV.
                Arith.ColorToHSV(Color.FromArgb((int)R, (int)G, (int)B), out H, out S, out V);
                // 0 <= H <= 360, 0 <= S <= 1, 0 <= V <= 1

                // HSV transform.
                H = H + dH;
                S = Util.Saturate(S);
                V = Util.Saturate(V);

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

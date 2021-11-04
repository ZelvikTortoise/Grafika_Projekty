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
                Arith.ColorToHSV(ic, out H, out S, out V);
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



          /*  // Text params -> script context.
            // Any global pre-processing is allowed here.
            f.contextCreate = (in Bitmap input, in string param) =>
            {
                if (string.IsNullOrEmpty(param))
                    return null;

                Dictionary<string, string> p = Util.ParseKeyValueList(param);

                float freq = 30.0f;
                float size = 5.0f;
                float bgnd = 1.0f;


                // TODO: CHANGE!
                // freq=<float>
                if (Util.TryParse(p, "freq", ref freq))
                    freq = Util.Clamp(freq, 0.01f, 1000.0f);

                // size=<float>
                if (Util.TryParse(p, "size", ref size))
                    size = Util.Clamp(size, 0.01f, 1000.0f);

                // bgnd=<float>
                if (Util.TryParse(p, "bgnd", ref bgnd))
                    bgnd = Util.Clamp(bgnd, 0, 1.0f);

                Dictionary<string, object> sc = new Dictionary<string, object>();
                sc["freq"] = freq;
                sc["size"] = size;
                sc["bgnd"] = bgnd;
                sc["tooltip"] = "freq=<float> .. density frequency for image generation (default=30)\r" +
                                "size=<float> .. size of the cardioid (default=5)\r" +
                                "bgnd=<float> .. 1 - background visible, otherwise not (default=1)";

                return sc;
            };


            f.pixelCreate = (
                in ImageContext ic,
                out float R,
                out float G,
                out float B) =>
            {
                R = 0;
                G = 0;
                B = 0;
            };                  */
        }
    }
}

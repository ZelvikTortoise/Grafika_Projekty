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
    public class ModuleHSVInternal : ModuleFormula
    {
        /// <summary>
        /// Mandatory plain constructor.
        /// </summary>
        public ModuleHSVInternal()
        {
            param = "fast";
        }

        /// <summary>
        /// Author's full name (SurnameFirstname).
        /// </summary>
        public override string Author => "MacekLukas";

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

        private double GetModifier(double H, double S, double V)
        {
            if (H >= 350 && S < 0.50 && V > 0.5)
            {
                return 0;
            }

            if (H >= 315 && S >= 0.20)
            {
                return 0;
            }

            if (H <= 30 && S >= 0.40 && V < 0.75)
            {
                return 0;
            }

            if (H >= 250 && S < 0.10)
            {
                return 0;
            }

            if (H >= 317 && S < 0.50 && V > 0.5)
            {
                return 1 - (H - 317) / 33;
            }

            if (H >= 345 && S < 0.50)
            {
                return 1 - (V / 0.50);
            }

            if (S < 0.50 && H >= 317)
            {
                return Math.Max((1 - (H - 317) / 33), (1 - (V / 0.50)));
            }

            // S < 0.50
            if (H >= 317 && S >= 0.30)
            {
                return 1 - ((S - 0.30) / 0.20);
            }

            if (H > 50 && H < 100)
            {
                return (H - 50) / 50;
            }

            if (H > 50)
            {
                return 1;
            }

            // H <= 50
            if ((S < 0.17 || S > 0.68) && V >= 0.5)
            {
                return 0;
            }

            if (S < 0.17)
            {
                return 1 - (V / 0.50);
            }

            if (H <= 23 && V >= 0.20)
            {
                return 0;
            }

            if (H <= 5 && V >= 0.15)
            {
                return 0;
            }

            // V < 0.40
            if (H <= 23)
            {
                return 1 - (V / 0.40);
            }

            // V < 0.25
            if (H <= 5)
            {
                return 1 - (V / 0.25);
            }

            if (V > 0.30 && V <= 0.80)
            {
                return 1 - ((V - 0.30) / 0.50);
            }

            /*
            if (S > 0.17 && V >= 0.9)
            {
                return 1 - ((S - 0.17) / 0.10);
            }*/

            return 0;
        }
        // Note: I know this code is a mess... I kind of had to do it and then delete everything and start again... I could do it better
        //       but don't have the time and the nerves for it anymore...
        //       The hardest part for me was to choose how much do I want to support skin in dark images and how much I can afford to not
        //       recolor in bright images... When I started doing compromises, it was like everything started going wrong...
        //                                                                                                                          Lukas

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

                float dH = 180.0f;

                // dH=<float>
                if (Util.TryParse(p, "dH", ref dH))
                    dH = Util.Clamp(dH, 0, 360);

                Dictionary<string, object> sc = new Dictionary<string, object>();
                sc["dH"] = dH;
                sc["tooltip"] = "dH=<float> .. changes the color of each pixel by rotating H for dH degrees in HSV representation (default=180)"; //\r +
                                                                                                                                                  //"freq=<float> .. density frequency for image generation (default=12)";

                return sc;
            };

            // R <-> B channel swap with weights.
            f.pixelTransform0 = (
              in ImageContext ic,
              ref float R,
              ref float G,
              ref float B) =>
            {
                float dH = 180.0f;
                Util.TryParse(ic.context, "dH", ref dH);

                double H, S, V;
                double r, g, b;

                Arith.ColorToHSV(Color.FromArgb((int)(R * 255), (int)(G * 255), (int)(B * 255)), out H, out S, out V);
                // 0 <= H <= 360, 0 <= S <= 1, 0 <= V <= 1

                // HSV transform.
                H = H + GetModifier(H, S, V) * dH;
                S = Util.Saturate(S);
                V = Util.Saturate(V);
                // TODO: Gradual changes according to the accuracy!

                // Conversion back to RGB.
                Arith.HSVtoRGB(H, S, V, out r, out g, out b);
                R = Util.Saturate((float)r);
                G = Util.Saturate((float)g);
                B = Util.Saturate((float)b);

                // Output color was modified.
                return true;
            };         

            return f;
        }
    }
}
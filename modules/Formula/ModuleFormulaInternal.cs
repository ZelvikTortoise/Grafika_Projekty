using System;
using System.Collections.Generic;
using System.Drawing;
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
        public ModuleFormulaInternal()
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
        public override string Name => "FormuleInternal";

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
        /// Generated colors except white and black.
        /// RGB for these colors in order are FF0000, FFA500, FFFF00, 00FF00, 00FFFF, 0000FF, 800080 and FFC0CB.
        /// </summary>
        enum Col { R, O, Y, G, C, B, Pu, Pi };

        private void Rotate(ref double x, ref double y, double degrees)
        {
            double newX, newY;
            newX = Math.Cos(degrees) * x - Math.Sin(degrees) * y;
            newY = Math.Sin(degrees) * x + Math.Cos(degrees) * y;

            x = newX;
            y = newY;
        }

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

                float freq = 30.0f;
                float size = 5.0f;
                float bgnd = 1.0f;

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
                    // Scaling x, y to be in {0, 1]:
                    double x = ic.x / (double)Math.Max(1, ic.width  - 1);
                    double y = ic.y / (double)Math.Max(1, ic.height - 1);

                    // Getting a uniform scale with the origin at the image center:
                    if (ic.width > ic.height)
                    {
                        // Landscape.
                        x -= 0.5;
                        y = ic.height * (y - 0.5) / ic.width;
                    }
                    else
                    {
                        // Portrait.
                        x = ic.width * (x - 0.5) / ic.height;
                        y -= 0.5;
                    }

                    // Customizing the scales:
                    float freq = 30.0f;
                    Util.TryParse(ic.context, "freq", ref freq);

                    x *= freq;
                    y *= freq;                   

                    // Rotation:
                    Rotate(ref x, ref y, Math.PI / 2);

                    // Getting the cardioid's parameter:
                    float a = 5.0f;
                    Util.TryParse(ic.context, "size", ref a);

                    // Filled cardioid:
                    if ((x * x + y * y - a * a) * (x * x + y * y - a * a) - 4 * a * a * ((x - a) * (x - a) + y * y) <= 0)
                    {
                        double rr = x * x + y * y;
                        Col col;
                        int rrRounded = (int)Math.Round(rr);
                        bool mod2yields1 = (rrRounded & 1) > 0;
                        bool mod4yields23 = (rrRounded & 2) > 0;
                        bool mod8yields4567 = (rrRounded & 4) > 0;

                        if (!mod2yields1)
                        {
                            if (!mod4yields23)
                            {
                                if (!mod8yields4567)
                                {
                                    col = Col.R;
                                }
                                else
                                {
                                    col = Col.C;
                                }
                            }
                            else
                            {
                                if (!mod8yields4567)
                                {
                                    col = Col.Y;
                                }
                                else
                                {
                                    col = Col.Pu;
                                }
                            }
                        }
                        else
                        {
                            if (!mod4yields23)
                            {
                                if (!mod8yields4567)
                                {
                                    col = Col.O;
                                }
                                else
                                {
                                    col = Col.B;
                                }
                            }
                            else
                            {
                                if (!mod8yields4567)
                                {
                                    col = Col.G;
                                }
                                else
                                {
                                    col = Col.Pi;
                                }
                            }
                        }

                        switch (col)
                        {
                            case Col.R:
                                R = 1.0f;
                                G = 0;
                                B = 0;
                                break;
                            case Col.O:
                                R = 1.0f;
                                G = 0.647f;
                                B = 0;
                                break;
                            case Col.Y:
                                R = 1.0f;
                                G = 1.0f;
                                B = 0f;
                                break;
                            case Col.G:
                                R = 0;
                                G = 1.0f;
                                B = 0;
                                break;
                            case Col.C:
                                R = 0;
                                G = 1.0f;
                                B = 1.0f;
                                break;
                            case Col.B:
                                R = 0;
                                G = 0;
                                B = 1.0f;
                                break;
                            case Col.Pu:
                                R = 0.5f;
                                G = 0;
                                B = 0.5f;
                                break;
                            case Col.Pi:
                                R = 1.0f;
                                G = 0.753f;
                                B = 0.796f;
                                break;
                            default:
                                // Black color because we found some kind of a black hole in binary.
                                R = 0;
                                G = 0;
                                B = 0;
                                break;
                        }                        
                    }
                    else
                    {
                        // Adjusting visibility of the background:
                        float bgnd = 1.0f;
                        Util.TryParse(ic.context, "bgnd", ref bgnd);
                        if (Math.Abs(bgnd - 1) < 1E-9)
                        {
                            // Background visible.
                            R = 1.0f;
                            G = 0.753f;
                            B = 0.796f;
                        }
                        else
                        {
                            // Using white color - background "invisible".
                            R = 1.0f;
                            G = 1.0f;
                            B = 1.0f;
                        }
                    }                    
                };

            return f;
        }
    }
}

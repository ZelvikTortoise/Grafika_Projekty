using System;
using System.Collections.Generic;
using System.Drawing;
using LineCanvas;
using Utilities;

namespace _092lines
{    
    public struct Edge
    {
        public Point Start { get; }
        public Point End { get; }
        public bool Vertical { get; }
        public bool Wavy { get; }

        public Edge(Point start, Point end, bool wavy)
        {
            this.Start = start;
            this.End = end;
            this.Vertical = start.X == end.X ? true : false;
            this.Wavy = wavy;
        }
    }

    public class Lines
    {
        /// <summary>
        /// Form data initialization.
        /// </summary>
        /// <param name="name">Your first-name and last-name.</param>
        /// <param name="wid">Initial image width in pixels.</param>
        /// <param name="hei">Initial image height in pixels.</param>
        /// <param name="param">Optional text to initialize the form's text-field.</param>
        /// <param name="tooltip">Optional tooltip = param help.</param>
        public static void InitParams(out string name, out int wid, out int hei, out string param, out string tooltip)
        {
            // {{

            // Put your name here.
            name = "Lukáš Macek";

            // Image size in pixels.
            wid = 800;
            hei = 600;

            // Specific animation params.
            param = "width=1.0,anti=true,seed=12";

            // Tooltip = help.
            tooltip = "width=<int>, anti[=<bool>], seed=<int>";

            // }}
        }

        private static void DrawCube(Canvas c, int startX, int startY, float lLength, int seed)
        {
            int len = (int)lLength;
            int lenh = (int)(0.5 * len);
            int lenhhh = len + lenh;

            if (len == 0 || lenh == 0)
            {
                return;
            }

            Point A = new Point(startX, startY);
            Point B = new Point(startX + len, startY);
            Point C = new Point(startX + lenhhh, startY - lenh);
            Point D = new Point(startX + lenh, startY - lenh);
            Point E = new Point(startX, startY - len);
            Point F = new Point(startX + len, startY - len);
            Point G = new Point(startX + lenhhh, startY - lenhhh);
            Point H = new Point(startX + lenh, startY - lenhhh);

            Edge AB = new Edge(A, B, false);
            Edge BC = new Edge(B, C, false);
            Edge CD = new Edge(C, D, true); // wavy
            Edge DA = new Edge(D, A, true); // wavy
            Edge AE = new Edge(A, E, false);
            Edge BF = new Edge(B, F, false); 
            Edge CG = new Edge(C, G, false); 
            Edge DH = new Edge(D, H, true); // wavy
            Edge EF = new Edge(E, F, false);
            Edge FG = new Edge(F, G, false);
            Edge GH = new Edge(G, H, false);
            Edge HE = new Edge(H, E, false);
            Edge[] edges = { AB, BC, CD, DA,
                             AE, BF, CG, DH,
                             EF, FG, GH, HE
                           };

            Edge e;
            for (int i = 0; i < edges.Length; i++)
            {
                e = edges[i];

                c.Line(e.Start.X, e.Start.Y, e.End.X, e.End.Y); // Remove.

                if (e.Wavy)
                {
                    // TODO
                }
                else
                {
                    // c.Line(e.Start.X, e.Start.Y, e.End.X, e.End.Y);
                }
            }            
        }

        /// <summary>
        /// Draw the image into the initialized Canvas object.
        /// </summary>
        /// <param name="c">Canvas ready for your drawing.</param>
        /// <param name="param">Optional string parameter from the form.</param>
        public static void Draw(Canvas c, string param)
        {
            // Input params.
            float penWidth = 1.0f;   // pen width
            bool antialias = true;  // use anti-aliasing?
            int seed = 12;     // random generator seed

            Dictionary<string, string> p = Util.ParseKeyValueList(param);
            if (p.Count > 0)
            {
                // with=<line-width>
                if (Util.TryParse(p, "width", ref penWidth))
                {
                    if (penWidth < 0.0f)
                        penWidth = 0.0f;
                }

                // anti[=<bool>]
                Util.TryParse(p, "anti", ref antialias);

                // seed=<int>
                Util.TryParse(p, "seed", ref seed);
            }

            // TODO:
            // c.Line(x1, y1, x2, y2);

            const int padding = 10;
            const int minLineLength = 10;
            float lineLength = 66;
            int startX = 100, startY = 100;

            DrawCube(c, startX, startY, lineLength, seed);

            
            /*
            int wq = c.Width / 4;
            int hq = c.Height / 4;
            int wh = wq + wq;
            int hh = hq + hq;
            int minh = Math.Min(wh, hh);
            double t;
            int i, j;
            double cx, cy, angle, x, y;

            c.Clear(Color.Black);

            // 1st quadrant - star.
            c.SetPenWidth(penWidth);
            c.SetAntiAlias(antialias);

            const int MAX_LINES = 30;
            for (i = 0, t = 0.0; i < MAX_LINES; i++, t += 1.0 / MAX_LINES)
            {
              c.SetColor(Color.FromArgb(i * 255 / MAX_LINES, 255, 255 - i * 255 / MAX_LINES)); // [0,255,255] -> [255,255,0]
              c.Line(t * wh, 0, wh - t * wh, hh);
            }
            for (i = 0, t = 0.0; i < MAX_LINES; i++, t += 1.0 / MAX_LINES)
            {
              c.SetColor(Color.FromArgb(255, 255 - i * 255 / MAX_LINES, i * 255 / MAX_LINES)); // [255,255,0] -> [255,0,255]
              c.Line(0, hh - t * hh, wh, t * hh);
            }

            // 2nd quadrant - random hatched squares.
            double size = minh / 10.0;
            double padding = size * Math.Sqrt(0.5);
            c.SetColor(Color.LemonChiffon);
            c.SetPenWidth(1.0f);
            Random r = (seed == 0) ? new Random() : new Random(seed);

            for (i = 0; i < objects; i++)
            {
              do
                cx = r.NextDouble() * wh;
              while (cx < padding ||
                     cx > wh - padding);

              c.SetAntiAlias(cx > wq);
              cx += wh;

              do
                cy = r.NextDouble() * hh;
              while (cy < padding ||
                     cy > hh - padding);

              angle = r.NextDouble() * Math.PI;

              double dirx = Math.Sin(angle) * size * 0.5;
              double diry = Math.Cos(angle) * size * 0.5;
              cx -= dirx - diry;
              cy -= diry + dirx;
              double dx = -diry * 2.0 / hatches;
              double dy = dirx * 2.0 / hatches;
              double linx = dirx + dirx;
              double liny = diry + diry;

              for (j = 0; j++ < hatches; cx += dx, cy += dy)
                c.Line(cx, cy, cx + linx, cy + liny);
            }

            // 3rd quadrant - random stars.
            c.SetColor(Color.LightCoral);
            c.SetPenWidth(penWidth);
            size = minh / 16.0;
            padding = size;
            const int MAX_SIDES = 30;
            List<PointF> v = new List<PointF>(MAX_SIDES + 1);

            for (i = 0; i < objects; i++)
            {
              do
                cx = r.NextDouble() * wh;
              while (cx < padding ||
                     cx > wh - padding);

              c.SetAntiAlias(cx > wq);

              do
                cy = r.NextDouble() * hh;
              while (cy < padding ||
                     cy > hh - padding);
              cy += hh;

              int sides = r.Next(3, MAX_SIDES);
              double dAngle = Math.PI * 2.0 / sides;

              v.Clear();
              angle = 0.0;

              for (j = 0; j++ < sides; angle += dAngle)
              {
                double rad = size * (0.1 + 0.9 * r.NextDouble());
                x = cx + rad * Math.Sin(angle);
                y = cy + rad * Math.Cos(angle);
                v.Add(new PointF((float)x, (float)y));
              }
              v.Add(v[0]);
              c.PolyLine(v);
            }

            // 4th quadrant - Brownian motion.
            c.SetPenWidth(penWidth);
            c.SetAntiAlias(true);
            size = minh / 10.0;
            padding = size;

            for (i = 0; i < objects; i++)
            {
              do
                x = r.NextDouble() * wh;
              while (x < padding ||
                     x > wh - padding);

              do
                y = r.NextDouble() * hh;
              while (y < padding ||
                     y > hh - padding);

              c.SetColor(Color.FromArgb(127 + r.Next(0, 128),
                                        127 + r.Next(0, 128),
                                        127 + r.Next(0, 128)));

              for (j = 0; j++ < 1000;)
              {
                angle = r.NextDouble() * Math.PI * 2.0;
                double rad = size * r.NextDouble();
                cx = x + rad * Math.Sin(angle);
                cy = y + rad * Math.Cos(angle);
                if (cx < 0.0 || cx > wh ||
                    cy < 0.0 || cy > hh)
                  break;

                c.Line(x + wh, y + hh, cx + wh, cy + hh);
                x = cx;
                y = cy;
                if (r.NextDouble() > prob)
                  break;
              }
              */
        }
    }
}

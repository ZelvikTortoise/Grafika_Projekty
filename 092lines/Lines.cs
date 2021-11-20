using System;
using System.Collections.Generic;
using System.Drawing;
using LineCanvas;
using Utilities;

namespace _092lines
{    
    public struct Cube
    {
        public Edge[] Edges { get; }

        public static Faces GetOppositeFace(Faces f)
        {
            if ((int)f < 0 || (int)f > 5)
            {
                throw new ArgumentException($"{nameof(GetOppositeFace)} function: Indeces of the enum {nameof(Faces)} can be only from 0 to 5.\nRecieved value: {f}, index: {(int)f}.");
            }

            switch (f)
            {
                case Faces.Down:
                    return Faces.Up;
                case Faces.Front:
                        return Faces.Back;
                case Faces.Right:
                    return Faces.Left;
                case Faces.Back:
                    return Faces.Front;
                case Faces.Left:
                    return Faces.Right;
                case Faces.Up:
                    return Faces.Down;
                default:
                    throw new ArgumentException($"{nameof(GetOppositeFace)} function: Unknown value of the enum {nameof(Faces)}.\nRecieved value: {f}.");
            }                
        }

        public static bool GetCommonEdgeNum(Faces f1, Faces f2, out int edgeNum)
        {
            if (Cube.FacesOpposite(f1, f2))
            {
                edgeNum = -1;
                return false;
            }

            int[] eNums1 = Cube.GetEdgeNums(f1);
            int[] eNums2 = Cube.GetEdgeNums(f2);

            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    if (eNums1[i] == eNums2[j])
                    {
                        edgeNum = eNums1[i];
                        return true;
                    }                        
                }
            }

            edgeNum = -2;
            return false;
        }

        public static bool GetCommonFace(int e1, int e2, out Faces face)
        {
            Faces[] faces1 = Cube.GetEdgeFaces(e1);
            Faces[] faces2 = Cube.GetEdgeFaces(e2);

            for (int i = 0; i <= 1; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (faces1[i] == faces2[j])
                    {
                        face = faces1[i];
                        return true;
                    }
                }
            }

            face = Faces.Down;
            return false;
        }

        public static bool FacesOpposite(Faces f1, Faces f2)
        {
            // Argument range:
            if ((int)f1 < 0 || (int)f1 > 5 || (int)f2 < 0 || (int)f2 > 5)
            {
                throw new ArgumentException($"{nameof(FacesOpposite)} function: Unknown value of the enum {nameof(Faces)}.\nRecieved values: {f1}, {f2}.");
            }

            // Degenerated case:
            if (f1 == f2)
            {
                return false;
            }
                
            // Swapping:
            if ((int)f1 > (int)f2)
            {
                Faces temp =f1;
                f1 = f2;
                f2 = temp;
            }

            // Now (int)f1 < (int)f2:
            switch (f1)
            {
                case Faces.Down:
                    if (f2 == Faces.Up)
                        return true;
                    break;
                case Faces.Front:
                    if (f2 == Faces.Back)
                        return true;
                    break;
                case Faces.Right:
                    if (f2 == Faces.Left)
                        return true;
                    break;               
            }

            return false;
        }

        public static Faces[] GetEdgeFaces(int edgeIndex)
        {
            switch (edgeIndex)
            {
                case 0:
                    return new Faces[] { Faces.Down, Faces.Front };
                case 1:
                    return new Faces[] { Faces.Down, Faces.Right };
                case 2:
                    return new Faces[] { Faces.Down, Faces.Back };
                case 3:
                    return new Faces[] { Faces.Down, Faces.Left };
                case 4:
                    return new Faces[] { Faces.Front, Faces.Left };
                case 5:
                    return new Faces[] { Faces.Front, Faces.Right };
                case 6:
                    return new Faces[] { Faces.Right, Faces.Back };
                case 7:
                    return new Faces[] { Faces.Back, Faces.Left };
                case 8:
                    return new Faces[] { Faces.Front, Faces.Up };
                case 9:
                    return new Faces[] { Faces.Right, Faces.Up };
                case 10:
                    return new Faces[] { Faces.Back, Faces.Up };
                case 11:
                    return new Faces[] { Faces.Left, Faces.Up };
                default:
                    throw new ArgumentException($"{nameof(GetEdgeFaces)} function: Indeces of edges are numbered from 0 to 11.\n Received index: {edgeIndex}.");
            }
        }

        public static int[] GetEdgeNums(Faces face)
        {
            switch (face)
            {
                case Faces.Down:
                    return new int[4] { 0, 1, 2, 3 };
                case Faces.Front:
                    return new int[4] { 0, 4, 5, 8 };
                case Faces.Right:
                    return new int[4] { 1, 5, 6, 9 };
                case Faces.Back:
                    return new int[4] { 2, 6, 7, 10 };
                case Faces.Left:
                    return new int[4] { 3, 4, 7, 11 };
                case Faces.Up:
                    return new int[4] { 8, 9, 10, 11 };
                default:
                    throw new ArgumentException($"{nameof(GetEdgeNums)} function: Unknown value of the enum {nameof(Faces)}.\nRecieved value: {face}.");
            }
        }

        public static bool EdgesOpposite(int e1Index, int e2Index)
        {
            // Argument range:
            if (e1Index < 0 || e1Index > 11 || e2Index < 0 || e2Index > 11)
            {
                throw new ArgumentException($"{nameof(EdgesOpposite)} function: Indeces of edges are numbered from 0 to 11.\n Received indeces: {e1Index}, {e2Index}.");
            }

            // Degenerated case:
            if (e1Index == e2Index)
            {
                return false;
            }

            // Swapping:
            if (e1Index > e2Index)
            {
                int temp = e1Index;
                e1Index = e2Index;
                e2Index = temp;
            }

            // Now e1Index < e2Index:
            switch (e1Index)
            {
                case 0:
                    if (e2Index == 10)
                        return true;
                    break;
                case 1:
                    if (e2Index == 11)
                        return true;
                    break;
                case 2:
                    if (e2Index == 8)
                        return true;
                    break;
                case 3:
                    if (e2Index == 9)
                        return true;
                    break;
                case 4:
                    if (e2Index == 6)
                        return true;
                    break;
                case 5:
                    if (e2Index == 7)
                        return true;
                    break;
            }

            return false;
        }

        public static bool EdgesShareFace(int e1Index, int e2Index)
        {
            // Argument range:
            if (e1Index < 0 || e1Index > 11 || e2Index < 0 || e2Index > 11)
            {
                throw new ArgumentException($"{nameof(EdgesShareFace)} function: Indeces of edges are numbered from 0 to 11.\n Received indeces: {e1Index}, {e2Index}.");
            }            

            // Degenerated case:
            if (e1Index == e2Index)
            {
                return true;
            }

            // Swapping:
            if (e1Index > e2Index)
            {
                int temp = e1Index;
                e1Index = e2Index;
                e2Index = temp;
            }

            // Now e1Index < e2Index:
            switch (e1Index)
            {
                case 0:
                    if (e2Index == 1 || e2Index == 2 || e2Index == 3 || e2Index == 4 || e2Index == 5 || e2Index == 8)
                        return true;
                    break;
                case 1:
                    if (e2Index == 2 || e2Index == 3 || e2Index == 5 || e2Index == 6 || e2Index == 9)
                        return true;
                    break;
                case 2:
                    if (e2Index == 3 || e2Index == 6 || e2Index == 7 || e2Index == 10)
                        return true;
                    break;
                case 3:
                    if (e2Index == 4 || e2Index == 7 || e2Index == 11)
                        return true;
                    break;
                case 4:
                    if (e2Index == 5 || e2Index == 7 || e2Index == 8 || e2Index == 11)
                        return true;
                    break;
                case 5:
                    if (e2Index == 6 || e2Index == 8 || e2Index == 9)
                        return true;
                    break;
                case 6:
                    if (e2Index == 7 || e2Index == 9 || e2Index == 10)
                        return true;
                    break;
                case 7:
                    if (e2Index == 10 || e2Index == 11)
                        return true;
                    break;
                case 8:
                    if (e2Index == 9 || e2Index == 10 || e2Index == 11)
                        return true;
                    break;
                case 9:
                    if (e2Index == 10 || e2Index == 11)
                        return true;
                    break;
                case 10:
                    if (e2Index == 11)
                        return true;
                    break;
            }

            return false;
        }

        public Cube(Edge[] edges)
        {
            this.Edges = edges != null ? edges : new Edge[12];
        }
    }

    public enum Faces { Down, Front, Right, Back, Left, Up }    // Not Bottom / Top, so every face has a unique starting letter.

    public struct Edge
    {
        public Point Start { get; }
        public Point End { get; }
        public bool Vertical { get; }
        public bool Dashed { get; }

        public Edge(Point start, Point end, bool dashed)
        {
            this.Start = start;
            this.End = end;
            this.Vertical = start.X == end.X ? true : false;
            this.Dashed = dashed;
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

        private static void MarkPoint(Canvas c, Color colorAfter, Point point, int lineLength)
        {
            c.SetColor(Color.Red);
            int d = lineLength / 14;

            c.Line(point.X - d, point.Y - d, point.X + d, point.Y + d);
            c.Line(point.X - d, point.Y + d, point.X + d, point.Y - d);

            c.SetColor(colorAfter);
        }

        private static Point GetRandomedPointOnEdge(Random random, Edge e)
        {
            int x1, x2, y1, y2;
            x1 = e.Start.X;
            x2 = e.End.X;
            y1 = e.Start.Y;
            y2 = e.End.Y;

            {
                int temp;
                if (x1 > x2)
                {
                    temp = x1;
                    x1 = x2;
                    x2 = temp;
                }

                if (y1 > y2)
                {
                    temp = y1;
                    y1 = y2;
                    y2 = temp;
                }
            }

            if (e.Vertical)
            {
                return new Point(x1, random.Next(y1, y2 + 1));
            }
            else if (y1 == y2)
            {
                return new Point(random.Next(x1, x2 + 1), y1);
            }
            else
            {
                // Using a bit of analytic geometry:
                int u1 = e.End.X - e.Start.X;
                int u2 = e.End.Y - e.Start.Y;
                float k = (1.0f * u2) / u1;
                float q = (-u2 * e.Start.X + u1 * e.Start.Y) / u1;

                int x = random.Next(x1, x2 + 1);
                int y = (int)(k * x + q);

                return new Point(x, y);
            }
        }

        private static void DrawDashedLine(Canvas c, Point start, Point end)
        {
            const int parts = 14;   // 3k + 2
            int dx = (end.X - start.X) / parts;
            int dy = (end.Y - start.Y) / parts;
            int lines = (parts - 2) / 3 + 1;

            int x = start.X;
            int y = start.Y;
            int newX = start.X + 2 * dx;
            int newY = start.Y + 2 * dy;

            for (int i = 1; i <= lines; i++)
            {
                c.Line(x, y, newX, newY);
                x += 3 * dx;
                newX += 3 * dx;
                y += 3 * dy;
                newY += 3 * dy;
            }

            int mx = Math.Sign(dx);
            int my = Math.Sign(dy);

            while (mx * newX < mx * end.X && my * newY < my * end.Y)
            {
                c.Line(x, y, newX, newY);
                x += 3 * dx;
                newX += 3 * dx;
                y += 3 * dy;
                newY += 3 * dy;
            }

            c.Line(x, y, end.X, end.Y);
        }

        private static void GenerateCubeSlice(Canvas c, Cube cube, Random random)
        {
            List<int> possibleEdgeNums = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            int[] edgeNums = new int[3];

            int edgeNumIndex, edgeNum;
            for (int i = 0; i <= 1; i++)
            {
                edgeNumIndex = random.Next(0, possibleEdgeNums.Count);                                
                edgeNum = possibleEdgeNums[edgeNumIndex];
                possibleEdgeNums.RemoveAt(edgeNumIndex);
                edgeNums[i] = edgeNum;                
            }

            // 3rd edge:

            if (Cube.EdgesOpposite(edgeNums[0], edgeNums[1]))
            {
                possibleEdgeNums = new List<int>();
                for (int i = 0; i <= 11; i++)
                {
                    if (i == edgeNums[0] || i == edgeNums[1])
                        continue;

                    if (Cube.EdgesShareFace(edgeNums[0], i) && Cube.EdgesShareFace(edgeNums[1], i))
                        possibleEdgeNums.Add(i);
                }

                edgeNums[2] = possibleEdgeNums[random.Next(0, possibleEdgeNums.Count)];
            }
            else if (Cube.EdgesShareFace(edgeNums[0], edgeNums[1]))
            {
                bool[] sharesExcatlyOne = new bool[12];
                for (int i = 0; i <= 11; i++)
                    sharesExcatlyOne[i] = false;

                for (int i = 0; i<= 11; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        if (Cube.EdgesShareFace(edgeNums[j], i))
                        {
                            if (sharesExcatlyOne[i])
                                sharesExcatlyOne[i] = false;
                            else
                                sharesExcatlyOne[i] = true;
                        }    
                    }
                }

                possibleEdgeNums = new List<int>();
                for (int i = 0; i <= 11; i++)
                {
                    if (sharesExcatlyOne[i])
                        possibleEdgeNums.Add(i);
                }

                edgeNums[2] = possibleEdgeNums[random.Next(0, possibleEdgeNums.Count)];
            }
            else
            {
                Faces[] faces1 = Cube.GetEdgeFaces(edgeNums[0]);
                Faces[] faces2 = Cube.GetEdgeFaces(edgeNums[1]);
                Faces[] oppositeFaces = new Faces[2];
                for (int i = 0; i <= 1; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        if (Cube.FacesOpposite(faces1[i], faces2[j]))
                        {
                            oppositeFaces[0] = faces1[i];
                            oppositeFaces[1] = faces2[j];
                            break;
                        }
                    }
                }

                Faces[] notOppositeFaces = new Faces[2];
                for (int i = 0; i <= 1; i++)
                {
                    if (faces1[i] != oppositeFaces[0])
                        notOppositeFaces[0] = faces1[i];

                    if (faces1[i] != oppositeFaces[1])
                        notOppositeFaces[1] = faces1[i];
                }

                possibleEdgeNums = new List<int>();
                for (int i = 0; i <= 1; i++)
                {
                    if (Cube.GetCommonEdgeNum(oppositeFaces[i], notOppositeFaces[1 - i], out edgeNum))
                        possibleEdgeNums.Add(edgeNum);
                    else
                        throw new Exception("Not able to get the common edge while getting the 3rd edge in the case where there should exist excatly one edge being common between the faces touching the edges.");

                    if (Cube.GetCommonEdgeNum(oppositeFaces[i], Cube.GetOppositeFace(notOppositeFaces[1 - i]), out edgeNum))
                        possibleEdgeNums.Add(edgeNum);
                    else
                        throw new Exception("Not able to get the common edge while getting the 3rd edge in the case where there should exist excatly one edge being common between the faces touching the edges.");
                }
                edgeNums[2] = possibleEdgeNums[random.Next(0, possibleEdgeNums.Count)];
            }

            // Getting the line length:
            int lineLength = cube.Edges[0].End.X - cube.Edges[0].Start.X;

            // Randoming points on already randomed edges:
            Point[] points = new Point[3];
            for (int i = 0; i <= 2; i++)
            {
                points[i] = GetRandomedPointOnEdge(random, cube.Edges[edgeNums[i]]);
                // Visualizing points:
                MarkPoint(c, Color.White, points[i], lineLength);
            }

            // TODO
            // Slicing the cube:

        }

        private static Cube DrawCube(Canvas c, int startX, int startY, float lLength)
        {
            int len = (int)lLength;
            int lenh = (int)(0.5 * len);
            int lenhhh = len + lenh;

            if (len == 0 || lenh == 0)
            {
                return new Cube();
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
            Edge CD = new Edge(C, D, true); // dahsed
            Edge DA = new Edge(D, A, true); // dahsed
            Edge AE = new Edge(A, E, false);
            Edge BF = new Edge(B, F, false);
            Edge CG = new Edge(C, G, false);
            Edge DH = new Edge(D, H, true); // dahsed
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

                if (e.Dashed)
                {
                    DrawDashedLine(c, e.Start, e.End);
                }
                else
                {
                    c.Line(e.Start.X, e.Start.Y, e.End.X, e.End.Y);
                }
            }

            return new Cube(edges);
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
            float lineLength = 150;
            int startX = 300, startY = 300;
            Random random = seed <= 0 ? new Random() : new Random(seed);

            Cube cube = DrawCube(c, startX, startY, lineLength);
            GenerateCubeSlice(c, cube, random);

            
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

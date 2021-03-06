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

        public static bool EdgeDashed(int edgeIndex)
        {
            if (edgeIndex < 0 || edgeIndex > 11)
                throw new ArgumentException($"{nameof(EdgeDashed)} function: Indeces of edges are numbered from 0 to 11.\n Received index: {edgeIndex}.");

            switch (edgeIndex)
            {
                case 2:
                case 3:
                case 7:
                    return true;
                default:
                    return false;
            }
        }

        public bool EdgesTouching(Edge e1, Edge e2)
        {
            if (e1.Start == e2.Start || e1.Start == e2.End || e1.End == e2.Start || e1.End == e2.End)
                return true;
            else
                return false;
        }

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
            // My name is here.
            name = "Lukáš Macek";

            // Image size in pixels.
            wid = 800;
            hei = 600;

            // Specific animation params.
            param = "width=1.0,anti=true,seed=12,a=56.0,gen=true,dash=true";

            // Tooltip = help.
            tooltip = "width=<float>, anti=<bool>, seed=<int>, a=<float>...length of the edges of each cube (min=28.0, max=196.0), gen=<bool>...try generate cubes?, dash=<bool>...hidden lines dashed (including crosses)?";
        }

        private static void MarkPoint(Canvas c, Color colorAfter, Point point, int lineLength, bool dashed)
        {
            c.SetColor(Color.Red);
            int d = lineLength / 14;

            if (dashed)
            {
                DrawDashedLine(c, new Point(point.X - d, point.Y - d), new Point(point.X + d, point.Y + d));
                DrawDashedLine(c, new Point(point.X - d, point.Y + d), new Point(point.X + d, point.Y - d));
            }
            else
            {
                c.Line(point.X - d, point.Y - d, point.X + d, point.Y + d);
                c.Line(point.X - d, point.Y + d, point.X + d, point.Y - d);
            }            

            c.SetColor(colorAfter);
        }

        /// <summary>
        /// Randoms an INNER point on the given edge using given instance of class Random (with used seed).
        /// </summary>
        /// <param name="random">Instance of Random class.</param>
        /// <param name="e">Edge on which we want to random the point.</param>
        /// <returns>Inner point on the given edge.</returns>
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
                return new Point(x1, random.Next(y1 + 1, y2));
            }
            else if (y1 == y2)
            {
                return new Point(random.Next(x1 + 1, x2), y1);
            }
            else
            {
                // Using a bit of analytic geometry:
                int u1 = e.End.X - e.Start.X;
                int u2 = e.End.Y - e.Start.Y;
                float k = (1.0f * u2) / u1;
                float q = (-u2 * e.Start.X + u1 * e.Start.Y) / u1;

                int x = random.Next(x1 + 1, x2);
                int y = (int)(k * x + q);

                return new Point(x, y);
            }
        }

        private static void DrawDashedLine(Canvas c, Point start, Point end)
        {
            const int parts = 14;   // 3k + 2
            float dx = (1.0f * (end.X - start.X)) / parts;
            float dy = (1.0f * (end.Y - start.Y)) / parts;
            int lines = (parts - 2) / 3 + 1;

            float x = start.X;
            float y = start.Y;
            float newX = (int)(start.X + 2 * dx);
            float newY = (int)(start.Y + 2 * dy);

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

            if (mx * x < mx * end.X && my * y < my * end.Y)
                c.Line(x, y, end.X, end.Y);
        }

        private static void GenerateCubeSlice(Canvas c, Cube cube, Random random, Color cubeColor, bool dashedHiddenCrosses)
        {
            List<int> possibleEdgeNums = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            int[] edgeNums = new int[3];

            // 1st and 2nd edge:
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

                for (int i = 0; i <= 11; i++)
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
                    if (sharesExcatlyOne[i] && !cube.EdgesTouching(cube.Edges[i], cube.Edges[edgeNums[0]]) && !cube.EdgesTouching(cube.Edges[i], cube.Edges[edgeNums[1]]))
                    {
                        possibleEdgeNums.Add(i);
                    }                        
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

                    if (faces2[i] != oppositeFaces[1])
                        notOppositeFaces[1] = faces2[i];
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

            // Randomizing points on already randomed edges:
            Point[] points = new Point[3];
            bool dashed;

            for (int i = 0; i <= 2; i++)
            {
                points[i] = GetRandomedPointOnEdge(random, cube.Edges[edgeNums[i]]);
                dashed = dashedHiddenCrosses ? Cube.EdgeDashed(edgeNums[i]) : false;

                // Visualizing points:               
                MarkPoint(c, cubeColor, points[i], lineLength, dashed);
            }

            // TODO (in future) ... so we have correct results available too, not just the problems

            // Slicing the cube:
            // This part of the code should highlight the sides of the polygon created by intersecting the cube and the plain defined by the 3 points in points[].
            // In Czech: Řez
            // Ideas: Class Problem - has Cube, Point[3], ProblemType (= if | else if | else in this function above), ...
            //        Then solve each type separately using two theorems (1. connecting points on the same face, 2. creating a parallel line on the opposite face).
            //        Note: The points[] generation was managed in such way so nothing else should be needed to solve the problems.
        }

        private static Cube DrawCube(Canvas c, int startX, int startY, float lLength, bool useDashing)
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

            // In order 0–11:
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

                if (e.Dashed && useDashing)
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
            float lineLength = 56;  // edge length of cube
            bool generateCubes = true; // generate cubes if possible?
            bool dashedHiddenLines = false; // dash hidden lines including crosses?

            Dictionary<string, string> p = Util.ParseKeyValueList(param);
            if (p.Count > 0)
            {
                // width=<float>
                if (Util.TryParse(p, "width", ref penWidth))
                    penWidth = (float)MathSupport.Arith.Clamp(penWidth, 1.0, 10.0);

                // anti=<bool>
                Util.TryParse(p, "anti", ref antialias);

                // seed=<int>
                Util.TryParse(p, "seed", ref seed);

                // a=<float>
                if (Util.TryParse(p, "a", ref lineLength))
                    lineLength = (float)MathSupport.Arith.Clamp(lineLength, 28.0, 196.0);

                // gen=<bool>
                Util.TryParse(p, "gen", ref generateCubes);

                // dash=<bool>
                Util.TryParse(p, "dash", ref dashedHiddenLines);
            }

            // Practical needed lengths:
            const int padding = 10;
            int cubeSize = (int)(1.5 * lineLength);
            int gap = padding + cubeSize;
            int actualHeight = c.Height - padding;
            int actualWidth = c.Width - gap;

            // Fun logging information (needed if no cubes are created):
            int cubes = 0;

            // Initialization:
            Color backgroundColor = Color.White;
            Color cubeColor = Color.Black;
            int currentX = padding;
            int currentY = gap;

            // Instancing the Random class:
            Random random = seed <= 0 ? new Random() : new Random(seed);

            // Setting the Canvas:
            c.Clear(backgroundColor);
            c.SetPenWidth(penWidth);
            c.SetColor(cubeColor);
            
            if (generateCubes)
            {
                Cube cube;
                while (currentY <= actualHeight)
                {
                    while (currentX <= actualWidth)
                    {
                        cube = DrawCube(c, currentX, currentY, lineLength, dashedHiddenLines);
                        GenerateCubeSlice(c, cube, random, cubeColor, dashedHiddenLines);
                        cubes++;

                        currentX += gap;
                    }

                    currentX = padding;
                    currentY += gap;
                }
            }            

            if (cubes == 0)
            {
                int segmentLength = 1;
                int mX = 1;
                int mY = -1;                
                currentX = c.Width / 2;
                currentY = c.Height / 2;
                int endX = currentX;
                int endY = currentY;
                bool moveVertically = true;

                while (endX >= 0 && endX <= c.Width && endY >= 0 && endY <= c.Height)
                {
                    if (moveVertically)
                    {
                        endY = currentY + mY * segmentLength;
                        c.Line(currentX, currentY, endX, endY);
                        mY *= -1;
                        currentY = endY;
                        moveVertically = false;
                    }
                    else
                    {
                        endX = currentX + mX * segmentLength;
                        c.Line(currentX, currentY, endX, endY);
                        mX *= -1;
                        currentX = endX;
                        segmentLength *= 2;
                        moveVertically = true;
                    }                    
                }

                // Cross.
                c.Line(0, 0, c.Width, c.Height);
                c.Line(c.Width, 0, 0, c.Height);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using MathSupport;

namespace _051colormap
{
    class KMeans
    {
        static Random random = new Random();
        byte[,] pixelCentroidIndeces;
        const int RGBmin = byte.MinValue;
        const int RGBmax = byte.MaxValue;
        byte freeCentroids;

        double[] centroidDistances;
        int[] centroidPixelCounts;
        int[] centroidTotalR;
        int[] centroidTotalG;
        int[] centroidTotalB;
        Color[] centroids;

        private Color[] GetUsedCentroids()
        {
            List<Color> usedColor = new List<Color>();

            for (int i = 0; i < centroids.Length; i++)
            {
                if (CentroidValid(i))
                {
                    usedColor.Add(centroids[i]);
                }
            }

            return usedColor.ToArray();
        }

        public void OrderCentroids()
        {
            Color[] newCentroids = new Color[centroids.Length];
            int indexDarkest;

            for (int i = 0; i < centroids.Length; i++)
            {
                CalculateDistance(i, Color.Black);
            }

            for (int i = 0; i < centroids.Length; i++)
            {
                indexDarkest = GetMinDistanceIndex();
                newCentroids[i] = centroids[indexDarkest];
                centroidDistances[indexDarkest] = double.MaxValue;
            }

            newCentroids.CopyTo(centroids, 0);
        }

        public void UseFreeCentroids()
        {
            byte total = (byte)centroids.Length;
            byte free = freeCentroids;
            byte used = (byte)(total - free);

            Color[] usedCentroids = GetUsedCentroids();

            if (used == 1)
            {
                int r = usedCentroids[0].R;
                int g = usedCentroids[0].G;
                int b = usedCentroids[0].B;
                int newR, newG, newB;
                int dr = 5;
                int dg = 5;
                int db = 5;

                int invalidIndex = 0;
                int mutliplier = 1;
                bool failed = false;
                for (int i = 0; i < free; i++)
                {
                    newR = r + dr * mutliplier;
                    newG = g + dg * mutliplier;
                    newB = b + db * mutliplier;

                    if (newR < 0 || newR > 255 || newG < 0 || newG > 255 || newB < 0 || newB > 255)
                    {
                        if (failed)
                        {
                            mutliplier = 0;
                        }
                        else
                        {
                            failed = true;
                        }
                        i--;
                    }
                    else
                    {
                        failed = false;

                        while (CentroidValid(invalidIndex))
                        {
                            invalidIndex++;
                        }
                        centroids[invalidIndex] = Color.FromArgb((int)newR, (int)newG, (int)newB);
                        invalidIndex++;
                    }

                    mutliplier *= -1;
                    if (mutliplier > 0)
                    {
                        mutliplier++;
                    }
                }
            }
            else if (used == 2)
            {
                float r = usedCentroids[0].R;
                float g = usedCentroids[0].G;
                float b = usedCentroids[0].B;
                float dr = (usedCentroids[1].R - r) / (total - 1.0f);
                float dg = (usedCentroids[1].G - g) / (total - 1.0f);
                float db = (usedCentroids[1].B - b) / (total - 1.0f);

                int invalidIndex = 0;
                for (int i = 0; i < free; i++)
                {
                    r += dr;
                    g += dg;
                    b += db;

                    while (CentroidValid(invalidIndex))
                    {
                        invalidIndex++;
                    }
                    centroids[invalidIndex] = Color.FromArgb((int)r, (int)g, (int)b);
                    invalidIndex++;
                }
            }
            else
            {
                // Searching for two colors with most occurencies.
                int max = int.MinValue;
                int max2 = int.MinValue;
                int indexMax = -1, indexMax2 = -1;
                for (int i = 0; i < total; i++)
                {
                    if (centroidPixelCounts[i] > max)
                    {
                        max2 = max;
                        max = centroidPixelCounts[i];
                        indexMax2 = indexMax;
                        indexMax = i;
                    }
                    else if (centroidPixelCounts[i] > max2)
                    {
                        max2 = centroidPixelCounts[i];
                        indexMax2 = i;
                    }
                }


                float r = centroids[indexMax].R;
                float g = centroids[indexMax].G;
                float b = centroids[indexMax].B;
                float dr = (centroids[indexMax2].R - r) / (total - 1.0f);
                float dg = (centroids[indexMax2].G - g) / (total - 1.0f);
                float db = (centroids[indexMax2].B - b) / (total - 1.0f);

                int invalidIndex = 0;
                for (int i = 0; i < free; i++)
                {
                    r += dr;
                    g += dg;
                    b += db;

                    while (CentroidValid(invalidIndex))
                    {
                        invalidIndex++;
                    }
                    centroids[invalidIndex] = Color.FromArgb((int)r, (int)g, (int)b);
                    invalidIndex++;
                }
            }
        }

        public void MoveCentroid(int centroidIndex)
        {
            // Arithmetic mean of RGB of the centroid:
            byte r = (byte)Math.Round(1.0 * centroidTotalR[centroidIndex] / centroidPixelCounts[centroidIndex]);
            byte g = (byte)Math.Round(1.0 * centroidTotalG[centroidIndex] / centroidPixelCounts[centroidIndex]);
            byte b = (byte)Math.Round(1.0 * centroidTotalB[centroidIndex] / centroidPixelCounts[centroidIndex]);

            centroids[centroidIndex] = Color.FromArgb(r, g, b);
        }

        public void AddRGBToFirstCentroid(Color pixel)
        {
            this.centroidTotalR[0] += pixel.R;
            this.centroidTotalG[0] += pixel.G;
            this.centroidTotalB[0] += pixel.B;
        }

        public byte GetFreeCentroidsCount()
        {
            return freeCentroids;
        }

        public void InvalidateCentroid(int centroidIndex)
        {
            this.centroids[centroidIndex] = Color.FromArgb(0, 0, 0, 0);
            this.freeCentroids++;
        }

        public bool CentroidListEmpty(int centroidIndex)
        {
            return this.centroidPixelCounts[centroidIndex] == 0;
        }

        public void ChangePixelCentroidIndex(int pixelX, int pixelY, Color pixel, byte newIndex)
        {
            if (newIndex > centroids.Length)
            {
                throw new ArgumentException("Centroid's index is out of range in {0} procedure!", nameof(ChangePixelCentroidIndex));
            }

            byte oldIndex = this.pixelCentroidIndeces[pixelX, pixelY];

            this.centroidPixelCounts[oldIndex]--;
            this.centroidTotalR[oldIndex] -= pixel.R;
            this.centroidTotalG[oldIndex] -= pixel.G;
            this.centroidTotalB[oldIndex] -= pixel.B;

            this.centroidPixelCounts[newIndex]++;
            this.centroidTotalR[newIndex] += pixel.R;
            this.centroidTotalG[newIndex] += pixel.G;
            this.centroidTotalB[newIndex] += pixel.B;

            this.pixelCentroidIndeces[pixelX, pixelY] = newIndex;
        }

        public int GetPixelCentroidIndex(int pixelX, int pixelY)
        {
            return pixelCentroidIndeces[pixelX, pixelY];
        }

        public byte GetMinDistanceIndex()
        {
            double min = double.MaxValue;
            byte index = byte.MaxValue;
            for (byte i = 0; i < centroidDistances.Length; i++)
            {
                if (centroidDistances[i] < min && CentroidValid(i))
                {
                    min = centroidDistances[i];
                    index = i;
                }                
            }            
            return index;
        }

        public Color[] GetCentroids()
        {
            return this.centroids;
        }

        public void RandomCentroids()
        {
            bool valid;
            byte r, g, b;

            for (int i = 0; i < this.centroids.Length; i++)
            {
                do
                {
                    r = (byte)random.Next(RGBmin, RGBmax + 1);
                    g = (byte)random.Next(RGBmin, RGBmax + 1);
                    b = (byte)random.Next(RGBmin, RGBmax + 1);

                    valid = true;
                    for (int j = 0; j < i; j++)
                    {
                        if (centroids[i].R == centroids[j].R && centroids[i].G == centroids[j].G && centroids[i].B == centroids[j].B)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                while (!valid);
                centroids[i] = Color.FromArgb((int)r, (int)g, (int)b);
            }            
        }

        public bool CentroidValid(int centroidIndex)
        {
            if (centroidIndex < 0 || centroidIndex >= centroids.Length)
            {
                throw new ArgumentException("Centroid's index is out of range in {0} function!", nameof(CentroidValid));
            }

            return this.centroids[centroidIndex].A == 255;
        }

        public void CalculateDistance(int centroidIndex, Color color)
        {
            double r = this.centroids[centroidIndex].R - color.R;
            double g = this.centroids[centroidIndex].G - color.G;
            double b = this.centroids[centroidIndex].B - color.B;

            double dist = Math.Sqrt(r * r + g * g + b * b);

            this.centroidDistances[centroidIndex] = dist;
        }

        public KMeans(int width, int height, int numberOfCentroids)
        {
            this.pixelCentroidIndeces = new byte[width, height];  // max 250 kB (500 × 500 pixels image)
            this.centroids = new Color[numberOfCentroids];
            this.centroidPixelCounts = new int[numberOfCentroids];
            this.centroidPixelCounts[0] = this.pixelCentroidIndeces.Length; // In 0th iteration every pixel belongs to the centroid of index 0.
            this.centroidTotalR = new int[numberOfCentroids];
            this.centroidTotalG = new int[numberOfCentroids];
            this.centroidTotalB = new int[numberOfCentroids];
            this.centroidDistances = new double[numberOfCentroids];
            this.freeCentroids = 0;     // None centroids are invalid at the start.
        }
    }
    class Colormap
    {
        /// <summary>
        /// Form data initialization.
        /// </summary>
        public static void InitForm(out string author)
        {
            author = "Lukáš Macek";
        }

        /// <summary>
        /// Generate a colormap based on input image.
        /// </summary>
        /// <param name="input">Input raster image.</param>
        /// <param name="numCol">Required colormap size (ignore it if you must).</param>
        /// <param name="centroids">Output palette (array of colors).</param>
        public static void Generate(Bitmap input, int numCol, out Color[] centroids)
        {
            int maxIterationCount = 50;     // new parameter (time <-> accuracy)

            int width = input.Width;
            int height = input.Height;

            KMeans kms = new KMeans(width, height, numCol);
            kms.RandomCentroids();

            bool cont = true;
            bool firstIteration = true;
            byte cIndex;

            int iterationNum = 0;
            while (cont)
            {
                iterationNum++;
                if (iterationNum > maxIterationCount)
                {
                    // Defense against infinite cycles.
                    break;
                }

                cont = false;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        // For every pixel:
                        Color p = input.GetPixel(i, j);
                        for (int k = 0; k < numCol; k++)
                        {                            
                            if (kms.CentroidValid(k))
                            {
                                kms.CalculateDistance(k, p);
                            }                            
                        }
                        cIndex = kms.GetMinDistanceIndex();
                        if (kms.GetPixelCentroidIndex(i, j) != cIndex)
                        {
                            kms.ChangePixelCentroidIndex(i, j, p, cIndex);
                            cont = true;
                        }
                        else if (firstIteration)
                        {
                            kms.AddRGBToFirstCentroid(p);
                            // PixelCount is defaulty asign to the first centroid.
                        }
                    }
                }

                for (int i = 0; i < numCol; i++)
                {
                    if (kms.CentroidValid(i))
                    {
                        if (!kms.CentroidListEmpty(i))
                        {
                            kms.MoveCentroid(i);
                        }
                        else
                        {
                            kms.InvalidateCentroid(i);
                        }
                    }
                }
                
                firstIteration = false;
            }

            kms.UseFreeCentroids();
            kms.OrderCentroids();

            centroids = kms.GetCentroids();
            
            // TODO: Repeating -> take more valid centroids, do some averaging.
            // TODO: Fast access for pixels.
        }
    }
}

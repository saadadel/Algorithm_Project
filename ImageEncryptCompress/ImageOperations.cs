using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
       /* public static int shift(ref int initialseed, int len, int pos)
        {
            int start, sec, end, l;
            int[] xor = new int[8];
            for (int i = 0; i < 8; i++)
            {
                start = (initialseed & (1 << (len - 1)));
                start = (start >> (len - 1));
                sec = (initialseed & (1 << pos));
                sec = (sec >> (pos));

                end = (start ^ sec);

                initialseed = (initialseed << 1);
                initialseed = (initialseed | end);
                xor[i] = end;
                l = (1 << len) - 1;
                initialseed = (initialseed & l);
            }
            int sum = 0;
            for (int i = 0; i < 8; i++)
            {
                if (xor[i] == 1)
                {
                    sum += 1 * (int)Math.Pow(2, 7 - i);
                }


            }
            return sum;


        }*/


        public static byte NewWorkingShift(ref string seed, int tap)
        {
            int length = seed.Length;
            string newSeed = "";
            for (int i = 1; i < length; i++)
                newSeed += seed[i];

            newSeed += Convert.ToInt32(seed[0] - 48) ^ Convert.ToInt32(seed[tap - 1] - 48);
            byte seed8bits;
            if (length >= 8)
                seed8bits = Convert.ToByte(newSeed.Substring(length - 8, 8), 2);
            else
                seed8bits = Convert.ToByte(newSeed, 2);
            seed = newSeed;
            return seed8bits;

        }
        public static RGBPixel[,] incrept(RGBPixel[,] ImageMatrix, ref string initialseed, int len, int pos)
        {
            Stopwatch stp = new Stopwatch();
            stp.Start();
            int xor = 0;
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixel[,] filter = new RGBPixel[Height, Width];
            //===================================
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    xor = NewWorkingShift(ref initialseed, pos);
                    filter[i, j].red = (byte)(ImageMatrix[i, j].red ^ xor);


                    xor = NewWorkingShift(ref initialseed, pos);
                    filter[i, j].green = (byte)(ImageMatrix[i, j].green ^ xor);

                    xor = NewWorkingShift(ref initialseed, pos);
                    filter[i, j].blue = (byte)(ImageMatrix[i, j].blue ^ xor);
                }
            }

            stp.Stop();
            TimeSpan ts = stp.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            MessageBox.Show("Encryption runtime " + elapsedTime);


            return filter;
        }
        public static void saveinbinaryfile(Dictionary<byte, string> red, Dictionary<byte, string> green, Dictionary<byte, string> blue, RGBPixel[,] ImageMatrix,string path)
        {

            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            byte savebyte = 0;
            int countstring = 0;
            int lengthofstring = 0;
            int countbit = 0;
            string ff;
            BinaryWriter br_writer;
            br_writer = new BinaryWriter(new FileStream(path, FileMode.Append));
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    lengthofstring = red[ImageMatrix[i, j].red].Length;
                    ff = red[ImageMatrix[i, j].red];
                    while (countbit < 8)
                    {
                        savebyte = (byte)(savebyte << 1);
                        savebyte = (byte)(savebyte | red[ImageMatrix[i, j].red][countstring] - '0');
                        countstring++;
                        countbit++;
                        if (countbit == 8)
                        {
                            br_writer.Write(savebyte);
                            savebyte = 0;
                            countbit = 0;
                        }
                        if (countstring == lengthofstring)
                        {
                            countstring = 0;
                            lengthofstring = 0;
                            break;
                        }
                    }

                }
            }
            if (savebyte != 0)
            {
                br_writer.Write(savebyte);
                savebyte = 0;
            }

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    lengthofstring = green[ImageMatrix[i, j].green].Length;
                    ff = green[ImageMatrix[i, j].green];
                    while (countbit < 8)
                    {
                        savebyte = (byte)(savebyte << 1);
                        savebyte = (byte)(savebyte | green[ImageMatrix[i, j].green][countstring] - '0');
                        countstring++;
                        countbit++;
                        if (countbit == 8)
                        {
                            br_writer.Write(savebyte);
                            savebyte = 0;
                            countbit = 0;
                        }
                        if (countstring == lengthofstring)
                        {
                            countstring = 0;
                            lengthofstring = 0;
                            break;
                        }
                    }

                }
            }
            if (savebyte != 0)
            {
                br_writer.Write(savebyte);
                savebyte = 0;
            }


            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    lengthofstring = blue[ImageMatrix[i, j].blue].Length;
                    ff = blue[ImageMatrix[i, j].blue];
                    while (countbit < 8)
                    {
                        savebyte = (byte)(savebyte << 1);
                        savebyte = (byte)(savebyte | blue[ImageMatrix[i, j].blue][countstring] - '0');
                        countstring++;
                        countbit++;
                        if (countbit == 8)
                        {
                            br_writer.Write(savebyte);
                            savebyte = 0;
                            countbit = 0;
                        }
                        if (countstring == lengthofstring)
                        {
                            countstring = 0;
                            lengthofstring = 0;
                            break;
                        }
                    }

                }
            }
            if (savebyte != 0)
            {
                br_writer.Write(savebyte);
                savebyte = 0;
            }
            br_writer.Close();
        }
        public static void getdic(ref Dictionary<string, byte> dec, string s, ref int index, string code)
        {
            if (index == s.Length)
            {
                return;
            }
            if (s[index] == '1')
            {
                byte b = Convert.ToByte(s.Substring(index + 1, 8), 2);
                index += 8;
                dec.Add(code, b);
                return;
            }
            index += 1;
            getdic(ref dec, s, ref index, code + "0");
            index += 1;
            getdic(ref dec, s, ref index, code + "1");
        }

    }
}

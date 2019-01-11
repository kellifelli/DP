using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System.Linq;

namespace DP
{
    class Test
    {
        public static void Rename(string slozka)
        {
            string[] directories = Directory.GetDirectories(slozka);
            for (int i = 1; i <= directories.Length; i++)
            {
                string[] seznamObrazku = Directory.GetFiles(directories[i - 1], "*.png", SearchOption.TopDirectoryOnly);

                for (int j = 0; j < seznamObrazku.Length; j++)
                {
                    Bitmap image = (Bitmap)Bitmap.FromFile(seznamObrazku[j]);
                    string nazev = ObecneMetody.DatumCasZNazvu(seznamObrazku[j], "\\", ".png");

                    image.Save("D:\\backup\\DP\\miskyVjednom\\" + i.ToString("D3") + "_" + nazev + ".png");
                }


            }
        }
    }
}


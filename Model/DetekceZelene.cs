using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace DP
{
    class DetekceZelene
    {

        static string ZiskejNazev(string obrazek)
        {
            string prvni_slovo = "\\";
            string druhe_slovo = ".png";
            int first = obrazek.LastIndexOf(prvni_slovo);
            int last = obrazek.LastIndexOf(druhe_slovo);
            string bezRound = obrazek.Substring(0, last);
            string bezFirst = obrazek.Substring(0, first + prvni_slovo.Length);

            return obrazek.Substring(bezFirst.Length, bezRound.Length - bezFirst.Length);
        }

        static Bitmap DejRozdilObrazku(string obrazek1, string obrazek2, Color diffColor)
        {
            Bitmap img1 = (Bitmap)Bitmap.FromFile(obrazek1);
            Bitmap img2 = (Bitmap)Bitmap.FromFile(obrazek2);
            Bitmap img3 = new Bitmap(img1.Width - 2, img1.Height - 2);




            for (int y = 0; y < img3.Height; y++)
                for (int x = 0; x < img3.Width; x++)
                {
                    Color c1 = img1.GetPixel(x, y);
                    Color c2 = img2.GetPixel(x, y);
                    if (c1 == c2) img3.SetPixel(x, y, c1);
                    else img3.SetPixel(x, y, diffColor);
                }


            return img3;


        }


        public static void NajdiCasRustu(string slozka)
        {

            string[] seznamObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);

            Console.WriteLine("prochazim misku");

            for (int i = 1; i < seznamObrazku.Length; i++)
            {
                Bitmap dif = DejRozdilObrazku(seznamObrazku[i - 1], seznamObrazku[i], Color.Transparent);
                Directory.CreateDirectory(slozka + "test\\");
                dif.Save(slozka + "test\\" + i + ".png");
            }


        }
    }
}
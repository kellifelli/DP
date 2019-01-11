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
        public static void NajdiCasRustu(string slozka)
        {
            string[] directories = Directory.GetDirectories(slozka);
            //NajdiCasRustuVmisce(directories[19], 19);
            for (int i = 1; i <= directories.Length; i++)
            {
                NajdiCasRustuVmisce(directories[i - 1], i);
            }
 
            /* 
            string[] seznamObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);

            Console.WriteLine("prochazim misku");

            for (int i = 1; i < seznamObrazku.Length; i++)
            {
                Bitmap dif = DejRozdilObrazku(seznamObrazku[i - 1], seznamObrazku[i], Color.Transparent);
                Directory.CreateDirectory(slozka + "test\\");
                dif.Save(slozka + "test\\" + i + ".png");
            }*/


        }

        public static void NajdiCasRustuVmisce(string slozka, int i)
        {

            /* Definice rozostøovacího filtru */
            GaussianBlur filterBlur = new GaussianBlur(10, 10);
            
            /* Definice modrého filtru */
            ColorFiltering filterGreen = new ColorFiltering();
            filterGreen.Red = new IntRange(55, 80);
            filterGreen.Green = new IntRange(55, 100);
            filterGreen.Blue = new IntRange(40, 65);


            /* Naètu si obrazky za každou misku */
            string[] seznamObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            /* Bitmap image1 = (Bitmap)Bitmap.FromFile(seznamObrazku[42]);
             Bitmap image2 = (Bitmap)Bitmap.FromFile(seznamObrazku[43]);
             string nazevObr = ObecneMetody.DatumCasZNazvu(seznamObrazku[42], "\\", ".png");
             filterBlur.ApplyInPlace(image1);
             image1.Save(slozka + "\\" + nazevObr + "_sedy.png");*/

            foreach (string obrazek in seznamObrazku)
            {

                Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
                string nazevObr = ObecneMetody.DatumCasZNazvu(obrazek, "\\", ".png");
                filterBlur.ApplyInPlace(image);
                //filterGreen.ApplyInPlace(image);
                image.Save(slozka + "\\" + nazevObr + "_sedy.png");
                //Console.WriteLine(obrazek);
            }


        }


    }
}
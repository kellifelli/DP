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
    class Program
    {
        static void Main(string[] args)
        {
            // string slozka = "temp\\01\\";
            // string orezanyObrazky = OrezKraju.OrezaniObrazkuVeSlozce(slozka);

            //ted mam v temp\\orezany\\01 orezany obrazky a ve filePaths mam jejich nazvy
            //RozrezaniNaMisky.RozrezObrazkyVeSlozce(orezanyObrazky);

            // D:\\repos\\DP\\DP\\temp\\01\\orezany
            /* Console.WriteLine("zadejte cestu: ");
             string cesta = Console.ReadLine();
  */
            RozrezaniNaMisky.RozrezObrazkyVeSlozce("D:\\repos\\DP\\DP\\temp\\01\\orezany");
            Console.ReadKey(true);
            //Test.NactiBody();
        }

    }
}

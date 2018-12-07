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

            string[] filePaths = Directory.GetFiles("temp\\01\\", "*.png", SearchOption.TopDirectoryOnly);
            OrezKraju.OrezaniObrazkuVeSlozce(filePaths);
            //ted mam v temp\\orezany\\01 orezany obrazky a ve filePaths mam jejich nazvy
            string[] orezanyObrazky = Directory.GetFiles("temp\\orezany\\01\\", "*.png", SearchOption.TopDirectoryOnly);
            RozrezaniNaMisky.DetekceKrizku(orezanyObrazky);


        }

    }
}

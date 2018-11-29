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
            OrezaniObrazkuVeSlozce(filePaths);

        }

        /*Metoda která dostane jako vstup pole s nazvy souboru a pokusí se načíst všechny obrazky a ořezat a uložit je do nové složky*/

        static void OrezaniObrazkuVeSlozce(string[] directory)
        {
            int u = 1;
            foreach (string obrazek in directory)
            {

                Bitmap bezModreho = ModryOkraj(obrazek);
                bezModreho.Save("output\\01\\" + u + ".png", ImageFormat.Png);
                u++;
            }
        }
        static Bitmap ModryOkraj(string obrazek)
        {

            // nactu obrazek
            Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
            Bitmap puvodniImage = (Bitmap)Bitmap.FromFile(obrazek);

            //vytahnuti modre
            ColorFiltering filterBlue = new ColorFiltering();
            filterBlue.Red = new IntRange(18, 35);
            filterBlue.Green = new IntRange(23, 40);
            filterBlue.Blue = new IntRange(40, 85);
            filterBlue.ApplyInPlace(image);
            //rozostreni
            GaussianBlur filterBlur = new GaussianBlur(4, 11);
            filterBlur.ApplyInPlace(image);

            BlobCounter bc = new BlobCounter();

            bc.FilterBlobs = true;
            bc.MinHeight = 500;
            bc.MinWidth = 500;

            bc.ProcessImage(image);

            Blob[] blobs = bc.GetObjectsInformation();

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = bc.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;

                // use the shape checker to extract the corner points
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                {
                    // only do things if the corners form a rectangle
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
                    {
                        // here i use the graphics class to draw an overlay, but you
                        // could also just use the cornerPoints list to calculate your
                        // x, y, width, height values.
                        List<System.Drawing.Point> Points = new List<System.Drawing.Point>();
                        foreach (var point in cornerPoints)
                        {
                            Points.Add(new System.Drawing.Point(point.X, point.Y));
                        }

                        /* uauauauauauaua */
                        System.Drawing.Point[] poleRohu = new System.Drawing.Point[Points.Count];
                        poleRohu = Points.ToArray();

                        /* Tady zpracuju rohove body a vypocitam si vzdalenosti */
                        int novaSirka = (poleRohu[2].X - poleRohu[0].X);
                        int novaVyska = (poleRohu[3].Y - poleRohu[1].Y);
                        int noveX = poleRohu[0].X;
                        int noveY = poleRohu[1].Y;

                        // vyrezu obrazek
                        Rectangle vyrez = new Rectangle(noveX, noveY, novaSirka, novaVyska);
                        //odrezu modrou barvu do image - puvodni barva
                        puvodniImage = puvodniImage.Clone(vyrez, puvodniImage.PixelFormat);

                        vyrez = new Rectangle(0, 0, puvodniImage.Width, puvodniImage.Height);
                        //do puvodniho image ulozim copii orezanyho
                        image = puvodniImage.Clone(vyrez, puvodniImage.PixelFormat);

                    }
                }
            }

            //vytahni barvu
            ColorFiltering filterBrown = new ColorFiltering();
            filterBrown.Red = new IntRange(35, 45);
            filterBrown.Green = new IntRange(32, 42);
            filterBrown.Blue = new IntRange(30, 40);
            filterBrown.ApplyInPlace(image);

            filterBlur.ApplyInPlace(image);


            bc.FilterBlobs = true;
            bc.MinHeight = 500;
            bc.MinWidth = 500;

            bc.ProcessImage(image);
            blobs = bc.GetObjectsInformation();
            shapeChecker = new SimpleShapeChecker();
            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = bc.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                {
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
                    {
                        List<System.Drawing.Point> Points = new List<System.Drawing.Point>();
                        foreach (var point in cornerPoints)
                        {
                            Points.Add(new System.Drawing.Point(point.X, point.Y));
                        }

                        /* uauauauauauaua */
                        System.Drawing.Point[] poleRohu = new System.Drawing.Point[Points.Count];
                        poleRohu = Points.ToArray();

                        /* Tady zpracuju rohove body a vypocitam si vzdalenosti */
                        int novaSirka = (poleRohu[2].X - poleRohu[0].X);
                        int novaVyska = (poleRohu[3].Y - poleRohu[1].Y);
                        int noveX = poleRohu[0].X;
                        int noveY = poleRohu[1].Y;

                        // vyrezu obrazek
                        Rectangle vyrez = new Rectangle(noveX, noveY, novaSirka, novaVyska);
                        // ulozim vyrezany
                        image = puvodniImage.Clone(vyrez, puvodniImage.PixelFormat);

                    }
                }
            }
            return image;
        }
    }
}

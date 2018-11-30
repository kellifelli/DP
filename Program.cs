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
            //OrezaniObrazkuVeSlozce(filePaths);
            //ted mam v temp\\orezany\\01 orezany obrazky a ve filePaths mam jejich nazvy
            DetekceKrizku();



        }

        static void DetekceKrizku()
        {
            
            Bitmap obrazek = (Bitmap)Bitmap.FromFile("temp\\orezany\\01\\1.png");
            Bitmap vzor = (Bitmap)Bitmap.FromFile("temp\\1_vzor.png");

            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.921f);
            
            TemplateMatch[] matchings = tm.ProcessImage(obrazek, vzor);

            BitmapData data = obrazek.LockBits(
                                new Rectangle(0, 0, obrazek.Width, obrazek.Height),
                                ImageLockMode.ReadWrite, obrazek.PixelFormat);
            foreach (TemplateMatch m in matchings)
            {

                Drawing.Rectangle(data, m.Rectangle, Color.White);

                Console.WriteLine(m.Rectangle.Location.ToString());
            
            }
            obrazek.UnlockBits(data);

            

        }


























        /*Metoda která dostane jako vstup pole s nazvy souboru a pokusí se načíst všechny obrazky a ořezat a uložit je do nové složky*/

        static void OrezaniObrazkuVeSlozce(string[] directory)
        {
            var hodiny = System.Diagnostics.Stopwatch.StartNew();

            //Bitmap bezModreho = ModryOkraj(obrazek);
            int[] rozmery = ModryOkraj(directory[0]);
            int u = 1;
            foreach (string obrazek in directory)
            {
                var hodinyObrazku = System.Diagnostics.Stopwatch.StartNew();
                // vyrezu obrazek
                Rectangle vyrez = new Rectangle(rozmery[0], rozmery[1], rozmery[2], rozmery[3]);
                // ulozim vyrezany
                Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
                Bitmap bezModreho = image.Clone(vyrez, image.PixelFormat);
                bezModreho.Save("temp\\orezany\\01\\" + u + ".png", ImageFormat.Png);

                hodinyObrazku.Stop();
                Console.WriteLine("To byl " + u + "/" + directory.Length + " obrazek. Trval " + hodinyObrazku.Elapsed.TotalSeconds + ". Celkem uz jedu " + hodiny.Elapsed.TotalSeconds + " sekund.");
                u++;
            }

            hodiny.Stop();
            Console.WriteLine("Celej set o " + directory.Length + " obrazcich trval: " + hodiny.Elapsed.TotalSeconds + " sekund. (V minutách: " + hodiny.Elapsed.TotalMinutes + ".)");
        }
        static int[] ModryOkraj(string obrazek)
        {

            // nactu obrazek
            Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
            Bitmap puvodniImage = (Bitmap)Bitmap.FromFile(obrazek);
            //
            int[] rozmery = new int[4];
            int noveX = 0;
            int noveY = 0;

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
                        noveX = poleRohu[0].X;
                        noveY = poleRohu[1].Y;

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
                        rozmery[2] = (poleRohu[2].X - poleRohu[0].X);
                        rozmery[3] = (poleRohu[3].Y - poleRohu[1].Y);
                        rozmery[0] = poleRohu[0].X + noveX;
                        rozmery[1] = poleRohu[1].Y + noveY;

                    }
                }
            }
            return rozmery;
        }
    }
}

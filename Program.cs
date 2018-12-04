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
            string[] filePaths = Directory.GetFiles("temp\\orezany\\01\\", "*.png", SearchOption.TopDirectoryOnly);
            //OrezaniObrazkuVeSlozce(filePaths);
            //ted mam v temp\\orezany\\01 orezany obrazky a ve filePaths mam jejich nazvy
            DetekceKrizku(filePaths);


        }

        static void DetekceKrizku(string[] slozka)
        {
            Bitmap vzor = (Bitmap)Bitmap.FromFile("temp\\1_vzor.png");
            int u = 1;
            foreach (string soubor in slozka)
            {
                var hodiny = System.Diagnostics.Stopwatch.StartNew();
                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);


                Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
                // apply the filter
                Bitmap obrazekSedy = filter.Apply(obrazek);
                Bitmap vzorSedy = filter.Apply(vzor);



                // create filter
                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width * 7 / 12, obrazek.Height * 7 / 12);
                ResizeBilinear filterSize2 = new ResizeBilinear(vzor.Width * 7 / 12, vzor.Height * 7 / 12);
                // apply the filter
                obrazekSedy = filterSize1.Apply(obrazekSedy);
                vzorSedy = filterSize2.Apply(vzorSedy);
                obrazek = filterSize1.Apply(obrazek);

                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.962f);
                TemplateMatch[] matchings = tm.ProcessImage(obrazekSedy, vzorSedy);

                BitmapData data = obrazek.LockBits(
                                    new Rectangle(0, 0, obrazek.Width, obrazek.Height),
                                    ImageLockMode.ReadWrite, obrazek.PixelFormat);
                //int[] souradniceX = new int[9];
                //int[] souradniceY = new int[10];

                List<int> souradniceX = new List<int>();
                List<int> souradniceY = new List<int>();


                foreach (TemplateMatch m in matchings)
                {

                    int x = m.Rectangle.Location.X + m.Rectangle.Width / 2;
                    int y = m.Rectangle.Location.Y + m.Rectangle.Height / 2;
                    souradniceX.Add(x);
                    souradniceY.Add(y);

                    //Drawing.Rectangle(data, m.Rectangle, Color.Red);
                    //vykresleni bodu - stredu nalezeneho krizku

                    Drawing.Rectangle(data, new Rectangle(x, y, 1, 1), Color.Red);

                }

                obrazek.UnlockBits(data);
                obrazek.Save("temp\\krizky\\" + u + ".png");

                hodiny.Stop();
                Console.WriteLine(u + "ty obrazek trval" + hodiny.Elapsed.TotalSeconds);
                u++;

                souradniceX.Sort();
                souradniceY.Sort();
                //2d pole v prvnim sloupci je hodnota x stredu a ve druhem je hodnota sloupce ve kterem se nachazi
                int[,] souradniceXX = new int[souradniceX.Count, 2];
                int sloupec = 1;
                for (int i = 0; i < souradniceX.Count; i++)
                {
                    souradniceXX[i, 0] = souradniceX[i];
                    souradniceXX[i, 1] = sloupec;
                    if (i + 1 < souradniceX.Count)
                    {
                        if (souradniceX[i + 1] - souradniceX[i] > 10)
                        {
                            sloupec++;
                        }
                    }
                }

                int[,] souradniceYY = new int[souradniceY.Count, 2];
                int radek = 1;
                for (int i = 0; i < souradniceY.Count; i++)
                {
                    souradniceYY[i, 0] = souradniceY[i];
                    souradniceYY[i, 1] = radek;
                    if (i + 1 < souradniceY.Count)
                    {
                        if (souradniceY[i + 1] - souradniceY[i] > 10)
                        {
                            radek++;
                        }
                    }
                    //  Console.WriteLine("y: " + souradniceYY[i, 0] + "je ve radku " + souradniceYY[i, 1]);
                }

                int prumer = 0;
                int suma = 0;
                int pocet = 0;
                List<int> prumeryX = new List<int>();
                List<int> prumeryY = new List<int>();


                for (int i = 1; i < souradniceX.Count; i++)
                {

                    if (souradniceXX[i, 1] == souradniceXX[i - 1, 1])
                    {
                        suma = suma + souradniceXX[i - 1, 0];
                        pocet++;
                    }
                    else
                    {
                        prumer = (int)(suma / pocet);
                        prumeryX.Add(prumer);
                        suma = 0;
                        pocet = 0;
                    }


                }
                suma = 0;
                pocet = 0;
                for (int i = 1; i < souradniceY.Count; i++)
                {
                    if (souradniceYY[i, 1] == souradniceYY[i - 1, 1])
                    {
                        suma = suma + souradniceYY[i - 1, 0];
                        pocet++;
                    }
                    else
                    {
                        prumer = (int)(suma / pocet);
                        prumeryY.Add(prumer);
                        suma = 0;
                        pocet = 0;
                    }


                }


                for (int i = 0; i < prumeryX.Count; i++)
                {
                    Console.WriteLine(prumeryX[i]);
                }
                Console.WriteLine();
                for (int i = 0; i < prumeryY.Count; i++)
                {
                    Console.WriteLine(prumeryY[i]);
                }

                if (u > 1) { break; }

            }
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

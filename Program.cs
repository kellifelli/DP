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
            //ted mam v temp\\orezany\\01 orezany obrazky a ve filePaths mam jejich nazvy
            string[] orezanyObrazky = Directory.GetFiles("temp\\orezany\\01\\", "*.png", SearchOption.TopDirectoryOnly);
            DetekceKrizku(orezanyObrazky);


        }

        static void DetekceKrizku(string[] slozka)
        {
            Bitmap vzor = (Bitmap)Bitmap.FromFile("temp\\1_vzor.png");

            int u = 1;
            foreach (string soubor in slozka)
            {
                var hodiny = System.Diagnostics.Stopwatch.StartNew();
                //načtení obrazku
                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);
                // Definice filtrů - šedá, velikost
                Grayscale filterSeda = new Grayscale(0.2125, 0.7154, 0.0721);
                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width * 7 / 12, obrazek.Height * 7 / 12);
                ResizeBilinear filterSize2 = new ResizeBilinear(vzor.Width * 7 / 12, vzor.Height * 7 / 12);


                //aplikace šedého filtru
                Bitmap obrazekSedy = filterSeda.Apply(obrazek);
                Bitmap vzorSedy = filterSeda.Apply(vzor);
                //zmenšení obrazků
                obrazekSedy = filterSize1.Apply(obrazekSedy);
                vzorSedy = filterSize2.Apply(vzorSedy);
                obrazek = filterSize1.Apply(obrazek);


                //vyhledavácí alg - v ObrazekSedy vyhleda výskyty vzorSedy
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.962f);
                TemplateMatch[] matchings = tm.ProcessImage(obrazekSedy, vzorSedy);

                BitmapData data = obrazek.LockBits(new Rectangle(0, 0, obrazek.Width, obrazek.Height),
                                    ImageLockMode.ReadWrite, obrazek.PixelFormat);

                //definice listů - souřadnice x a y nalezených křížků
                List<int> souradniceX = new List<int>();
                List<int> souradniceY = new List<int>();

                //projdu nalezené křížky a vypočítané souřadnice středů přidám do přiravených listů 
                foreach (TemplateMatch m in matchings)
                {
                    int x = m.Rectangle.Location.X + m.Rectangle.Width / 2;
                    int y = m.Rectangle.Location.Y + m.Rectangle.Height / 2;
                    souradniceX.Add(x);
                    souradniceY.Add(y);
                }

                //souřadnice je třeba setřídit - při rozdělovaní do sloupců (řádků) poznám jeden sloupec(řádek) tak, že vzdálenost není moc velká, sloupce (řádky) jsou cca 90px široké
                souradniceX.Sort();
                souradniceY.Sort();

                //2d pole v prvnim sloupci je hodnota x stredu a ve druhem je hodnota sloupce ve kterem se nachazi
                int[,] souradniceXX = new int[souradniceX.Count, 2];
                int[,] souradniceYY = new int[souradniceY.Count, 2];
                int sloupec = 1;
                int radek = 1;

                //cyklem projdu oba pomocné listy (jsou stejně velké) a určím pro každý bod jeho sloupec
                for (int i = 0; i < souradniceX.Count; i++)
                {
                    souradniceXX[i, 0] = souradniceX[i];
                    souradniceXX[i, 1] = sloupec;
                    souradniceYY[i, 0] = souradniceY[i];
                    souradniceYY[i, 1] = radek;
                    if (i + 1 < souradniceX.Count) // krome posledniho overuju vzdalenost
                    {
                        if (souradniceX[i + 1] - souradniceX[i] > 10)
                        {
                            sloupec++;
                        }
                        if (souradniceY[i + 1] - souradniceY[i] > 10)
                        {
                            radek++;
                        }
                    }
                    //  Console.WriteLine("y: " + souradniceYY[i, 0] + "je ve radku " + souradniceYY[i, 1]);
                }
                /* ted pro seznam X(Y) souradnic (o kazde vim z ktereho je sloupe/radku) vypocitam prumer za sloupec/radek a tim ziskam informaci o jakemkoliv bodu  */
                int prumer = 0;
                int suma = 0;
                int pocet = 0;
                List<int> prumeryX = new List<int>();
                List<int> prumeryY = new List<int>();


                for (int i = 0; i < souradniceX.Count; i++)
                {
                    if ((i + 1) < souradniceX.Count)
                    {
                        if (souradniceXX[i, 1] != souradniceXX[i + 1, 1])
                        {
                            suma = suma + souradniceXX[i, 0];
                            pocet++;
                            prumer = (int)(suma / pocet);
                            prumeryX.Add(prumer);
                            suma = 0;
                            pocet = 0;
                        }
                        else
                        {
                            suma = suma + souradniceXX[i, 0];
                            pocet++;
                        }
                    }
                    else
                    {
                        suma = suma + souradniceXX[i, 0];
                        pocet++;
                        prumer = (int)(suma / pocet);
                        prumeryX.Add(prumer);

                    }
                }
                suma = 0;
                pocet = 0;
                for (int i = 0; i < souradniceY.Count; i++)
                {
                    if ((i + 1) < souradniceY.Count)
                    {
                        if (souradniceYY[i, 1] != souradniceYY[i + 1, 1])
                        {
                            suma = suma + souradniceYY[i, 0];
                            pocet++;
                            prumer = (int)(suma / pocet);
                            prumeryY.Add(prumer);
                            suma = 0;
                            pocet = 0;
                        }
                        else
                        {
                            suma = suma + souradniceYY[i, 0];
                            pocet++;
                        }
                    }
                    else
                    {
                        suma = suma + souradniceYY[i, 0];
                        pocet++;
                        prumer = (int)(suma / pocet);
                        prumeryY.Add(prumer);

                    }


                }

                List<int[]> krizkyOfiko = new List<int[]>();
                for (int i = 0; i < prumeryX.Count; i++)
                {
                    for (int j = 0; j < prumeryY.Count; j++)
                    {
                        int[] bod = new int[2];
                        bod[0] = prumeryX[i];
                        bod[1] = prumeryY[j];
                        krizkyOfiko.Add(bod);
                    }
                }
                foreach (int[] m in krizkyOfiko)
                {
                    //vykresleni bodu - stredu nalezeneho krizku
                    Drawing.Rectangle(data, new Rectangle(m[0], m[1], 2, 2), Color.Red);

                }
                //tady todle je naprosto KONSTANTA za tímto bude ještě hodně kodu ...
                if (krizkyOfiko.Count == 90)
                {
                    /* tady se pokusim rozrezat obrazky na radky */
                    int jj = 1;

                    int vyskaRadku = 0;
                    int rohY = 0;
                    int sirkaSloupce = 0;
                    int rohX = 0;

                    for (int i = 0; i <= prumeryY.Count; i++)
                    {

                        if (i == 0)
                        {
                            vyskaRadku = prumeryY[i];
                            rohY = 0;
                        }
                        else if (i == prumeryY.Count)
                        {
                            vyskaRadku = obrazek.Height - prumeryY[(i - 1)];
                            rohY = prumeryY[(i - 1)];
                        }
                        else
                        {
                            vyskaRadku = prumeryY[i] - prumeryY[(i - 1)];
                            rohY = prumeryY[(i - 1)];
                        }

                        // vyrezu obrazek
                        Rectangle vyrez = new Rectangle(0, rohY, obrazek.Width, vyskaRadku);
                        //
                        Bitmap obrazekRadek = obrazek.Clone(vyrez, obrazek.PixelFormat);
                        //obrazekRadek.Save("temp\\radky\\" + u + "\\" + (i + 1) + ".png");
                        for (int j = 0; j <= prumeryX.Count; j++)
                        {
                            Directory.CreateDirectory("temp\\misky\\" + jj);
                            if (j == 0)
                            {
                                sirkaSloupce = prumeryX[j];
                                rohX = 0;
                            }
                            else if (j == prumeryX.Count)
                            {
                                sirkaSloupce = obrazek.Width - prumeryX[(j - 1)];
                                rohX = prumeryX[(j - 1)];
                            }
                            else
                            {
                                sirkaSloupce = prumeryX[j] - prumeryX[(j - 1)];
                                rohX = prumeryX[(j - 1)];
                            }

                            Rectangle vyrezMiska = new Rectangle(rohX, rohY, sirkaSloupce, obrazekRadek.Height);
                            Bitmap obrazekMiska = obrazek.Clone(vyrezMiska, obrazekRadek.PixelFormat);
                            obrazekMiska.Save("temp\\misky\\" + jj + "\\" + u + ".png");
                            jj++;
                        }


                    }
                }



                obrazek.UnlockBits(data);
                obrazek.Save("temp\\krizky\\" + u + ".png");

                hodiny.Stop();
                Console.WriteLine(u + "ty obrazek trval" + hodiny.Elapsed.TotalSeconds);
                u++;














                //if (u > 1) { break; }

            }
        }










        /*Metoda která dostane jako vstup pole s nazvy souboru a pokusí se načíst všechny obrazky a ořezat a uložit je do nové složky*/

        static void OrezaniObrazkuVeSlozce(string[] directory)
        {
            var hodiny = System.Diagnostics.Stopwatch.StartNew();

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
                        rozmery[0] = poleRohu[0].X + noveX;
                        rozmery[1] = poleRohu[1].Y + noveY;
                        rozmery[2] = (poleRohu[2].X - poleRohu[0].X);
                        rozmery[3] = (poleRohu[3].Y - poleRohu[1].Y);


                    }
                }
            }
            return rozmery;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace DP
{
    public static class OrezKraju
    {
        /* Metoda kter� dostane jako vstup n�zev slo�ky od kud bude br�t obr�zky. Z prvn�ho obr�zku z�sk� rozm�ry a podle t�ch pak o�e�e v�echny ostatn�.
            Pracuje tak, �e na�te seznam v�ech obr�zk� a pouze z prvn�ho vyt�hne rozm�ry. Ty pak aplikuje na ostatn� a o�e�e je. D� se pou��vat v p��pad� kdy se 
            mezi fotkami neh�be s m�sou. Pokud by se h�balo tak je t�eba ka�d� obr�zek o�ezat zvl�t a tam kde to o�e�e blb� tak ten vynechat nebo n�co.
         */
        public static string OrezaniObrazkuVeSlozce(string nazevSlozky)
        {
            /* za��n�m m��it celkov� �as*/
            var hodiny = System.Diagnostics.Stopwatch.StartNew();
            /* Vytvo��m si c�lovou slo�ku pro ulo�en� o�ezan�ch obrazk�. */
            string nazevSlozkyOrezanych = nazevSlozky + "orezany\\";
            Directory.CreateDirectory(nazevSlozkyOrezanych);

            /* Z�skej n�zvy v�ech .png soubor� (v�ech obrazk�) ve vstupn� slo�ce. */
            string[] directory = Directory.GetFiles(nazevSlozky, "*.png", SearchOption.TopDirectoryOnly);

            /* Z prvn�ho obr�zku vyt�hnu rozm�ry podle kter�ch pak o�e�u ostatn� obrazky
                - neaplikuju detekci kraju na v�echny proto�e nekde je vyple sv�tlo, nebo rostlinky p�esahuj� p�es okraj a t�m padem to pad�. */
            Console.WriteLine("Z prvniho obrazku vytahuji rozmery.");
            int[] rozmery = DetekujModryOkrajObrazku(directory[0]);
            Console.WriteLine("Z prvniho obrazku jsem z�skal rozm�ry. Sou�adnice leveho horniho okraje jsou :  X: " + rozmery[0] + ", Y: " + rozmery[1] + ", ���ka: " + rozmery[2] + ", v��ka: " + rozmery[3] + ".");

            /* Projdu postupn� v�echny obrazky - o�e�u je podle rozm�r� a ulo��m do p�edem vytvo�en� slo�ky.*/
            int u = 1;
            foreach (string obrazek in directory)
            {
                /* Na�tu obr�zek. */
                Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
                /* O�e�u ho */
                Bitmap bezModreho = image.Clone(new Rectangle(rozmery[0], rozmery[1], rozmery[2], rozmery[3]), image.PixelFormat);
                /* Z n�zvu obr�zku vyt�hnu jen �as a datum kdy byl zachycen. */
                string imageTime = ObecneMetody.DatumCasZNazvu(obrazek, "date-", "_round");
                /* O�ezan� obr�zek ulo��m s upraven�m n�zvem */
                bezModreho.Save(nazevSlozkyOrezanych + u.ToString("D3") + "_" + imageTime + ".png", ImageFormat.Png);
                /* Vyp�u info, na�tu ��slo obr�zku a jedu d�l. */
                Console.WriteLine("To byl " + u + "/" + directory.Length + " obrazek. Celkem �asu od za��tku programu ub�hlo " + hodiny.Elapsed.TotalSeconds + " sekund.");
                u++;
            }
            /* Zastav�m hodiny a vyp�u info o dokon�en� operace. */
            hodiny.Stop();
            Console.WriteLine("Celej set o " + directory.Length + " obrazcich trval: " + hodiny.Elapsed.TotalSeconds + " sekund. (V minut�ch: " + hodiny.Elapsed.TotalMinutes + ".)");
            /* Vrac�m n�zev slo�ky, kde jsou o�ezan� obr�zky. */
            return nazevSlozkyOrezanych;
        }





        /* Metoda, kter� dostane na vstup n�zev obrazku na kter�m m� naj�t modrou misku a vr�t� sou�adnici lev�ho horn�ho rohu, ���ku a v��ku. */
        public static int[] DetekujModryOkrajObrazku(string obrazek)
        {
            /* Obr�zek na�tu dvakr�t - na prvn�m prov�d�m �pravy a druh� pak o�e�u a ulo��m. */
            Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
            Bitmap puvodniImage = (Bitmap)Bitmap.FromFile(obrazek);
            /* Definice pomocn�ch prom�nn�ch: */
            int[] souradniceRozmeryOrezu = new int[4];
            int levyHorniX = 0;
            int levyHorniY = 0;

            /* Definice rozost�ovac�ho filtru */
            GaussianBlur filterBlur = new GaussianBlur(4, 11);

            /* Definice modr�ho filtru */
            ColorFiltering filterBlue = new ColorFiltering();
            filterBlue.Red = new IntRange(18, 35);
            filterBlue.Green = new IntRange(23, 40);
            filterBlue.Blue = new IntRange(40, 85);

            /* Definice hn�d�ho  filtru */
            ColorFiltering filterBrown = new ColorFiltering();
            filterBrown.Red = new IntRange(35, 45);
            filterBrown.Green = new IntRange(32, 42);
            filterBrown.Blue = new IntRange(30, 40);

            /* Aplikace modr�ho filtru - vyt�hne mi z p�vodn�ho obr�zku modrou barvu. */
            filterBlue.ApplyInPlace(image);
            /* Na vyt�hnutou modrou vrstvu aplikuji rozost�en� abych v n�m mohl vyhledat obd�ln�k. */
            filterBlur.ApplyInPlace(image);

            /* Definice vyhled�v��e obdeln�k�. */
            BlobCounter bc = new BlobCounter();
            bc.FilterBlobs = true;
            bc.MinHeight = 500;
            bc.MinWidth = 500;

            /* Aplikce vyhled�va�e obdeln�k�. */
            bc.ProcessImage(image);

            Blob[] blobs = bc.GetObjectsInformation();

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = bc.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;

                /* kontrola jestli nalezen� obdeln�k je ctyruhelnik a do pomocn� prom�nn� dopln�n� jeho rohov�ch bod�. */
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                {
                    /* Kontrola jestli nalezen� obdeln�k je fakt obdeln�k. */
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
                    {
                        /* proch�z�m nalezen� sou�adnice a vytvo��m si z nich kolekci bod�, podle kter�ch pak o�e�u modr� obdeln�k. */
                        List<System.Drawing.Point> Points = new List<System.Drawing.Point>();
                        foreach (var point in cornerPoints)
                        {
                            Points.Add(new System.Drawing.Point(point.X, point.Y));
                        }

                        /* Tady zpracuju rohove body a vypocitam si vzdalenosti */
                        int sirkaModrehoObdelnika = (Points[2].X - Points[0].X);
                        int vyskaModrehoObdelnika = (Points[3].Y - Points[1].Y);
                        levyHorniX = Points[0].X;
                        levyHorniY = Points[1].Y;
                       
                        /* O�e�u p�vodn� obr�zek podle z�skan�ch rozm�r� modr�ho okraje. */
                        puvodniImage = puvodniImage.Clone(new Rectangle(levyHorniX, levyHorniY, sirkaModrehoObdelnika, vyskaModrehoObdelnika), puvodniImage.PixelFormat);

                        /* Upravovan� obr�zek nahrad�m o�ezem p�vodn�ho - chyst�m se na n�m hledat je�t� �edou misku, kterou to o�e�u je�t� l�pe. */
                        image = puvodniImage.Clone(new Rectangle(0, 0, puvodniImage.Width, puvodniImage.Height), puvodniImage.PixelFormat);
                    }
                }
            }
            /* Te� jsem ve f�zi kdy v prom�nn� image mame o�ezan� obr�zek v p�vodn�m tvaru.
                Aplikuji hned� filtr a jedu to co s modrou,  */
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

                        /* Tady zpracuju rohov� bod a vypo��t�m rozm�ry. */
                        souradniceRozmeryOrezu[0] = Points[0].X + levyHorniX;
                        souradniceRozmeryOrezu[1] = Points[1].Y + levyHorniY;
                        souradniceRozmeryOrezu[2] = Points[2].X - Points[0].X;
                        souradniceRozmeryOrezu[3] = Points[3].Y - Points[1].Y;

                    }
                }
            }
            /* Vr�t�m z�skan� hodnoty
                1. pozice - lev� horn� sou�adnice X.
                2. pozice - lev� horn� sou�adnice Y.
                3. pozice - ���ka o�ezu.
                4. pozice - v��ka o�ezu. 
             */
            return souradniceRozmeryOrezu;
        }
    }
}

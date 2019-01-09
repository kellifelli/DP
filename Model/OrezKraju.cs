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
        /* Metoda která dostane jako vstup název složky od kud bude brát obrázky. Z prvního obrázku získá rozmìry a podle tìch pak oøeže všechny ostatní.
            Pracuje tak, že naète seznam všech obrázkù a pouze z prvního vytáhne rozmìry. Ty pak aplikuje na ostatní a oøeže je. Dá se používat v pøípadì kdy se 
            mezi fotkami nehýbe s mísou. Pokud by se hýbalo tak je tøeba každý obrázek oøezat zvlášt a tam kde to oøeže blbì tak ten vynechat nebo nìco.
         */
        public static string OrezaniObrazkuVeSlozce(string nazevSlozky)
        {
            /* zaèínám mìøit celkový èas*/
            var hodiny = System.Diagnostics.Stopwatch.StartNew();
            /* Vytvoøím si cílovou složku pro uložení oøezaných obrazkù. */
            string nazevSlozkyOrezanych = nazevSlozky + "orezany\\";
            Directory.CreateDirectory(nazevSlozkyOrezanych);

            /* Získej názvy všech .png souborù (všech obrazkù) ve vstupní složce. */
            string[] directory = Directory.GetFiles(nazevSlozky, "*.png", SearchOption.TopDirectoryOnly);

            /* Z prvního obrázku vytáhnu rozmìry podle kterých pak oøežu ostatní obrazky
                - neaplikuju detekci kraju na všechny protože nekde je vyple svìtlo, nebo rostlinky pøesahují pøes okraj a tím padem to padá. */
            Console.WriteLine("Z prvniho obrazku vytahuji rozmery.");
            int[] rozmery = DetekujModryOkrajObrazku(directory[0]);
            Console.WriteLine("Z prvniho obrazku jsem získal rozmìry. Souøadnice leveho horniho okraje jsou :  X: " + rozmery[0] + ", Y: " + rozmery[1] + ", šíøka: " + rozmery[2] + ", výška: " + rozmery[3] + ".");

            /* Projdu postupnì všechny obrazky - oøežu je podle rozmìrù a uložím do pøedem vytvoøené složky.*/
            int u = 1;
            foreach (string obrazek in directory)
            {
                /* Naètu obrázek. */
                Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
                /* Oøežu ho */
                Bitmap bezModreho = image.Clone(new Rectangle(rozmery[0], rozmery[1], rozmery[2], rozmery[3]), image.PixelFormat);
                /* Z názvu obrázku vytáhnu jen èas a datum kdy byl zachycen. */
                string imageTime = ObecneMetody.DatumCasZNazvu(obrazek, "date-", "_round");
                /* Oøezaný obrázek uložím s upraveným názvem */
                bezModreho.Save(nazevSlozkyOrezanych + u.ToString("D3") + "_" + imageTime + ".png", ImageFormat.Png);
                /* Vypíšu info, naètu èíslo obrázku a jedu dál. */
                Console.WriteLine("To byl " + u + "/" + directory.Length + " obrazek. Celkem èasu od zaèátku programu ubìhlo " + hodiny.Elapsed.TotalSeconds + " sekund.");
                u++;
            }
            /* Zastavím hodiny a vypíšu info o dokonèení operace. */
            hodiny.Stop();
            Console.WriteLine("Celej set o " + directory.Length + " obrazcich trval: " + hodiny.Elapsed.TotalSeconds + " sekund. (V minutách: " + hodiny.Elapsed.TotalMinutes + ".)");
            /* Vracím název složky, kde jsou oøezané obrázky. */
            return nazevSlozkyOrezanych;
        }





        /* Metoda, která dostane na vstup název obrazku na kterém má najít modrou misku a vrátí souøadnici levého horního rohu, šíøku a výšku. */
        public static int[] DetekujModryOkrajObrazku(string obrazek)
        {
            /* Obrázek naètu dvakrát - na prvním provádím úpravy a druhý pak oøežu a uložím. */
            Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
            Bitmap puvodniImage = (Bitmap)Bitmap.FromFile(obrazek);
            /* Definice pomocných promìnných: */
            int[] souradniceRozmeryOrezu = new int[4];
            int levyHorniX = 0;
            int levyHorniY = 0;

            /* Definice rozostøovacího filtru */
            GaussianBlur filterBlur = new GaussianBlur(4, 11);

            /* Definice modrého filtru */
            ColorFiltering filterBlue = new ColorFiltering();
            filterBlue.Red = new IntRange(18, 35);
            filterBlue.Green = new IntRange(23, 40);
            filterBlue.Blue = new IntRange(40, 85);

            /* Definice hnìdého  filtru */
            ColorFiltering filterBrown = new ColorFiltering();
            filterBrown.Red = new IntRange(35, 45);
            filterBrown.Green = new IntRange(32, 42);
            filterBrown.Blue = new IntRange(30, 40);

            /* Aplikace modrého filtru - vytáhne mi z pùvodního obrázku modrou barvu. */
            filterBlue.ApplyInPlace(image);
            /* Na vytáhnutou modrou vrstvu aplikuji rozostøení abych v nìm mohl vyhledat obdélník. */
            filterBlur.ApplyInPlace(image);

            /* Definice vyhledáváèe obdelníkù. */
            BlobCounter bc = new BlobCounter();
            bc.FilterBlobs = true;
            bc.MinHeight = 500;
            bc.MinWidth = 500;

            /* Aplikce vyhledávaèe obdelníkù. */
            bc.ProcessImage(image);

            Blob[] blobs = bc.GetObjectsInformation();

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = bc.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;

                /* kontrola jestli nalezený obdelník je ctyruhelnik a do pomocné promìnné doplnìní jeho rohových bodù. */
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                {
                    /* Kontrola jestli nalezený obdelník je fakt obdelník. */
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
                    {
                        /* procházím nalezené souøadnice a vytvoøím si z nich kolekci bodù, podle kterých pak oøežu modrý obdelník. */
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
                       
                        /* Oøežu pùvodní obrázek podle získaných rozmìrù modrého okraje. */
                        puvodniImage = puvodniImage.Clone(new Rectangle(levyHorniX, levyHorniY, sirkaModrehoObdelnika, vyskaModrehoObdelnika), puvodniImage.PixelFormat);

                        /* Upravovaný obrázek nahradím oøezem pùvodního - chystám se na nìm hledat ještì šedou misku, kterou to oøežu ještì lépe. */
                        image = puvodniImage.Clone(new Rectangle(0, 0, puvodniImage.Width, puvodniImage.Height), puvodniImage.PixelFormat);
                    }
                }
            }
            /* Teï jsem ve fázi kdy v promìnné image mame oøezaný obrázek v pùvodním tvaru.
                Aplikuji hnedý filtr a jedu to co s modrou,  */
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

                        /* Tady zpracuju rohový bod a vypoèítám rozmìry. */
                        souradniceRozmeryOrezu[0] = Points[0].X + levyHorniX;
                        souradniceRozmeryOrezu[1] = Points[1].Y + levyHorniY;
                        souradniceRozmeryOrezu[2] = Points[2].X - Points[0].X;
                        souradniceRozmeryOrezu[3] = Points[3].Y - Points[1].Y;

                    }
                }
            }
            /* Vrátím získané hodnoty
                1. pozice - levé horní souøadnice X.
                2. pozice - levá horní souøadnice Y.
                3. pozice - šíøka oøezu.
                4. pozice - výška oøezu. 
             */
            return souradniceRozmeryOrezu;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System.Linq;

namespace DP
{
    class RozrezaniNaMisky
    {
        static int zmencovaciKonstanta = 5;

        /* Metoda která dostane na vstupu název složky, kde jsou oøezané obrázky od modré misky a rozdìlí je na misky.  */
        public static void RozrezObrazkyVeSlozce(string slozka)
        {
            /* Projdu všechny obrázky a získám souøadnice køížkù, které lze detekovat. */
            List<RozsirenyBod> souradniceZiskanychKrizu = ZiskejKrizkyZeVsechObrazku(slozka);

            /* Pokusik  zakreslení obrazku s nalezenými køížky - smazat, todle nepotøebuji  2018 01 08 v tento den napsáno*/
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            Bitmap obrazek = (Bitmap)Bitmap.FromFile(slozkaObrazku[0]);
            ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width / zmencovaciKonstanta, obrazek.Height / zmencovaciKonstanta);
            /* Na  obrázek použiji èernobílý filter a zmenším ho. */

            obrazek = filterSize1.Apply(obrazek);
            BitmapData data = obrazek.LockBits(new Rectangle(0, 0, obrazek.Width, obrazek.Height),
                    ImageLockMode.ReadWrite, obrazek.PixelFormat);
            foreach (var m in souradniceZiskanychKrizu)
            {
                //vykresleni bodu - stredu nalezeneho krizku
                Drawing.Rectangle(data, new Rectangle(m.X, m.Y, 2, 2), Color.Red);

            }
            obrazek.UnlockBits(data);
            obrazek.Save("temp\\krizek_nelezene.png");
            Console.WriteLine(souradniceZiskanychKrizu.Count);

            /* Pokusik */
            /* foreach (var item in souradniceZiskanychKrizu)
             {
                 Console.WriteLine("x> " + item.X + "Y: " + item.Y);
             }

             if (souradniceZiskanychKrizu.Count < 90)
             {
                 //musim dopocitat ostatni body
                 DopocitejOstatniBody(souradniceZiskanychKrizu, slozka); //metoda nebude nic vracet jenom tam proste doda dalsi body
             } */

            // v dalsim kroce musim urcit sirku a vysku misky a tu budu rezat vsude stejnou
            // neco ve smyslu nejmensi rozdil mezi vsemi body vetesi jak 10 a vyska to same 



        }

        static void DopocitejOstatniBody(List<RozsirenyBod> souradniceZiskanychKrizu, string slozka)
        {

            souradniceZiskanychKrizu.Add(new RozsirenyBod(5, 6, true, 10));
        }

        /* Metoda, která na vstupu dostane složku s obrázky na kterých bude hledat køížky. Postupnì projde všechny obrázky - ne všechny køížky jdou detekovat. */
        static List<RozsirenyBod> ZiskejKrizkyZeVsechObrazku(string slozka)
        {

            /* Naèteme pole souborù, které budeme procháze, jako jejich názvy. */
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            /* Definice pomocné promìnné - jaký vzor budu hledat. */
            string umisteniVzoru = "temp\\1_vzor.png";
            Bitmap vzor = (Bitmap)Bitmap.FromFile(umisteniVzoru);

            /*Definice šedého filtru. */
            Grayscale filterSeda = new Grayscale(0.2125, 0.7154, 0.0721);

            /* Zmìnšení a aplikování šedého filtru na vzor. */
            ResizeBilinear zmenseniVzoru = new ResizeBilinear(vzor.Width / zmencovaciKonstanta, vzor.Height / zmencovaciKonstanta);

            vzor = filterSeda.Apply(vzor);
            vzor = zmenseniVzoru.Apply(vzor);

            /* Vytvoøím si kolekci rozšíøených bodù, kde si budu ukládat jednotlivé nalezené souøadnice køížkù. */
            List<RozsirenyBod> nalezeneBody = new List<RozsirenyBod>();

            foreach (string soubor in slozkaObrazku)
            {
                /* Spouštím hodiny abych vedìl jak dlouho trvají jednotlivé obrázky. */
                var hodiny = System.Diagnostics.Stopwatch.StartNew();
                /* Z názvu obrázku zase vytahuju pouze jméno - teï už tam mam i index takže v dalším kroku nemusím ukladat nic extra */
                string nazevObrazku = ObecneMetody.DatumCasZNazvu(soubor, "\\", ".png");

                /* naèítám obrázek */
                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);
                /* Definice zmenšovacího filtru na originále to trvá strašnì dlouho. */
                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width / zmencovaciKonstanta, obrazek.Height / zmencovaciKonstanta);
                /* Na  obrázek použiji èernobílý filter a zmenším ho. */
                obrazek = filterSeda.Apply(obrazek);
                obrazek = filterSize1.Apply(obrazek);

                /* Vyhledávací algoritmus, který dostane "kde" a "co" hledat. na zaèátku je tøeba definovat s jakou pøesností - ta už se zdá být docela vychytaná. */
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.968f);
                TemplateMatch[] matchings = tm.ProcessImage(obrazek, vzor);

                foreach (TemplateMatch m in matchings)
                {
                    /* nalezený køížek momentálnì vystupuje jako okraj obrázku vzor - tzn dopoèítám jeho prostøední souøadnici. A tento bod uložím do kolekce */
                    nalezeneBody.Add(new RozsirenyBod((m.Rectangle.Width / 2) + m.Rectangle.Location.X, (m.Rectangle.Height / 2) + m.Rectangle.Location.Y));
                }

                /* Zastavím hodiny pro jednotlivé obrázky a vypíšu info o tom jak dlouho trvalo naèíst  */
                hodiny.Stop();
                Console.WriteLine("Obr: " + nazevObrazku + " trval " + hodiny.Elapsed.TotalSeconds + " sekund.");

            }
            /* V této èásti probìhne sjednocení nalezených bodù. Z každého brazku mùžu mít jiné køížky nalezené a vìtšina jich je tam víckrát s nestejnými souøadnicemi - mohou se lišit tøeba i o 1 pixel na ose X napø.
                je tudíž tøeba je pogrupovat tak abych v každé grupì mìl jen jeden bod z každého obrazku a z této grupy nalést reprezentativní bod.
             */

            /* Nejprve kolekci bodù seøadím podle X a podle Y. */
            nalezeneBody = nalezeneBody.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            /* Definice pomocné promìnné - kolikátáSkupina - procházím postupnì všechny bodz a pøiøazuji jim tuto informaci, vypovídá o tom kolikátá nalezená to je. To že je nalezené první nemusí znamenat že to je první bod na obrazku atd ... */
            int kolikataSkupina = 1;

            /* Cyklem projdu kolekci bodu a doplnim o kolikaty bod se jedna. Znovu upozornìní že kolikaty = 10, mùže být klidnì polsední na obrazku. */
            for (int i = 0; i < nalezeneBody.Count; i++)
            {
                /* Ovìøuji v pøípadì, že jsem již bodu informaci nepøiložil. 0 = nevím kolikátý to je. */
                if (nalezeneBody[i].kolikaty == 0)
                {
                    /* Doplním mu informaci */
                    nalezeneBody[i].kolikaty = kolikataSkupina;
                    /* a naleznu všechny jeho kamrády ve skupinì. */
                    for (int j = i + 1; j < nalezeneBody.Count; j++)
                    {
                        /* Pokud je vzdálenost obou bodu i (tomu hledám kamarády) a bodu j menší na obì strany než konstanta, tak bod "j" pøiøadím do stejné skupiny jako jsem pøiøadil bod "i" */
                        if ((Math.Abs(nalezeneBody[j].X - nalezeneBody[i].X) < 8) && (Math.Abs(nalezeneBody[j].Y - nalezeneBody[i].Y) < 8))
                        {
                            if (nalezeneBody[j].kolikaty == 0)
                            {
                                nalezeneBody[j].kolikaty = kolikataSkupina;
                            }
                        }
                    }
                    kolikataSkupina++;
                }
            }

            /* NalezenéBody: kolekce bodù, které nesou informaci o svých souøadnicích a každý bod ví do které skupiny patøí.
                Teï potøebuji z každé skupiny nají jednoho ideálního zástupce, možná by se dala využít nìjaká echt minimalizaèní metoda, ale myslím že prùmìr bohatì postaèí. */

            /* Definice pomocných promìnných */
            int sumaX = 0, sumaY = 0, pocet = 0, prumerX = 0, prumerY = 0, k = 0;

            /* nalezenéBody seøadím podle toho do které skupiny patøí a zaènu je procházet tak abych každému bodu mohl pøiøadit informaci o "ideálním bodu" jeho skupiny. */
            nalezeneBody = nalezeneBody.OrderBy(p => p.kolikaty).ToList();

            /* Z nalezenýchBodù vyfiltruju pouze tìch pár optimálních. */
            List<RozsirenyBod> finálniNalezeneBody = new List<RozsirenyBod>();
            for (int i = 0; i < nalezeneBody.Count; i++)
            {
                sumaX = sumaX + nalezeneBody[i].X;
                sumaY = sumaY + nalezeneBody[i].Y;
                pocet++;
                /* Pokud jsem v 0 až pøedposledním bodì musím ovìøit jestli další má stejnou skupinu bodu, nebo je to poslední bod a tak jim mužu dosadit prumìr */
                if (((i + 1) < nalezeneBody.Count && (nalezeneBody[i].kolikaty != nalezeneBody[i + 1].kolikaty)) || ((i + 1) == nalezeneBody.Count))
                {
                    prumerX = (int)Math.Round(((double)(sumaX / pocet)), 0);
                    prumerY = (int)Math.Round(((double)(sumaY / pocet)), 0);
                    /* Vypoèítám prùmìrné X a prùmìrné Y  a zpìtnì dosadím bodùm, které nemají (nemají jen ty co patøí do skupiny s vypoèitanými prùmìry) informaci o prùmìrném, tedy optimálním X a Y za skupinu.*/
                    k = i;
                    while (!nalezeneBody[k].prumerDosazen)
                    {
                        nalezeneBody[k].prumerneX = prumerX;
                        nalezeneBody[k].prumerneY = prumerY;
                        nalezeneBody[k].prumerDosazen = true;
                        if (k != 0)
                        {
                            k--;
                        }
                    }
                    /* Neco jako filtrace, proste pridavam jen ty body co potrebuju - ty :optimamalni: */
                    finálniNalezeneBody.Add(new RozsirenyBod(prumerX, prumerY, false, nalezeneBody[i].kolikaty));
                    /* Null promennych */
                    sumaX = 0;
                    sumaY = 0;
                    pocet = 0;
                    prumerX = 0;
                    prumerY = 0;
                }
            }
            return finálniNalezeneBody;
        }
    }
}

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
        static int zmencovaciKonstanta = 3;

        /* Metoda která dostane na vstupu název složky, kde jsou oøezané obrázky od modré misky a rozdìlí je na misky.  */
        public static void RozrezObrazkyVeSlozce(string slozka)
        {
            /* Projdu všechny obrázky a získám souøadnice køížkù, které lze detekovat. */
            List<RozsirenyBod> souradniceZiskanychKrizu = ZiskejKrizkyZeVsechObrazku(slozka);
            /* Z nalezenych krizku dopocitam prumerny X a prumerny Y a dopocitam chybejici krizky at jich mam 90*/
            List<RozsirenyBod> dopocitaneKrizky = DopocitejOstatniBody(souradniceZiskanychKrizu, slozka);

            ObecneMetody.vykresliNalezeneKrizkyDoObrazku(slozka, zmencovaciKonstanta, dopocitaneKrizky, "D:\\repos\\DP\\DP\\temp\\krizek_nelezene.png");



        }


        /* Metoda, která na vstupu dostane složku s obrázky na kterých bude hledat køížky. Postupnì projde všechny obrázky - ne všechny køížky jdou detekovat. */
        static List<RozsirenyBod> ZiskejKrizkyZeVsechObrazku(string slozka)
        {

            /* Naèteme pole souborù, které budeme procháze, jako jejich názvy. */
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            /* Definice pomocné promìnné - jaký vzor budu hledat. */
            string umisteniVzoru = "D:\\repos\\DP\\DP\\temp\\1_vzor.png";
            Bitmap vzor = (Bitmap)Bitmap.FromFile(umisteniVzoru);

            /*Definice šedého filtru. */
            Grayscale filterSeda = new Grayscale(0.2125, 0.7154, 0.0721);

            /* Zmìnšení a aplikování šedého filtru na vzor. */
            ResizeBilinear zmenseniVzoru = new ResizeBilinear(vzor.Width / zmencovaciKonstanta, vzor.Height / zmencovaciKonstanta);

            vzor = filterSeda.Apply(vzor);
            vzor = zmenseniVzoru.Apply(vzor);

            /* Vytvoøím si kolekci rozšíøených bodù, kde si budu ukládat jednotlivé nalezené souøadnice køížkù. */
            List<RozsirenyBod> nalezeneBody = new List<RozsirenyBod>();

            /* Todle by bylo dobré rozhodit do více vláken. */
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
                Console.WriteLine("Hledání køížkù v obrázku Obr: " + nazevObrazku + " trvalo " + hodiny.Elapsed.TotalSeconds + " sekund.");

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
            List<RozsirenyBod> filtrNalezeneBody = new List<RozsirenyBod>();
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
                    filtrNalezeneBody.Add(new RozsirenyBod(prumerX, prumerY, false, nalezeneBody[i].kolikaty));
                    /* Null promennych */
                    sumaX = 0;
                    sumaY = 0;
                    pocet = 0;
                    prumerX = 0;
                    prumerY = 0;
                }
            }
            return filtrNalezeneBody;
        }


        static List<RozsirenyBod> DopocitejOstatniBody(List<RozsirenyBod> souradniceZiskanychKrizu, string slozka)
        {

            /* Rozmery obrazku - tady mozna at se nacte jen prvni */
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            Bitmap image = (Bitmap)Bitmap.FromFile(slozkaObrazku[0]);

            ResizeBilinear zmenseniObrazku = new ResizeBilinear(image.Width / zmencovaciKonstanta, image.Height / zmencovaciKonstanta);
            image = zmenseniObrazku.Apply(image);
            int sirka = image.Width;
            int vyska = image.Height;


            /* List nalezenych køížkù je po sloupcích takze ja projdu list a podivam se kde se X lisi o par pixelu zase tydle X dopocitam tak aby v každé skupinì bylo 9 kusù */
            int sumaX = 0, sumaY = 0, pocet = 0, prumerX = 0, prumerY = 0; 
            List<int> prumerneX = new List<int>();
            List<int> prumerneY = new List<int>();

            /* Dopoèet X souøadnic, po tomto cyklu budu mit  */
            for (int i = 0; i < souradniceZiskanychKrizu.Count; i++)
            {
                sumaX = sumaX + souradniceZiskanychKrizu[i].X;
                pocet++;
                if (((i + 1) < souradniceZiskanychKrizu.Count && Math.Abs(souradniceZiskanychKrizu[i].X - souradniceZiskanychKrizu[i + 1].X) > 5) || ((i + 1) == souradniceZiskanychKrizu.Count))
                {

                    prumerX = (int)Math.Round(((double)(sumaX / pocet)), 0);
                    prumerneX.Add(prumerX);
                    prumerX = 0;
                    sumaX = 0;
                    pocet = 0;
                }

            }

            souradniceZiskanychKrizu = souradniceZiskanychKrizu.OrderBy(p => p.Y).ToList();

            for (int i = 0; i < souradniceZiskanychKrizu.Count; i++)
            {
                sumaY = sumaY + souradniceZiskanychKrizu[i].Y;
                pocet++;
                if (((i + 1) < souradniceZiskanychKrizu.Count && Math.Abs(souradniceZiskanychKrizu[i].Y - souradniceZiskanychKrizu[i + 1].Y) > 5) || ((i + 1) == souradniceZiskanychKrizu.Count))
                {

                    prumerY = (int)Math.Round(((double)(sumaY / pocet)), 0);
                    prumerneY.Add(prumerY);
                    prumerY = 0;
                    sumaY = 0;
                    pocet = 0;
                }

            }



            /* V pøípadì že mi nìjaké body chybí tak je dopoèítam podle vzdalenosti */
            /* chzbí mi nìjaký sloupec */

            int prumernaVzdalenost;
            int sumaVzdalenosti = 0;
            if (prumerneX.Count < 10)
            {
                /* projdu kazdou souradnici radku a vypocitam vzdalenost */
                for (int i = 0; (i + 1) < prumerneX.Count; i++)
                {
                    sumaVzdalenosti += Math.Abs(prumerneX[i] - prumerneX[i + 1]);
                    //Console.WriteLine("Vzdalenost: " + Math.Abs(prumerneX[i] - prumerneX[i + 1]));
                }
                prumernaVzdalenost = (int)Math.Round(((double)(sumaVzdalenosti / (prumerneX.Count - 1))), 0);
                // Console.WriteLine("Prumer Vzdalenost: " + prumernaVzdalenost);

                /* Teï musim zjistit jestli mezi každým bodem cca prùmìrná vzdálenost, tzn jestli vzdálenost je +/- 5 pixelù dejme tomu a když nebude tak pøidám bod + prumérná vzdálenost*/

                /* ted budu prochazet prumerneX a cekovat vzdalenost  */

                for (int i = 0; i < prumerneX.Count; i++)
                {
                    if (i == 0)
                    {
                        if (prumerneX[i] > (prumernaVzdalenost + 5))
                        {
                            prumerneX.Add(prumernaVzdalenost);
                        }
                    }
                    else if ((i + 1) == prumerneX.Count)
                    {
                        if (Math.Abs(prumerneX[i] - sirka) > (prumernaVzdalenost + 5))
                        {
                            prumerneX.Add(prumerneX[i] + prumernaVzdalenost);
                        }
                    }
                    else
                    {
                        if (Math.Abs(prumerneX[i] - prumerneX[i + 1]) > (prumernaVzdalenost + 5))
                        {
                            prumerneX.Add(prumerneX[i] + prumernaVzdalenost);
                        }
                    }
                }

            }









            sumaVzdalenosti = 0;
            /* chbí mi nìjaký radek */
            if (prumerneY.Count < 9)
            {
                for (int i = 0; (i + 1) < prumerneY.Count; i++)
                {
                    sumaVzdalenosti += Math.Abs(prumerneY[i] - prumerneY[i + 1]);
                    //Console.WriteLine("Vzdalenost: " + Math.Abs(prumerneY[i] - prumerneY[i + 1]));
                }
                prumernaVzdalenost = (int)Math.Round(((double)(sumaVzdalenosti / (prumerneY.Count - 1))), 0);
                //Console.WriteLine("Prumer Vzdalenost: " + prumernaVzdalenost);


                for (int i = 0; i < prumerneY.Count; i++)
                {
                    if (i == 0)
                    {
                        if (prumerneY[i] > (prumernaVzdalenost + 5))
                        {
                            prumerneY.Add(prumernaVzdalenost);
                        }
                    }
                    else if ((i + 1) == prumerneY.Count)
                    {
                        if (Math.Abs(prumerneY[i] - vyska) > (prumernaVzdalenost + 5))
                        {
                            prumerneY.Add(prumerneY[i] + prumernaVzdalenost);
                        }
                    }
                    else
                    {
                        if (Math.Abs(prumerneY[i] - prumerneY[i + 1]) > (prumernaVzdalenost + 5))
                        {
                            prumerneY.Add(prumerneY[i] + prumernaVzdalenost);
                        }
                    }
                }
            }



            /* Definice návratu */
            List<RozsirenyBod> dopocitaneKrizky = new List<RozsirenyBod>();
            for (int i = 0; i < prumerneX.Count; i++)
            {
                for (int j = 0; j < prumerneY.Count; j++)
                {
                    dopocitaneKrizky.Add(new RozsirenyBod(prumerneX[i], prumerneY[j]));
                }
            }

            RozrezNaMisky(prumerneX, prumerneY, slozka);
            /* Kontrolní výpis 
                        foreach (var item in prumerneX)
                        {
                            Console.WriteLine(item);
                        }
            */

            return dopocitaneKrizky;
        }

        static void RozrezNaMisky(List<int> prumerneX, List<int> prumerneY, string slozka)
        {
            /* Vytvoøím si složky kam budu ukladat jednotlivé misky to pak muzu nìkam dat dolu protože budu procházet jednotlivé køížky */
            for (int i = 1; i <= ((prumerneX.Count + 1) * (prumerneY.Count + 1)); i++)
            {
                Directory.CreateDirectory(slozka + "\\" + i.ToString("D3"));
            }


            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            for (int o = 1; o <= slozkaObrazku.Length; o++)
            {
                Bitmap obrazek = (Bitmap)Bitmap.FromFile(slozkaObrazku[o - 1]);
                ResizeBilinear zmenseniObrazku = new ResizeBilinear(obrazek.Width / zmencovaciKonstanta, obrazek.Height / zmencovaciKonstanta);
                obrazek = zmenseniObrazku.Apply(obrazek);
                int sirka = obrazek.Width;
                int vyska = obrazek.Height;

                int[] rozmery = new int[4];
                Rectangle vyrezMisky = new Rectangle(rozmery[0], rozmery[1], rozmery[2], rozmery[3]);
                int miska = 1;
                for (int i = 0; i <= prumerneY.Count; i++) //icko mi oznacuje radek
                {
                    for (int j = 0; j <= prumerneX.Count; j++)
                    {
                        if (i == 0)                                                               // jsem li na prvnim radku  
                        {
                            if (j == 0)                                                                 // jsem li v prvnim sloupci
                            {
                                rozmery[0] = 0; // X zaèatku
                                rozmery[1] = 0; // Y zaèatku
                                rozmery[2] = prumerneX[j] - 1; // šíøka
                                rozmery[3] = prumerneY[i] - 1; // výška
                            }
                            else if (j == prumerneX.Count)                                              // jsem li v poslednim sloupci 
                            {
                                rozmery[0] = prumerneX[j - 1]; // X zaèatku
                                rozmery[1] = 0; // Y zaèatku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - sirka) - 1; // šíøka
                                rozmery[3] = prumerneY[i] - 1; // výška

                            }
                            else                                                                        // jsem li v prostrednim sloupci
                            {
                                rozmery[0] = prumerneX[j - 1]; // X zaèatku
                                rozmery[1] = 0; // Y zaèatku
                                rozmery[2] = Math.Abs(prumerneX[j] - prumerneX[j - 1]) - 1; // šíøka
                                rozmery[3] = prumerneY[i] - 1; // výška
                            }
                        }
                        else if (i == prumerneY.Count)                                            // jsem li v poslednim radku 
                        {
                            if (j == 0)                                                                 // jsem li v prvnim sloupci
                            {
                                rozmery[0] = 0; // X zaèatku
                                rozmery[1] = prumerneY[i - 1]; // Y zaèatku
                                rozmery[2] = prumerneX[j] - 1; // šíøka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - vyska) - 1; // výška
                            }
                            else if (j == prumerneX.Count)                                        // jsem li v poslednim sloupci 
                            {
                                rozmery[0] = prumerneX[j - 1]; // X zaèatku
                                rozmery[1] = prumerneY[i - 1]; // Y zaèatku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - sirka) - 1; // šíøka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - vyska) - 1; // výška

                            }
                            else                                                                        // jsem uprostred sloupcu
                            {
                                rozmery[0] = prumerneX[j - 1]; // X zaèatku
                                rozmery[1] = prumerneY[i - 1]; // Y zaèatku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - prumerneX[j]) - 1; // šíøka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - vyska) - 1; // výška
                            }

                        }
                        else                                                                    // jsem uprostred radkama
                        {
                            if (j == 0)                                                                 // jsem li v prvnim sloupci
                            {
                                rozmery[0] = 0; // X zaèatku
                                rozmery[1] = prumerneY[i - 1]; // Y zaèatku
                                rozmery[2] = prumerneX[j] - 1; // šíøka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - prumerneY[i]) - 1; // výška
                            }
                            else if (j == prumerneX.Count)                                              // jsem li v poslednim sloupci  
                            {
                                rozmery[0] = prumerneX[j - 1]; // X zaèatku
                                rozmery[1] = prumerneY[i - 1]; // Y zaèatku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - sirka) - 1; // šíøka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - prumerneY[i]) - 1; // výška

                            }
                            else                                                                         // jsem uprostred nekde
                            {
                                rozmery[0] = prumerneX[j - 1]; // X zaèatku
                                rozmery[1] = prumerneY[i - 1]; // Y zaèatku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - prumerneX[j]) - 1; // šíøka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - prumerneY[i]) - 1; // výška
                            }
                        }
                        vyrezMisky = new Rectangle(rozmery[0], rozmery[1], rozmery[2], rozmery[3]);
                        Bitmap orezanyObrazek = obrazek.Clone(vyrezMisky, obrazek.PixelFormat);
                        string nazevObr = ObecneMetody.DatumCasZNazvu(slozkaObrazku[o - 1], "\\", ".png");
                        orezanyObrazek.Save(slozka + "\\" + miska.ToString("D3") + "\\" + nazevObr + ".png");
                        miska++;
                    }
                }

                miska = 1;



            }
        }
    }
}

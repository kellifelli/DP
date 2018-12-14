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

        /*na vstupu prijde nazev slozky kde jsou orezany obrazky,  */
        public static void RozrezObrazkyVeSlozce(string slozka)
        {

            List<RozsirenyBod> souradniceZiskanychKrizu = ZiskejKrizkyZeVsechObrazku(slozka);

            foreach (var item in souradniceZiskanychKrizu)
            {
                Console.WriteLine("x> " + item.X + "Y: " + item.Y);
            }

            if (souradniceZiskanychKrizu.Count < 90)
            {
                //musim dopocitat ostatni body
                //DopocitejOstatniBody(souradniceZiskanychKrizu) //metoda nebude nic vracet jenom tam proste doda dalsi body
            }

            // v dalsim kroce musim urcit sirku a vysku misky a tu budu rezat vsude stejnou
            // neco ve smyslu nejmensi rozdil mezi vsemi body vetesi jak 10 a vyska to same 



        }


        static List<RozsirenyBod> ZiskejKrizkyZeVsechObrazku(string slozka)
        {
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            string umisteniVzoru = "temp\\1_vzor.png";
            string vykresleniKrizku = slozka + "\\test" + "krizky\\";
            Bitmap vzor = (Bitmap)Bitmap.FromFile(umisteniVzoru);

            Grayscale filterSeda = new Grayscale(0.2125, 0.7154, 0.0721);
            //11/24 je dobrej cas a dobrej pomer cca 1/2
            ResizeBilinear filterSize2 = new ResizeBilinear(vzor.Width * 11 / 24, vzor.Height * 11 / 24);

            List<RozsirenyBod> kolekceBodu2 = new List<RozsirenyBod>();

            foreach (string soubor in slozkaObrazku)
            {
                var hodiny = System.Diagnostics.Stopwatch.StartNew();
                string nazevObrazku = ZiskejNazev(soubor);

                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);

                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width * 11 / 24, obrazek.Height * 11 / 24);
                //aplikace šedého filtru
                Bitmap obrazekSedy = filterSeda.Apply(obrazek);
                Bitmap vzorSedy = filterSeda.Apply(vzor);
                //zmenšení obrazkù
                obrazekSedy = filterSize1.Apply(obrazekSedy);
                vzorSedy = filterSize2.Apply(vzorSedy);
                obrazek = filterSize1.Apply(obrazek);
                //vyhledavácí alg - v ObrazekSedy vyhleda výskyty vzorSedy
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.968f);
                TemplateMatch[] matchings = tm.ProcessImage(obrazekSedy, vzorSedy);

                foreach (TemplateMatch m in matchings)
                {
                    kolekceBodu2.Add(new RozsirenyBod((m.Rectangle.Width / 2) + m.Rectangle.Location.X, (m.Rectangle.Height / 2) + m.Rectangle.Location.Y));
                }

                hodiny.Stop();
                Console.WriteLine("obrazek trval " + hodiny.Elapsed.TotalSeconds);
            }
            //ted musim dopocitat prumery respektive jinak urcit minimum ... kazde skupiny - mam je po X- kách 
            kolekceBodu2 = kolekceBodu2.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            // to že má oznaèení kolikaty = 1 neznamena ze je to prvni na obrazku, je to prvni detekovany 
            int kolikaty = 1;
            //tady timto cyklem si vytvorim kolekci bodu ktera ma X,Y a informaci o tom ktery bod na obrazku to je 
            for (int i = 0; i < kolekceBodu2.Count; i++)
            {
                if (kolekceBodu2[i].kolikaty == 0)
                {
                    kolekceBodu2[i].kolikaty = kolikaty;

                    for (int j = i + 1; j < kolekceBodu2.Count; j++)
                    {
                        if ((Math.Abs(kolekceBodu2[j].X - kolekceBodu2[i].X) < 8) && (Math.Abs(kolekceBodu2[j].Y - kolekceBodu2[i].Y) < 8))
                        {
                            if (kolekceBodu2[j].kolikaty == 0)
                            {
                                kolekceBodu2[j].kolikaty = kolikaty;
                            }
                        }
                    }
                    kolikaty++;
                }
            }

            // pomocne promenne
            int sumaX = 0, sumaY = 0, pocet = 0, prumerX = 0, prumerY = 0, k = 0;
            //seradim si body podle toho ke kteremu oficialne patri
            kolekceBodu2 = kolekceBodu2.OrderBy(p => p.kolikaty).ToList();
            for (int i = 0; i < kolekceBodu2.Count; i++)
            {
                sumaX = sumaX + kolekceBodu2[i].X;
                sumaY = sumaY + kolekceBodu2[i].Y;
                pocet++;
                if ((i + 1) < kolekceBodu2.Count && (kolekceBodu2[i].kolikaty != kolekceBodu2[i + 1].kolikaty)) // kdyz jsem v predposlednim bode
                {
                    prumerX = (int)Math.Round(((double)(sumaX / pocet)), 0);
                    prumerY = (int)Math.Round(((double)(sumaY / pocet)), 0);
                    //prictu sumu a vzpocitam prumer a pak vypocitam body a vynuluju sumu a prumer a zpetnym cyklem dosadim prumer zpatky do vsech bodu co ho jeste nemaji dopocitan
                    //projedu zpetne
                    k = i;
                    while (!kolekceBodu2[k].prumerDosazen)
                    {
                        kolekceBodu2[k].prumerneX = prumerX;
                        kolekceBodu2[k].prumerneY = prumerY;
                        kolekceBodu2[k].prumerDosazen = true;
                        if (k != 0)
                        {
                            k--;
                        }
                    }
                    sumaX = 0;
                    sumaY = 0;
                    pocet = 0;
                    prumerX = 0;
                    prumerY = 0;
                }

            }

            //kolekce ktera neobsahuje vsechny  krizy obrazky
            List<RozsirenyBod> kolekceBodu3 = new List<RozsirenyBod>();
            kolikaty = 1;
            for (int i = 0; i < kolekceBodu2.Count; i++)
            {
                if (kolekceBodu2[i].kolikaty == kolikaty)
                {
                    kolekceBodu3.Add(new RozsirenyBod(kolekceBodu2[i].X, kolekceBodu2[i].Y));
                    kolikaty++;
                }
            }
            return kolekceBodu3;
        }




        static string ZiskejNazev(string obrazek)
        {
            string prvni_slovo = "\\";
            string druhe_slovo = ".png";
            int first = obrazek.LastIndexOf(prvni_slovo);
            int last = obrazek.LastIndexOf(druhe_slovo);
            string bezRound = obrazek.Substring(0, last);
            string bezFirst = obrazek.Substring(0, first + prvni_slovo.Length);

            return obrazek.Substring(bezFirst.Length, bezRound.Length - bezFirst.Length);
        }

















        public static void DetekceKrizku(string slozka)
        {
            var hodinyKomplet = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Zacinam nacitat orezeny obrazky a rozdelavat je na misky.");
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            string umisteniVzoru = "temp\\1_vzor.png";
            string umisteniMisek = slozka + "misky\\";
            string vykresleniKrizku = slozka + "krizky\\";
            Bitmap vzor = (Bitmap)Bitmap.FromFile(umisteniVzoru);

            int u = 1;
            foreach (string soubor in slozkaObrazku)
            {
                /*tadz kdyz efektivne zjistim nazev souboru a nahradit ho nazvem obrazku, ale to uz asi musim na zacatku */


                var hodiny = System.Diagnostics.Stopwatch.StartNew();
                string nazevObrazku = ZiskejNazev(soubor);

                Console.WriteLine("jedu " + u + "-ty obrazek.");
                //naètení obrazku
                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);
                // Definice filtrù - šedá, velikost
                Grayscale filterSeda = new Grayscale(0.2125, 0.7154, 0.0721);
                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width * 7 / 12, obrazek.Height * 7 / 12);
                ResizeBilinear filterSize2 = new ResizeBilinear(vzor.Width * 7 / 12, vzor.Height * 7 / 12);


                //aplikace šedého filtru
                Bitmap obrazekSedy = filterSeda.Apply(obrazek);
                Bitmap vzorSedy = filterSeda.Apply(vzor);
                //zmenšení obrazkù
                obrazekSedy = filterSize1.Apply(obrazekSedy);
                vzorSedy = filterSize2.Apply(vzorSedy);
                obrazek = filterSize1.Apply(obrazek);


                //vyhledavácí alg - v ObrazekSedy vyhleda výskyty vzorSedy
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.962f);
                TemplateMatch[] matchings = tm.ProcessImage(obrazekSedy, vzorSedy);

                BitmapData data = obrazek.LockBits(new Rectangle(0, 0, obrazek.Width, obrazek.Height),
                                    ImageLockMode.ReadWrite, obrazek.PixelFormat);

                //definice listù - souøadnice x a y nalezených køížkù
                List<int> souradniceX = new List<int>();
                List<int> souradniceY = new List<int>();

                //projdu nalezené køížky a vypoèítané souøadnice støedù pøidám do pøiravených listù 
                foreach (TemplateMatch m in matchings)
                {
                    int x = m.Rectangle.Location.X + m.Rectangle.Width / 2;
                    int y = m.Rectangle.Location.Y + m.Rectangle.Height / 2;
                    souradniceX.Add(x);
                    souradniceY.Add(y);
                }

                //souøadnice je tøeba setøídit - pøi rozdìlovaní do sloupcù (øádkù) poznám jeden sloupec(øádek) tak, že vzdálenost není moc velká, sloupce (øádky) jsou cca 90px široké
                souradniceX.Sort();
                souradniceY.Sort();

                //2d pole v prvnim sloupci je hodnota x stredu a ve druhem je hodnota sloupce ve kterem se nachazi
                int[,] souradniceXX = new int[souradniceX.Count, 2];
                int[,] souradniceYY = new int[souradniceY.Count, 2];
                int sloupec = 1;
                int radek = 1;

                //cyklem projdu oba pomocné listy (jsou stejnì velké) a urèím pro každý bod jeho sloupec
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
                //pomocné listy ... vypoèítám prùmìr za každý øádek/sloupec
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
                        krizkyOfiko.Add(new int[] { prumeryX[i], prumeryY[j] });
                    }
                }
                foreach (int[] m in krizkyOfiko)
                {
                    //vykresleni bodu - stredu nalezeneho krizku
                    Drawing.Rectangle(data, new Rectangle(m[0], m[1], 2, 2), Color.Red);

                }
                //tady todle je naprosto KONSTANTA za tímto bude ještì hodnì kodu ...
                if (krizkyOfiko.Count == 90)
                {
                    /* tady se pokusim rozrezat obrazky na misky */
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
                        // vyrezu radek
                        Bitmap obrazekRadek = obrazek.Clone(new Rectangle(0, rohY, obrazek.Width, vyskaRadku), obrazek.PixelFormat);
                        //obrazekRadek.Save("temp\\radky\\" + u + "\\" + (i + 1) + ".png");
                        //jdu prochazet radek a rozrezu ho na misky - ctverce
                        for (int j = 0; j <= prumeryX.Count; j++)
                        {
                            Directory.CreateDirectory(umisteniMisek + jj);
                            if (j == 0) // prvni
                            {
                                sirkaSloupce = prumeryX[j];
                                rohX = 0;
                            }
                            else if (j == prumeryX.Count) // posledni
                            {
                                sirkaSloupce = obrazek.Width - prumeryX[(j - 1)];
                                rohX = prumeryX[(j - 1)];
                            }
                            else //prostredni
                            {
                                sirkaSloupce = prumeryX[j] - prumeryX[(j - 1)];
                                rohX = prumeryX[(j - 1)];
                            }
                            Bitmap obrazekMiska = obrazek.Clone(new Rectangle(rohX, rohY, sirkaSloupce, obrazekRadek.Height), obrazekRadek.PixelFormat);
                            obrazekMiska.Save(umisteniMisek + jj + "\\" + nazevObrazku + ".png");
                            jj++;
                        }
                    }
                }

                obrazek.UnlockBits(data);

                Directory.CreateDirectory(vykresleniKrizku);
                obrazek.Save(vykresleniKrizku + nazevObrazku + ".png");

                hodiny.Stop();
                Console.WriteLine("Rozrezani obrazku na misky, " + u + "-ty obrazek trval: " + hodiny.Elapsed.TotalSeconds + ". Celkem uz jedu: " + hodinyKomplet.Elapsed.TotalMinutes + " minut.");
                u++;
                //if (u > 1) { break; }
            }
        }
    }
}

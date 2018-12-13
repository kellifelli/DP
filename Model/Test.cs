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
    class Test
    {


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

                BitmapData data = obrazek.LockBits(new Rectangle(0, 0, obrazek.Width, obrazek.Height),
                                    ImageLockMode.ReadWrite, obrazek.PixelFormat);

                foreach (TemplateMatch m in matchings)
                {
                    Drawing.Rectangle(data, m.Rectangle, Color.Red);
                    kolekceBodu2.Add(new RozsirenyBod((m.Rectangle.Width / 2) + m.Rectangle.Location.X, (m.Rectangle.Height / 2) + m.Rectangle.Location.Y));
                }
                obrazek.UnlockBits(data);

                Directory.CreateDirectory(vykresleniKrizku);
                obrazek.Save(vykresleniKrizku + nazevObrazku + ".png");

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
            int sumaX = 0;
            int sumaY = 0;
            int pocet = 0;
            int prumerX = 0;
            int prumerY = 0;
            int k = 0;
            kolekceBodu2 = kolekceBodu2.OrderBy(p => p.kolikaty).ToList();
            for (int i = 0; i < kolekceBodu2.Count; i++)
            {
                if ((i + 1) < kolekceBodu2.Count) // kdyz jsem v predposlednim bode
                {
                    if (kolekceBodu2[i].kolikaty == kolekceBodu2[i + 1].kolikaty)
                    {
                        //prictu sumu a pocet kolik jich mam tech bodu 
                        sumaX = sumaX + kolekceBodu2[i].X;
                        sumaY = sumaY + kolekceBodu2[i].Y;
                        pocet++;
                    }
                    else
                    {
                        sumaX = sumaX + kolekceBodu2[i].X;
                        sumaY = sumaY + kolekceBodu2[i].Y;
                        pocet++;
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
                else
                {
                    sumaX = sumaX + kolekceBodu2[i].X;
                    sumaY = sumaY + kolekceBodu2[i].Y;
                    pocet++;
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

            foreach (var item in kolekceBodu2)
            {
                if (item.kolikaty == 1)
                {
                    Console.WriteLine("X: " + item.X + ", Y: " + item.Y + " kolikaty: " + item.kolikaty + " prumery X " + item.prumerneX + " prumery Y " + item.prumerneY);
                }
            }

            foreach (string soubor in slozkaObrazku)
            {
                string nazevObrazku = ZiskejNazev(soubor);

                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);
                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width * 11 / 24, obrazek.Height * 11 / 24);

                //zmenšení obrazkù

                obrazek = filterSize1.Apply(obrazek);


                BitmapData data = obrazek.LockBits(new Rectangle(0, 0, obrazek.Width, obrazek.Height),
                                    ImageLockMode.ReadWrite, obrazek.PixelFormat);

                foreach (var item in kolekceBodu2)
                {
                    Drawing.Rectangle(data, new Rectangle(item.prumerneX, item.prumerneY, 2, 2), Color.Yellow);
                }
                obrazek.UnlockBits(data);

                Directory.CreateDirectory(vykresleniKrizku);
                obrazek.Save(vykresleniKrizku + nazevObrazku + "_prumer.png");

            }
        }
    }
}


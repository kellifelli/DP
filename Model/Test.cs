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
            ResizeBilinear filterSize2 = new ResizeBilinear(vzor.Width * 1 / 4, vzor.Height * 1 / 4);

            List<System.Drawing.Point> kolekceBodu = new List<System.Drawing.Point>();
            List<RozsirenyBod> kolekceBodu2 = new List<RozsirenyBod>();

            foreach (string soubor in slozkaObrazku)
            {
                var hodiny = System.Diagnostics.Stopwatch.StartNew();
                string nazevObrazku = ZiskejNazev(soubor);

                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);

                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width * 1 / 4, obrazek.Height * 1 / 4);
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


                    kolekceBodu.Add(m.Rectangle.Location);
                    kolekceBodu2.Add(new RozsirenyBod(m.Rectangle.Location.X, m.Rectangle.Location.Y));


                }




                obrazek.UnlockBits(data);

                Directory.CreateDirectory(vykresleniKrizku);
                obrazek.Save(vykresleniKrizku + nazevObrazku + ".png");

                hodiny.Stop();
                Console.WriteLine("obrazek trval " + hodiny.Elapsed.TotalSeconds);



            }
            //kolekce bodu je ze vsech obrazku ... 
            //kolekceBodu = kolekceBodu.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            foreach (var item in kolekceBodu)
            {
                //Console.WriteLine("X: " + item.X + ", Y: " + item.Y);




            }

            //ted musim dopocitat prumery respektive jinak urcit minimum ... kazde skupiny - mam je po X- kách 
            kolekceBodu2 = kolekceBodu2.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            int kolikaty = 1;
            for (int i = 0; i < kolekceBodu2.Count; i++)
            {
                if (kolekceBodu2[i].kolikaty == 0)
                {
                    kolekceBodu2[i].kolikaty = kolikaty;
                    //potrebuju dat do skupiny 
                    for (int j = i + 1; j < kolekceBodu2.Count; j++)
                    {
                        if ((Math.Abs(kolekceBodu2[j].X - kolekceBodu2[i].X) < 5) && (Math.Abs(kolekceBodu2[j].Y - kolekceBodu2[i].Y) < 5))
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
            foreach (var item in kolekceBodu2)
            {
                Console.WriteLine("X: " + item.X + ", Y: " + item.Y + " kolikaty: " + item.kolikaty);
            }

        }


    }
}


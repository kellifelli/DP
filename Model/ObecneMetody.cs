using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace DP

{
    class ObecneMetody
    {
        /* Metoda dostane na vstupu
        Nazev obrazku
        prvni slovo - na jeho konci zaène oøezávat
        druhé slovo - na jeho zaèátku skonèí s oøezem
        
        vrátí vlastnì string z názvu mezi prvním a druhým slovem.
         */
        public static string DatumCasZNazvu(string obrazek, string prvniSlovo, string druheSlovo)
        {
            string bezDruhehoSlova = obrazek.Substring(0, obrazek.LastIndexOf(druheSlovo));
            string bezPrvnihoSlova = obrazek.Substring(0, obrazek.LastIndexOf(prvniSlovo) + prvniSlovo.Length);

            return obrazek.Substring(bezPrvnihoSlova.Length, bezDruhehoSlova.Length - bezPrvnihoSlova.Length);
        }

        public static void vykresliNalezeneKrizkyDoObrazku(string slozka, int zmencovaciKonstanta, List<RozsirenyBod> souradniceKrizu, string jakUlozit)
        {
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            Bitmap obrazek = (Bitmap)Bitmap.FromFile(slozkaObrazku[0]);
            ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width / zmencovaciKonstanta, obrazek.Height / zmencovaciKonstanta);
            obrazek = filterSize1.Apply(obrazek);
            BitmapData data = obrazek.LockBits(new Rectangle(0, 0, obrazek.Width, obrazek.Height), ImageLockMode.ReadWrite, obrazek.PixelFormat);
            foreach (var m in souradniceKrizu)
            {
                //vykresleni bodu - stredu nalezeneho krizku
                Drawing.Rectangle(data, new Rectangle(m.X, m.Y, 2, 2), Color.Red);

            }
            obrazek.UnlockBits(data);
            obrazek.Save(jakUlozit);




            souradniceKrizu = souradniceKrizu.OrderBy(p => p.X).ToList();

            /* Vypisu vsechny souradnice 

            Console.WriteLine(souradniceKrizu.Count);
            foreach (var item in souradniceKrizu)
            {
                Console.WriteLine("X: " + item.X + " Y: " + item.Y);
            }
            Console.WriteLine("Xxxxxx: " + souradniceKrizu.Count);
*/
        }
    }
}




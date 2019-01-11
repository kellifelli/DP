using System;
using System.Drawing;
using System.IO;

namespace DP
{
    public static class UrceniCastiDne
    {

        public static void UrciDenANocVObrazcich(string slozka)
        {

            string[] seznamObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            foreach (string obrazek in seznamObrazku)
            {
                Bitmap image = (Bitmap)Bitmap.FromFile(obrazek);
                string nazevObr = ObecneMetody.DatumCasZNazvu(obrazek, "date-", "_round");
                image = image.Clone(new Rectangle(image.Width - 50, image.Height - 100, image.Width - (image.Width - 50), image.Height - (image.Height - 100)), image.PixelFormat);
                int[] total = new int[3]; // R, G, B
                int[] prumery = new int[3];
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {

                        total[0] += image.GetPixel(x, y).R;
                        total[1] += image.GetPixel(x, y).G;
                        total[2] += image.GetPixel(x, y).B;

                    }
                }

                prumery[0] = total[0] / (image.Width * image.Height);
                prumery[1] = total[0] / (image.Width * image.Height);
                prumery[2] = total[0] / (image.Width * image.Height);

                if (prumery[0] < 50 && prumery[0] < 50 && prumery[0] < 50)
                {
                    Console.WriteLine(obrazek + "    R:" + prumery[0] + "    G:" + prumery[1] + "    B:" + prumery[2] + "        NOC");
                }
                else
                {
                    Console.WriteLine(obrazek + "    R:" + prumery[0] + "    G:" + prumery[1] + "    B:" + prumery[2] + "        DEN");
                }
                //     Directory.CreateDirectory(slozka + "\\rohy\\");
                //      image.Save(slozka + "\\rohy\\" + nazevObr + "_roh.png");
            }

        }
    }
}
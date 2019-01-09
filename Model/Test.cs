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
        public static void NactiBody()
        {
            string line;
            int sirkaObrazku = 800, vyskaObrazku = 633;

            StreamReader sc = new StreamReader("temp\\body.txt");

            List<RozsirenyBod> bodiky = new List<RozsirenyBod>();
            bool smeNaX = true;
            int x = 0, y = 0;



            while ((line = sc.ReadLine()) != null)
            {
                if (smeNaX)
                {
                    Int32.TryParse(line, out x);
                    smeNaX = false;
                }
                else
                {
                    Int32.TryParse(line, out y);
                    smeNaX = true;
                    bodiky.Add(new RozsirenyBod(x, y));
                }
            }
            sc.Close();

            List<int> sirky = new List<int>();
            for (int i = 0; i < bodiky.Count; i++)
            {
                for (int j = i + 1; j < bodiky.Count; j++)
                {   
                   int sirka = bodiky[j].X - bodiky[i].X;
                   if (sirka > 5) 
                   sirky.Add(sirka);     
                }
            }

            foreach (int item in sirky)
            {
                Console.WriteLine(item);
            }

            // tak tady mam nacteny bodiky. v originale bude "seznamNalezenychKrizku" nebo tak neco

        }
    }
}


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace DP
{
    class Program
    {
        static void Main(string[] args)
        {
            // string cesta =  "D:\\repos\\DP\\DP\\temp\\01";
            string cesta = "D:\\repos\\DP\\DP\\temp\\01_XX\\orezany";
            //Console.WriteLine("zadejte cestu: ");
            //string cesta = Console.ReadLine();
            /* Začínám měřit čas programu. */
            var hodiny = System.Diagnostics.Stopwatch.StartNew();

            /* V Nultém kroce si zjistim kde je den a kde je noc a informaci o tom teprve někde uložím ať s ní pak mužu dál pracovat
                0. */
            //UrceniCastiDne.UrciDenANocVObrazcich(cesta);

            /* V prvním kroce ořežu modre okraje. A uložím do složky "orezany" 
               1. */
            //string orezanyObrazky = OrezKraju.OrezaniObrazkuVeSlozce(cesta);
            /* Ve druhém kroce rozřežu na misky a poukládám podle čísla misky. např. první miska bude {zadaná_cesta}/  
               1. */
            //RozrezaniNaMisky.RozrezObrazkyVeSlozce(orezanyObrazky);


            //ted mam v temp\\orezany\\01\\orezany  - rozdeleny na misky #endregion



            //DetekceZelene.NajdiCasRustu(cesta);


            Test.Rename(cesta);

            hodiny.Stop();
            Console.WriteLine("sekundty " + hodiny.Elapsed.TotalSeconds + " sekund. (V minutách: " + hodiny.Elapsed.TotalMinutes + ".)" + "V hodinach: " + hodiny.Elapsed.TotalHours + ".)");
            /* Počkám než se stiskne enter, aby mi nezmizly hned ty  vysledky. */
            Console.ReadKey(true);
        }

    }
}

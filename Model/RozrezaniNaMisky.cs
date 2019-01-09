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

        /* Metoda kter� dostane na vstupu n�zev slo�ky, kde jsou o�ezan� obr�zky od modr� misky a rozd�l� je na misky.  */
        public static void RozrezObrazkyVeSlozce(string slozka)
        {
            /* Projdu v�echny obr�zky a z�sk�m sou�adnice k��k�, kter� lze detekovat. */
            List<RozsirenyBod> souradniceZiskanychKrizu = ZiskejKrizkyZeVsechObrazku(slozka);

            /* Pokusik  zakreslen� obrazku s nalezen�mi k��ky - smazat, todle nepot�ebuji  2018 01 08 v tento den naps�no*/
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            Bitmap obrazek = (Bitmap)Bitmap.FromFile(slozkaObrazku[0]);
            ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width / zmencovaciKonstanta, obrazek.Height / zmencovaciKonstanta);
            /* Na  obr�zek pou�iji �ernob�l� filter a zmen��m ho. */

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

        /* Metoda, kter� na vstupu dostane slo�ku s obr�zky na kter�ch bude hledat k��ky. Postupn� projde v�echny obr�zky - ne v�echny k��ky jdou detekovat. */
        static List<RozsirenyBod> ZiskejKrizkyZeVsechObrazku(string slozka)
        {

            /* Na�teme pole soubor�, kter� budeme proch�ze, jako jejich n�zvy. */
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            /* Definice pomocn� prom�nn� - jak� vzor budu hledat. */
            string umisteniVzoru = "temp\\1_vzor.png";
            Bitmap vzor = (Bitmap)Bitmap.FromFile(umisteniVzoru);

            /*Definice �ed�ho filtru. */
            Grayscale filterSeda = new Grayscale(0.2125, 0.7154, 0.0721);

            /* Zm�n�en� a aplikov�n� �ed�ho filtru na vzor. */
            ResizeBilinear zmenseniVzoru = new ResizeBilinear(vzor.Width / zmencovaciKonstanta, vzor.Height / zmencovaciKonstanta);

            vzor = filterSeda.Apply(vzor);
            vzor = zmenseniVzoru.Apply(vzor);

            /* Vytvo��m si kolekci roz���en�ch bod�, kde si budu ukl�dat jednotliv� nalezen� sou�adnice k��k�. */
            List<RozsirenyBod> nalezeneBody = new List<RozsirenyBod>();

            foreach (string soubor in slozkaObrazku)
            {
                /* Spou�t�m hodiny abych ved�l jak dlouho trvaj� jednotliv� obr�zky. */
                var hodiny = System.Diagnostics.Stopwatch.StartNew();
                /* Z n�zvu obr�zku zase vytahuju pouze jm�no - te� u� tam mam i index tak�e v dal��m kroku nemus�m ukladat nic extra */
                string nazevObrazku = ObecneMetody.DatumCasZNazvu(soubor, "\\", ".png");

                /* na��t�m obr�zek */
                Bitmap obrazek = (Bitmap)Bitmap.FromFile(soubor);
                /* Definice zmen�ovac�ho filtru na origin�le to trv� stra�n� dlouho. */
                ResizeBilinear filterSize1 = new ResizeBilinear(obrazek.Width / zmencovaciKonstanta, obrazek.Height / zmencovaciKonstanta);
                /* Na  obr�zek pou�iji �ernob�l� filter a zmen��m ho. */
                obrazek = filterSeda.Apply(obrazek);
                obrazek = filterSize1.Apply(obrazek);

                /* Vyhled�vac� algoritmus, kter� dostane "kde" a "co" hledat. na za��tku je t�eba definovat s jakou p�esnost� - ta u� se zd� b�t docela vychytan�. */
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.968f);
                TemplateMatch[] matchings = tm.ProcessImage(obrazek, vzor);

                foreach (TemplateMatch m in matchings)
                {
                    /* nalezen� k��ek moment�ln� vystupuje jako okraj obr�zku vzor - tzn dopo��t�m jeho prost�edn� sou�adnici. A tento bod ulo��m do kolekce */
                    nalezeneBody.Add(new RozsirenyBod((m.Rectangle.Width / 2) + m.Rectangle.Location.X, (m.Rectangle.Height / 2) + m.Rectangle.Location.Y));
                }

                /* Zastav�m hodiny pro jednotliv� obr�zky a vyp�u info o tom jak dlouho trvalo na��st  */
                hodiny.Stop();
                Console.WriteLine("Obr: " + nazevObrazku + " trval " + hodiny.Elapsed.TotalSeconds + " sekund.");

            }
            /* V t�to ��sti prob�hne sjednocen� nalezen�ch bod�. Z ka�d�ho brazku m��u m�t jin� k��ky nalezen� a v�t�ina jich je tam v�ckr�t s nestejn�mi sou�adnicemi - mohou se li�it t�eba i o 1 pixel na ose X nap�.
                je tud� t�eba je pogrupovat tak abych v ka�d� grup� m�l jen jeden bod z ka�d�ho obrazku a z t�to grupy nal�st reprezentativn� bod.
             */

            /* Nejprve kolekci bod� se�ad�m podle X a podle Y. */
            nalezeneBody = nalezeneBody.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            /* Definice pomocn� prom�nn� - kolik�t�Skupina - proch�z�m postupn� v�echny bodz a p�i�azuji jim tuto informaci, vypov�d� o tom kolik�t� nalezen� to je. To �e je nalezen� prvn� nemus� znamenat �e to je prvn� bod na obrazku atd ... */
            int kolikataSkupina = 1;

            /* Cyklem projdu kolekci bodu a doplnim o kolikaty bod se jedna. Znovu upozorn�n� �e kolikaty = 10, m��e b�t klidn� polsedn� na obrazku. */
            for (int i = 0; i < nalezeneBody.Count; i++)
            {
                /* Ov��uji v p��pad�, �e jsem ji� bodu informaci nep�ilo�il. 0 = nev�m kolik�t� to je. */
                if (nalezeneBody[i].kolikaty == 0)
                {
                    /* Dopln�m mu informaci */
                    nalezeneBody[i].kolikaty = kolikataSkupina;
                    /* a naleznu v�echny jeho kamr�dy ve skupin�. */
                    for (int j = i + 1; j < nalezeneBody.Count; j++)
                    {
                        /* Pokud je vzd�lenost obou bodu i (tomu hled�m kamar�dy) a bodu j men�� na ob� strany ne� konstanta, tak bod "j" p�i�ad�m do stejn� skupiny jako jsem p�i�adil bod "i" */
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

            /* Nalezen�Body: kolekce bod�, kter� nesou informaci o sv�ch sou�adnic�ch a ka�d� bod v� do kter� skupiny pat��.
                Te� pot�ebuji z ka�d� skupiny naj� jednoho ide�ln�ho z�stupce, mo�n� by se dala vyu��t n�jak� echt minimaliza�n� metoda, ale mysl�m �e pr�m�r bohat� posta��. */

            /* Definice pomocn�ch prom�nn�ch */
            int sumaX = 0, sumaY = 0, pocet = 0, prumerX = 0, prumerY = 0, k = 0;

            /* nalezen�Body se�ad�m podle toho do kter� skupiny pat�� a za�nu je proch�zet tak abych ka�d�mu bodu mohl p�i�adit informaci o "ide�ln�m bodu" jeho skupiny. */
            nalezeneBody = nalezeneBody.OrderBy(p => p.kolikaty).ToList();

            /* Z nalezen�chBod� vyfiltruju pouze t�ch p�r optim�ln�ch. */
            List<RozsirenyBod> fin�lniNalezeneBody = new List<RozsirenyBod>();
            for (int i = 0; i < nalezeneBody.Count; i++)
            {
                sumaX = sumaX + nalezeneBody[i].X;
                sumaY = sumaY + nalezeneBody[i].Y;
                pocet++;
                /* Pokud jsem v 0 a� p�edposledn�m bod� mus�m ov��it jestli dal�� m� stejnou skupinu bodu, nebo je to posledn� bod a tak jim mu�u dosadit prum�r */
                if (((i + 1) < nalezeneBody.Count && (nalezeneBody[i].kolikaty != nalezeneBody[i + 1].kolikaty)) || ((i + 1) == nalezeneBody.Count))
                {
                    prumerX = (int)Math.Round(((double)(sumaX / pocet)), 0);
                    prumerY = (int)Math.Round(((double)(sumaY / pocet)), 0);
                    /* Vypo��t�m pr�m�rn� X a pr�m�rn� Y  a zp�tn� dosad�m bod�m, kter� nemaj� (nemaj� jen ty co pat�� do skupiny s vypo�itan�mi pr�m�ry) informaci o pr�m�rn�m, tedy optim�ln�m X a Y za skupinu.*/
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
                    fin�lniNalezeneBody.Add(new RozsirenyBod(prumerX, prumerY, false, nalezeneBody[i].kolikaty));
                    /* Null promennych */
                    sumaX = 0;
                    sumaY = 0;
                    pocet = 0;
                    prumerX = 0;
                    prumerY = 0;
                }
            }
            return fin�lniNalezeneBody;
        }
    }
}

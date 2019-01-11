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

        /* Metoda kter� dostane na vstupu n�zev slo�ky, kde jsou o�ezan� obr�zky od modr� misky a rozd�l� je na misky.  */
        public static void RozrezObrazkyVeSlozce(string slozka)
        {
            /* Projdu v�echny obr�zky a z�sk�m sou�adnice k��k�, kter� lze detekovat. */
            List<RozsirenyBod> souradniceZiskanychKrizu = ZiskejKrizkyZeVsechObrazku(slozka);
            /* Z nalezenych krizku dopocitam prumerny X a prumerny Y a dopocitam chybejici krizky at jich mam 90*/
            List<RozsirenyBod> dopocitaneKrizky = DopocitejOstatniBody(souradniceZiskanychKrizu, slozka);

            ObecneMetody.vykresliNalezeneKrizkyDoObrazku(slozka, zmencovaciKonstanta, dopocitaneKrizky, "D:\\repos\\DP\\DP\\temp\\krizek_nelezene.png");



        }


        /* Metoda, kter� na vstupu dostane slo�ku s obr�zky na kter�ch bude hledat k��ky. Postupn� projde v�echny obr�zky - ne v�echny k��ky jdou detekovat. */
        static List<RozsirenyBod> ZiskejKrizkyZeVsechObrazku(string slozka)
        {

            /* Na�teme pole soubor�, kter� budeme proch�ze, jako jejich n�zvy. */
            string[] slozkaObrazku = Directory.GetFiles(slozka, "*.png", SearchOption.TopDirectoryOnly);
            /* Definice pomocn� prom�nn� - jak� vzor budu hledat. */
            string umisteniVzoru = "D:\\repos\\DP\\DP\\temp\\1_vzor.png";
            Bitmap vzor = (Bitmap)Bitmap.FromFile(umisteniVzoru);

            /*Definice �ed�ho filtru. */
            Grayscale filterSeda = new Grayscale(0.2125, 0.7154, 0.0721);

            /* Zm�n�en� a aplikov�n� �ed�ho filtru na vzor. */
            ResizeBilinear zmenseniVzoru = new ResizeBilinear(vzor.Width / zmencovaciKonstanta, vzor.Height / zmencovaciKonstanta);

            vzor = filterSeda.Apply(vzor);
            vzor = zmenseniVzoru.Apply(vzor);

            /* Vytvo��m si kolekci roz���en�ch bod�, kde si budu ukl�dat jednotliv� nalezen� sou�adnice k��k�. */
            List<RozsirenyBod> nalezeneBody = new List<RozsirenyBod>();

            /* Todle by bylo dobr� rozhodit do v�ce vl�ken. */
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
                Console.WriteLine("Hled�n� k��k� v obr�zku Obr: " + nazevObrazku + " trvalo " + hodiny.Elapsed.TotalSeconds + " sekund.");

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
            List<RozsirenyBod> filtrNalezeneBody = new List<RozsirenyBod>();
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


            /* List nalezenych k��k� je po sloupc�ch takze ja projdu list a podivam se kde se X lisi o par pixelu zase tydle X dopocitam tak aby v ka�d� skupin� bylo 9 kus� */
            int sumaX = 0, sumaY = 0, pocet = 0, prumerX = 0, prumerY = 0; 
            List<int> prumerneX = new List<int>();
            List<int> prumerneY = new List<int>();

            /* Dopo�et X sou�adnic, po tomto cyklu budu mit  */
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



            /* V p��pad� �e mi n�jak� body chyb� tak je dopo��tam podle vzdalenosti */
            /* chzb� mi n�jak� sloupec */

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

                /* Te� musim zjistit jestli mezi ka�d�m bodem cca pr�m�rn� vzd�lenost, tzn jestli vzd�lenost je +/- 5 pixel� dejme tomu a kdy� nebude tak p�id�m bod + prum�rn� vzd�lenost*/

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
            /* chb� mi n�jak� radek */
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



            /* Definice n�vratu */
            List<RozsirenyBod> dopocitaneKrizky = new List<RozsirenyBod>();
            for (int i = 0; i < prumerneX.Count; i++)
            {
                for (int j = 0; j < prumerneY.Count; j++)
                {
                    dopocitaneKrizky.Add(new RozsirenyBod(prumerneX[i], prumerneY[j]));
                }
            }

            RozrezNaMisky(prumerneX, prumerneY, slozka);
            /* Kontroln� v�pis 
                        foreach (var item in prumerneX)
                        {
                            Console.WriteLine(item);
                        }
            */

            return dopocitaneKrizky;
        }

        static void RozrezNaMisky(List<int> prumerneX, List<int> prumerneY, string slozka)
        {
            /* Vytvo��m si slo�ky kam budu ukladat jednotliv� misky to pak muzu n�kam dat dolu proto�e budu proch�zet jednotliv� k��ky */
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
                                rozmery[0] = 0; // X za�atku
                                rozmery[1] = 0; // Y za�atku
                                rozmery[2] = prumerneX[j] - 1; // ���ka
                                rozmery[3] = prumerneY[i] - 1; // v��ka
                            }
                            else if (j == prumerneX.Count)                                              // jsem li v poslednim sloupci 
                            {
                                rozmery[0] = prumerneX[j - 1]; // X za�atku
                                rozmery[1] = 0; // Y za�atku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - sirka) - 1; // ���ka
                                rozmery[3] = prumerneY[i] - 1; // v��ka

                            }
                            else                                                                        // jsem li v prostrednim sloupci
                            {
                                rozmery[0] = prumerneX[j - 1]; // X za�atku
                                rozmery[1] = 0; // Y za�atku
                                rozmery[2] = Math.Abs(prumerneX[j] - prumerneX[j - 1]) - 1; // ���ka
                                rozmery[3] = prumerneY[i] - 1; // v��ka
                            }
                        }
                        else if (i == prumerneY.Count)                                            // jsem li v poslednim radku 
                        {
                            if (j == 0)                                                                 // jsem li v prvnim sloupci
                            {
                                rozmery[0] = 0; // X za�atku
                                rozmery[1] = prumerneY[i - 1]; // Y za�atku
                                rozmery[2] = prumerneX[j] - 1; // ���ka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - vyska) - 1; // v��ka
                            }
                            else if (j == prumerneX.Count)                                        // jsem li v poslednim sloupci 
                            {
                                rozmery[0] = prumerneX[j - 1]; // X za�atku
                                rozmery[1] = prumerneY[i - 1]; // Y za�atku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - sirka) - 1; // ���ka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - vyska) - 1; // v��ka

                            }
                            else                                                                        // jsem uprostred sloupcu
                            {
                                rozmery[0] = prumerneX[j - 1]; // X za�atku
                                rozmery[1] = prumerneY[i - 1]; // Y za�atku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - prumerneX[j]) - 1; // ���ka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - vyska) - 1; // v��ka
                            }

                        }
                        else                                                                    // jsem uprostred radkama
                        {
                            if (j == 0)                                                                 // jsem li v prvnim sloupci
                            {
                                rozmery[0] = 0; // X za�atku
                                rozmery[1] = prumerneY[i - 1]; // Y za�atku
                                rozmery[2] = prumerneX[j] - 1; // ���ka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - prumerneY[i]) - 1; // v��ka
                            }
                            else if (j == prumerneX.Count)                                              // jsem li v poslednim sloupci  
                            {
                                rozmery[0] = prumerneX[j - 1]; // X za�atku
                                rozmery[1] = prumerneY[i - 1]; // Y za�atku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - sirka) - 1; // ���ka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - prumerneY[i]) - 1; // v��ka

                            }
                            else                                                                         // jsem uprostred nekde
                            {
                                rozmery[0] = prumerneX[j - 1]; // X za�atku
                                rozmery[1] = prumerneY[i - 1]; // Y za�atku
                                rozmery[2] = Math.Abs(prumerneX[j - 1] - prumerneX[j]) - 1; // ���ka
                                rozmery[3] = Math.Abs(prumerneY[i - 1] - prumerneY[i]) - 1; // v��ka
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



using System;

namespace DP
{
    class RozsirenyBod
    {
        public int X;
        public int Y;
        public bool dopocitany; // indikator ur�uj�c� jestli je bod dopo��tan� z ostatn�ch nalezen�ch bod�
        public int kolikaty; // kolikaty je to bod na velke m�se - 0 ozna�uje neur�en� bod, kolikaty ze vsech nalezenych - mu�e b�t 1 ale ve skute�nosti je to na originalnim obrazku a� t�eba 3 proto�e ty prvn� dva jsem nenasel a budu je muset dopocitat :/

        public bool prumerDosazen;
        public int prumerneX; // dopocitane prumerne X ze vsech nalezenych bodu
        public int prumerneY; // dopocitane prumerne X ze vsech nalezenych bodu

        public RozsirenyBod(int x, int y, bool dopocitany = false, int kolikaty = 0)
        {
            this.X = x;
            this.Y = y;
            this.dopocitany = dopocitany;
            this.kolikaty = kolikaty;

        }


    }
}

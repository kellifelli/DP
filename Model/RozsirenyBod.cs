

using System;

namespace DP
{
    class RozsirenyBod
    {
        public int X;
        public int Y;
        public bool dopocitany; // indikator urèující jestli je bod dopoèítaný z ostatních nalezených bodù
        public int kolikaty; // kolikaty je to bod na velke míse - 0 oznaèuje neurèený bod, kolikaty ze vsech nalezenych - muže být 1 ale ve skuteènosti je to na originalnim obrazku až tøeba 3 protože ty první dva jsem nenasel a budu je muset dopocitat :/

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

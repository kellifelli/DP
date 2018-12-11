

using System;

namespace DP
{
    class RozsirenyBod
    {
        public int X;
        public int Y;
        public bool dopocitany; // indikator urèující jestli je bod dopoèítaný z ostatních nalezených bodù
        public int kolikaty; // kolikaty je to bod na velke míse - 0 oznaèuje neurèený bod

        public RozsirenyBod(int x, int y, bool dopocitany = false, int kolikaty = 0)
        {
            this.X = x;
            this.Y = y;
            this.dopocitany = dopocitany;
            this.kolikaty = kolikaty;

        }


    }
}

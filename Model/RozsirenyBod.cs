

using System;

namespace DP
{
    class RozsirenyBod
    {
        public int X;
        public int Y;
        public bool dopocitany; // indikator ur�uj�c� jestli je bod dopo��tan� z ostatn�ch nalezen�ch bod�
        public int kolikaty; // kolikaty je to bod na velke m�se - 0 ozna�uje neur�en� bod

        public RozsirenyBod(int x, int y, bool dopocitany = false, int kolikaty = 0)
        {
            this.X = x;
            this.Y = y;
            this.dopocitany = dopocitany;
            this.kolikaty = kolikaty;

        }


    }
}

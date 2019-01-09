using System;

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

    }
}

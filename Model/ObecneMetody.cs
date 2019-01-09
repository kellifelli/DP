using System;

namespace DP

{
    class ObecneMetody
    {
        /* Metoda dostane na vstupu
        Nazev obrazku
        prvni slovo - na jeho konci za�ne o�ez�vat
        druh� slovo - na jeho za��tku skon�� s o�ezem
        
        vr�t� vlastn� string z n�zvu mezi prvn�m a druh�m slovem.
         */
        public static string DatumCasZNazvu(string obrazek, string prvniSlovo, string druheSlovo)
        {
            string bezDruhehoSlova = obrazek.Substring(0, obrazek.LastIndexOf(druheSlovo));
            string bezPrvnihoSlova = obrazek.Substring(0, obrazek.LastIndexOf(prvniSlovo) + prvniSlovo.Length);

            return obrazek.Substring(bezPrvnihoSlova.Length, bezDruhehoSlova.Length - bezPrvnihoSlova.Length);
        }

    }
}

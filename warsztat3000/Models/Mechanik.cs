using System;
using System.Collections.Generic;
using System.Text;

namespace warsztat3000.Models
{
    public class Mechanik
    {
        public int Id { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Specjalizacja { get; set; }
        public string Telefon { get; set; }
        public bool CzyAktywny { get; set; } = true;
        public string PelneNazwisko => $"{Imie} {Nazwisko}";



        public List<Naprawa> ProwadzoneNaprawy { get; set; }
        public List<ZadanieNaprawy> WykonaneZadania { get; set; }
    }
}

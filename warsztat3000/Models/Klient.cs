using System;
using System.Collections.Generic;
using System.Text;

namespace warsztat3000.Models
{
    public class Klient
    {
        public int Id { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public DateTime DataDodania { get; set; } = DateTime.Now;

        public List<Pojazd> Pojazdy { get; set; }
    }
}

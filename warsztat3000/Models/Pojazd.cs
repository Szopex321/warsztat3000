using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace warsztat3000.Models
{
    public class Pojazd
    {
        public int Id { get; set; }
        public int KlientId { get; set; }
        [ForeignKey("KlientId")]
        public Klient Klient { get; set; }

        public string NrRejestracyjny { get; set; }
        public string VIN { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public int RokProdukcji { get; set; }
        public DateTime DataDodania { get; set; } = DateTime.Now;

        public List<Naprawa> Naprawy { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace warsztat3000.Models
{
    public class Naprawa
    {
        public int Id { get; set; }
        public int PojazdId { get; set; }
        [ForeignKey("PojazdId")]
        public Pojazd Pojazd { get; set; }

        public int MechanikProwadzacyId { get; set; }
        [ForeignKey("MechanikProwadzacyId")]
        public Mechanik MechanikProwadzacy { get; set; }

        public DateTime DataRozpoczecia { get; set; }
        public DateTime? PrzewidywanyKoniec { get; set; }
        public DateTime? RzeczywistyKoniec { get; set; }
        public int ProcentUkonczenia { get; set; } = 0;
        public string Status { get; set; }
        public string QrToken { get; set; }
        public string UwagiTechniczne { get; set; }
        public DateTime Utworzono { get; set; } = DateTime.Now;

        public ICollection<ZadanieNaprawy> ZadaniaNaprawy { get; set; } = new List<ZadanieNaprawy>();

        // (Jeśli planujesz używać kosztorysu tak samo, dodaj też to:)
        public ICollection<KosztorysPozycja> KosztorysPozycje { get; set; } = new List<KosztorysPozycja>();
    }
}

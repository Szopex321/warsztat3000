using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace warsztat3000.Models
{
    public class KosztorysPozycja
    {
        public int Id { get; set; }
        public int NaprawaId { get; set; }
        [ForeignKey("NaprawaId")]
        public Naprawa Naprawa { get; set; }

        public string NazwaPozycji { get; set; }
        public decimal Ilosc { get; set; }
        public decimal CenaJednostkowaBrutto { get; set; }
        public int Vat { get; set; } = 23;
        public string TypPozycji { get; set; } = TypyKosztorysu.Czesc;
    }
}

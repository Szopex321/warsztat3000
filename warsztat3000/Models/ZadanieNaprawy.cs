using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace warsztat3000.Models
{
    public class ZadanieNaprawy
    {
        public int Id { get; set; }
        public int NaprawaId { get; set; }
        [ForeignKey("NaprawaId")]
        public Naprawa Naprawa { get; set; }

        public string NazwaZadania { get; set; }
        public bool CzyWykonane { get; set; } = false;

        public int? MechanikWykonawcaId { get; set; }
        [ForeignKey("MechanikWykonawcaId")]
        public Mechanik MechanikWykonawca { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace warsztat3000.Models
{
    /// <summary>
    /// Reprezentuje pojedynczą wizytę naprawczą pojazdu w warsztacie.
    /// </summary>
    /// <remarks>
    /// Encja łączy pojazd, mechanika prowadzącego, zadania, kosztorys i status.
    /// Jest centralnym rekordem używanym zarówno przez ekran aktywnej naprawy,
    /// historię warsztatu, jak i lokalną stronę statusu QR.
    /// </remarks>
    /// <seealso cref="ZadanieNaprawy"/>
    /// <seealso cref="KosztorysPozycja"/>
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
        public bool CzyRozpoczeta { get; set; }
        public bool CzyZakonczona { get; set; }
        public DateTime? FaktycznyStart { get; set; }
        public DateTime? FaktycznyKoniec { get; set; }
        public decimal Roboczogodziny { get; set; }
        public int ProcentUkonczenia { get; set; } = 0;
        public string Status { get; set; }
        public string QrToken { get; set; }
        public string UwagiTechniczne { get; set; }
        public DateTime Utworzono { get; set; } = DateTime.Now;

        public ICollection<ZadanieNaprawy> ZadaniaNaprawy { get; set; } = new List<ZadanieNaprawy>();

        /// <summary>
        /// Pozycje kosztorysu przypisane do tej naprawy, w tym części oraz robocizna.
        /// </summary>
        /// <value>
        /// Kolekcja używana do wyliczenia ceny wizyty i do przygotowania widoku kosztorysu.
        /// </value>
        public ICollection<KosztorysPozycja> KosztorysPozycje { get; set; } = new List<KosztorysPozycja>();
    }
}

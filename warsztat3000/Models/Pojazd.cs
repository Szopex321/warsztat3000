using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace warsztat3000.Models
{
    /// <summary>
    /// Opisuje samochód klienta obsługiwany przez warsztat.
    /// </summary>
    /// <remarks>
    /// Numer VIN jest traktowany jako główny identyfikator biznesowy pojazdu.
    /// Dzięki temu aplikacja może wykryć, czy auto było już wcześniej przyjęte do warsztatu.
    /// </remarks>
    /// <seealso cref="Klient"/>
    /// <seealso cref="Naprawa"/>
    public class Pojazd
    {
        public int Id { get; set; }
        public int KlientId { get; set; }
        [ForeignKey("KlientId")]
        public Klient Klient { get; set; }

        public string NrRejestracyjny { get; set; }
        /// <summary>
        /// Numer identyfikacyjny pojazdu używany do wykrywania duplikatów.
        /// </summary>
        /// <value>
        /// Wartość powinna mieć 17 znaków i jest unikalna w bazie danych.
        /// </value>
        public string VIN { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public int RokProdukcji { get; set; }
        public DateTime DataDodania { get; set; } = DateTime.Now;

        public List<Naprawa> Naprawy { get; set; }
    }
}

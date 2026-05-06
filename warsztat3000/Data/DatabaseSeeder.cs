using System;
using System.Linq;
using warsztat3000.Models;

namespace warsztat3000.Data
{
    public static class DatabaseSeeder
    {
        public static void WypelnijBaze(WarsztatDbContext db)
        {
            if (db.Mechanicy.Any()) return;

            var mechanik1 = new Mechanik { Imie = "Jan", Nazwisko = "Kowalski", Specjalizacja = "Elektromechanik", Telefon = "111-222-333" };
            var mechanik2 = new Mechanik { Imie = "Piotr", Nazwisko = "Nowak", Specjalizacja = "Blacharz", Telefon = "444-555-666" };

            var klient1 = new Klient { Imie = "Anna", Nazwisko = "Zalewska", Telefon = "777-888-999", Email = "anna.z@example.com" };

            var pojazd1 = new Pojazd
            {
                Klient = klient1,
                Marka = "Toyota",
                Model = "Corolla",
                RokProdukcji = 2018,
                NrRejestracyjny = "RZE12345",
                VIN = "JTD1234567890ABCD"
            };

            var naprawa1 = new Naprawa
            {
                Pojazd = pojazd1,
                MechanikProwadzacy = mechanik1,
                Status = "W NAPRAWIE",
                DataRozpoczecia = DateTime.Now.AddDays(-2),
                UwagiTechniczne = "Stuka w lewym przednim kole.",
                QrToken = Guid.NewGuid().ToString()
            };

            var zadanie1 = new ZadanieNaprawy
            {
                Naprawa = naprawa1,
                NazwaZadania = "Wymiana sworznia wahacza",
                CzyWykonane = false
            };

            db.Mechanicy.AddRange(mechanik1, mechanik2);
            db.Klienci.Add(klient1);
            db.Pojazdy.Add(pojazd1);
            db.Naprawy.Add(naprawa1);
            db.ZadaniaNaprawy.Add(zadanie1);

            db.SaveChanges();
        }
    }
}
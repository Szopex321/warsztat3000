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
            var klient2 = new Klient { Imie = "Marek", Nazwisko = "Wójcik", Telefon = "500-600-700", Email = "marek.w@example.com" };

            var pojazd1 = new Pojazd { Klient = klient1, Marka = "Toyota", Model = "Corolla", RokProdukcji = 2018, NrRejestracyjny = "RZE12345", VIN = "JTD1234567890ABCD" };
            var pojazd2 = new Pojazd { Klient = klient1, Marka = "Ford", Model = "Focus", RokProdukcji = 2012, NrRejestracyjny = "WAW9876", VIN = "WF0BXX12345678" };
            var pojazd3 = new Pojazd { Klient = klient2, Marka = "VW", Model = "Golf", RokProdukcji = 2000, NrRejestracyjny = "KR12345", VIN = "WVWZZZ1JZXW123456" };
            var pojazd4 = new Pojazd { Klient = klient2, Marka = "Opel", Model = "Astra", RokProdukcji = 1999, NrRejestracyjny = "PO56789", VIN = "W0L0AHL48Y2123456" };

            var naprawa1 = new Naprawa
            {
                Pojazd = pojazd1,
                MechanikProwadzacy = mechanik1,
                Status = "W NAPRAWIE",
                DataRozpoczecia = DateTime.Now.AddDays(-2),
                ProcentUkonczenia = 50,
                UwagiTechniczne = "Stuka w lewym przednim kole.",
                QrToken = Guid.NewGuid().ToString()
            };

            var zadanie1 = new ZadanieNaprawy { Naprawa = naprawa1, NazwaZadania = "Wymiana sworznia wahacza", CzyWykonane = true };
            var zadanie2 = new ZadanieNaprawy { Naprawa = naprawa1, NazwaZadania = "Zbieżność", CzyWykonane = false };

            var naprawa2 = new Naprawa
            {
                Pojazd = pojazd2,
                MechanikProwadzacy = mechanik2,
                Status = "ZAKOŃCZONE",
                ProcentUkonczenia = 100,
                DataRozpoczecia = DateTime.Now.AddDays(-15),
                RzeczywistyKoniec = DateTime.Now.AddDays(-1),
                UwagiTechniczne = "Wymiana rozrządu i pompy wody.",
                QrToken = Guid.NewGuid().ToString()
            };
            var zadanie3 = new ZadanieNaprawy { Naprawa = naprawa2, NazwaZadania = "Wymiana rozrządu", CzyWykonane = true };

            db.Mechanicy.AddRange(mechanik1, mechanik2);
            db.Klienci.AddRange(klient1, klient2);

            db.Pojazdy.AddRange(pojazd1, pojazd2, pojazd3, pojazd4);

            db.Naprawy.AddRange(naprawa1, naprawa2);
            db.ZadaniaNaprawy.AddRange(zadanie1, zadanie2, zadanie3);

            db.SaveChanges();
        }
    }
}
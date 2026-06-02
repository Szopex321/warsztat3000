using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Models;

namespace warsztat3000.Data
{
    public static class DatabaseSeeder
    {
        public static void UpewnijSieZeSchematAktualny(WarsztatDbContext db)
        {
            db.Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS MarkiPojazdow (Id INTEGER NOT NULL CONSTRAINT PK_MarkiPojazdow PRIMARY KEY AUTOINCREMENT, Nazwa TEXT NOT NULL)");
            db.Database.ExecuteSqlRaw(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_MarkiPojazdow_Nazwa ON MarkiPojazdow (Nazwa)");
            db.Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS ModelePojazdow (Id INTEGER NOT NULL CONSTRAINT PK_ModelePojazdow PRIMARY KEY AUTOINCREMENT, MarkaPojazduId INTEGER NOT NULL, Nazwa TEXT NOT NULL, CONSTRAINT FK_ModelePojazdow_MarkiPojazdow_MarkaPojazduId FOREIGN KEY (MarkaPojazduId) REFERENCES MarkiPojazdow (Id) ON DELETE CASCADE)");
            db.Database.ExecuteSqlRaw(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_ModelePojazdow_MarkaPojazduId_Nazwa ON ModelePojazdow (MarkaPojazduId, Nazwa)");

            try
            {
                db.Database.ExecuteSqlRaw(
                    $"ALTER TABLE ZadaniaNaprawy ADD COLUMN StrefaPojazdu TEXT NOT NULL DEFAULT '{StrefyPojazdu.Silnik}'");
            }
            catch
            {
                // Kolumna już istnieje w aktualnej bazie.
            }

            DodajKolumneJesliBrakuje(db, "Naprawy", "CzyRozpoczeta", "INTEGER NOT NULL DEFAULT 0");
            DodajKolumneJesliBrakuje(db, "Naprawy", "CzyZakonczona", "INTEGER NOT NULL DEFAULT 0");
            DodajKolumneJesliBrakuje(db, "Naprawy", "FaktycznyStart", "TEXT NULL");
            DodajKolumneJesliBrakuje(db, "Naprawy", "FaktycznyKoniec", "TEXT NULL");
            DodajKolumneJesliBrakuje(db, "Naprawy", "Roboczogodziny", "TEXT NOT NULL DEFAULT '0'");
            DodajKolumneJesliBrakuje(db, "KosztorysPozycje", "TypPozycji", $"TEXT NOT NULL DEFAULT '{TypyKosztorysu.Czesc}'");

            db.Database.ExecuteSqlRaw("UPDATE Pojazdy SET VIN = 'WF0BXXGCBKDA12345' WHERE VIN = 'WF0BXX12345678'");
            db.Database.ExecuteSqlRaw("UPDATE Pojazdy SET Marka = 'Volkswagen' WHERE Marka = 'VW'");
        }

        public static void WypelnijBaze(WarsztatDbContext db)
        {
            WypelnijKatalogPojazdow(db);

            if (db.Mechanicy.Any()) return;

            var mechanik1 = new Mechanik { Imie = "Jan", Nazwisko = "Kowalski", Specjalizacja = "Elektromechanik", Telefon = "111-222-333" };
            var mechanik2 = new Mechanik { Imie = "Piotr", Nazwisko = "Nowak", Specjalizacja = "Blacharz", Telefon = "444-555-666" };

            var klient1 = new Klient { Imie = "Anna", Nazwisko = "Zalewska", Telefon = "777-888-999", Email = "anna.z@example.com" };
            var klient2 = new Klient { Imie = "Marek", Nazwisko = "Wójcik", Telefon = "500-600-700", Email = "marek.w@example.com" };

            var pojazd1 = new Pojazd { Klient = klient1, Marka = "Toyota", Model = "Corolla", RokProdukcji = 2018, NrRejestracyjny = "RZE12345", VIN = "JTD1234567890ABCD" };
            var pojazd2 = new Pojazd { Klient = klient1, Marka = "Ford", Model = "Focus", RokProdukcji = 2012, NrRejestracyjny = "WAW9876", VIN = "WF0BXXGCBKDA12345" };
            var pojazd3 = new Pojazd { Klient = klient2, Marka = "Volkswagen", Model = "Golf", RokProdukcji = 2000, NrRejestracyjny = "KR12345", VIN = "WVWZZZ1JZXW123456" };
            var pojazd4 = new Pojazd { Klient = klient2, Marka = "Opel", Model = "Astra", RokProdukcji = 1999, NrRejestracyjny = "PO56789", VIN = "W0L0AHL48Y2123456" };

            var naprawa1 = new Naprawa
            {
                Pojazd = pojazd1,
                MechanikProwadzacy = mechanik1,
                Status = "W NAPRAWIE",
                DataRozpoczecia = DateTime.Now.AddDays(-2),
                PrzewidywanyKoniec = DateTime.Now.AddDays(5),
                RzeczywistyKoniec = DateTime.Now.AddDays(5),
                CzyRozpoczeta = true,
                CzyZakonczona = false,
                FaktycznyStart = DateTime.Now.AddHours(-5),
                Roboczogodziny = 0,
                ProcentUkonczenia = 50,
                UwagiTechniczne = "Stuka w lewym przednim kole.",
                QrToken = NowyTokenQr()
            };

            var zadanie1 = new ZadanieNaprawy { Naprawa = naprawa1, NazwaZadania = "Wymiana sworznia wahacza", CzyWykonane = true, StrefaPojazdu = StrefyPojazdu.LewyPrzod };
            var zadanie2 = new ZadanieNaprawy { Naprawa = naprawa1, NazwaZadania = "Zbieżność", CzyWykonane = false, StrefaPojazdu = StrefyPojazdu.PrawyPrzod };

            var naprawa2 = new Naprawa
            {
                Pojazd = pojazd2,
                MechanikProwadzacy = mechanik2,
                Status = "ZAKOŃCZONE",
                ProcentUkonczenia = 100,
                DataRozpoczecia = DateTime.Now.AddDays(-15),
                PrzewidywanyKoniec = DateTime.Now.AddDays(-2),
                CzyRozpoczeta = true,
                CzyZakonczona = true,
                FaktycznyStart = DateTime.Now.AddDays(-15),
                FaktycznyKoniec = DateTime.Now.AddDays(-1),
                Roboczogodziny = 3,
                RzeczywistyKoniec = DateTime.Now.AddDays(-1),
                UwagiTechniczne = "Wymiana rozrządu i pompy wody.",
                QrToken = NowyTokenQr()
            };
            var zadanie3 = new ZadanieNaprawy { Naprawa = naprawa2, NazwaZadania = "Wymiana rozrządu", CzyWykonane = true, StrefaPojazdu = StrefyPojazdu.Silnik };

            var czesc1 = new KosztorysPozycja { Naprawa = naprawa1, NazwaPozycji = "Sworzeń wahacza", Ilosc = 1, CenaJednostkowaBrutto = 180, TypPozycji = TypyKosztorysu.Czesc };
            var robocizna2 = new KosztorysPozycja { Naprawa = naprawa2, NazwaPozycji = "Robocizna - 3 h", Ilosc = 3, CenaJednostkowaBrutto = 250, TypPozycji = TypyKosztorysu.Robocizna };

            db.Mechanicy.AddRange(mechanik1, mechanik2);
            db.Klienci.AddRange(klient1, klient2);

            db.Pojazdy.AddRange(pojazd1, pojazd2, pojazd3, pojazd4);

            db.Naprawy.AddRange(naprawa1, naprawa2);
            db.ZadaniaNaprawy.AddRange(zadanie1, zadanie2, zadanie3);
            db.KosztorysPozycje.AddRange(czesc1, robocizna2);

            db.SaveChanges();
        }

        private static void DodajKolumneJesliBrakuje(WarsztatDbContext db, string tabela, string kolumna, string definicja)
        {
            try
            {
                db.Database.ExecuteSqlRaw($"ALTER TABLE {tabela} ADD COLUMN {kolumna} {definicja}");
            }
            catch
            {
                // Kolumna już istnieje w aktualnej bazie.
            }
        }

        private static void WypelnijKatalogPojazdow(WarsztatDbContext db)
        {
            if (db.MarkiPojazdow.Any())
                return;

            DodajMarke(db, "Toyota", "Corolla", "Yaris", "Avensis", "RAV4", "Auris");
            DodajMarke(db, "Volkswagen", "Golf", "Passat", "Polo", "Tiguan", "Touran");
            DodajMarke(db, "Ford", "Focus", "Mondeo", "Fiesta", "Kuga", "Transit");
            DodajMarke(db, "Opel", "Astra", "Corsa", "Insignia", "Vectra", "Zafira");
            DodajMarke(db, "BMW", "Seria 1", "Seria 3", "Seria 5", "X3", "X5");
            DodajMarke(db, "Audi", "A3", "A4", "A6", "Q5", "Q7");
            DodajMarke(db, "Mercedes-Benz", "Klasa A", "Klasa C", "Klasa E", "GLC", "Sprinter");
            DodajMarke(db, "Skoda", "Fabia", "Octavia", "Superb", "Kodiaq", "Kamiq");
            DodajMarke(db, "Renault", "Clio", "Megane", "Scenic", "Kadjar", "Trafic");
            DodajMarke(db, "Peugeot", "206", "207", "308", "508", "3008");
            DodajMarke(db, "Fiat", "Punto", "Panda", "Tipo", "500", "Ducato");
            DodajMarke(db, "Hyundai", "i20", "i30", "Tucson", "Santa Fe", "Kona");
            DodajMarke(db, "Kia", "Ceed", "Sportage", "Rio", "Sorento", "Picanto");
            DodajMarke(db, "Honda", "Civic", "Accord", "CR-V", "Jazz", "HR-V");
            DodajMarke(db, "Nissan", "Qashqai", "Micra", "Juke", "X-Trail", "Primastar");

            db.SaveChanges();
        }

        private static void DodajMarke(WarsztatDbContext db, string nazwa, params string[] modele)
        {
            var marka = new MarkaPojazdu { Nazwa = nazwa };
            foreach (var model in modele)
            {
                marka.Modele.Add(new ModelPojazdu { Nazwa = model });
            }

            db.MarkiPojazdow.Add(marka);
        }

        private static string NowyTokenQr()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        }
    }
}
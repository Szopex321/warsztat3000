using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.Views
{
    public partial class DodajNapraweDialog : Window
    {
        private readonly int _pojazdId;

        public DodajNapraweDialog()
        {
            InitializeComponent();
            WczytajMechanikow();
            PlanowanyKoniecBox.Text = DateTime.Now.AddDays(7).ToString("dd.MM.yyyy");
        }

        public DodajNapraweDialog(Pojazd pojazd) : this()
        {
            _pojazdId = pojazd.Id;
            PojazdText.Text = $"{pojazd.NrRejestracyjny} - {pojazd.Marka} {pojazd.Model}";
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void Rozpocznij_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pojazdId == 0)
                {
                    PokazBlad("Najpierw wybierz pojazd z bazy.");
                    return;
                }

                if (MechanikCombo.SelectedItem is not Mechanik mechanik)
                {
                    PokazBlad("Wybierz mechanika prowadzącego.");
                    return;
                }

                if (!DateTime.TryParse(PlanowanyKoniecBox.Text, out var planowanyKoniec))
                {
                    PokazBlad("Podaj planowany koniec w formacie dd.mm.rrrr.");
                    return;
                }

                using (var db = new WarsztatDbContext())
                {
                    db.Database.EnsureCreated();
                    DatabaseSeeder.UpewnijSieZeSchematAktualny(db);

                    var istniejeAktywna = db.Naprawy.Any(n => n.PojazdId == _pojazdId && !n.CzyZakonczona && (n.Status == null || n.Status != "ZAKOŃCZONE"));
                    if (istniejeAktywna)
                    {
                        PokazBlad("Ten pojazd ma już aktywną naprawę.");
                        return;
                    }

                    var naprawa = new Naprawa
                    {
                        PojazdId = _pojazdId,
                        MechanikProwadzacyId = mechanik.Id,
                        DataRozpoczecia = DateTime.Now,
                        PrzewidywanyKoniec = planowanyKoniec.Date,
                        RzeczywistyKoniec = planowanyKoniec.Date,
                        Status = "PRZYJĘTA",
                        ProcentUkonczenia = 0,
                        CzyRozpoczeta = false,
                        CzyZakonczona = false,
                        Roboczogodziny = 0,
                        UwagiTechniczne = (UwagiBox.Text ?? string.Empty).Trim(),
                        QrToken = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                        Utworzono = DateTime.Now
                    };

                    db.Naprawy.Add(naprawa);
                    db.SaveChanges();
                }

                Close(true);
            }
            catch (Exception ex)
            {
                var szczegoly = ex.InnerException?.Message ?? ex.Message;
                PokazBlad($"Nie udało się dodać naprawy: {szczegoly}");
            }
        }

        private void WczytajMechanikow()
        {
            using (var db = new WarsztatDbContext())
            {
                db.Database.EnsureCreated();
                DatabaseSeeder.UpewnijSieZeSchematAktualny(db);

                var mechanicy = db.Mechanicy
                    .AsNoTracking()
                    .Where(m => m.CzyAktywny)
                    .OrderBy(m => m.Nazwisko)
                    .ToList();

                MechanikCombo.ItemsSource = mechanicy;

                if (mechanicy.Count == 0)
                {
                    MechanikCombo.IsEnabled = false;
                    DodajNapraweButton.IsEnabled = false;
                    PokazBlad("Brak aktywnych mechaników. Najpierw dodaj albo aktywuj mechanika.");
                    return;
                }
            }

            MechanikCombo.SelectedIndex = 0;
        }

        private void PokazBlad(string komunikat)
        {
            BladText.Text = komunikat;
            BladText.IsVisible = true;
        }
    }
}

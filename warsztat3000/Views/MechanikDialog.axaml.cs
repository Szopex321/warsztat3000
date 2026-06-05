using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using warsztat3000.Data;
using warsztat3000.Models;
using warsztat3000.Services;

namespace warsztat3000.Views
{
    public partial class MechanikDialog : Window
    {
        private readonly int? _mechanikId;

        public MechanikDialog()
        {
            InitializeComponent();
        }

        public MechanikDialog(int mechanikId) : this()
        {
            _mechanikId = mechanikId;
            Title = "EDYTUJ MECHANIKA";
            WczytajMechanika();
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Waliduj())
                    return;

                using (var db = new WarsztatDbContext())
                {
                    Mechanik mechanik;
                    if (_mechanikId.HasValue)
                    {
                        mechanik = db.Mechanicy.FirstOrDefault(m => m.Id == _mechanikId.Value);
                        if (mechanik == null)
                        {
                            PokazBlad("Nie znaleziono mechanika do edycji.");
                            return;
                        }
                    }
                    else
                    {
                        mechanik = new Mechanik();
                        db.Mechanicy.Add(mechanik);
                    }

                    mechanik.Imie = ImieBox.Text!.Trim();
                    mechanik.Nazwisko = NazwiskoBox.Text!.Trim();
                    mechanik.Specjalizacja = SpecjalizacjaBox.Text!.Trim();
                    mechanik.Telefon = TelefonBox.Text?.Trim();
                    mechanik.CzyAktywny = AktywnyCheck.IsChecked == true;

                    db.SaveChanges();
                }

                Close(true);
            }
            catch (Exception ex)
            {
                PokazBlad($"Nie udało się zapisać mechanika: {ex.Message}");
            }
        }

        private void WczytajMechanika()
        {
            using (var db = new WarsztatDbContext())
            {
                var mechanik = db.Mechanicy.FirstOrDefault(m => m.Id == _mechanikId);
                if (mechanik == null)
                    return;

                ImieBox.Text = mechanik.Imie;
                NazwiskoBox.Text = mechanik.Nazwisko;
                SpecjalizacjaBox.Text = mechanik.Specjalizacja;
                TelefonBox.Text = mechanik.Telefon;
                AktywnyCheck.IsChecked = mechanik.CzyAktywny;
            }
        }

        private bool Waliduj()
        {
            if (string.IsNullOrWhiteSpace(ImieBox.Text) ||
                string.IsNullOrWhiteSpace(NazwiskoBox.Text) ||
                string.IsNullOrWhiteSpace(SpecjalizacjaBox.Text))
            {
                PokazBlad("Uzupełnij imię, nazwisko i specjalizację.");
                return false;
            }

            if (!ValidationService.TelefonPoprawny(TelefonBox.Text))
            {
                PokazBlad("Telefon może zawierać cyfry, spacje, myślniki i plus.");
                return false;
            }

            return true;
        }

        private void PokazBlad(string komunikat)
        {
            BladText.Text = komunikat;
            BladText.IsVisible = true;
        }
    }
}

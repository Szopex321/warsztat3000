using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;
using warsztat3000.Services;

namespace warsztat3000.Views
{
    public partial class NowyPojazdDialog : Window
    {
        private readonly int? _pojazdId;
        private readonly CarCatalogService _carCatalogService = new();
        private List<MarkaPojazdu> _marki = new();
        private List<Klient> _klienci = new();
        public int? ZapisanyPojazdId { get; private set; }

        public NowyPojazdDialog()
        {
            InitializeComponent();
            WczytajKlientow();
            WczytajKatalog();
            UstawTrybSprawdzaniaVin(true);
        }

        public NowyPojazdDialog(int pojazdId) : this()
        {
            _pojazdId = pojazdId;
            Title = "EDYTUJ POJAZD";
            WczytajPojazd();
            UstawTrybSprawdzaniaVin(false);
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Waliduj(out var rok, out var marka, out var model))
                    return;

                using (var db = new WarsztatDbContext())
                {
                    var rejestracja = RejestracjaBox.Text!.Trim().ToUpperInvariant();
                    var vin = VinBox.Text!.Trim().ToUpperInvariant();
                    var duplikat = db.Pojazdy.Any(p =>
                        p.Id != (_pojazdId ?? 0) &&
                        (p.VIN == vin || p.NrRejestracyjny == rejestracja));

                    if (duplikat)
                    {
                        PokazBlad("Pojazd o takim VIN lub numerze rejestracyjnym już istnieje.");
                        return;
                    }

                    Pojazd? zapisanyPojazd;
                    if (_pojazdId.HasValue)
                    {
                        zapisanyPojazd = db.Pojazdy
                            .Include(p => p.Klient)
                            .FirstOrDefault(p => p.Id == _pojazdId.Value);

                        if (zapisanyPojazd == null)
                        {
                            PokazBlad("Nie znaleziono pojazdu do edycji.");
                            return;
                        }

                        UzupelnijPojazd(zapisanyPojazd, rok, marka, model, db);
                    }
                    else
                    {
                        zapisanyPojazd = new Pojazd();
                        UzupelnijPojazd(zapisanyPojazd, rok, marka, model, db);
                        db.Pojazdy.Add(zapisanyPojazd);
                    }

                    db.SaveChanges();
                    ZapisanyPojazdId = zapisanyPojazd.Id;
                }

                Close(true);
            }
            catch (Exception ex)
            {
                PokazBlad($"Nie udało się zapisać pojazdu: {ex.Message}");
            }
        }

        private void SprawdzVin_Click(object sender, RoutedEventArgs e)
        {
            var vin = (VinStartBox.Text ?? string.Empty).Trim().ToUpperInvariant();
            if (!ValidationService.VinPoprawny(vin))
            {
                PokazBlad("VIN musi mieć 17 znaków i składać się z liter oraz cyfr.");
                return;
            }

            using (var db = new WarsztatDbContext())
            {
                db.Database.EnsureCreated();
                DatabaseSeeder.UpewnijSieZeSchematAktualny(db);

                var istniejacyPojazd = db.Pojazdy
                    .AsNoTracking()
                    .FirstOrDefault(p => p.VIN == vin);

                if (istniejacyPojazd != null)
                {
                    ZapisanyPojazdId = istniejacyPojazd.Id;
                    Close(true);
                    return;
                }
            }

            VinBox.Text = vin;
            UstawTrybSprawdzaniaVin(false);
            PokazInfo("Nie znaleziono pojazdu po VIN. Uzupełnij dane i zapisz nowe auto.");
        }

        private void WczytajPojazd()
        {
            using (var db = new WarsztatDbContext())
            {
                var pojazd = db.Pojazdy
                    .Include(p => p.Klient)
                    .FirstOrDefault(p => p.Id == _pojazdId);

                if (pojazd == null)
                    return;

                ImieBox.Text = pojazd.Klient?.Imie;
                NazwiskoBox.Text = pojazd.Klient?.Nazwisko;
                TelefonBox.Text = pojazd.Klient?.Telefon;
                EmailBox.Text = pojazd.Klient?.Email;
                RejestracjaBox.Text = pojazd.NrRejestracyjny;
                KlientCombo.SelectedItem = _klienci.FirstOrDefault(k => k.Id == pojazd.KlientId);
                UstawMarkeIModel(pojazd.Marka, pojazd.Model);
                RokBox.Text = pojazd.RokProdukcji.ToString();
                VinBox.Text = pojazd.VIN;
            }
        }

        private bool Waliduj(out int rok, out string marka, out string model)
        {
            rok = 0;
            marka = string.Empty;
            model = string.Empty;

            if (NowyKlientCheck.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(ImieBox.Text) || string.IsNullOrWhiteSpace(NazwiskoBox.Text))
                {
                    PokazBlad("Podaj imię i nazwisko nowego klienta.");
                    return false;
                }

                if (!ValidationService.EmailPoprawny(EmailBox.Text))
                {
                    PokazBlad("Podaj poprawny e-mail klienta.");
                    return false;
                }

                if (!ValidationService.TelefonPoprawny(TelefonBox.Text))
                {
                    PokazBlad("Telefon może zawierać cyfry, spacje, myślniki i plus.");
                    return false;
                }
            }
            else if (KlientCombo.SelectedItem is not Klient)
            {
                PokazBlad("Wybierz klienta z bazy albo zaznacz dodawanie nowego klienta.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(RejestracjaBox.Text))
            {
                PokazBlad("Podaj numer rejestracyjny.");
                return false;
            }

            if (MarkaCombo.SelectedItem is not MarkaPojazdu wybranaMarka)
            {
                PokazBlad("Wybierz markę pojazdu.");
                return false;
            }

            marka = wybranaMarka.Nazwa;
            model = (ModelRecznyBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(model) && ModelCombo.SelectedItem is ModelPojazdu wybranyModel)
                model = wybranyModel.Nazwa;

            if (string.IsNullOrWhiteSpace(model))
            {
                PokazBlad("Wybierz model albo wpisz model ręcznie.");
                return false;
            }

            if (!int.TryParse(RokBox.Text, out rok) || !ValidationService.RokPoprawny(rok))
            {
                PokazBlad("Podaj poprawny rok produkcji.");
                return false;
            }

            if (!ValidationService.VinPoprawny(VinBox.Text))
            {
                PokazBlad("VIN musi mieć 17 znaków i składać się z liter oraz cyfr.");
                return false;
            }

            return true;
        }

        private void UzupelnijPojazd(Pojazd pojazd, int rok, string marka, string model, WarsztatDbContext db)
        {
            if (NowyKlientCheck.IsChecked == true)
            {
                var klient = new Klient();
                UzupelnijKlienta(klient);
                pojazd.Klient = klient;
            }
            else if (KlientCombo.SelectedItem is Klient klient)
            {
                pojazd.KlientId = klient.Id;
                pojazd.Klient = db.Klienci.First(k => k.Id == klient.Id);
            }

            pojazd.NrRejestracyjny = RejestracjaBox.Text?.Trim().ToUpperInvariant();
            pojazd.Marka = marka;
            pojazd.Model = model;
            pojazd.RokProdukcji = rok;
            pojazd.VIN = VinBox.Text?.Trim().ToUpperInvariant();
        }

        private void UzupelnijKlienta(Klient klient)
        {
            klient.Imie = ImieBox.Text?.Trim();
            klient.Nazwisko = NazwiskoBox.Text?.Trim();
            klient.Telefon = TelefonBox.Text?.Trim();
            klient.Email = EmailBox.Text?.Trim();
        }

        private void WczytajKlientow()
        {
            using (var db = new WarsztatDbContext())
            {
                _klienci = db.Klienci
                    .AsNoTracking()
                    .OrderBy(k => k.Nazwisko)
                    .ThenBy(k => k.Imie)
                    .ToList();
            }

            KlientCombo.ItemsSource = _klienci;
            if (_klienci.Count > 0)
                KlientCombo.SelectedIndex = 0;
            else
                UstawTrybNowegoKlienta(true);
        }

        private void WczytajKatalog()
        {
            _marki = _carCatalogService.PobierzMarki();
            MarkaCombo.ItemsSource = _marki;
            if (_marki.Count > 0)
                MarkaCombo.SelectedIndex = 0;
        }

        private void MarkaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarkaCombo.SelectedItem is not MarkaPojazdu marka)
                return;

            ModelCombo.ItemsSource = _carCatalogService.PobierzModele(marka.Id);
            ModelCombo.SelectedIndex = 0;
        }

        private void NowyKlient_Click(object sender, RoutedEventArgs e)
        {
            UstawTrybNowegoKlienta(NowyKlientCheck.IsChecked == true);
        }

        private void UstawTrybNowegoKlienta(bool nowyKlient)
        {
            NowyKlientCheck.IsChecked = nowyKlient;
            NowyKlientPanel.IsVisible = nowyKlient;
            KlientCombo.IsEnabled = !nowyKlient;
        }

        private void UstawTrybSprawdzaniaVin(bool sprawdzanieVin)
        {
            VinCheckPanel.IsVisible = sprawdzanieVin;
            FormularzPanel.IsVisible = !sprawdzanieVin;
            ZapiszButton.IsVisible = !sprawdzanieVin;
        }

        private void UstawMarkeIModel(string? marka, string? model)
        {
            var wybranaMarka = _marki.FirstOrDefault(m => string.Equals(m.Nazwa, marka, StringComparison.OrdinalIgnoreCase));
            if (wybranaMarka == null && string.Equals(marka, "VW", StringComparison.OrdinalIgnoreCase))
                wybranaMarka = _marki.FirstOrDefault(m => m.Nazwa == "Volkswagen");

            if (wybranaMarka == null)
                return;

            MarkaCombo.SelectedItem = wybranaMarka;
            var modele = _carCatalogService.PobierzModele(wybranaMarka.Id);
            ModelCombo.ItemsSource = modele;
            var wybranyModel = modele.FirstOrDefault(m => string.Equals(m.Nazwa, model, StringComparison.OrdinalIgnoreCase));
            if (wybranyModel != null)
            {
                ModelCombo.SelectedItem = wybranyModel;
                ModelRecznyBox.Text = string.Empty;
            }
            else
            {
                ModelRecznyBox.Text = model;
            }
        }

        private void PokazBlad(string komunikat)
        {
            BladText.Foreground = Brushes.Red;
            BladText.Text = komunikat;
            BladText.IsVisible = true;
        }

        private void PokazInfo(string komunikat)
        {
            BladText.Foreground = Brushes.LightGray;
            BladText.Text = komunikat;
            BladText.IsVisible = true;
        }
    }
}

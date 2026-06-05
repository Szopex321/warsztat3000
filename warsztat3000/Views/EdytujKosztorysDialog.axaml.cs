using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.Views
{
    public partial class EdytujKosztorysDialog : Window
    {
        private readonly int _naprawaId;
        private readonly ObservableCollection<KosztorysPozycja> _pozycje = new();

        public EdytujKosztorysDialog()
        {
            InitializeComponent();
            PozycjeGrid.ItemsSource = _pozycje;
        }

        public EdytujKosztorysDialog(int naprawaId) : this()
        {
            _naprawaId = naprawaId;
            WczytajPozycje();
        }

        private void Dodaj_Click(object sender, RoutedEventArgs e)
        {
            if (_naprawaId == 0 ||
                string.IsNullOrWhiteSpace(NazwaBox.Text))
            {
                PokazBlad("Podaj nazwę części.");
                return;
            }

            if (!TryParseDecimal(IloscBox.Text, out var ilosc) || ilosc <= 0)
            {
                PokazBlad("Ilość musi być większa od zera.");
                return;
            }

            if (!TryParseDecimal(CenaBox.Text, out var cena) || cena < 0)
            {
                PokazBlad("Cena nie może być ujemna.");
                return;
            }

            var pozycja = new KosztorysPozycja
            {
                NaprawaId = _naprawaId,
                NazwaPozycji = NazwaBox.Text.Trim(),
                Ilosc = ilosc,
                CenaJednostkowaBrutto = cena,
                Vat = 23,
                TypPozycji = TypyKosztorysu.Czesc
            };

            using (var db = new WarsztatDbContext())
            {
                db.KosztorysPozycje.Add(pozycja);
                db.SaveChanges();
            }

            _pozycje.Add(pozycja);
            NazwaBox.Text = string.Empty;
            IloscBox.Text = string.Empty;
            CenaBox.Text = string.Empty;
            BladText.IsVisible = false;
        }

        private void Usun_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Control control || control.DataContext is not KosztorysPozycja pozycja)
                return;

            if (pozycja.TypPozycji == TypyKosztorysu.Robocizna)
            {
                PokazBlad("Robocizna jest wyliczana automatycznie po zakończeniu naprawy.");
                return;
            }

            using (var db = new WarsztatDbContext())
            {
                var doUsuniecia = db.KosztorysPozycje.FirstOrDefault(p => p.Id == pozycja.Id);
                if (doUsuniecia != null)
                {
                    db.KosztorysPozycje.Remove(doUsuniecia);
                    db.SaveChanges();
                }
            }

            _pozycje.Remove(pozycja);
        }

        private void Zakoncz_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void WczytajPozycje()
        {
            using (var db = new WarsztatDbContext())
            {
                var pozycje = db.KosztorysPozycje
                    .AsNoTracking()
                    .Where(p => p.NaprawaId == _naprawaId)
                    .OrderBy(p => p.Id)
                    .ToList();

                foreach (var pozycja in pozycje)
                    _pozycje.Add(pozycja);
            }
        }

        private static bool TryParseDecimal(string? value, out decimal result)
        {
            return decimal.TryParse(value?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        private void PokazBlad(string komunikat)
        {
            BladText.Text = komunikat;
            BladText.IsVisible = true;
        }
    }
}

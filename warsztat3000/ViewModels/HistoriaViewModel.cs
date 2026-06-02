using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;
using warsztat3000.Services;
using warsztat3000.Views;

namespace warsztat3000.ViewModels
{
    public class HistoriaViewModel : ViewModelBase
    {
        public ObservableCollection<HistoriaPojazduViewModel> PojazdyZHistoria { get; }
        public ObservableCollection<WizytaHistoriiViewModel> WizytyWybranegoPojazdu { get; }
        private readonly List<HistoriaPojazduViewModel> _wszystkiePojazdyZHistoria = new();

        private string _tekstWyszukiwania = string.Empty;
        public string TekstWyszukiwania
        {
            get => _tekstWyszukiwania;
            set
            {
                if (SetProperty(ref _tekstWyszukiwania, value))
                    FiltrujPojazdy();
            }
        }

        private HistoriaPojazduViewModel? _wybranyPojazd;
        public HistoriaPojazduViewModel? WybranyPojazd
        {
            get => _wybranyPojazd;
            set
            {
                if (SetProperty(ref _wybranyPojazd, value))
                {
                    ZaladujWizyty(value?.PojazdId);
                    OnPropertyChanged(nameof(NaglowekWybranegoPojazdu));
                }
            }
        }

        public string NaglowekWybranegoPojazdu => WybranyPojazd == null
            ? "Wybierz auto z listy"
            : $"{WybranyPojazd.Rejestracja} - {WybranyPojazd.MarkaModel}";

        public HistoriaViewModel()
        {
            PojazdyZHistoria = new ObservableCollection<HistoriaPojazduViewModel>();
            WizytyWybranegoPojazdu = new ObservableCollection<WizytaHistoriiViewModel>();
            WczytajHistorie();
        }

        public void Odswiez()
        {
            var poprzedniPojazdId = WybranyPojazd?.PojazdId;
            WczytajHistorie();

            WybranyPojazd = PojazdyZHistoria.FirstOrDefault(p => p.PojazdId == poprzedniPojazdId)
                ?? PojazdyZHistoria.FirstOrDefault();
        }

        public void PokazHistoriePojazdu(int pojazdId)
        {
            WybranyPojazd = PojazdyZHistoria.FirstOrDefault(p => p.PojazdId == pojazdId)
                ?? PojazdyZHistoria.FirstOrDefault();
        }

        private void WczytajHistorie()
        {
            _wszystkiePojazdyZHistoria.Clear();
            PojazdyZHistoria.Clear();

            using (var db = new WarsztatDbContext())
            {
                var pojazdy = db.Pojazdy
                    .AsNoTracking()
                    .Include(p => p.Klient)
                    .Where(p => p.Naprawy.Any())
                    .OrderBy(p => p.NrRejestracyjny)
                    .ToList();

                var naprawyInfo = db.Naprawy
                    .AsNoTracking()
                    .Select(n => new
                    {
                        n.PojazdId,
                        Data = n.RzeczywistyKoniec ?? n.FaktycznyKoniec ?? n.PrzewidywanyKoniec ?? n.DataRozpoczecia
                    })
                    .ToList()
                    .GroupBy(n => n.PojazdId)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            LiczbaWizyt = g.Count(),
                            OstatniaWizyta = g.Max(n => n.Data)
                        });

                foreach (var pojazd in pojazdy)
                {
                    if (!naprawyInfo.TryGetValue(pojazd.Id, out var info))
                        continue;

                    _wszystkiePojazdyZHistoria.Add(new HistoriaPojazduViewModel(pojazd, info.LiczbaWizyt, info.OstatniaWizyta));
                }
            }

            FiltrujPojazdy();
            WybranyPojazd ??= PojazdyZHistoria.FirstOrDefault();
        }

        private void FiltrujPojazdy()
        {
            var aktualnieWybranyId = WybranyPojazd?.PojazdId;
            var filtr = (TekstWyszukiwania ?? string.Empty).Trim().ToLowerInvariant();
            var pojazdy = string.IsNullOrWhiteSpace(filtr)
                ? _wszystkiePojazdyZHistoria
                : _wszystkiePojazdyZHistoria.Where(p =>
                    p.MarkaModel.ToLowerInvariant().Contains(filtr) ||
                    p.Wlasciciel.ToLowerInvariant().Contains(filtr) ||
                    p.Rejestracja.ToLowerInvariant().Contains(filtr))
                .ToList();

            PojazdyZHistoria.Clear();
            foreach (var pojazd in pojazdy)
                PojazdyZHistoria.Add(pojazd);

            if (PojazdyZHistoria.Any(p => p.PojazdId == aktualnieWybranyId))
                WybranyPojazd = PojazdyZHistoria.First(p => p.PojazdId == aktualnieWybranyId);
            else
                WybranyPojazd = PojazdyZHistoria.FirstOrDefault();
        }

        private void ZaladujWizyty(int? pojazdId)
        {
            WizytyWybranegoPojazdu.Clear();
            if (pojazdId == null)
                return;

            using (var db = new WarsztatDbContext())
            {
                var wizyty = db.Naprawy
                    .AsNoTracking()
                    .Include(n => n.Pojazd)
                        .ThenInclude(p => p.Klient)
                    .Include(n => n.MechanikProwadzacy)
                    .Include(n => n.ZadaniaNaprawy)
                    .Include(n => n.KosztorysPozycje)
                    .Where(n => n.PojazdId == pojazdId.Value)
                    .ToList()
                    .OrderByDescending(n => n.RzeczywistyKoniec ?? n.FaktycznyKoniec ?? n.PrzewidywanyKoniec ?? n.DataRozpoczecia);

                foreach (var wizyta in wizyty)
                    WizytyWybranegoPojazdu.Add(new WizytaHistoriiViewModel(wizyta));
            }
        }
    }

    public class HistoriaPojazduViewModel
    {
        public int PojazdId { get; }
        public string Rejestracja { get; }
        public string MarkaModel { get; }
        public string Wlasciciel { get; }
        public string LiczbaWizytOpis { get; }
        public string OstatniaWizytaOpis { get; }

        public HistoriaPojazduViewModel(Pojazd pojazd, int liczbaWizyt, System.DateTime ostatniaWizyta)
        {
            PojazdId = pojazd.Id;
            Rejestracja = pojazd.NrRejestracyjny ?? "Brak rejestracji";
            MarkaModel = BrakJesliPuste($"{pojazd.Marka} {pojazd.Model}".Trim(), "Brak danych pojazdu");
            Wlasciciel = pojazd.Klient == null
                ? "Brak właściciela"
                : BrakJesliPuste($"{pojazd.Klient.Imie} {pojazd.Klient.Nazwisko}".Trim(), "Brak danych właściciela");
            LiczbaWizytOpis = liczbaWizyt == 1 ? "1 wizyta" : $"{liczbaWizyt} wizyty";
            OstatniaWizytaOpis = $"ostatnia: {ostatniaWizyta:dd.MM.yyyy}";
        }

        private static string BrakJesliPuste(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
    }

    public class WizytaHistoriiViewModel
    {
        private readonly Naprawa _naprawa;

        public string DataOpis { get; }
        public string StatusOpis { get; }
        public string MechanikOpis { get; }
        public string CzynnosciOpis { get; }
        public string CenaOpis { get; }
        public IAsyncRelayCommand PokazKosztorysCommand { get; }

        public WizytaHistoriiViewModel(Naprawa naprawa)
        {
            _naprawa = naprawa;
            PokazKosztorysCommand = new AsyncRelayCommand(PokazKosztorys);

            var data = naprawa.RzeczywistyKoniec ?? naprawa.FaktycznyKoniec ?? naprawa.PrzewidywanyKoniec ?? naprawa.DataRozpoczecia;
            DataOpis = data.ToString("dd.MM.yyyy");
            StatusOpis = naprawa.Status ?? "Brak statusu";
            MechanikOpis = naprawa.MechanikProwadzacy == null
                ? "Brak mechanika"
                : $"{naprawa.MechanikProwadzacy.Imie} {naprawa.MechanikProwadzacy.Nazwisko}";

            var czynnosci = naprawa.ZadaniaNaprawy
                .OrderBy(z => z.Id)
                .Select(z => $"- {z.NazwaZadania}")
                .ToList();

            CzynnosciOpis = czynnosci.Count == 0
                ? "Brak wpisanych czynności"
                : string.Join(System.Environment.NewLine, czynnosci);

            var cena = naprawa.KosztorysPozycje.Sum(p => p.Ilosc * p.CenaJednostkowaBrutto);
            CenaOpis = $"{cena:F2} zł";
        }

        private async Task PokazKosztorys()
        {
            await AppDialogService.ShowWindowAsync(new WydrukujKosztorysDialog(_naprawa, _naprawa.KosztorysPozycje));
        }
    }
}
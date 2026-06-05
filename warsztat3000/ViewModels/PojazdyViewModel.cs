using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// ViewModel zakładki z pojazdami posiadającymi aktywne naprawy.
    /// </summary>
    /// <remarks>
    /// Klasa odpowiada za filtrowanie listy pojazdów, otwieranie edycji danych auta
    /// oraz przekazanie wybranego pojazdu do widoku aktywnej naprawy.
    /// </remarks>
    /// <seealso cref="NaprawaViewModel"/>
    public partial class PojazdyViewModel : ViewModelBase
    {
        public ObservableCollection<Pojazd> ListaPojazdow { get; set; }
        public Action<Pojazd> OnPojazdWybrany { get; set; }
        public Action? DaneZmienione { get; set; }
        private List<Pojazd> _wszystkiePojazdy = new();

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

        private Pojazd _wybranyPojazd;
        public Pojazd WybranyPojazd
        {
            get => _wybranyPojazd;
            set
            {
                _wybranyPojazd = value;
                OnPropertyChanged(nameof(WybranyPojazd));

                if (value != null)
                {
                    OnPojazdWybrany?.Invoke(value);

                    _wybranyPojazd = null;
                    OnPropertyChanged(nameof(WybranyPojazd));
                }
            }
        }

        /// <summary>
        /// Tworzy listę pojazdów i od razu ładuje z bazy aktywne naprawy.
        /// </summary>
        public PojazdyViewModel()
        {
            ListaPojazdow = new ObservableCollection<Pojazd>();
            WczytajPojazdyZBazy();
        }

        /// <summary>
        /// Ponownie wczytuje listę pojazdów po zmianach w innych zakładkach.
        /// </summary>
        /// <remarks>
        /// Metoda jest wywoływana m.in. po dodaniu naprawy, edycji pojazdu albo usunięciu wpisu.
        /// </remarks>
        public void Odswiez()
        {
            WczytajPojazdyZBazy();
        }

        private void WczytajPojazdyZBazy()
        {
            using (var db = new WarsztatDbContext())
            {
                _wszystkiePojazdy = db.Pojazdy
                    .Include(p => p.Klient)
                    .Where(p => p.Naprawy.Any(n => !n.CzyZakonczona && (n.Status == null || n.Status != "ZAKOŃCZONE")))
                    .OrderBy(p => p.NrRejestracyjny)
                    .ToList();
            }

            FiltrujPojazdy();
        }

        private void FiltrujPojazdy()
        {
            var filtr = (TekstWyszukiwania ?? string.Empty).Trim().ToLowerInvariant();
            var pojazdy = string.IsNullOrWhiteSpace(filtr)
                ? _wszystkiePojazdy
                : _wszystkiePojazdy.Where(p =>
                    (p.NrRejestracyjny ?? string.Empty).ToLowerInvariant().Contains(filtr) ||
                    (p.VIN ?? string.Empty).ToLowerInvariant().Contains(filtr) ||
                    (p.Marka ?? string.Empty).ToLowerInvariant().Contains(filtr) ||
                    (p.Model ?? string.Empty).ToLowerInvariant().Contains(filtr) ||
                    (p.Klient?.Imie ?? string.Empty).ToLowerInvariant().Contains(filtr) ||
                    (p.Klient?.Nazwisko ?? string.Empty).ToLowerInvariant().Contains(filtr))
                .ToList();

            ListaPojazdow.Clear();
            foreach (var pojazd in pojazdy)
                ListaPojazdow.Add(pojazd);
        }

        /// <summary>
        /// Dodaje pojazd, a następnie opcjonalnie tworzy dla niego pierwszą aktywną naprawę.
        /// </summary>
        /// <remarks>
        /// Po zapisaniu naprawy metoda wywołuje <see cref="OnPojazdWybrany"/>, aby użytkownik
        /// od razu trafił do ekranu prowadzenia zlecenia.
        /// </remarks>
        [RelayCommand]
        private async Task NowyPojazd()
        {
            var dialog = new NowyPojazdDialog();
            var zapisanoPojazd = await AppDialogService.ShowDialogAsync(dialog);
            if (!zapisanoPojazd || dialog.ZapisanyPojazdId == null)
                return;

            Pojazd? pojazd;
            using (var db = new WarsztatDbContext())
            {
                pojazd = db.Pojazdy
                    .Include(p => p.Klient)
                    .FirstOrDefault(p => p.Id == dialog.ZapisanyPojazdId.Value);
            }

            if (pojazd == null)
            {
                Odswiez();
                DaneZmienione?.Invoke();
                return;
            }

            var zapisanoNaprawe = await AppDialogService.ShowDialogAsync(new DodajNapraweDialog(pojazd));
            Odswiez();
            DaneZmienione?.Invoke();

            if (zapisanoNaprawe)
                OnPojazdWybrany?.Invoke(pojazd);
        }

        /// <summary>
        /// Otwiera formularz edycji danych wybranego pojazdu.
        /// </summary>
        /// <param name="pojazd">Pojazd wskazany na liście aktywnych napraw.</param>
        [RelayCommand]
        private async Task EdytujPojazd(Pojazd pojazd)
        {
            if (pojazd == null)
                return;

            var zapisano = await AppDialogService.ShowDialogAsync(new NowyPojazdDialog(pojazd.Id));
            if (!zapisano)
                return;

            Odswiez();
            DaneZmienione?.Invoke();
        }

        /// <summary>
        /// Usuwa pojazd wraz z naprawami, zadaniami i pozycjami kosztorysu.
        /// </summary>
        /// <param name="pojazd">Pojazd przeznaczony do usunięcia.</param>
        /// <remarks>
        /// Jeżeli po usunięciu auta klient nie ma już żadnych pojazdów, usuwany jest również
        /// rekord klienta. Dzięki temu baza demonstracyjna nie gromadzi osieroconych właścicieli.
        /// </remarks>
        [RelayCommand]
        private async Task UsunPojazd(Pojazd pojazd)
        {
            if (pojazd == null)
                return;

            using (var db = new WarsztatDbContext())
            {
                var naprawyIds = db.Naprawy
                    .Where(n => n.PojazdId == pojazd.Id)
                    .Select(n => n.Id)
                    .ToList();

                db.KosztorysPozycje.RemoveRange(db.KosztorysPozycje.Where(k => naprawyIds.Contains(k.NaprawaId)));
                db.ZadaniaNaprawy.RemoveRange(db.ZadaniaNaprawy.Where(z => naprawyIds.Contains(z.NaprawaId)));
                db.Naprawy.RemoveRange(db.Naprawy.Where(n => n.PojazdId == pojazd.Id));

                var pojazdDoUsuniecia = db.Pojazdy.FirstOrDefault(p => p.Id == pojazd.Id);
                if (pojazdDoUsuniecia != null)
                    db.Pojazdy.Remove(pojazdDoUsuniecia);

                var klientId = pojazd.KlientId;
                db.SaveChanges();

                if (!db.Pojazdy.Any(p => p.KlientId == klientId))
                {
                    var klient = db.Klienci.FirstOrDefault(k => k.Id == klientId);
                    if (klient != null)
                    {
                        db.Klienci.Remove(klient);
                        db.SaveChanges();
                    }
                }
            }

            Odswiez();
            DaneZmienione?.Invoke();
            await AppDialogService.ShowMessageAsync("Usunięto pojazd", "Pojazd i powiązane naprawy zostały usunięte z bazy.");
        }
    }
}
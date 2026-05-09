using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class NaprawaViewModel : ViewModelBase
    {
        private Naprawa _aktywnaNaprawa;
        public Naprawa AktywnaNaprawa
        {
            get => _aktywnaNaprawa;
            set
            {
                _aktywnaNaprawa = value;
                OnPropertyChanged(nameof(AktywnaNaprawa));
                OnPropertyChanged(nameof(InicjalyKlienta));
                OnPropertyChanged(nameof(ImieNazwiskoKlienta));
                OnPropertyChanged(nameof(ImieNazwiskoMechanika));
            }
        }

        public string InicjalyKlienta => AktywnaNaprawa?.Pojazd?.Klient != null
            ? $"{AktywnaNaprawa.Pojazd.Klient.Imie[0]}{AktywnaNaprawa.Pojazd.Klient.Nazwisko[0]}"
            : "??";

        public string ImieNazwiskoKlienta => AktywnaNaprawa?.Pojazd?.Klient != null
            ? $"{AktywnaNaprawa.Pojazd.Klient.Imie} {AktywnaNaprawa.Pojazd.Klient.Nazwisko}"
            : "Brak danych";

        public string ImieNazwiskoMechanika => AktywnaNaprawa?.MechanikProwadzacy != null
            ? $"{AktywnaNaprawa.MechanikProwadzacy.Imie} {AktywnaNaprawa.MechanikProwadzacy.Nazwisko}"
            : "Brak mechanika";

        private int _procentUkonczenia;
        public int ProcentUkonczenia
        {
            get => _procentUkonczenia;
            set { if (_procentUkonczenia != value) { _procentUkonczenia = value; OnPropertyChanged(nameof(ProcentUkonczenia)); } }
        }

        public ObservableCollection<ZadanieViewModel> ListaZadan { get; set; }

        public NaprawaViewModel()
        {
            ListaZadan = new ObservableCollection<ZadanieViewModel>();
            WczytajDaneZBazy();
        }

        private void WczytajDaneZBazy()
        {
            using (var db = new WarsztatDbContext())
            {
                db.Database.EnsureCreated();

                var naprawaZBazy = db.Naprawy
                    .Include(n => n.Pojazd)
                        .ThenInclude(p => p.Klient)
                    .Include(n => n.MechanikProwadzacy)
                    .Include(n => n.ZadaniaNaprawy)
                    .FirstOrDefault();

                if (naprawaZBazy != null)
                {
                    AktywnaNaprawa = naprawaZBazy;

                    foreach (var zadanie in naprawaZBazy.ZadaniaNaprawy)
                    {
                        DodajZadanie(zadanie.NazwaZadania, zadanie.CzyWykonane);
                    }
                }
            }

            PrzeliczProgres();
        }

        private void DodajZadanie(string nazwa, bool czyWykonane)
        {
            var zadanie = new ZadanieViewModel(nazwa, czyWykonane);
            zadanie.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ZadanieViewModel.CzyWykonane))
                    PrzeliczProgres();
            };
            ListaZadan.Add(zadanie);
        }

        private void PrzeliczProgres()
        {
            if (ListaZadan.Count == 0) { ProcentUkonczenia = 0; return; }
            double wykonane = ListaZadan.Count(z => z.CzyWykonane);
            ProcentUkonczenia = (int)Math.Round((wykonane / ListaZadan.Count) * 100);
        }
        public void ZaladujPojazd(Pojazd wybraneAuto)
        {
            using (var db = new WarsztatDbContext())
            {
                var aktywna = db.Naprawy
                    .Include(n => n.Pojazd).ThenInclude(p => p.Klient)
                    .Include(n => n.MechanikProwadzacy)
                    .Include(n => n.ZadaniaNaprawy)
                    .FirstOrDefault(n => n.PojazdId == wybraneAuto.Id && n.Status != "ZAKOŃCZONE");

                ListaZadan.Clear();

                if (aktywna != null)
                {
                    AktywnaNaprawa = aktywna;
                    foreach (var z in aktywna.ZadaniaNaprawy)
                    {
                        DodajZadanie(z.NazwaZadania, z.CzyWykonane);
                    }
                }
                else
                {
                    var pelneAuto = db.Pojazdy.Include(p => p.Klient).First(p => p.Id == wybraneAuto.Id);

                    AktywnaNaprawa = new Naprawa
                    {
                        Pojazd = pelneAuto,
                        MechanikProwadzacy = null,
                        Status = "Brak aktywnej naprawy",
                        ProcentUkonczenia = 0
                    };
                }

                PrzeliczProgres();
            }
        }
    }


}
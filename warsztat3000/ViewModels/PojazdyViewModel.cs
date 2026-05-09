using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class PojazdyViewModel : ViewModelBase
    {
        public ObservableCollection<Pojazd> ListaPojazdow { get; set; }
        public Action<Pojazd> OnPojazdWybrany { get; set; }
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

        public PojazdyViewModel()
        {
            ListaPojazdow = new ObservableCollection<Pojazd>();
            WczytajPojazdyZBazy();
        }

        private void WczytajPojazdyZBazy()
        {
            using (var db = new WarsztatDbContext())
            {
                var pojazdy = db.Pojazdy.Include(p => p.Klient).ToList();
                foreach (var pojazd in pojazdy) ListaPojazdow.Add(pojazd);
            }
        }
    }
}
using System;
using System.Collections.ObjectModel;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public NaprawaViewModel NaprawaVM { get; }
        public PojazdyViewModel PojazdyVM { get; }
        public HistoriaViewModel HistoriaVM { get; }
        public MechanicyViewModel MechanicyVM { get; }

        private int _wybranaZakladka;
        public int WybranaZakladka
        {
            get => _wybranaZakladka;
            set { _wybranaZakladka = value; OnPropertyChanged(nameof(WybranaZakladka)); }
        }

        public MainWindowViewModel()
        {
            NaprawaVM = new NaprawaViewModel();
            PojazdyVM = new PojazdyViewModel();
            HistoriaVM = new HistoriaViewModel();
            MechanicyVM = new MechanicyViewModel();

            PojazdyVM.OnPojazdWybrany = PrzejdzDoNaprawy;
        }

        private void PrzejdzDoNaprawy(Pojazd wybraneAuto)
        {
            WybranaZakladka = 0;
            NaprawaVM.ZaladujPojazd(wybraneAuto);
        }
    }
}
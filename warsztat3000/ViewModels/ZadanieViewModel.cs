using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class ZadanieViewModel : INotifyPropertyChanged
    {
        public int Id { get; }
        public string Nazwa { get; set; }
        public string StrefaPojazdu { get; set; }
        public string StrefaNazwa => StrefyPojazdu.Etykieta(StrefaPojazdu);

        private bool _czyWykonane;
        public bool CzyWykonane
        {
            get => _czyWykonane;
            set { if (_czyWykonane != value) { _czyWykonane = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CzyWykonane))); } }
        }

        public ZadanieViewModel(int id, string nazwa, bool czyWykonane, string strefaPojazdu)
        {
            Id = id;
            Nazwa = nazwa;
            StrefaPojazdu = strefaPojazdu;
            CzyWykonane = czyWykonane;
        }

        public ZadanieViewModel(string nazwa, bool czyWykonane)
            : this(0, nazwa, czyWykonane, StrefyPojazdu.Silnik)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace warsztat3000.ViewModels
{
    public class ZadanieViewModel : INotifyPropertyChanged
    {
        public string Nazwa { get; set; }
        private bool _czyWykonane;
        public bool CzyWykonane
        {
            get => _czyWykonane;
            set { if (_czyWykonane != value) { _czyWykonane = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CzyWykonane))); } }
        }
        public ZadanieViewModel(string nazwa, bool czyWykonane) { Nazwa = nazwa; CzyWykonane = czyWykonane; }
        public event PropertyChangedEventHandler PropertyChanged;


    }
}

using System.Collections.Generic;
using System.Linq;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class StrefaUszkodzenViewModel : ViewModelBase
    {
        public StrefaUszkodzenViewModel(string kod)
        {
            Kod = kod;
            Nazwa = StrefyPojazdu.Etykieta(kod);
        }

        public string Kod { get; }
        public string Nazwa { get; }

        private int _liczbaZadan;
        public int LiczbaZadan
        {
            get => _liczbaZadan;
            private set => SetProperty(ref _liczbaZadan, value);
        }

        private string _opisZadan = "Brak napraw w tej strefie";
        public string OpisZadan
        {
            get => _opisZadan;
            private set => SetProperty(ref _opisZadan, value);
        }

        public void UstawZadania(IEnumerable<ZadanieViewModel> zadania)
        {
            var lista = zadania.ToList();
            LiczbaZadan = lista.Count;
            OpisZadan = lista.Count == 0
                ? "Brak napraw w tej strefie"
                : string.Join("\n", lista.Select(z => z.CzyWykonane ? $"Wykonane: {z.Nazwa}" : $"Do zrobienia: {z.Nazwa}"));
        }
    }
}

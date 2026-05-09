using System.Collections.ObjectModel;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public PojazdyViewModel PojazdyVM { get; }
        public NaprawaViewModel NaprawaVM { get; }
        public HistoriaViewModel HistoriaVM { get; }

        public MainWindowViewModel()
        {
            PojazdyVM = new PojazdyViewModel();
            NaprawaVM = new NaprawaViewModel();
            HistoriaVM = new HistoriaViewModel();
        }
    }
}
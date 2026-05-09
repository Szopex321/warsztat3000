using System.Collections.ObjectModel;
using warsztat3000.Models;
// Upewnij się, że masz tu odpowiednie usingi dla swoich modeli

namespace warsztat3000.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Twoja obecna lista mechaników
        public ObservableCollection<Mechanik> Mechanicy { get; set; }

        // NOWE: Dodajemy ViewModel dla Twojego nowego widoku
        public NaprawaViewModel NaprawaVM { get; }

        public MainWindowViewModel()
        {
            // ... Tutaj masz swoje obecne pobieranie mechaników z bazy ...

            // Inicjalizujemy nowy ViewModel dla napraw
            NaprawaVM = new NaprawaViewModel();
        }
    }
}
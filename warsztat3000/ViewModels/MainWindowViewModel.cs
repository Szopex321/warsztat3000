using System.Collections.ObjectModel;
using System.Linq;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Mechanik> Mechanicy { get; set; }

        public MainWindowViewModel()
        {
            using var db = new WarsztatDbContext();

            var listaZBazy = db.Mechanicy.ToList();
            Mechanicy = new ObservableCollection<Mechanik>(listaZBazy);
        }
    }
}
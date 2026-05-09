using System.Collections.ObjectModel;
using System.Linq;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class MechanicyViewModel : ViewModelBase
    {
        public ObservableCollection<Mechanik> ListaMechanikow { get; set; }

        public MechanicyViewModel()
        {
            ListaMechanikow = new ObservableCollection<Mechanik>();
            WczytajMechanikowZBazy();
        }

        private void WczytajMechanikowZBazy()
        {
            using (var db = new WarsztatDbContext())
            {
                var mechanicy = db.Mechanicy.ToList();
                foreach (var mech in mechanicy)
                {
                    ListaMechanikow.Add(mech);
                }
            }
        }
    }
}
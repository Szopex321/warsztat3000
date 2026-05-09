using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.ViewModels
{
    public class HistoriaViewModel : ViewModelBase
    {
        public ObservableCollection<Naprawa> ZakonczoneNaprawy { get; set; }

        public HistoriaViewModel()
        {
            ZakonczoneNaprawy = new ObservableCollection<Naprawa>();
            WczytajHistorie();
        }

        private void WczytajHistorie()
        {
            using (var db = new WarsztatDbContext())
            {
                var historiaZBazy = db.Naprawy
                    .Include(n => n.Pojazd)
                        .ThenInclude(p => p.Klient)
                    .Include(n => n.MechanikProwadzacy)
                    .Where(n => n.Status == "ZAKOŃCZONE")
                    .OrderByDescending(n => n.RzeczywistyKoniec)
                    .ToList();

                foreach (var naprawa in historiaZBazy)
                {
                    ZakonczoneNaprawy.Add(naprawa);
                }
            }
        }
    }
}
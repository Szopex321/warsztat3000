using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using warsztat3000.Data;
using warsztat3000.Models;
using warsztat3000.Services;
using warsztat3000.Views;

namespace warsztat3000.ViewModels
{
    public partial class MechanicyViewModel : ViewModelBase
    {
        public ObservableCollection<Mechanik> ListaMechanikow { get; set; }

        public MechanicyViewModel()
        {
            ListaMechanikow = new ObservableCollection<Mechanik>();
            WczytajMechanikowZBazy();
        }

        public void Odswiez()
        {
            WczytajMechanikowZBazy();
        }

        private void WczytajMechanikowZBazy()
        {
            ListaMechanikow.Clear();

            using (var db = new WarsztatDbContext())
            {
                var mechanicy = db.Mechanicy.ToList();
                foreach (var mech in mechanicy)
                {
                    ListaMechanikow.Add(mech);
                }
            }
        }

        [RelayCommand]
        private async Task DodajMechanika()
        {
            var zapisano = await AppDialogService.ShowDialogAsync(new MechanikDialog());
            if (zapisano)
                Odswiez();
        }

        [RelayCommand]
        private async Task EdytujMechanika(Mechanik mechanik)
        {
            if (mechanik == null)
                return;

            var zapisano = await AppDialogService.ShowDialogAsync(new MechanikDialog(mechanik.Id));
            if (zapisano)
                Odswiez();
        }

        [RelayCommand]
        private async Task DezaktywujMechanika(Mechanik mechanik)
        {
            if (mechanik == null)
                return;

            using (var db = new WarsztatDbContext())
            {
                var zBazy = db.Mechanicy.FirstOrDefault(m => m.Id == mechanik.Id);
                if (zBazy != null)
                {
                    zBazy.CzyAktywny = !zBazy.CzyAktywny;
                    db.SaveChanges();
                }
            }

            Odswiez();
            await AppDialogService.ShowMessageAsync("Zmieniono status", "Status aktywności mechanika został zapisany.");
        }
    }
}
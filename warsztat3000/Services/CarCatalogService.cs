using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;

namespace warsztat3000.Services
{
    public class CarCatalogService
    {
        public List<MarkaPojazdu> PobierzMarki()
        {
            using (var db = new WarsztatDbContext())
            {
                DatabaseSeeder.UpewnijSieZeSchematAktualny(db);
                DatabaseSeeder.WypelnijBaze(db);

                return db.MarkiPojazdow
                    .AsNoTracking()
                    .OrderBy(m => m.Nazwa)
                    .ToList();
            }
        }

        public List<ModelPojazdu> PobierzModele(int markaId)
        {
            using (var db = new WarsztatDbContext())
            {
                return db.ModelePojazdow
                    .AsNoTracking()
                    .Where(m => m.MarkaPojazduId == markaId)
                    .OrderBy(m => m.Nazwa)
                    .ToList();
            }
        }
    }
}

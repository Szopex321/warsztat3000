using Microsoft.EntityFrameworkCore;
using warsztat3000.Models;

namespace warsztat3000.Data
{
    public class WarsztatDbContext : DbContext
    {
        public DbSet<Mechanik> Mechanicy { get; set; }
        public DbSet<Klient> Klienci { get; set; }
        public DbSet<Pojazd> Pojazdy { get; set; }
        public DbSet<Naprawa> Naprawy { get; set; }
        public DbSet<ZadanieNaprawy> ZadaniaNaprawy { get; set; }
        public DbSet<KosztorysPozycja> KosztorysPozycje { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=warsztat3000.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pojazd>()
                .HasIndex(p => p.VIN)
                .IsUnique();

            modelBuilder.Entity<Naprawa>()
                .HasOne(n => n.MechanikProwadzacy)
                .WithMany(m => m.ProwadzoneNaprawy)
                .HasForeignKey(n => n.MechanikProwadzacyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
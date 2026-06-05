using Microsoft.EntityFrameworkCore;
using warsztat3000.Models;

namespace warsztat3000.Data
{
    /// <summary>
    /// Główny kontekst Entity Framework Core dla lokalnej bazy danych warsztatu.
    /// </summary>
    /// <remarks>
    /// Kontekst korzysta z pliku SQLite <c>warsztat3000.db</c>, dzięki czemu aplikacja
    /// może działać bez osobnego serwera bazy danych. Definiuje również najważniejsze
    /// ograniczenia domenowe, np. unikalność numeru VIN i relację naprawy z mechanikiem.
    /// </remarks>
    /// <seealso cref="DatabaseSeeder"/>
    public class WarsztatDbContext : DbContext
    {
        public DbSet<Mechanik> Mechanicy { get; set; }
        public DbSet<Klient> Klienci { get; set; }
        public DbSet<Pojazd> Pojazdy { get; set; }
        public DbSet<Naprawa> Naprawy { get; set; }
        public DbSet<ZadanieNaprawy> ZadaniaNaprawy { get; set; }
        public DbSet<KosztorysPozycja> KosztorysPozycje { get; set; }
        public DbSet<MarkaPojazdu> MarkiPojazdow { get; set; }
        public DbSet<ModelPojazdu> ModelePojazdow { get; set; }

        /// <summary>
        /// Ustawia lokalne połączenie SQLite używane przez całą aplikację.
        /// </summary>
        /// <param name="options">Builder konfiguracji dostawcy bazy danych.</param>
        /// <remarks>
        /// Connection string jest celowo prosty i lokalny, ponieważ projekt ma działać
        /// jako aplikacja desktopowa uruchamiana na jednym komputerze.
        /// </remarks>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=warsztat3000.db");
        }

        /// <summary>
        /// Konfiguruje relacje i indeksy, których nie da się bezpiecznie wywnioskować
        /// wyłącznie z nazw właściwości modelu.
        /// </summary>
        /// <param name="modelBuilder">Builder modelu EF Core używany podczas tworzenia schematu.</param>
        /// <remarks>
        /// Relacja <see cref="Naprawa.MechanikProwadzacy"/> ma ograniczone usuwanie,
        /// aby historyczne naprawy nie znikały po dezaktywacji lub usunięciu mechanika.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pojazd>()
                .HasIndex(p => p.VIN)
                .IsUnique();

            modelBuilder.Entity<MarkaPojazdu>()
                .HasIndex(m => m.Nazwa)
                .IsUnique();

            modelBuilder.Entity<ModelPojazdu>()
                .HasIndex(m => new { m.MarkaPojazduId, m.Nazwa })
                .IsUnique();

            modelBuilder.Entity<ModelPojazdu>()
                .HasOne(m => m.MarkaPojazdu)
                .WithMany(m => m.Modele)
                .HasForeignKey(m => m.MarkaPojazduId);

            modelBuilder.Entity<Naprawa>()
                .HasOne(n => n.MechanikProwadzacy)
                .WithMany(m => m.ProwadzoneNaprawy)
                .HasForeignKey(n => n.MechanikProwadzacyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
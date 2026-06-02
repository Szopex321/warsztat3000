using System.Collections.Generic;
using System.Linq;

namespace warsztat3000.Models
{
    public record StrefaPojazduOpcja(string Kod, string Nazwa);

    public static class StrefyPojazdu
    {
        public const string LewyPrzod = "LewyPrzod";
        public const string LewySrodek = "LewySrodek";
        public const string LewyTyl = "LewyTyl";
        public const string PrawyPrzod = "PrawyPrzod";
        public const string PrawySrodek = "PrawySrodek";
        public const string PrawyTyl = "PrawyTyl";
        public const string Silnik = "Silnik";

        public static IReadOnlyList<StrefaPojazduOpcja> Wszystkie { get; } =
        [
            new(LewyPrzod, "Lewy przód"),
            new(LewySrodek, "Lewa strona"),
            new(LewyTyl, "Lewy tył"),
            new(PrawyPrzod, "Prawy przód"),
            new(PrawySrodek, "Prawa strona"),
            new(PrawyTyl, "Prawy tył"),
            new(Silnik, "Silnik")
        ];

        public static string Etykieta(string? kod)
        {
            return Wszystkie.FirstOrDefault(s => s.Kod == kod)?.Nazwa ?? "Pozostałe";
        }
    }
}

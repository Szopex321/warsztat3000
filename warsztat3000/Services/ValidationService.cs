using System;
using System.Linq;

namespace warsztat3000.Services
{
    /// <summary>
    /// Centralizuje proste reguły walidacji formularzy używanych przy obsłudze warsztatu.
    /// </summary>
    /// <remarks>
    /// Walidacja jest celowo lekka i lokalna. Jej zadaniem jest wychwycenie typowych literówek
    /// przed zapisem do SQLite, a nie pełna zgodność z normami poczty, telefonii czy VIN.
    /// </remarks>
    public static class ValidationService
    {
        /// <summary>
        /// Sprawdza podstawową poprawność adresu email.
        /// </summary>
        /// <param name="email">Adres podany w formularzu klienta.</param>
        /// <returns>
        /// <c>true</c>, gdy pole jest puste albo zawiera znak <c>@</c> oraz kropkę po tym znaku.
        /// </returns>
        /// <remarks>
        /// Puste pole jest akceptowane, ponieważ email nie jest wymagany do przyjęcia pojazdu.
        /// </remarks>
        public static bool EmailPoprawny(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var value = email.Trim();
            return value.Contains('@') && value.Contains('.') && value.IndexOf('@') < value.LastIndexOf('.');
        }

        /// <summary>
        /// Weryfikuje, czy numer telefonu zawiera tylko znaki akceptowane w formularzu.
        /// </summary>
        /// <param name="telefon">Numer telefonu klienta lub mechanika.</param>
        /// <returns>
        /// <c>true</c>, gdy numer jest pusty albo składa się z cyfr, spacji, myślników i znaku plus.
        /// </returns>
        public static bool TelefonPoprawny(string? telefon)
        {
            if (string.IsNullOrWhiteSpace(telefon))
                return true;

            return telefon.All(c => char.IsDigit(c) || c == ' ' || c == '-' || c == '+');
        }

        /// <summary>
        /// Sprawdza, czy VIN ma długość wymaganą dla numeru identyfikacyjnego pojazdu.
        /// </summary>
        /// <param name="vin">Numer VIN wpisany podczas dodawania pojazdu.</param>
        /// <returns>
        /// <c>true</c>, gdy VIN ma dokładnie 17 znaków i składa się z liter lub cyfr.
        /// </returns>
        /// <remarks>
        /// VIN jest wymagany i unikalny w bazie, ponieważ służy do wykrywania już znanych pojazdów.
        /// </remarks>
        public static bool VinPoprawny(string? vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
                return false;

            var value = vin.Trim();
            return value.Length == 17 && value.All(c => char.IsLetterOrDigit(c));
        }

        /// <summary>
        /// Sprawdza, czy rok produkcji mieści się w realistycznym zakresie dla obsługiwanych aut.
        /// </summary>
        /// <param name="rok">Rok produkcji wpisany w formularzu pojazdu.</param>
        /// <returns>
        /// <c>true</c>, gdy rok jest nie mniejszy niż 1950 i nie większy niż następny rok kalendarzowy.
        /// </returns>
        public static bool RokPoprawny(int rok)
        {
            return rok >= 1950 && rok <= DateTime.Now.Year + 1;
        }
    }
}

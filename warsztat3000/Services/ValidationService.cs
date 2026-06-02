using System;
using System.Linq;

namespace warsztat3000.Services
{
    public static class ValidationService
    {
        public static bool EmailPoprawny(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var value = email.Trim();
            return value.Contains('@') && value.Contains('.') && value.IndexOf('@') < value.LastIndexOf('.');
        }

        public static bool TelefonPoprawny(string? telefon)
        {
            if (string.IsNullOrWhiteSpace(telefon))
                return true;

            return telefon.All(c => char.IsDigit(c) || c == ' ' || c == '-' || c == '+');
        }

        public static bool VinPoprawny(string? vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
                return false;

            var value = vin.Trim();
            return value.Length == 17 && value.All(c => char.IsLetterOrDigit(c));
        }

        public static bool RokPoprawny(int rok)
        {
            return rok >= 1950 && rok <= DateTime.Now.Year + 1;
        }
    }
}

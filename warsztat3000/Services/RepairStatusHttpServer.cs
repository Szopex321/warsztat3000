using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;

namespace warsztat3000.Services
{
    /// <summary>
    /// Udostępnia lokalną stronę HTML ze statusem naprawy, używaną przez kody QR.
    /// </summary>
    /// <remarks>
    /// Serwer działa tylko lokalnie na komputerze z uruchomioną aplikacją. Nie jest to publiczne API,
    /// lecz prosty mechanizm demonstracyjny pozwalający klientowi zobaczyć postęp naprawy po otwarciu
    /// linku z kodu QR.
    /// </remarks>
    /// <seealso cref="CreateStatusUrl(int, string)"/>
    public static class RepairStatusHttpServer
    {
        private const int DefaultPort = 5055;
        private const int LastFallbackPort = 5065;
        private static HttpListener? _listener;
        private static CancellationTokenSource? _cancellation;
        private static int _currentPort = DefaultPort;

        /// <summary>
        /// Tworzy link statusu oparty wyłącznie na tokenie naprawy.
        /// </summary>
        /// <param name="token">Token zapisany przy naprawie i używany do odszukania rekordu.</param>
        /// <returns>Adres lokalnej strony statusu naprawy.</returns>
        /// <remarks>
        /// Przed zwróceniem adresu metoda upewnia się, że lokalny serwer HTTP jest uruchomiony.
        /// </remarks>
        public static string CreateStatusUrl(string token)
        {
            EnsureStarted();
            return $"http://localhost:{_currentPort}/status/{Uri.EscapeDataString(token)}";
        }

        /// <summary>
        /// Tworzy link statusu zawierający identyfikator naprawy oraz token QR.
        /// </summary>
        /// <param name="repairId">Identyfikator naprawy w lokalnej bazie SQLite.</param>
        /// <param name="token">Token zabezpieczający link przed przypadkowym zgadywaniem adresów.</param>
        /// <returns>Adres lokalnej strony statusu dla konkretnej naprawy.</returns>
        /// <example>
        /// <code>
        /// var url = RepairStatusHttpServer.CreateStatusUrl(naprawa.Id, naprawa.QrToken);
        /// </code>
        /// </example>
        public static string CreateStatusUrl(int repairId, string token)
        {
            EnsureStarted();
            return $"http://localhost:{_currentPort}/status/{repairId}/{Uri.EscapeDataString(token)}";
        }

        /// <summary>
        /// Uruchamia lokalny serwer HTTP na pierwszym wolnym porcie z obsługiwanego zakresu.
        /// </summary>
        /// <remarks>
        /// Domyślny port to 5055. Jeśli jest zajęty, aplikacja próbuje kolejnych portów do 5065.
        /// Metoda jest idempotentna, więc ponowne wywołanie nie tworzy drugiego serwera.
        /// </remarks>
        public static void Start()
        {
            if (_listener != null)
                return;

            for (var port = DefaultPort; port <= LastFallbackPort; port++)
            {
                try
                {
                    _cancellation = new CancellationTokenSource();
                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://localhost:{port}/");
                    _listener.Start();
                    _currentPort = port;
                    _ = Task.Run(() => ListenAsync(_cancellation.Token));
                    return;
                }
                catch
                {
                    Stop();
                }
            }
        }

        private static void EnsureStarted()
        {
            if (_listener?.IsListening == true)
                return;

            Start();
        }

        /// <summary>
        /// Zatrzymuje lokalny serwer statusu i zwalnia zajęty port.
        /// </summary>
        /// <remarks>
        /// Metoda jest wywoływana przy zamykaniu aplikacji. Błędy zamykania są ignorowane,
        /// ponieważ nie powinny blokować wyjścia z programu desktopowego.
        /// </remarks>
        public static void Stop()
        {
            try
            {
                _cancellation?.Cancel();
                _listener?.Stop();
                _listener?.Close();
            }
            catch
            {
                // Zamykanie serwera nie powinno blokować zamykania aplikacji.
            }
            finally
            {
                _cancellation = null;
                _listener = null;
                _currentPort = DefaultPort;
            }
        }

        private static async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listener?.IsListening == true)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context), cancellationToken);
                }
                catch
                {
                    if (!cancellationToken.IsCancellationRequested)
                        Stop();
                }
            }
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var route = ExtractRoute(context.Request.Url);
            var html = string.IsNullOrWhiteSpace(route.Token)
                ? RenderNotFoundPage()
                : RenderStatusPage(route.Token, route.RepairId);

            var bytes = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }

        private static (int? RepairId, string? Token) ExtractRoute(Uri? url)
        {
            var parts = url?.AbsolutePath
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts == null || parts.Length < 2 || !parts[0].Equals("status", StringComparison.OrdinalIgnoreCase))
                return (null, null);

            if (parts.Length >= 3 && int.TryParse(parts[1], out var repairId))
                return (repairId, Uri.UnescapeDataString(parts[2]).Trim());

            return (null, Uri.UnescapeDataString(parts[1]).Trim());
        }

        private static string RenderStatusPage(string token, int? repairId)
        {
            using var db = new WarsztatDbContext();
            var naprawa = db.Naprawy
                .AsNoTracking()
                .Include(n => n.Pojazd)
                    .ThenInclude(p => p.Klient)
                .Include(n => n.MechanikProwadzacy)
                .Include(n => n.ZadaniaNaprawy)
                .FirstOrDefault(n => (repairId != null && n.Id == repairId.Value) || n.QrToken == token);

            if (naprawa == null)
                return RenderNotFoundPage();

            var pojazd = naprawa.Pojazd;
            var klient = pojazd?.Klient;
            var mechanik = naprawa.MechanikProwadzacy;
            var zadania = naprawa.ZadaniaNaprawy.Any()
                ? string.Join("", naprawa.ZadaniaNaprawy.OrderBy(z => z.Id).Select(z =>
                    $"<li>{Encode(z.NazwaZadania)} - {(z.CzyWykonane ? "wykonane" : "do zrobienia")}</li>"))
                : "<li>Brak wpisanych zadań.</li>";

            return WrapPage($@"
                <section class=""card"">
                    <p class=""label"">Status naprawy</p>
                    <h1>{Encode(naprawa.Status)}</h1>
                    <div class=""progress""><div style=""width:{Math.Clamp(naprawa.ProcentUkonczenia, 0, 100)}%""></div></div>
                    <p>{naprawa.ProcentUkonczenia}% ukończenia</p>
                    <p class=""hint"">Strona odświeża status automatycznie co 15 sekund.</p>
                </section>

                <section class=""card"">
                    <p class=""label"">Pojazd</p>
                    <h2>{Encode(pojazd?.NrRejestracyjny)} - {Encode(pojazd?.Marka)} {Encode(pojazd?.Model)}</h2>
                    <p>VIN: {Encode(pojazd?.VIN)}</p>
                    <p>Właściciel: {Encode(klient?.Imie)} {Encode(klient?.Nazwisko)}</p>
                </section>

                <section class=""card"">
                    <p class=""label"">Naprawa</p>
                    <p>Mechanik: {Encode(mechanik?.Imie)} {Encode(mechanik?.Nazwisko)}</p>
                    <p>Przyjęto: {naprawa.DataRozpoczecia:dd.MM.yyyy}</p>
                    <p>Planowany koniec: {naprawa.PrzewidywanyKoniec:dd.MM.yyyy}</p>
                    <h3>Zadania</h3>
                    <ul>{zadania}</ul>
                </section>");
        }

        private static string RenderNotFoundPage()
        {
            return WrapPage(@"<section class=""card""><h1>Nie znaleziono naprawy</h1><p>Sprawdź link albo wygeneruj kod QR ponownie w aplikacji.</p></section>");
        }

        private static string WrapPage(string body)
        {
            return $@"<!doctype html>
<html lang=""pl"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <meta http-equiv=""refresh"" content=""15"">
    <title>Status naprawy</title>
    <style>
        body {{ margin: 0; font-family: Arial, sans-serif; background: #0B0A10; color: white; }}
        main {{ max-width: 760px; margin: 0 auto; padding: 24px; }}
        .card {{ background: #15141E; border: 1px solid #2A2A35; border-radius: 12px; padding: 18px; margin-bottom: 14px; }}
        .label {{ color: #8B8B99; font-size: 12px; text-transform: uppercase; margin: 0 0 6px; }}
        .hint {{ color: #8B8B99; font-size: 13px; }}
        h1, h2, h3 {{ margin-top: 0; color: #00FF00; }}
        .progress {{ height: 12px; background: #2A2A35; border-radius: 999px; overflow: hidden; }}
        .progress div {{ height: 100%; background: #00FF00; }}
        li {{ margin: 6px 0; }}
    </style>
</head>
<body>
    <main>{body}</main>
</body>
</html>";
        }

        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "Brak danych" : value);
        }
    }
}

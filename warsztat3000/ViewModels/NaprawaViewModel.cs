using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using warsztat3000.Data;
using warsztat3000.Models;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using warsztat3000.Services;
using warsztat3000.Views;

namespace warsztat3000.ViewModels
{
    public partial class NaprawaViewModel : ViewModelBase
    {
        private const decimal StawkaRoboczogodziny = 250m;
        public Action? DaneZmienione { get; set; }
        public Action<int>? NaprawaZakonczona { get; set; }

        private Naprawa _aktywnaNaprawa;
        public Naprawa AktywnaNaprawa
        {
            get => _aktywnaNaprawa;
            set
            {
                _aktywnaNaprawa = value;
                _planowanyKoniecText = value?.PrzewidywanyKoniec?.ToString("dd.MM.yyyy") ?? string.Empty;
                OnPropertyChanged(nameof(AktywnaNaprawa));
                OnPropertyChanged(nameof(InicjalyKlienta));
                OnPropertyChanged(nameof(ImieNazwiskoKlienta));
                OnPropertyChanged(nameof(ImieNazwiskoMechanika));
                OnPropertyChanged(nameof(CzyNaprawaRozpoczeta));
                OnPropertyChanged(nameof(CzyNaprawaZakonczona));
                OnPropertyChanged(nameof(CzyJestAktywnaNaprawa));
                OnPropertyChanged(nameof(CzasNaprawyOpis));
                OnPropertyChanged(nameof(RoboczogodzinyOpis));
                OnPropertyChanged(nameof(PlanowanyKoniecText));
            }
        }

        public string InicjalyKlienta
        {
            get
            {
                var klient = AktywnaNaprawa?.Pojazd?.Klient;
                if (klient == null)
                    return "??";

                return $"{PierwszaLiteraLubZnak(klient.Imie)}{PierwszaLiteraLubZnak(klient.Nazwisko)}";
            }
        }

        public string ImieNazwiskoKlienta
        {
            get
            {
                var klient = AktywnaNaprawa?.Pojazd?.Klient;
                if (klient == null)
                    return "Brak danych";

                return PolaczNazwe(klient.Imie, klient.Nazwisko);
            }
        }

        public string ImieNazwiskoMechanika => AktywnaNaprawa?.MechanikProwadzacy != null
            ? PolaczNazwe(AktywnaNaprawa.MechanikProwadzacy.Imie, AktywnaNaprawa.MechanikProwadzacy.Nazwisko)
            : "Brak mechanika";

        public bool CzyNaprawaRozpoczeta => AktywnaNaprawa?.CzyRozpoczeta == true;
        public bool CzyNaprawaZakonczona => AktywnaNaprawa?.CzyZakonczona == true;
        public bool CzyJestAktywnaNaprawa => AktywnaNaprawa?.Id > 0;

        public string CzasNaprawyOpis
        {
            get
            {
                if (AktywnaNaprawa?.FaktycznyStart == null)
                    return "Nie rozpoczęto";

                var koniec = AktywnaNaprawa.FaktycznyKoniec ?? DateTime.Now;
                var czas = koniec - AktywnaNaprawa.FaktycznyStart.Value;
                return $"{Math.Max(0, czas.TotalHours):F1} h";
            }
        }

        public string RoboczogodzinyOpis => $"{AktywnaNaprawa?.Roboczogodziny ?? 0} h x {StawkaRoboczogodziny:F2} zł";

        private string _planowanyKoniecText = string.Empty;
        public string PlanowanyKoniecText
        {
            get => _planowanyKoniecText;
            set => SetProperty(ref _planowanyKoniecText, value);
        }

        private int _procentUkonczenia;
        public int ProcentUkonczenia
        {
            get => _procentUkonczenia;
            set { if (_procentUkonczenia != value) { _procentUkonczenia = value; OnPropertyChanged(nameof(ProcentUkonczenia)); } }
        }

        public ObservableCollection<ZadanieViewModel> ListaZadan { get; set; }

        public ObservableCollection<KosztorysPozycja> ListaCzesci { get; set; }
        public ObservableCollection<StrefaPojazduOpcja> DostepneStrefy { get; set; }

        public StrefaUszkodzenViewModel LewyPrzodStrefa { get; } = new(StrefyPojazdu.LewyPrzod);
        public StrefaUszkodzenViewModel LewySrodekStrefa { get; } = new(StrefyPojazdu.LewySrodek);
        public StrefaUszkodzenViewModel LewyTylStrefa { get; } = new(StrefyPojazdu.LewyTyl);
        public StrefaUszkodzenViewModel PrawyPrzodStrefa { get; } = new(StrefyPojazdu.PrawyPrzod);
        public StrefaUszkodzenViewModel PrawySrodekStrefa { get; } = new(StrefyPojazdu.PrawySrodek);
        public StrefaUszkodzenViewModel PrawyTylStrefa { get; } = new(StrefyPojazdu.PrawyTyl);
        public StrefaUszkodzenViewModel SilnikStrefa { get; } = new(StrefyPojazdu.Silnik);

        private string _noweZadanieNazwa = string.Empty;
        public string NoweZadanieNazwa
        {
            get => _noweZadanieNazwa;
            set => SetProperty(ref _noweZadanieNazwa, value);
        }

        private StrefaPojazduOpcja _wybranaStrefaDlaNowegoZadania;
        public StrefaPojazduOpcja WybranaStrefaDlaNowegoZadania
        {
            get => _wybranaStrefaDlaNowegoZadania;
            set => SetProperty(ref _wybranaStrefaDlaNowegoZadania, value);
        }

        private string _tekstWyszukiwarki;
        public string TekstWyszukiwarki
        {
            get => _tekstWyszukiwarki;
            set { _tekstWyszukiwarki = value; OnPropertyChanged(nameof(TekstWyszukiwarki)); }
        }

        private decimal _razemNetto;
        public decimal RazemNetto
        {
            get => _razemNetto;
            set { _razemNetto = value; OnPropertyChanged(nameof(RazemNetto)); }
        }

        private decimal _razemVat;
        public decimal RazemVat
        {
            get => _razemVat;
            set { _razemVat = value; OnPropertyChanged(nameof(RazemVat)); }
        }

        private decimal _razemBrutto;
        public decimal RazemBrutto
        {
            get => _razemBrutto;
            set { _razemBrutto = value; OnPropertyChanged(nameof(RazemBrutto)); }
        }

        public NaprawaViewModel()
        {
            ListaZadan = new ObservableCollection<ZadanieViewModel>();
            ListaCzesci = new ObservableCollection<KosztorysPozycja>();
            DostepneStrefy = new ObservableCollection<StrefaPojazduOpcja>(StrefyPojazdu.Wszystkie);
            _wybranaStrefaDlaNowegoZadania = DostepneStrefy.First(s => s.Kod == StrefyPojazdu.Silnik);
            WczytajDaneZBazy();
        }

        private void WczytajDaneZBazy()
        {
            ListaZadan.Clear();
            ListaCzesci.Clear();

            using (var db = new WarsztatDbContext())
            {
                db.Database.EnsureCreated();
                DatabaseSeeder.UpewnijSieZeSchematAktualny(db);

                var naprawaZBazy = db.Naprawy
                    .Include(n => n.Pojazd)
                        .ThenInclude(p => p.Klient)
                    .Include(n => n.MechanikProwadzacy)
                    .Include(n => n.ZadaniaNaprawy)
                    .Include(n => n.KosztorysPozycje)
                    .Where(n => !n.CzyZakonczona && (n.Status == null || n.Status != "ZAKOŃCZONE"))
                    .OrderByDescending(n => n.DataRozpoczecia)
                    .FirstOrDefault();

                if (naprawaZBazy != null)
                {
                    AktywnaNaprawa = naprawaZBazy;

                    foreach (var zadanie in naprawaZBazy.ZadaniaNaprawy)
                    {
                        DodajZadanie(zadanie.Id, zadanie.NazwaZadania, zadanie.CzyWykonane, zadanie.StrefaPojazdu);
                    }

                    foreach (var czesc in naprawaZBazy.KosztorysPozycje)
                    {
                        ListaCzesci.Add(czesc);
                    }
                    PrzeliczKosztorys();
                }
                else
                {
                    UstawPustyStan();
                }
            }

            PrzeliczProgres();
            OdswiezStrefy();
        }

        private void PrzeliczKosztorys()
        {
            decimal brutto = 0;
            decimal vatTotal = 0;
            
            foreach (var pozycja in ListaCzesci)
            {
                decimal sumaBruttoPozycji = pozycja.Ilosc * pozycja.CenaJednostkowaBrutto;
                brutto += sumaBruttoPozycji;
                
                decimal netto = sumaBruttoPozycji / (1m + (pozycja.Vat / 100m));
                vatTotal += sumaBruttoPozycji - netto;
            }

            RazemBrutto = brutto;
            RazemVat = vatTotal;
            RazemNetto = brutto - vatTotal;
        }

        [RelayCommand]
        public async Task NowyPojazd()
        {
            var dialog = new NowyPojazdDialog();
            var zapisanoPojazd = await AppDialogService.ShowDialogAsync(dialog);
            if (!zapisanoPojazd || dialog.ZapisanyPojazdId == null)
                return;

            using var db = new WarsztatDbContext();
            var pojazd = db.Pojazdy
                .Include(p => p.Klient)
                .FirstOrDefault(p => p.Id == dialog.ZapisanyPojazdId.Value);

            if (pojazd == null)
                return;

            ZaladujPojazd(pojazd);

            var zapisanoNaprawe = await AppDialogService.ShowDialogAsync(new DodajNapraweDialog(pojazd));
            if (zapisanoNaprawe)
                ZaladujPojazd(pojazd);

            DaneZmienione?.Invoke();
        }

        [RelayCommand]
        public async Task DodajNaprawe()
        {
            var pojazd = AktywnaNaprawa?.Pojazd;
            if (pojazd == null || pojazd.Id == 0)
            {
                await AppDialogService.ShowMessageAsync("Brak pojazdu", "Najpierw wybierz pojazd z bazy albo dodaj nowy pojazd.");
                return;
            }

            var zapisano = await AppDialogService.ShowDialogAsync(new DodajNapraweDialog(pojazd));
            if (!zapisano)
                return;

            ZaladujPojazd(pojazd);
            DaneZmienione?.Invoke();
        }

        [RelayCommand]
        public async Task WydrukujKosztorys()
        {
            if (AktywnaNaprawa == null)
                return;

            await AppDialogService.ShowWindowAsync(new WydrukujKosztorysDialog(AktywnaNaprawa, ListaCzesci));
        }

        [RelayCommand]
        public async Task PokazKodQr()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
            {
                await AppDialogService.ShowMessageAsync("Brak naprawy", "Kod QR jest dostępny po rozpoczęciu naprawy.");
                return;
            }

            using (var db = new WarsztatDbContext())
            {
                var naprawa = db.Naprawy.FirstOrDefault(n => n.Id == AktywnaNaprawa.Id);
                if (naprawa == null)
                    return;

                if (string.IsNullOrWhiteSpace(naprawa.QrToken))
                {
                    naprawa.QrToken = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
                    db.SaveChanges();
                }

                AktywnaNaprawa.QrToken = naprawa.QrToken;
            }

            var statusUrl = RepairStatusHttpServer.CreateStatusUrl(AktywnaNaprawa.Id, AktywnaNaprawa.QrToken);
            await AppDialogService.ShowWindowAsync(new PokazKodQrDialog(AktywnaNaprawa.QrToken, statusUrl));
        }

        [RelayCommand]
        public async Task SzukajPojazdu()
        {
            var tekst = (TekstWyszukiwarki ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(tekst))
                return;

            var filtr = tekst.ToLower();
            using (var db = new WarsztatDbContext())
            {
                var pojazd = db.Pojazdy
                    .Include(p => p.Klient)
                    .FirstOrDefault(p =>
                        (p.NrRejestracyjny ?? string.Empty).ToLower().Contains(filtr) ||
                        (p.VIN ?? string.Empty).ToLower().Contains(filtr) ||
                        (p.Marka ?? string.Empty).ToLower().Contains(filtr) ||
                        (p.Model ?? string.Empty).ToLower().Contains(filtr) ||
                        (p.Klient != null && (
                            (p.Klient.Imie ?? string.Empty).ToLower().Contains(filtr) ||
                            (p.Klient.Nazwisko ?? string.Empty).ToLower().Contains(filtr))));

                if (pojazd == null)
                {
                    await AppDialogService.ShowMessageAsync("Nie znaleziono", "Nie znaleziono pojazdu pasującego do wyszukiwania.");
                    return;
                }

                ZaladujPojazd(pojazd);
            }
        }

        [RelayCommand]
        public async Task EdytujKosztorys()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
            {
                await AppDialogService.ShowMessageAsync("Brak naprawy", "Najpierw dodaj naprawę dla wybranego pojazdu.");
                return;
            }

            await AppDialogService.ShowDialogAsync(new EdytujKosztorysDialog(AktywnaNaprawa.Id));
            ZaladujPojazd(AktywnaNaprawa.Pojazd);
            DaneZmienione?.Invoke();
        }

        [RelayCommand]
        public async Task RozpocznijNaprawe()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
            {
                await AppDialogService.ShowMessageAsync("Brak naprawy", "Najpierw dodaj naprawę dla wybranego pojazdu.");
                return;
            }

            if (AktywnaNaprawa.CzyRozpoczeta)
                return;

            using (var db = new WarsztatDbContext())
            {
                var naprawa = db.Naprawy.FirstOrDefault(n => n.Id == AktywnaNaprawa.Id);
                if (naprawa == null)
                    return;

                naprawa.CzyRozpoczeta = true;
                naprawa.FaktycznyStart = DateTime.Now;
                naprawa.Status = "W NAPRAWIE";
                db.SaveChanges();

                AktywnaNaprawa.CzyRozpoczeta = naprawa.CzyRozpoczeta;
                AktywnaNaprawa.FaktycznyStart = naprawa.FaktycznyStart;
                AktywnaNaprawa.Status = naprawa.Status;
            }

            OdswiezStatusNaprawy();
            DaneZmienione?.Invoke();
        }

        [RelayCommand]
        public async Task ZakonczNaprawe()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
            {
                await AppDialogService.ShowMessageAsync("Brak naprawy", "Nie ma aktywnej naprawy do zakończenia.");
                return;
            }

            if (!AktywnaNaprawa.CzyRozpoczeta)
            {
                await AppDialogService.ShowMessageAsync("Naprawa nie rozpoczęta", "Najpierw zaznacz rozpoczęcie naprawy.");
                return;
            }

            if (AktywnaNaprawa.CzyZakonczona)
                return;

            var potwierdzono = await AppDialogService.ShowConfirmationAsync(
                "Zakończyć naprawę?",
                "Czy na pewno chcesz zakończyć naprawę? Po potwierdzeniu zostanie wyliczony czas pracy i robocizna w kosztorysie.");

            if (!potwierdzono)
                return;

            var zakonczonyPojazd = AktywnaNaprawa.Pojazd;
            var zakonczonyPojazdId = AktywnaNaprawa.PojazdId != 0
                ? AktywnaNaprawa.PojazdId
                : AktywnaNaprawa.Pojazd?.Id ?? 0;

            using (var db = new WarsztatDbContext())
            {
                var naprawa = db.Naprawy.FirstOrDefault(n => n.Id == AktywnaNaprawa.Id);
                if (naprawa == null)
                    return;

                naprawa.CzyZakonczona = true;
                naprawa.FaktycznyKoniec = DateTime.Now;
                naprawa.RzeczywistyKoniec = naprawa.FaktycznyKoniec;
                naprawa.Status = "ZAKOŃCZONE";
                naprawa.ProcentUkonczenia = 100;
                naprawa.Roboczogodziny = WyliczRoboczogodziny(naprawa);
                db.SaveChanges();

                AktywnaNaprawa.CzyZakonczona = naprawa.CzyZakonczona;
                AktywnaNaprawa.FaktycznyKoniec = naprawa.FaktycznyKoniec;
                AktywnaNaprawa.RzeczywistyKoniec = naprawa.RzeczywistyKoniec;
                AktywnaNaprawa.Status = naprawa.Status;
                AktywnaNaprawa.ProcentUkonczenia = naprawa.ProcentUkonczenia;
                AktywnaNaprawa.Roboczogodziny = naprawa.Roboczogodziny;
                ProcentUkonczenia = 100;
            }

            AktualizujRobocizne();
            UstawPustyStan(zakonczonyPojazd);
            DaneZmienione?.Invoke();
            if (zakonczonyPojazdId != 0)
                NaprawaZakonczona?.Invoke(zakonczonyPojazdId);
        }

        [RelayCommand]
        public async Task DodajZadanieDoNaprawy()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
            {
                await AppDialogService.ShowMessageAsync("Brak naprawy", "Najpierw dodaj naprawę dla wybranego pojazdu.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NoweZadanieNazwa))
                return;

            var zadanie = new ZadanieNaprawy
            {
                NaprawaId = AktywnaNaprawa.Id,
                NazwaZadania = NoweZadanieNazwa.Trim(),
                StrefaPojazdu = WybranaStrefaDlaNowegoZadania?.Kod ?? StrefyPojazdu.Silnik,
                CzyWykonane = false
            };

            using (var db = new WarsztatDbContext())
            {
                db.ZadaniaNaprawy.Add(zadanie);
                db.SaveChanges();
            }

            DodajZadanie(zadanie.Id, zadanie.NazwaZadania, zadanie.CzyWykonane, zadanie.StrefaPojazdu);
            NoweZadanieNazwa = string.Empty;
            PrzeliczProgres();
            ZapiszProgres();
            OdswiezStrefy();
            DaneZmienione?.Invoke();
        }

        [RelayCommand]
        public async Task ZapiszPlanowanyKoniec()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
            {
                await AppDialogService.ShowMessageAsync("Brak naprawy", "Najpierw dodaj naprawę dla wybranego pojazdu.");
                return;
            }

            if (!DateTime.TryParse(PlanowanyKoniecText, out var planowanyKoniec))
            {
                await AppDialogService.ShowMessageAsync("Niepoprawna data", "Podaj planowany koniec w formacie dd.mm.rrrr, np. 24.03.2026.");
                return;
            }

            using (var db = new WarsztatDbContext())
            {
                var naprawa = db.Naprawy.FirstOrDefault(n => n.Id == AktywnaNaprawa.Id);
                if (naprawa == null)
                    return;

                naprawa.PrzewidywanyKoniec = planowanyKoniec.Date;
                db.SaveChanges();
            }

            AktywnaNaprawa.PrzewidywanyKoniec = planowanyKoniec.Date;
            _planowanyKoniecText = planowanyKoniec.ToString("dd.MM.yyyy");
            OnPropertyChanged(nameof(PlanowanyKoniecText));
            DaneZmienione?.Invoke();
        }

        [RelayCommand]
        public void UsunZadanie(ZadanieViewModel zadanie)
        {
            if (zadanie == null)
                return;

            using (var db = new WarsztatDbContext())
            {
                var doUsuniecia = db.ZadaniaNaprawy.FirstOrDefault(z => z.Id == zadanie.Id);
                if (doUsuniecia != null)
                {
                    db.ZadaniaNaprawy.Remove(doUsuniecia);
                    db.SaveChanges();
                }
            }

            ListaZadan.Remove(zadanie);
            PrzeliczProgres();
            ZapiszProgres();
            OdswiezStrefy();
            DaneZmienione?.Invoke();
        }

        private void DodajZadanie(int id, string nazwa, bool czyWykonane, string strefaPojazdu)
        {
            var zadanie = new ZadanieViewModel(id, nazwa, czyWykonane, strefaPojazdu);
            zadanie.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ZadanieViewModel.CzyWykonane))
                {
                    if (sender is ZadanieViewModel zmienioneZadanie)
                        ZapiszStatusZadania(zmienioneZadanie);

                    PrzeliczProgres();
                    ZapiszProgres();
                    OdswiezStrefy();
                }
            };
            ListaZadan.Add(zadanie);
        }

        private void DodajZadanie(string nazwa, bool czyWykonane)
        {
            DodajZadanie(0, nazwa, czyWykonane, StrefyPojazdu.Silnik);
        }

        private void PrzeliczProgres()
        {
            if (ListaZadan.Count == 0)
            {
                ProcentUkonczenia = 0;
                if (AktywnaNaprawa != null)
                    AktywnaNaprawa.ProcentUkonczenia = 0;
                return;
            }

            double wykonane = ListaZadan.Count(z => z.CzyWykonane);
            ProcentUkonczenia = (int)Math.Round((wykonane / ListaZadan.Count) * 100);
            if (AktywnaNaprawa != null)
                AktywnaNaprawa.ProcentUkonczenia = ProcentUkonczenia;
        }

        private void ZapiszStatusZadania(ZadanieViewModel zadanie)
        {
            if (zadanie.Id == 0)
                return;

            using (var db = new WarsztatDbContext())
            {
                var zadanieZBazy = db.ZadaniaNaprawy.FirstOrDefault(z => z.Id == zadanie.Id);
                if (zadanieZBazy == null)
                    return;

                zadanieZBazy.CzyWykonane = zadanie.CzyWykonane;
                db.SaveChanges();
            }
        }

        private void ZapiszProgres()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
                return;

            using (var db = new WarsztatDbContext())
            {
                var naprawa = db.Naprawy.FirstOrDefault(n => n.Id == AktywnaNaprawa.Id);
                if (naprawa == null)
                    return;

                naprawa.ProcentUkonczenia = ProcentUkonczenia;
                db.SaveChanges();
            }

            DaneZmienione?.Invoke();
        }

        private static decimal WyliczRoboczogodziny(Naprawa naprawa)
        {
            if (naprawa.FaktycznyStart == null || naprawa.FaktycznyKoniec == null)
                return 0;

            var godziny = (decimal)(naprawa.FaktycznyKoniec.Value - naprawa.FaktycznyStart.Value).TotalHours;
            return Math.Max(1, Math.Ceiling(godziny));
        }

        private void AktualizujRobocizne()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0 || AktywnaNaprawa.Roboczogodziny <= 0)
                return;

            using (var db = new WarsztatDbContext())
            {
                var robocizna = db.KosztorysPozycje
                    .FirstOrDefault(p => p.NaprawaId == AktywnaNaprawa.Id && p.TypPozycji == TypyKosztorysu.Robocizna);

                if (robocizna == null)
                {
                    robocizna = new KosztorysPozycja
                    {
                        NaprawaId = AktywnaNaprawa.Id,
                        TypPozycji = TypyKosztorysu.Robocizna,
                        Vat = 23
                    };
                    db.KosztorysPozycje.Add(robocizna);
                }

                robocizna.NazwaPozycji = $"Robocizna - {AktywnaNaprawa.Roboczogodziny} h";
                robocizna.Ilosc = AktywnaNaprawa.Roboczogodziny;
                robocizna.CenaJednostkowaBrutto = StawkaRoboczogodziny;
                db.SaveChanges();
            }

            OdswiezKosztorys();
        }

        private void OdswiezKosztorys()
        {
            if (AktywnaNaprawa == null || AktywnaNaprawa.Id == 0)
                return;

            ListaCzesci.Clear();
            using (var db = new WarsztatDbContext())
            {
                var pozycje = db.KosztorysPozycje
                    .Where(p => p.NaprawaId == AktywnaNaprawa.Id)
                    .OrderBy(p => p.TypPozycji == TypyKosztorysu.Robocizna ? 1 : 0)
                    .ThenBy(p => p.Id)
                    .ToList();

                foreach (var pozycja in pozycje)
                    ListaCzesci.Add(pozycja);
            }

            PrzeliczKosztorys();
        }

        private void OdswiezStatusNaprawy()
        {
            OnPropertyChanged(nameof(AktywnaNaprawa));
            OnPropertyChanged(nameof(CzyNaprawaRozpoczeta));
            OnPropertyChanged(nameof(CzyNaprawaZakonczona));
            OnPropertyChanged(nameof(CzyJestAktywnaNaprawa));
            OnPropertyChanged(nameof(CzasNaprawyOpis));
            OnPropertyChanged(nameof(RoboczogodzinyOpis));
            OnPropertyChanged(nameof(PlanowanyKoniecText));
        }

        private void OdswiezStrefy()
        {
            LewyPrzodStrefa.UstawZadania(ListaZadan.Where(z => z.StrefaPojazdu == StrefyPojazdu.LewyPrzod));
            LewySrodekStrefa.UstawZadania(ListaZadan.Where(z => z.StrefaPojazdu == StrefyPojazdu.LewySrodek));
            LewyTylStrefa.UstawZadania(ListaZadan.Where(z => z.StrefaPojazdu == StrefyPojazdu.LewyTyl));
            PrawyPrzodStrefa.UstawZadania(ListaZadan.Where(z => z.StrefaPojazdu == StrefyPojazdu.PrawyPrzod));
            PrawySrodekStrefa.UstawZadania(ListaZadan.Where(z => z.StrefaPojazdu == StrefyPojazdu.PrawySrodek));
            PrawyTylStrefa.UstawZadania(ListaZadan.Where(z => z.StrefaPojazdu == StrefyPojazdu.PrawyTyl));
            SilnikStrefa.UstawZadania(ListaZadan.Where(z => z.StrefaPojazdu == StrefyPojazdu.Silnik));
        }

        private void UstawPustyStan(Pojazd? pojazd = null)
        {
            ListaZadan.Clear();
            ListaCzesci.Clear();
            AktywnaNaprawa = new Naprawa
            {
                Id = 0,
                Pojazd = pojazd,
                Status = "Brak aktywnej naprawy",
                ProcentUkonczenia = 0
            };
            ProcentUkonczenia = 0;
            PrzeliczKosztorys();
            OdswiezStrefy();
        }

        public void ZaladujPojazd(Pojazd wybraneAuto)
        {
            using (var db = new WarsztatDbContext())
            {
                var aktywna = db.Naprawy
                    .Include(n => n.Pojazd).ThenInclude(p => p.Klient)
                    .Include(n => n.MechanikProwadzacy)
                    .Include(n => n.ZadaniaNaprawy)
                    .Include(n => n.KosztorysPozycje)
                    .FirstOrDefault(n => n.PojazdId == wybraneAuto.Id && !n.CzyZakonczona && (n.Status == null || n.Status != "ZAKOŃCZONE"));

                ListaZadan.Clear();
                ListaCzesci.Clear();

                if (aktywna != null)
                {
                    AktywnaNaprawa = aktywna;
                    foreach (var z in aktywna.ZadaniaNaprawy)
                    {
                        DodajZadanie(z.Id, z.NazwaZadania, z.CzyWykonane, z.StrefaPojazdu);
                    }
                    foreach (var c in aktywna.KosztorysPozycje)
                    {
                        ListaCzesci.Add(c);
                    }
                }
                else
                {
                    var pelneAuto = db.Pojazdy.Include(p => p.Klient).First(p => p.Id == wybraneAuto.Id);
                    UstawPustyStan(pelneAuto);
                }

                PrzeliczKosztorys();
                PrzeliczProgres();
                OdswiezStrefy();
            }
        }

        private static string PierwszaLiteraLubZnak(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "?"
                : value.Trim()[0].ToString().ToUpperInvariant();
        }

        private static string PolaczNazwe(string? pierwszy, string? drugi)
        {
            var nazwa = $"{pierwszy} {drugi}".Trim();
            return string.IsNullOrWhiteSpace(nazwa) ? "Brak danych" : nazwa;
        }
    }


}
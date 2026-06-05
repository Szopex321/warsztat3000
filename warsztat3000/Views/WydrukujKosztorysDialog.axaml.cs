using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using warsztat3000.Models;
using warsztat3000.Services;

namespace warsztat3000.Views
{
    public partial class WydrukujKosztorysDialog : Window
    {
        private Naprawa? _naprawa;
        private List<KosztorysPozycja> _pozycje = new();
        private decimal _razem;

        public WydrukujKosztorysDialog()
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public WydrukujKosztorysDialog(Naprawa naprawa, IEnumerable<KosztorysPozycja> pozycje) : this()
        {
            _naprawa = naprawa;
            _pozycje = pozycje.ToList();
            PojazdText.Text = $"Dane pojazdu: {naprawa.Pojazd?.NrRejestracyjny} - {naprawa.Pojazd?.Marka} {naprawa.Pojazd?.Model}";
            WlascicielText.Text = $"Właściciel: {naprawa.Pojazd?.Klient?.Imie} {naprawa.Pojazd?.Klient?.Nazwisko}";

            PozycjePanel.Children.Clear();
            _razem = 0;

            foreach (var pozycja in _pozycje)
            {
                var suma = pozycja.Ilosc * pozycja.CenaJednostkowaBrutto;
                _razem += suma;
                PozycjePanel.Children.Add(new TextBlock
                {
                    Text = $"{pozycja.TypPozycji}: {pozycja.NazwaPozycji} | {pozycja.Ilosc} x {pozycja.CenaJednostkowaBrutto:F2} zł = {suma:F2} zł",
                    Foreground = Avalonia.Media.Brushes.Black,
                    Margin = new Avalonia.Thickness(0, 3)
                });
            }

            if (!_pozycje.Any())
            {
                PozycjePanel.Children.Add(new TextBlock
                {
                    Text = "Brak pozycji kosztorysu.",
                    Foreground = Avalonia.Media.Brushes.Black,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 30)
                });
            }

            RazemText.Text = $"Razem: {_razem:F2} zł";
        }

        private async void ZapiszPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_naprawa == null)
                return;

            var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
            if (storage == null)
                return;

            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Zapisz kosztorys jako PDF",
                SuggestedFileName = $"kosztorys_{SafeFileName(_naprawa.Pojazd?.NrRejestracyjny)}.pdf",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PDF")
                    {
                        Patterns = new[] { "*.pdf" }
                    }
                }
            });

            if (file == null)
                return;

            GeneratePdf(file.Path.LocalPath);
            await AppDialogService.ShowMessageAsync("Zapisano PDF", $"Kosztorys zapisano do pliku:\n{file.Path.LocalPath}");
        }

        private async void Drukuj_Click(object sender, RoutedEventArgs e)
        {
            if (_naprawa == null)
                return;

            var tempPath = Path.Combine(Path.GetTempPath(), $"kosztorys_{Guid.NewGuid():N}.pdf");
            GeneratePdf(tempPath);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    Verb = "print",
                    UseShellExecute = true
                });
                await AppDialogService.ShowMessageAsync("Drukowanie", "Kosztorys został przekazany do drukowania.");
            }
            catch
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
                await AppDialogService.ShowMessageAsync("Drukowanie", "Nie udało się uruchomić drukowania automatycznie. Otworzyłem PDF, użyj opcji drukowania w programie.");
            }
        }

        private void Zamknij_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GeneratePdf(string path)
        {
            if (_naprawa == null)
                return;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().AlignCenter().Text("WARSZTAT 3000").FontSize(24).Bold();
                        column.Item().Text("Zestawienie kosztów naprawy").FontSize(18).Bold();
                        column.Item().Text($"Pojazd: {_naprawa.Pojazd?.NrRejestracyjny} - {_naprawa.Pojazd?.Marka} {_naprawa.Pojazd?.Model}");
                        column.Item().Text($"Właściciel: {_naprawa.Pojazd?.Klient?.Imie} {_naprawa.Pojazd?.Klient?.Nazwisko}");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Typ");
                                header.Cell().Element(HeaderCell).Text("Pozycja");
                                header.Cell().Element(HeaderCell).Text("Ilość");
                                header.Cell().Element(HeaderCell).Text("Cena");
                                header.Cell().Element(HeaderCell).Text("Suma");
                            });

                            foreach (var pozycja in _pozycje)
                            {
                                var suma = pozycja.Ilosc * pozycja.CenaJednostkowaBrutto;
                                table.Cell().Element(BodyCell).Text(pozycja.TypPozycji);
                                table.Cell().Element(BodyCell).Text(pozycja.NazwaPozycji);
                                table.Cell().Element(BodyCell).Text($"{pozycja.Ilosc}");
                                table.Cell().Element(BodyCell).Text($"{pozycja.CenaJednostkowaBrutto:F2} zł");
                                table.Cell().Element(BodyCell).Text($"{suma:F2} zł");
                            }
                        });

                        if (!_pozycje.Any())
                            column.Item().Text("Brak pozycji kosztorysu.");

                        column.Item().AlignRight().Text($"Razem: {_razem:F2} zł").FontSize(14).Bold();
                        column.Item().PaddingTop(30).AlignCenter().Text("Dziękujemy za skorzystanie z naszych usług!").FontSize(9).FontColor(Colors.Grey.Darken2);
                    });
                });
            }).GeneratePdf(path);
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container.Border(1).Background(Colors.Grey.Lighten2).Padding(5).DefaultTextStyle(x => x.Bold());
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container.Border(1).Padding(5);
        }

        private static string SafeFileName(string? value)
        {
            var text = string.IsNullOrWhiteSpace(value) ? "pojazd" : value;
            foreach (var invalid in Path.GetInvalidFileNameChars())
                text = text.Replace(invalid, '_');
            return text;
        }
    }
}

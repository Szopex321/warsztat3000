using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using warsztat3000.ViewModels;
using warsztat3000.Views;
using warsztat3000.Data;
using warsztat3000.Services;

namespace warsztat3000
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            using (var db = new WarsztatDbContext())
            {
                db.Database.EnsureCreated();
                DatabaseSeeder.UpewnijSieZeSchematAktualny(db);
                DatabaseSeeder.WypelnijBaze(db);
            }

            RepairStatusHttpServer.Start();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += (_, _) => RepairStatusHttpServer.Stop();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
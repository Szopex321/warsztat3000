using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace warsztat3000.Services
{
    public static class AppDialogService
    {
        private static Window? MainWindow =>
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        public static async Task<bool> ShowDialogAsync(Window dialog)
        {
            var owner = MainWindow;
            if (owner == null)
            {
                dialog.Show();
                return false;
            }

            return await dialog.ShowDialog<bool>(owner);
        }

        public static async Task ShowWindowAsync(Window dialog)
        {
            var owner = MainWindow;
            if (owner == null)
            {
                dialog.Show();
                return;
            }

            await dialog.ShowDialog(owner);
        }

        public static async Task ShowMessageAsync(string title, string message)
        {
            var window = new Window
            {
                Title = title,
                Width = 420,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Avalonia.Media.Brush.Parse("#0B0A10"),
                Foreground = Avalonia.Media.Brushes.White,
                CanResize = false,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Spacing = 16,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            Foreground = Avalonia.Media.Brush.Parse("#00FF00"),
                            FontSize = 18,
                            FontWeight = Avalonia.Media.FontWeight.Bold
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        new Button
                        {
                            Content = "OK",
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Padding = new Thickness(18, 8),
                            Background = Avalonia.Media.Brush.Parse("#00FF00"),
                            Foreground = Avalonia.Media.Brushes.Black
                        }
                    }
                }
            };

            if (window.Content is StackPanel panel && panel.Children[^1] is Button okButton)
            {
                okButton.Click += (_, _) => window.Close();
            }

            await ShowWindowAsync(window);
        }

        public static async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var result = false;
            var confirmButton = new Button
            {
                Content = "Tak",
                Padding = new Thickness(18, 8),
                Background = Avalonia.Media.Brush.Parse("#00FF00"),
                Foreground = Avalonia.Media.Brushes.Black
            };
            var cancelButton = new Button
            {
                Content = "Nie",
                Padding = new Thickness(18, 8),
                Background = Avalonia.Media.Brushes.Transparent,
                Foreground = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brush.Parse("#2A2A35")
            };

            var window = new Window
            {
                Title = title,
                Width = 430,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Avalonia.Media.Brush.Parse("#0B0A10"),
                Foreground = Avalonia.Media.Brushes.White,
                CanResize = false,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Spacing = 16,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            Foreground = Avalonia.Media.Brush.Parse("#00FF00"),
                            FontSize = 18,
                            FontWeight = Avalonia.Media.FontWeight.Bold
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Spacing = 10,
                            Children = { cancelButton, confirmButton }
                        }
                    }
                }
            };

            cancelButton.Click += (_, _) => window.Close();
            confirmButton.Click += (_, _) =>
            {
                result = true;
                window.Close();
            };

            await ShowWindowAsync(window);
            return result;
        }
    }
}

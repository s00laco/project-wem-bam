using System;
using System.Linq;
using System.Windows;
using WemBam.Logging;

namespace WemBam
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingsWindow? existingWindow = Application.Current.Windows
                    .OfType<SettingsWindow>()
                    .FirstOrDefault();

                if (existingWindow != null)
                {
                    if (existingWindow.WindowState == WindowState.Minimized)
                    {
                        existingWindow.WindowState = WindowState.Normal;
                    }

                    existingWindow.Activate();
                    existingWindow.Focus();
                    return;
                }

                SettingsWindow settingsWindow = new()
                {
                    Owner = this
                };

                settingsWindow.Show();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to open the Settings window.");

                MessageBox.Show(
                    "Wem Bam was unable to open the Settings window.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
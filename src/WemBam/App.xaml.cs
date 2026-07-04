using System;
using System.Windows;
using WemBam.Database;

namespace WemBam
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                DatabaseManager.Initialize();
            }
            catch (Exception ex)
            {
                // TODO: Log exception once logging is implemented.

                MessageBox.Show(
                    "Wem Bam was unable to initialize its database.\n\n" +
                    "The application cannot continue.\n\n" +
                    ex.Message,
                    "Database Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();

                return;
            }

            base.OnStartup(e);
        }
    }
}
using System;
using System.Windows;
using WemBam.Database;
using WemBam.Logging;

namespace WemBam
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Initialize();

            Logger.Information("Application started.");

            try
            {
                DatabaseManager.Initialize();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Database initialization failed.");

                MessageBox.Show(
                    "Wem Bam was unable to initialize its database.\n\n" +
                    "The application cannot continue.\n\n" +
                    "See the application log for additional details.",
                    "Database Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();

                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Shutdown();

            base.OnExit(e);
        }
    }
}
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WemBam.Logging;
using WemBam.Models;
using WemBam.Services;

namespace WemBam
{
    public partial class MainWindow : Window
    {
        private readonly BackgroundTaskManager _backgroundTaskManager =
            BackgroundTaskManager.Instance;

        private readonly DispatcherTimer _elapsedTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        private BackgroundTaskProgress? _latestProgress;

        public MainWindow()
        {
            InitializeComponent();

            _backgroundTaskManager.TaskStarted +=
                BackgroundTaskManager_TaskStarted;

            _backgroundTaskManager.ProgressChanged +=
                BackgroundTaskManager_ProgressChanged;

            _backgroundTaskManager.TaskCompleted +=
                BackgroundTaskManager_TaskCompleted;

            _backgroundTaskManager.TaskCancelled +=
                BackgroundTaskManager_TaskCancelled;

            _elapsedTimer.Tick +=
                ElapsedTimer_Tick;

            ResetBackgroundTaskDisplay();
        }

        private void SettingsMenuItem_Click(
            object sender,
            RoutedEventArgs e)
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

        private void BackgroundTaskManager_TaskStarted(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _latestProgress = null;

                BackgroundTaskProgressBar.Visibility =
                    Visibility.Visible;

                PercentageTextBlock.Visibility =
                    Visibility.Collapsed;

                BackgroundTaskProgressBar.IsIndeterminate = true;
                BackgroundTaskProgressBar.Value = 0;

                StatusTextBlock.Text = "Preparing...";

                StatusToolTipStatusText.Text =
                    "Status: Preparing...";

                StatusToolTipItemsProcessedText.Text =
                    "Items Processed: 0";

                StatusToolTipTotalItemsText.Text =
                    string.Empty;

                StatusToolTipElapsedText.Text =
                    "Elapsed: 00:00:00";

                ElapsedTimeTextBlock.Text =
                    "00:00:00";

                _elapsedTimer.Start();
            });
        }

        private void BackgroundTaskManager_ProgressChanged(
            object? sender,
            BackgroundTaskProgress progress)
        {
            Dispatcher.Invoke(() =>
            {
                _latestProgress = progress;

                StatusTextBlock.Text =
                    progress.StatusMessage;

                if (progress.TotalItems.HasValue)
                {
                    BackgroundTaskProgressBar.IsIndeterminate = false;

                    BackgroundTaskProgressBar.Value =
                        progress.PercentageComplete ?? 0;

                    PercentageTextBlock.Visibility =
                        Visibility.Visible;

                    PercentageTextBlock.Text =
                        $"{progress.PercentageComplete ?? 0:0}%";
                }
                else
                {
                    BackgroundTaskProgressBar.IsIndeterminate = true;
                    BackgroundTaskProgressBar.Value = 0;

                    PercentageTextBlock.Visibility =
                        Visibility.Collapsed;

                    PercentageTextBlock.Text =
                        string.Empty;
                }

                StatusToolTipStatusText.Text =
                    $"Status: {progress.StatusMessage}";

                StatusToolTipItemsProcessedText.Text =
                    $"Items Processed: {progress.ItemsProcessed}";

                StatusToolTipTotalItemsText.Text =
                    progress.TotalItems.HasValue
                        ? $"Total Items: {progress.TotalItems.Value}"
                        : string.Empty;

                StatusToolTipElapsedText.Text =
                    $"Elapsed: {FormatElapsed(progress.Elapsed)}";

                ElapsedTimeTextBlock.Text =
                    FormatElapsed(progress.Elapsed);
            });
        }
        private void BackgroundTaskManager_TaskCompleted(
    object? sender,
    BackgroundOperationResult result)
        {
            Dispatcher.Invoke(ResetBackgroundTaskDisplay);
        }

        private void BackgroundTaskManager_TaskCancelled(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(ResetBackgroundTaskDisplay);
        }

        private void ElapsedTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (_latestProgress is null)
            {
                ElapsedTimeTextBlock.Text =
                    FormatElapsed(_backgroundTaskManager.Elapsed);

                StatusToolTipElapsedText.Text =
                    $"Elapsed: {FormatElapsed(_backgroundTaskManager.Elapsed)}";

                return;
            }

            TimeSpan elapsed =
                _backgroundTaskManager.Elapsed;

            ElapsedTimeTextBlock.Text =
                FormatElapsed(elapsed);

            StatusToolTipElapsedText.Text =
                $"Elapsed: {FormatElapsed(elapsed)}";
        }

        private void ResetBackgroundTaskDisplay()
        {
            _elapsedTimer.Stop();

            _latestProgress = null;

            StatusTextBlock.Text = "Ready";

            BackgroundTaskProgressBar.IsIndeterminate = false;
            BackgroundTaskProgressBar.Value = 0;
            BackgroundTaskProgressBar.Visibility =
                Visibility.Collapsed;

            PercentageTextBlock.Text =
                string.Empty;

            PercentageTextBlock.Visibility =
                Visibility.Collapsed;

            ElapsedTimeTextBlock.Text =
                string.Empty;

            StatusToolTipStatusText.Text =
                "Status: Ready";

            StatusToolTipItemsProcessedText.Text =
                string.Empty;

            StatusToolTipTotalItemsText.Text =
                string.Empty;

            StatusToolTipElapsedText.Text =
                string.Empty;
        }

        private static string FormatElapsed(
            TimeSpan elapsed)
        {
            return elapsed.ToString(@"hh\:mm\:ss");
        }

        protected override void OnClosed(
            EventArgs e)
        {
            _elapsedTimer.Stop();

            _elapsedTimer.Tick -=
                ElapsedTimer_Tick;

            _backgroundTaskManager.TaskStarted -=
                BackgroundTaskManager_TaskStarted;

            _backgroundTaskManager.ProgressChanged -=
                BackgroundTaskManager_ProgressChanged;

            _backgroundTaskManager.TaskCompleted -=
                BackgroundTaskManager_TaskCompleted;

            _backgroundTaskManager.TaskCancelled -=
                BackgroundTaskManager_TaskCancelled;

            base.OnClosed(e);
        }
    }
}
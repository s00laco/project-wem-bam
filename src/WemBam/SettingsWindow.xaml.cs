using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Archives;
using NAudio.Wave;
using Noggog;
using WemBam.Logging;
using WemBam.Models;
using WemBam.Services;
using WemBam.Services.Audio;
using WemBam.Services.Audio.Interop;
using WemBam.Services.Audio.Playback;

namespace WemBam
{
    public partial class SettingsWindow : Window
    {
        private readonly SourceManager _sourceManager =
            SourceManager.Instance;

        private readonly BackgroundTaskManager _backgroundTaskManager =
            BackgroundTaskManager.Instance;

        private readonly DispatcherTimer _elapsedTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        private BackgroundTaskProgress? _latestProgress;

        private BackgroundTaskProgress? _latestWwiseMetadataProgress;

        private string? _wwiseMetadataJsonPath;

        private bool _isWwiseMetadataImportRunning;

        public SettingsWindow()
        {
            InitializeComponent();

            SourcesDataGrid.ItemsSource = _sourceManager.Sources;

            ShowGeneralPage();

            NavigationListBox.SelectedIndex = 0;

            NavigationListBox.SelectionChanged +=
                NavigationListBox_SelectionChanged;

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

            ResetIndexingDisplay();

            RefreshIndexStatusDisplay();
        }

        private void NavigationListBox_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (NavigationListBox.SelectedIndex)
            {
                case 0:
                    ShowGeneralPage();
                    break;

                case 1:
                    ShowSourcesPage();
                    break;

                case 2:
                    ShowAdvancedPage();
                    break;
            }
        }

        private void ShowGeneralPage()
        {
            GeneralPage.Visibility = Visibility.Visible;
            SourcesPage.Visibility = Visibility.Collapsed;
            AdvancedPage.Visibility = Visibility.Collapsed;
        }

        private void ShowSourcesPage()
        {
            GeneralPage.Visibility = Visibility.Collapsed;
            SourcesPage.Visibility = Visibility.Visible;
            AdvancedPage.Visibility = Visibility.Collapsed;
        }

        private void ShowAdvancedPage()
        {
            GeneralPage.Visibility = Visibility.Collapsed;
            SourcesPage.Visibility = Visibility.Collapsed;
            AdvancedPage.Visibility = Visibility.Visible;
        }

        private async void IndexSourcesButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_backgroundTaskManager.IsRunning)
            {
                _backgroundTaskManager.Cancel();
                return;
            }

            Logger.Information("Index started.");

            IndexSourcesOperation operation = new(
                _sourceManager.Sources);

            await _backgroundTaskManager.StartAsync(operation);
        }

        private void BackgroundTaskManager_TaskStarted(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_isWwiseMetadataImportRunning)
                {
                    _latestWwiseMetadataProgress = null;

                    WwiseMetadataProgressPanel.Visibility =
                        Visibility.Visible;

                    WwiseMetadataProgressBar.IsIndeterminate = true;
                    WwiseMetadataProgressBar.Value = 0;

                    WwiseMetadataStatusTextBlock.Text =
                        "Preparing...";

                    WwiseMetadataItemsProcessedTextBlock.Text =
                        "0";

                    WwiseMetadataElapsedTimeTextBlock.Text =
                        "00:00:00";

                    _elapsedTimer.Start();

                    return;
                }

                _latestProgress = null;

                IndexProgressPanel.Visibility =
                    Visibility.Visible;

                IndexProgressBar.Visibility =
                    Visibility.Visible;

                IndexProgressBar.IsIndeterminate = true;
                IndexProgressBar.Value = 0;

                CurrentOperationTextBlock.Text =
                    "Preparing...";

                ItemsProcessedTextBlock.Text = "0";

                ElapsedTimeTextBlock.Text = "00:00:00";

                IndexSourcesButton.Content = "Cancel";

                _elapsedTimer.Start();
            });
        }

        private void BackgroundTaskManager_ProgressChanged(
            object? sender,
            BackgroundTaskProgress progress)
        {
            Dispatcher.Invoke(() =>
            {
                if (_isWwiseMetadataImportRunning)
                {
                    _latestWwiseMetadataProgress = progress;

                    WwiseMetadataStatusTextBlock.Text =
                        progress.StatusMessage;

                    if (progress.TotalItems.HasValue)
                    {
                        WwiseMetadataItemsProcessedTextBlock.Text =
                            $"{progress.ItemsProcessed} / {progress.TotalItems.Value}";

                        WwiseMetadataProgressBar.IsIndeterminate = false;

                        WwiseMetadataProgressBar.Value =
                            progress.PercentageComplete ?? 0;
                    }
                    else
                    {
                        WwiseMetadataItemsProcessedTextBlock.Text =
                            progress.ItemsProcessed.ToString();

                        WwiseMetadataProgressBar.IsIndeterminate = true;
                        WwiseMetadataProgressBar.Value = 0;
                    }

                    WwiseMetadataElapsedTimeTextBlock.Text =
                        FormatElapsed(progress.Elapsed);

                    return;
                }

                _latestProgress = progress;

                CurrentOperationTextBlock.Text =
                    progress.StatusMessage;

                if (progress.TotalItems.HasValue)
                {
                    ItemsProcessedTextBlock.Text =
                        $"{progress.ItemsProcessed} / {progress.TotalItems.Value}";

                    IndexProgressBar.IsIndeterminate = false;

                    IndexProgressBar.Value =
                        progress.PercentageComplete ?? 0;
                }
                else
                {
                    ItemsProcessedTextBlock.Text =
                        progress.ItemsProcessed.ToString();

                    IndexProgressBar.IsIndeterminate = true;
                    IndexProgressBar.Value = 0;
                }

                ElapsedTimeTextBlock.Text =
                    FormatElapsed(progress.Elapsed);
            });
        }

        private void BackgroundTaskManager_TaskCompleted(
            object? sender,
            BackgroundOperationResult result)
        {
            Dispatcher.Invoke(() =>
            {
                if (_isWwiseMetadataImportRunning)
                {
                    Logger.Information(
                        $"Wwise metadata import completed. Events processed: {result.ItemsProcessed}");

                    _isWwiseMetadataImportRunning = false;

                    ResetWwiseMetadataDisplay();

                    return;
                }

                Logger.Information(
                    $"Index completed. Files processed: {result.ItemsProcessed}");

                switch (result.State)
                {
                    case BackgroundTaskState.Completed:
                        IndexStatusManager.Instance.SetUpToDate(
                            result.ItemsProcessed);
                        break;

                    case BackgroundTaskState.CompletedWithWarnings:
                        IndexStatusManager.Instance.SetPartiallyIndexed(
                            result.ItemsProcessed);
                        break;

                    case BackgroundTaskState.Cancelled:
                    case BackgroundTaskState.Failed:
                        IndexStatusManager.Instance.PreserveCurrentState();
                        break;
                }

                RefreshIndexStatusDisplay();

                ResetIndexingDisplay();
            });
        }

        private void BackgroundTaskManager_TaskCancelled(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_isWwiseMetadataImportRunning)
                {
                    Logger.Information("Wwise metadata import cancelled.");

                    _isWwiseMetadataImportRunning = false;

                    ResetWwiseMetadataDisplay();

                    return;
                }

                Logger.Information("Index cancelled.");

                ResetIndexingDisplay();
            });
        }

        private void ElapsedTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (_isWwiseMetadataImportRunning)
            {
                WwiseMetadataElapsedTimeTextBlock.Text =
                    FormatElapsed(_backgroundTaskManager.Elapsed);

                return;
            }

            ElapsedTimeTextBlock.Text =
                FormatElapsed(_backgroundTaskManager.Elapsed);
        }

        private static string FormatElapsed(
            TimeSpan elapsed)
        {
            return elapsed.ToString(@"hh\:mm\:ss");
        }

        private void ResetIndexingDisplay()
        {
            _elapsedTimer.Stop();

            _latestProgress = null;

            IndexProgressPanel.Visibility =
                Visibility.Collapsed;

            IndexProgressBar.IsIndeterminate = false;
            IndexProgressBar.Value = 0;

            CurrentOperationTextBlock.Text =
                string.Empty;

            ItemsProcessedTextBlock.Text =
                string.Empty;

            ElapsedTimeTextBlock.Text =
                string.Empty;

            IndexSourcesButton.Content =
                "Index Sources";
        }

        private void ResetWwiseMetadataDisplay()
        {
            _elapsedTimer.Stop();

            _latestWwiseMetadataProgress = null;

            WwiseMetadataProgressPanel.Visibility =
                Visibility.Collapsed;

            WwiseMetadataProgressBar.IsIndeterminate = false;
            WwiseMetadataProgressBar.Value = 0;

            WwiseMetadataStatusTextBlock.Text =
                string.Empty;

            WwiseMetadataItemsProcessedTextBlock.Text =
                string.Empty;

            WwiseMetadataElapsedTimeTextBlock.Text =
                string.Empty;
        }

        private void MarkIndexOutOfDate()
        {
            IndexStatusManager.Instance.SetOutOfDate();

            RefreshIndexStatusDisplay();
        }

        private void RefreshIndexStatusDisplay()
        {
            IndexStatusTextBlock.Text =
                IndexStatusManager.Instance.Status;

            LastIndexedTextBlock.Text =
                IndexStatusManager.Instance.LastIndexed.HasValue
                    ? IndexStatusManager.Instance.LastIndexed.Value
                        .ToLocalTime()
                        .ToString("g")
                    : "—";
        }

        private void AddFolderButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                OpenFolderDialog dialog = new()
                {
                    Title = "Select Folder"
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                int addedCount = 0;
                int duplicateCount = 0;

                Source source = new()
                {
                    DisplayName = Path.GetFileName(dialog.FolderName),
                    Path = dialog.FolderName,
                    Type = SourceType.Folder
                };

                if (string.IsNullOrWhiteSpace(source.DisplayName))
                {
                    source.DisplayName = dialog.FolderName;
                }

                if (_sourceManager.AddSource(source))
                {
                    addedCount++;
                    MarkIndexOutOfDate();
                }
                else
                {
                    duplicateCount++;
                }

                ShowAddSourcesSummary(
                    addedCount,
                    duplicateCount);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Failed to add folder source.");

                MessageBox.Show(
                    "Wem Bam was unable to add the selected folder.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddFileButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new()
                {
                    Title = "Select File",
                    Multiselect = true,
                    Filter = "Supported Audio Files (*.ba2;*.bnk;*.wem;*.wav)|*.ba2;*.bnk;*.wem;*.wav|All Files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                int addedCount = 0;
                int duplicateCount = 0;

                foreach (string fileName in dialog.FileNames)
                {
                    Source source = new()
                    {
                        DisplayName = Path.GetFileName(fileName),
                        Path = fileName,
                        Type = SourceType.File
                    };

                    if (_sourceManager.AddSource(source))
                    {
                        addedCount++;
                        MarkIndexOutOfDate();
                    }
                    else
                    {
                        duplicateCount++;
                    }
                }

                ShowAddSourcesSummary(
                    addedCount,
                    duplicateCount);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Failed to add file source.");

                MessageBox.Show(
                    "Wem Bam was unable to add the selected file.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BrowseWwiseMetadataButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "SoundBanksInfo.json|SoundBanksInfo.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select SoundBanksInfo.json"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _wwiseMetadataJsonPath = dialog.FileName;

            WwiseMetadataJsonPathTextBox.Text =
                _wwiseMetadataJsonPath;

            ImportWwiseMetadataButton.IsEnabled = true;
        }

        private async void ImportWwiseMetadataButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_wwiseMetadataJsonPath))
            {
                return;
            }

            _isWwiseMetadataImportRunning = true;

            WwiseMetadataImportOperation operation =
                new(_wwiseMetadataJsonPath);

            await _backgroundTaskManager.StartAsync(operation);
        }

        private void RemoveSourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (SourcesDataGrid.SelectedItem is not Source source)
                {
                    return;
                }

                _sourceManager.RemoveSource(source);

                MarkIndexOutOfDate();
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Failed to remove source.");

                MessageBox.Show(
                    "Wem Bam was unable to remove the selected source.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ShowAddSourcesSummary(
            int addedCount,
            int duplicateCount)
        {
            if (addedCount == 0)
            {
                MessageBox.Show(
                    "No new sources were added.\n\nThe selected source(s) already exist.",
                    "Sources",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string message = addedCount == 1
                ? "Added 1 source."
                : $"Added {addedCount} sources.";

            if (duplicateCount > 0)
            {
                message += Environment.NewLine +
                           Environment.NewLine;

                message += duplicateCount == 1
                    ? "1 duplicate source was skipped."
                    : $"{duplicateCount} duplicate sources were skipped.";
            }

            MessageBox.Show(
                message,
                "Sources",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        protected override void OnClosed(
            EventArgs e)
        {
            _elapsedTimer.Stop();

            _elapsedTimer.Tick -=
                ElapsedTimer_Tick;

            NavigationListBox.SelectionChanged -=
                NavigationListBox_SelectionChanged;

            _backgroundTaskManager.TaskStarted -=
                BackgroundTaskManager_TaskStarted;

            _backgroundTaskManager.ProgressChanged -=
                BackgroundTaskManager_ProgressChanged;

            _backgroundTaskManager.TaskCompleted -=
                BackgroundTaskManager_TaskCompleted;

            _backgroundTaskManager.TaskCancelled -=
                BackgroundTaskManager_TaskCancelled;

            base.OnClosed(e);

            if (Owner is Window owner)
            {
                owner.Activate();
                owner.Focus();
            }
        }

        private void BrowseWemButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new()
                {
                    Title = "Select WEM File",
                    Filter = "Wwise Audio (*.wem)|*.wem",
                    Multiselect = false
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                SelectedWemFileTextBox.Text = dialog.FileName;

                PlayWemButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Failed to select playback test file.");

                MessageBox.Show(
                    "Wem Bam was unable to open the selected WEM file.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BrowseBa2Button_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new()
                {
                    Title = "Select BA2 Archive",
                    Filter = "Starfield Archives (*.ba2)|*.ba2",
                    Multiselect = false
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                IArchiveReader archive =
                    Archive.CreateReader(
                        GameRelease.Starfield,
                        new FilePath(dialog.FileName));

                string[] wemEntries = archive.Files
                    .Where(file =>
                        Path.GetExtension(file.Path)
                            .Equals(
                                ".wem",
                                StringComparison.OrdinalIgnoreCase))
                    .Select(file => file.Path)
                    .ToArray();

                SelectedBa2FileTextBox.Text =
                    dialog.FileName;

                SelectedBa2WemComboBox.ItemsSource =
                    wemEntries;

                SelectedBa2WemComboBox.SelectedIndex = -1;

                SelectedBa2WemComboBox.IsEnabled =
                    wemEntries.Length > 0;

                PlayBa2WemButton.IsEnabled = false;

                if (wemEntries.Length == 0)
                {
                    MessageBox.Show(
                        "The selected BA2 archive does not contain any WEM files.",
                        "BA2 Playback Test",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Failed to select BA2 playback test archive.");

                MessageBox.Show(
                    "Wem Bam was unable to open the selected BA2 archive.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SelectedBa2WemComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            PlayBa2WemButton.IsEnabled =
            SelectedBa2WemComboBox.SelectedIndex >= 0;
        }

        private void PlayBa2WemButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (SelectedBa2WemComboBox.SelectedItem is not string wemEntry)
                {
                    return;
                }

                AudioStreamRequest request = new()
                {
                    SourceType = SourceType.File,
                    ContainerPath = SelectedBa2FileTextBox.Text,
                    AssetPath = wemEntry
                };

                var streamProvider =
                    AudioStreamProviderFactory.Create(request);

                using Stream stream =
                    streamProvider.OpenStream(request);

                using LibVgmStream decoder =
                    new(
                        stream,
                        Path.GetFileName(wemEntry));

                VgmStreamWaveProvider provider =
                    new(decoder);

                using WaveOutEvent output =
                    new();

                output.Init(provider);

                output.Play();

                MessageBox.Show(
                    "Playback started.\n\nClick OK to stop playback.",
                    "BA2 Playback Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "BA2 playback validation failed.");

                MessageBox.Show(
                    ex.Message,
                    "BA2 Playback Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PlayWemButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                using FileStream stream =
                    File.OpenRead(
                        SelectedWemFileTextBox.Text);

                using LibVgmStream decoder =
                    new(
                        stream,
                        Path.GetFileName(
                            SelectedWemFileTextBox.Text));

                VgmStreamWaveProvider provider =
                    new(decoder);

                using WaveOutEvent output =
                    new();

                output.Init(provider);

                output.Play();

                MessageBox.Show(
                    "Playback started.\n\nClick OK to stop playback.",
                    "Playback Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Playback validation failed.");

                MessageBox.Show(
                    ex.Message,
                    "Playback Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}
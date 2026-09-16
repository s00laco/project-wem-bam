using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;
using WemBam.Contracts;
using WemBam.Database;
using WemBam.Logging;
using WemBam.Models;
using WemBam.Services;
using WemBam.Services.Audio;
using WemBam.Services.Audio.Interop;
using WemBam.Services.Audio.Playback;

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

        private readonly SearchEngine _searchEngine = new();

        private IReadOnlyList<Collection> _collections =
            Array.Empty<Collection>();

        private long? _selectedCollectionId;

        private AudioAssetDetails? _selectedAudioAssetDetails;

        private WaveOutEvent? _currentOutput;

        private Stream? _currentStream;

        private LibVgmStream? _currentDecoder;

        private VgmStreamWaveProvider? _currentWaveProvider;

        private SearchResult? _currentlyPlayingResult;

        private SearchResult? _lastPlayedResult;

        private bool _isStoppingPlayback;

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

            LoadCollections();
        }

        private void LoadCollections()
        {
            long? previouslySelectedCollectionId =
                _selectedCollectionId;

            _collections = DatabaseManager.GetCollections();

            CollectionsTreeView.Items.Clear();

            System.Windows.Controls.TreeViewItem allSoundsItem =
                new()
                {
                    Header = "All Sounds",
                    Tag = null,
                    IsSelected = previouslySelectedCollectionId is null
                };

            CollectionsTreeView.Items.Add(allSoundsItem);

            CollectionsTreeView.Items.Add(
                new System.Windows.Controls.TreeViewItem
                {
                    Header = "Favourites",
                    Tag = null,
                    IsEnabled = false
                });

            foreach (Collection collection in _collections)
            {
                System.Windows.Controls.TreeViewItem item =
                    new()
                    {
                        Header = collection.Name,
                        Tag = collection.Id,
                        IsSelected =
                            previouslySelectedCollectionId == collection.Id
                    };

                CollectionsTreeView.Items.Add(item);
            }
        }

        private void NewCollectionButton_Click(
                object sender,
                RoutedEventArgs e)
        {
            CollectionNameDialog dialog = new()
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            long collectionId =
                DatabaseManager.AddCollection(
                    dialog.CollectionName);

            LoadCollections();

            foreach (System.Windows.Controls.TreeViewItem item
                     in CollectionsTreeView.Items)
            {
                if (item.Tag is long id &&
                    id == collectionId)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void RenameCollectionButton_Click(
                object sender,
                RoutedEventArgs e)
        {
            if (_selectedCollectionId is not long collectionId)
            {
                return;
            }

            Collection? collection =
                _collections.FirstOrDefault(
                    item => item.Id == collectionId);

            if (collection is null)
            {
                return;
            }

            CollectionNameDialog dialog =
                new(
                    collection.Name,
                    "Rename Collection")
                {
                    Owner = this
                };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            DatabaseManager.RenameCollection(
                collectionId,
                dialog.CollectionName);

            LoadCollections();

            foreach (System.Windows.Controls.TreeViewItem item
                     in CollectionsTreeView.Items)
            {
                if (item.Tag is long id &&
                    id == collectionId)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void DeleteCollectionButton_Click(
                object sender,
                RoutedEventArgs e)
        {
            if (_selectedCollectionId is not long collectionId)
            {
                return;
            }

            Collection? collection =
                _collections.FirstOrDefault(
                    item => item.Id == collectionId);

            if (collection is null)
            {
                return;
            }

            MessageBoxResult confirmation =
                MessageBox.Show(
                    $"Delete the collection \"{collection.Name}\"?",
                    "Delete Collection",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            bool wasSelectedCollection =
                _selectedCollectionId == collectionId;

            DatabaseManager.DeleteCollection(
                collectionId);

            _selectedCollectionId = null;

            LoadCollections();

            if (wasSelectedCollection)
            {
                return;
            }
        }

        private void CollectionsButton_Click(
                object sender,
                RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.DataContext is not SearchResult result)
            {
                return;
            }

            CollectionMembershipDialog dialog =
                new(result.AudioAssetId)
    {
        Owner = this
    };

            if (dialog.ShowDialog() == true)
            {
                LoadCollections();

                IReadOnlyList<SearchResult> results =
                    _searchEngine.Search(
                        FilterTextBox.Text,
                        _selectedCollectionId);

                ResultsDataGrid.ItemsSource =
                    results;
            }
        }

        private void CollectionsTreeView_SelectedItemChanged(
                object sender,
                RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not System.Windows.Controls.TreeViewItem item)
            {
                return;
            }

            if (item.Header is string header &&
                header == "All Sounds")
            {
                _selectedCollectionId = null;
            }
            else if (item.Tag is long collectionId)
            {
                _selectedCollectionId = collectionId;
            }
            else
            {
                return;
            }

            IReadOnlyList<SearchResult> results =
                _searchEngine.Search(
                    FilterTextBox.Text,
                    _selectedCollectionId);

            ResultsDataGrid.ItemsSource =
                results;
        }

        private void ResultsDataGrid_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult result)
            {
                _selectedAudioAssetDetails = null;

                SelectedFilenameTextBox.Text = string.Empty;
                SelectedDurationTextBox.Text = string.Empty;
                SelectedLoopedTextBox.Text = string.Empty;
                SelectedWwisePathTextBox.Text = string.Empty;
                SelectedContainerPathTextBox.Text = string.Empty;
                SelectedAssetPathTextBox.Text = string.Empty;
                SelectedWwiseEventsListBox.ItemsSource = null;
                SelectedCategoriesListBox.ItemsSource = null;

                return;
            }

            _selectedAudioAssetDetails =
                DatabaseManager.GetAudioAssetDetails(
                    result.AudioAssetId);

            if (_selectedAudioAssetDetails is null)
            {
                return;
            }

            AudioAsset audioAsset =
                _selectedAudioAssetDetails.AudioAsset;

            SelectedFilenameTextBox.Text =
                audioAsset.FileName;

            SelectedDurationTextBox.Text =
                audioAsset.Duration?.ToString() ?? string.Empty;

            SelectedLoopedTextBox.Text =
                _selectedAudioAssetDetails.IsLooped ? "Yes" : "No";

            SelectedWwisePathTextBox.Text =
                _selectedAudioAssetDetails.WwisePath;

            AudioAssetSource? defaultSource =
                _selectedAudioAssetDetails.Sources
                    .FirstOrDefault(
                        source =>
                            source.Id == audioAsset.DefaultSourceId);

            SelectedContainerPathTextBox.Text =
                defaultSource?.ContainerPath ?? string.Empty;

            SelectedAssetPathTextBox.Text =
                defaultSource?.AssetPath ?? string.Empty;

            SelectedWwiseEventsListBox.ItemsSource =
                _selectedAudioAssetDetails.WwiseEvents;

            SelectedCategoriesListBox.ItemsSource =
                _selectedAudioAssetDetails.Categories;
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

        private void SearchButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            IReadOnlyList<SearchResult> results =
                _searchEngine.Search(
                    FilterTextBox.Text,
                    _selectedCollectionId);

            ResultsDataGrid.ItemsSource =
                results;
        }

        private void FilterTextBox_KeyDown(
    object sender,
    System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
            {
                return;
            }

            SearchButton_Click(
                SearchButton,
                new RoutedEventArgs());

            e.Handled = true;
        }

        private void CurrentOutput_PlaybackStopped(
                object? sender,
                StoppedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_isStoppingPlayback ||
                    sender is not WaveOutEvent output ||
                    !ReferenceEquals(output, _currentOutput) ||
                    _currentlyPlayingResult is null)
                {
                    return;
                }

                _currentlyPlayingResult.IsPlaying = false;

                _currentOutput = null;
                _currentWaveProvider = null;
                _currentDecoder?.Dispose();
                _currentDecoder = null;
                _currentStream?.Dispose();
                _currentStream = null;
                _currentlyPlayingResult = null;
            });
        }

        private void MainPlayPauseButton_Click(
                object sender,
                RoutedEventArgs e)
        {
            if (_currentOutput is not null &&
                _currentlyPlayingResult is not null)
            {
                if (_currentOutput.PlaybackState == PlaybackState.Playing)
                {
                    _currentOutput.Pause();
                    _currentlyPlayingResult.IsPlaying = false;
                }
                else if (_currentOutput.PlaybackState == PlaybackState.Paused)
                {
                    _currentOutput.Play();
                    _currentlyPlayingResult.IsPlaying = true;
                }

                return;
            }

            if (_lastPlayedResult is null)
            {
                return;
            }

            SearchResult result = _lastPlayedResult;

            AudioStreamRequest request = new()
            {
                SourceType = SourceType.File,
                ContainerPath = result.ContainerPath,
                AssetPath = result.AssetPath
            };

            IAudioStreamProvider streamProvider =
                AudioStreamProviderFactory.Create(request);

            Stream stream =
                streamProvider.OpenStream(request);

            LibVgmStream decoder =
                new(
                    stream,
                    result.FileName);

            VgmStreamWaveProvider waveProvider =
                new(decoder);

            WaveOutEvent output =
                new();

            output.PlaybackStopped += CurrentOutput_PlaybackStopped;

            output.Init(waveProvider);
            output.Play();

            _currentStream = stream;
            _currentDecoder = decoder;
            _currentWaveProvider = waveProvider;
            _currentOutput = output;
            _currentlyPlayingResult = result;
            _lastPlayedResult = result;

            result.IsPlaying = true;
        }

        private void StopButton_Click(
                object sender,
                RoutedEventArgs e)
        {
            if (_currentOutput is null)
            {
                return;
            }

            _isStoppingPlayback = true;

            if (_currentlyPlayingResult is not null)
            {
                _currentlyPlayingResult.IsPlaying = false;
            }

            _currentOutput.Stop();
            _currentOutput.Dispose();

            _isStoppingPlayback = false;

            _currentOutput = null;
            _currentWaveProvider = null;

            _currentDecoder?.Dispose();
            _currentDecoder = null;

            _currentStream?.Dispose();
            _currentStream = null;

            _currentlyPlayingResult = null;
        }

        private void PlayButton_Click(
            object sender,
            RoutedEventArgs e)
        {

            if (sender is not System.Windows.Controls.Button button ||
                button.DataContext is not SearchResult result)
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(result.AssetPath))
                {
                    return;
                }

                if (_currentlyPlayingResult == result &&
                    _currentOutput is not null)
                {
                    if (_currentOutput.PlaybackState == PlaybackState.Playing)
                    {
                        _currentOutput.Pause();
                        result.IsPlaying = false;
                    }
                    else if (_currentOutput.PlaybackState == PlaybackState.Paused)
                    {
                        _currentOutput.Play();
                        result.IsPlaying = true;
                    }

                    return;
                }

                _isStoppingPlayback = true;

                if (_currentlyPlayingResult is not null)
                {
                    _currentlyPlayingResult.IsPlaying = false;
                }

                _currentOutput?.Stop();
                _currentOutput?.Dispose();

                _isStoppingPlayback = false;

                _currentOutput = null;

                _currentDecoder?.Dispose();
                _currentDecoder = null;

                _currentStream?.Dispose();
                _currentStream = null;

                _currentWaveProvider = null;
                _currentlyPlayingResult = null;

                AudioStreamRequest request = new()
                {
                    SourceType = SourceType.File,
                    ContainerPath = result.ContainerPath,
                    AssetPath = result.AssetPath
                };

                IAudioStreamProvider streamProvider =
                    AudioStreamProviderFactory.Create(request);

                Stream stream =
                    streamProvider.OpenStream(request);

                LibVgmStream decoder =
                    new(
                        stream,
                        result.FileName);

                VgmStreamWaveProvider waveProvider =
                    new(decoder);

                WaveOutEvent output =
                    new();

                output.PlaybackStopped += CurrentOutput_PlaybackStopped;

                output.Init(waveProvider);
                output.Play();

                _currentStream = stream;
                _currentDecoder = decoder;
                _currentWaveProvider = waveProvider;
                _currentOutput = output;
                _currentlyPlayingResult = result;
                _lastPlayedResult = result;

                result.IsPlaying = true;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Failed to play audio asset.");

                MessageBox.Show(
                    "Wem Bam was unable to play the selected audio asset.",
                    "Playback Error",
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
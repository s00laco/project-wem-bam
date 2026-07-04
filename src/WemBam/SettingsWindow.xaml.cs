using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using WemBam.Models;
using WemBam.Services;

namespace WemBam
{
    public partial class SettingsWindow : Window
    {
        private readonly SourceManager _sourceManager = SourceManager.Instance;

        public SettingsWindow()
        {
            InitializeComponent();

            SourcesDataGrid.ItemsSource = _sourceManager.Sources;

            ShowGeneralPage();
            NavigationListBox.SelectedIndex = 0;

            NavigationListBox.SelectionChanged += NavigationListBox_SelectionChanged;
        }

        private void NavigationListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
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
                }
                else
                {
                    duplicateCount++;
                }

                ShowAddSourcesSummary(addedCount, duplicateCount);
            }
            catch (Exception)
            {
                // TODO: Log exception once logging is implemented.

                MessageBox.Show(
                    "Wem Bam was unable to add the selected folder.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
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
                    }
                    else
                    {
                        duplicateCount++;
                    }
                }

                ShowAddSourcesSummary(addedCount, duplicateCount);
            }
            catch (Exception)
            {
                // TODO: Log exception once logging is implemented.

                MessageBox.Show(
                    "Wem Bam was unable to add the selected file.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void RemoveSourceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SourcesDataGrid.SelectedItem is not Source source)
                {
                    return;
                }

                _sourceManager.RemoveSource(source);
            }
            catch (Exception)
            {
                // TODO: Log exception once logging is implemented.

                MessageBox.Show(
                    "Wem Bam was unable to remove the selected source.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ShowAddSourcesSummary(int addedCount, int duplicateCount)
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
                message += Environment.NewLine + Environment.NewLine;

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

        protected override void OnClosed(EventArgs e)
        {
            NavigationListBox.SelectionChanged -= NavigationListBox_SelectionChanged;

            base.OnClosed(e);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
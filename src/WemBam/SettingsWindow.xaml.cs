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

            SourcesListView.ItemsSource = _sourceManager.Sources;

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

                _sourceManager.AddSource(source);
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

                foreach (string fileName in dialog.FileNames)
                {
                    Source source = new()
                    {
                        DisplayName = Path.GetFileName(fileName),
                        Path = fileName,
                        Type = SourceType.File
                    };

                    _sourceManager.AddSource(source);
                }
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
                if (SourcesListView.SelectedItem is not Source source)
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
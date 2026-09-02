using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WemBam.Database;
using WemBam.Models;

namespace WemBam
{
    public partial class CollectionMembershipDialog : Window
    {
        private const string CollectionFilterPlaceholder =
            "Filter Collections...";

        private readonly long _audioAssetId;

        public IReadOnlyList<long> SelectedCollectionIds { get; private set; } =
            Array.Empty<long>();

        public CollectionMembershipDialog(
            long audioAssetId)
        {
            InitializeComponent();

            CollectionFilterTextBox.Text =
                CollectionFilterPlaceholder;

            _audioAssetId = audioAssetId;

            LoadCollections();
        }

        private void LoadCollections()
        {
            IReadOnlyList<Collection> collections =
                DatabaseManager.GetCollections();

            IReadOnlyList<long> existingCollectionIds =
                DatabaseManager.GetCollectionIdsForAudioAsset(
                    _audioAssetId);

            foreach (Collection collection in collections)
            {
                CollectionsListBox.Items.Add(
                    new System.Windows.Controls.ListBoxItem
                    {
                        Content = collection.Name,
                        Tag = collection.Id,
                        IsSelected =
                            existingCollectionIds.Contains(
                                collection.Id)
                    });
            }
        }

        private void ApplyButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SelectedCollectionIds =
                CollectionsListBox.SelectedItems
                    .Cast<System.Windows.Controls.ListBoxItem>()
                    .Where(item => item.Tag is long)
                    .Select(item => (long)item.Tag!)
                    .ToArray();

            DatabaseManager.SetAudioAssetCollections(
                _audioAssetId,
                SelectedCollectionIds);

            DialogResult = true;
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

            System.Windows.Controls.ListBoxItem item =
                new()
                {
                    Content = dialog.CollectionName,
                    Tag = collectionId,
                    IsSelected = true
                };

            CollectionsListBox.Items.Add(item);
        }

        private void CollectionFilterTextBox_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            if (CollectionFilterTextBox.Text ==
                CollectionFilterPlaceholder)
            {
                return;
            }

            string filter =
                CollectionFilterTextBox.Text.Trim();

            foreach (System.Windows.Controls.ListBoxItem item
                     in CollectionsListBox.Items)
            {
                string collectionName =
                    item.Content?.ToString() ?? string.Empty;

                item.Visibility =
                    string.IsNullOrWhiteSpace(filter) ||
                    collectionName.Contains(
                        filter,
                        StringComparison.OrdinalIgnoreCase)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }
        }

        private void CollectionFilterTextBox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (CollectionFilterTextBox.Text ==
                CollectionFilterPlaceholder)
            {
                CollectionFilterTextBox.Clear();
            }
        }

    }
}
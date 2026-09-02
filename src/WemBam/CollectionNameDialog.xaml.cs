using System.Windows;

namespace WemBam
{
    public partial class CollectionNameDialog : Window
    {
        public string CollectionName =>
            NameTextBox.Text.Trim();

        public CollectionNameDialog(
            string? initialName = null,
            string title = "New Collection")
        {
            InitializeComponent();

            Title = title;

            if (!string.IsNullOrWhiteSpace(initialName))
            {
                NameTextBox.Text = initialName;
                NameTextBox.SelectAll();
            }

            Loaded += (_, _) =>
            {
                NameTextBox.Focus();
            };
        }

        private void CreateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter a collection name.",
                    "Collection Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NameTextBox.Focus();

                return;
            }

            DialogResult = true;
        }
    }
}
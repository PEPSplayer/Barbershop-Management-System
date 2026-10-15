using System.Windows;

namespace HairSalonIS.Views
{
    public partial class SupplierWindow : Window
    {
        public string SupplierContact { get; private set; } = "";

        public SupplierWindow()
        {
            InitializeComponent();
        }

        public SupplierWindow(string contact)
        {
            InitializeComponent();

            ContactTextBox.Text = contact;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string contact = ContactTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(contact))
            {
                MessageBox.Show(
                    "Введите контакт поставщика.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (contact.Length != 11)
            {
                MessageBox.Show(
                    "Контакт поставщика должен состоять из 11 символов.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            SupplierContact = contact;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
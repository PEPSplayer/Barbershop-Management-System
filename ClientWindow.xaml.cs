using System.Windows;

namespace HairSalonIS.Views
{
    public partial class ClientWindow : Window
    {
        public string ClientFullName { get; private set; } = "";
        public string ClientPhone { get; private set; } = "";

        public ClientWindow()
        {
            InitializeComponent();
        }

        public ClientWindow(string fullName, string phone)
        {
            InitializeComponent();

            FullNameTextBox.Text = fullName;
            PhoneTextBox.Text = phone;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show(
                    "Введите ФИО клиента.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show(
                    "Введите телефон клиента.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (phone.Length != 11)
            {
                MessageBox.Show(
                    "Телефон должен состоять из 11 символов.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            ClientFullName = fullName;
            ClientPhone = phone;

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
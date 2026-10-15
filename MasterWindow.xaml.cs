using System.Windows;

namespace HairSalonIS.Views
{
    public partial class MasterWindow : Window
    {
        public string MasterFullName { get; private set; } = "";

        public MasterWindow()
        {
            InitializeComponent();
        }

        public MasterWindow(string fullName)
        {
            InitializeComponent();

            FullNameTextBox.Text = fullName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show(
                    "Введите ФИО мастера.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            MasterFullName = fullName;

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
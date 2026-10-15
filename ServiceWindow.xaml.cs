using System;
using System.Globalization;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class ServiceWindow : Window
    {
        public string ServiceName { get; private set; } = "";
        public decimal ServiceCost { get; private set; }
        public string ServiceCategory { get; private set; } = "";

        public ServiceWindow()
        {
            InitializeComponent();
        }

        public ServiceWindow(string name, decimal cost, string category)
        {
            InitializeComponent();

            NameTextBox.Text = name;
            CostTextBox.Text = cost.ToString(CultureInfo.InvariantCulture);
            CategoryTextBox.Text = category;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string costText = CostTextBox.Text.Trim().Replace(',', '.');
            string category = CategoryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(
                    "Введите название услуги.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(costText))
            {
                MessageBox.Show(
                    "Введите стоимость услуги.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (!decimal.TryParse(costText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal cost))
            {
                MessageBox.Show(
                    "Стоимость должна быть числом. Например: 1500 или 1500.50",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (cost <= 0)
            {
                MessageBox.Show(
                    "Стоимость услуги должна быть больше нуля.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show(
                    "Введите категорию услуги.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            ServiceName = name;
            ServiceCost = cost;
            ServiceCategory = category;

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
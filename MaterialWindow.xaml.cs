using System;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class MaterialWindow : Window
    {
        public string MaterialName { get; private set; } = "";
        public string MaterialCategory { get; private set; } = "";
        public int MaterialQuantity { get; private set; }

        public MaterialWindow()
        {
            InitializeComponent();
        }

        public MaterialWindow(string name, string category, int quantity)
        {
            InitializeComponent();

            MaterialNameTextBox.Text = name;
            MaterialCategoryTextBox.Text = category;
            MaterialQuantityTextBox.Text = quantity.ToString();

            MaterialNameTextBox.IsEnabled = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = MaterialNameTextBox.Text.Trim();
            string category = MaterialCategoryTextBox.Text.Trim();
            string quantityText = MaterialQuantityTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(
                    "Введите название материала.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show(
                    "Введите категорию материала.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (!int.TryParse(quantityText, out int quantity))
            {
                MessageBox.Show(
                    "Количество материала должно быть целым числом.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (quantity < 0)
            {
                MessageBox.Show(
                    "Количество материала не может быть отрицательным.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            MaterialName = name;
            MaterialCategory = category;
            MaterialQuantity = quantity;

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
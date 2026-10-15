using HairSalonIS.Data;
using System;
using System.Data;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class ShipmentWindow : Window
    {
        public int SupplierId { get; private set; }
        public string MaterialName { get; private set; } = "";
        public DateTime ShipmentDate { get; private set; }
        public int MaterialsNumber { get; private set; }

        public ShipmentWindow()
        {
            InitializeComponent();

            LoadSuppliers();
            LoadMaterials();

            ShipmentDatePicker.SelectedDate = DateTime.Today;
        }

        public ShipmentWindow(int supplierId, string materialName, DateTime shipmentDate, int materialsNumber)
        {
            InitializeComponent();

            LoadSuppliers();
            LoadMaterials();

            SupplierComboBox.SelectedValue = supplierId;
            MaterialComboBox.SelectedValue = materialName;
            ShipmentDatePicker.SelectedDate = shipmentDate;
            MaterialsNumberTextBox.Text = materialsNumber.ToString();
        }

        private void LoadSuppliers()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    id_supplier,
                    contact_supplier as ""Контакт поставщика""
                  from Supplier
                  order by contact_supplier"
            );

            SupplierComboBox.ItemsSource = table.DefaultView;
            SupplierComboBox.DisplayMemberPath = "Контакт поставщика";
            SupplierComboBox.SelectedValuePath = "id_supplier";
        }

        private void LoadMaterials()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    name_material,
                    name_material || ' — остаток: ' || quantity_material as material_display
                  from Material
                  order by name_material"
            );

            MaterialComboBox.ItemsSource = table.DefaultView;
            MaterialComboBox.DisplayMemberPath = "material_display";
            MaterialComboBox.SelectedValuePath = "name_material";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите поставщика.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (MaterialComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите материал.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (ShipmentDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите дату поставки.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (!int.TryParse(MaterialsNumberTextBox.Text.Trim(), out int materialsNumber))
            {
                MessageBox.Show(
                    "Количество материала должно быть целым числом.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (materialsNumber <= 0)
            {
                MessageBox.Show(
                    "Количество материала должно быть больше нуля.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            SupplierId = Convert.ToInt32(SupplierComboBox.SelectedValue);
            MaterialName = MaterialComboBox.SelectedValue.ToString() ?? "";
            ShipmentDate = ShipmentDatePicker.SelectedDate.Value.Date;
            MaterialsNumber = materialsNumber;

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
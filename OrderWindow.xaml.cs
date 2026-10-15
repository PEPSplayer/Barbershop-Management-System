using HairSalonIS.Data;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class OrderWindow : Window
    {
        public string CompanyNumberAdmin { get; private set; } = "";
        public int ClientId { get; private set; }
        public int MasterId { get; private set; }
        public bool StatusOrder { get; private set; }
        public DateTime OrderDate { get; private set; }
        public TimeSpan TimeFrom { get; private set; }
        public TimeSpan TimeBefore { get; private set; }

        public ObservableCollection<SelectedServiceItem> SelectedServices { get; private set; } = new ObservableCollection<SelectedServiceItem>();
        public ObservableCollection<SelectedMaterialItem> SelectedMaterials { get; private set; } = new ObservableCollection<SelectedMaterialItem>();

        public OrderWindow()
        {
            InitializeComponent();

            LoadAdministrators();
            LoadClients();
            LoadMasters();
            LoadServices();
            LoadMaterials();

            OrderDatePicker.SelectedDate = DateTime.Today;

            SelectedServicesListBox.ItemsSource = SelectedServices;
            SelectedMaterialsListBox.ItemsSource = SelectedMaterials;
        }

        public OrderWindow(
            int orderId,
            string companyNumberAdmin,
            int clientId,
            int masterId,
            bool statusOrder,
            DateTime orderDate,
            TimeSpan timeFrom,
            TimeSpan timeBefore)
        {
            InitializeComponent();

            LoadAdministrators();
            LoadClients();
            LoadMasters();
            LoadServices();
            LoadMaterials();

            SelectedServicesListBox.ItemsSource = SelectedServices;
            SelectedMaterialsListBox.ItemsSource = SelectedMaterials;

            AdministratorComboBox.SelectedValue = companyNumberAdmin;
            ClientComboBox.SelectedValue = clientId;
            MasterComboBox.SelectedValue = masterId;
            StatusCheckBox.IsChecked = statusOrder;
            OrderDatePicker.SelectedDate = orderDate;
            TimeFromTextBox.Text = timeFrom.ToString(@"hh\:mm");
            TimeBeforeTextBox.Text = timeBefore.ToString(@"hh\:mm");

            LoadOrderServices(orderId);
            LoadOrderMaterials(orderId);
        }

        private void LoadAdministrators()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    company_number_admin,
                    admin_full_name_admin as ""ФИО администратора""
                  from Administrator
                  order by admin_full_name_admin"
            );

            AdministratorComboBox.ItemsSource = table.DefaultView;
            AdministratorComboBox.DisplayMemberPath = "ФИО администратора";
            AdministratorComboBox.SelectedValuePath = "company_number_admin";
        }

        private void LoadClients()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    id_client,
                    full_name_client as ""ФИО клиента""
                  from Client
                  order by full_name_client"
            );

            ClientComboBox.ItemsSource = table.DefaultView;
            ClientComboBox.DisplayMemberPath = "ФИО клиента";
            ClientComboBox.SelectedValuePath = "id_client";
        }

        private void LoadMasters()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    id_master,
                    full_name_master as ""ФИО мастера""
                  from Master
                  order by full_name_master"
            );

            MasterComboBox.ItemsSource = table.DefaultView;
            MasterComboBox.DisplayMemberPath = "ФИО мастера";
            MasterComboBox.SelectedValuePath = "id_master";
        }

        private void LoadServices()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    name_service,
                    cost_service,
                    name_service || ' — ' || cost_service || ' руб.' as service_display
                  from Service
                  order by name_service"
            );

            ServiceComboBox.ItemsSource = table.DefaultView;
            ServiceComboBox.DisplayMemberPath = "service_display";
            ServiceComboBox.SelectedValuePath = "name_service";
        }

        private void LoadMaterials()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    name_material,
                    quantity_material,
                    name_material || ' — остаток: ' || quantity_material as material_display
                  from Material
                  order by name_material"
            );

            MaterialComboBox.ItemsSource = table.DefaultView;
            MaterialComboBox.DisplayMemberPath = "material_display";
            MaterialComboBox.SelectedValuePath = "name_material";
        }

        private void LoadOrderServices(int orderId)
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    s.name_service,
                    s.cost_service
                  from Includes i
                  join Service s on i.name_service = s.name_service
                  where i.id_order = @orderId
                  order by s.name_service",
                new NpgsqlParameter("@orderId", orderId)
            );

            foreach (DataRow row in table.Rows)
            {
                SelectedServices.Add(new SelectedServiceItem
                {
                    Name = row["name_service"].ToString() ?? "",
                    Cost = Convert.ToDecimal(row["cost_service"])
                });
            }
        }

        private void LoadOrderMaterials(int orderId)
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    m.name_material,
                    c.quantity_material_contains
                  from Contains c
                  join Material m on c.name_material = m.name_material
                  where c.id_order = @orderId
                  order by m.name_material",
                new NpgsqlParameter("@orderId", orderId)
            );

            foreach (DataRow row in table.Rows)
            {
                SelectedMaterials.Add(new SelectedMaterialItem
                {
                    Name = row["name_material"].ToString() ?? "",
                    Quantity = Convert.ToInt32(row["quantity_material_contains"])
                });
            }
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите услугу.", "Добавление услуги", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)ServiceComboBox.SelectedItem;
            string serviceName = ServiceComboBox.SelectedValue.ToString() ?? "";
            decimal cost = Convert.ToDecimal(selectedRow["cost_service"]);

            if (SelectedServices.Any(s => s.Name == serviceName))
            {
                MessageBox.Show("Эта услуга уже добавлена в заказ.", "Добавление услуги", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedServices.Add(new SelectedServiceItem
            {
                Name = serviceName,
                Cost = cost
            });
        }

        private void RemoveServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServicesListBox.SelectedItem is SelectedServiceItem selectedService)
            {
                SelectedServices.Remove(selectedService);
            }
        }

        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите материал.", "Добавление материала", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MaterialQuantityTextBox.Text.Trim(), out int quantity))
            {
                MessageBox.Show("Количество материала должно быть целым числом.", "Добавление материала", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show("Количество материала должно быть больше нуля.", "Добавление материала", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MaterialComboBox.SelectedItem;
            string materialName = MaterialComboBox.SelectedValue.ToString() ?? "";
            int availableQuantity = Convert.ToInt32(selectedRow["quantity_material"]);

            if (quantity > availableQuantity)
            {
                MessageBox.Show(
                    $"Недостаточно материала на складе. Доступно: {availableQuantity}",
                    "Добавление материала",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            SelectedMaterialItem? existingMaterial = SelectedMaterials.FirstOrDefault(m => m.Name == materialName);

            if (existingMaterial != null)
            {
                existingMaterial.Quantity += quantity;

                SelectedMaterialsListBox.Items.Refresh();
                return;
            }

            SelectedMaterials.Add(new SelectedMaterialItem
            {
                Name = materialName,
                Quantity = quantity
            });
        }

        private void RemoveMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterialsListBox.SelectedItem is SelectedMaterialItem selectedMaterial)
            {
                SelectedMaterials.Remove(selectedMaterial);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministratorComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите администратора.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClientComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MasterComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите мастера.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrderDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заказа.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(TimeFromTextBox.Text.Trim(), out TimeSpan timeFrom))
            {
                MessageBox.Show("Введите корректное время начала, например 10:00.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(TimeBeforeTextBox.Text.Trim(), out TimeSpan timeBefore))
            {
                MessageBox.Show("Введите корректное время окончания, например 11:00.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (timeBefore <= timeFrom)
            {
                MessageBox.Show("Время окончания должно быть позже времени начала.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedServices.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну услугу в заказ.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CompanyNumberAdmin = AdministratorComboBox.SelectedValue.ToString() ?? "";
            ClientId = Convert.ToInt32(ClientComboBox.SelectedValue);
            MasterId = Convert.ToInt32(MasterComboBox.SelectedValue);
            StatusOrder = StatusCheckBox.IsChecked == true;
            OrderDate = OrderDatePicker.SelectedDate.Value.Date;
            TimeFrom = timeFrom;
            TimeBefore = timeBefore;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class SelectedServiceItem
    {
        public string Name { get; set; } = "";
        public decimal Cost { get; set; }

        public override string ToString()
        {
            return $"{Name} — {Cost} руб.";
        }
    }

    public class SelectedMaterialItem
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{Name} — {Quantity} шт.";
            }
        }
    }
}
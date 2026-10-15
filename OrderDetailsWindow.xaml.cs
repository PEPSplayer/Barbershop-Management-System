using HairSalonIS.Data;
using Npgsql;
using System.Data;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly int orderId;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();

            this.orderId = orderId;

            LoadOrderInfo();
            LoadServiceComboBox();
            LoadMaterialComboBox();
            LoadOrderServices();
            LoadOrderMaterials();
        }

        private void LoadOrderInfo()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    o.id_order,
                    c.full_name_client,
                    m.full_name_master,
                    o.date_and_time_order
                  from ""Order"" o
                  join Client c on o.id_client = c.id_client
                  join Master m on o.id_master = m.id_master
                  where o.id_order = @orderId",
                new NpgsqlParameter("@orderId", orderId)
            );

            if (table.Rows.Count > 0)
            {
                DataRow row = table.Rows[0];

                OrderInfoTextBlock.Text =
                    $"Состав заказа №{row["id_order"]} — клиент: {row["full_name_client"]}, мастер: {row["full_name_master"]}";
            }
            else
            {
                OrderInfoTextBlock.Text = $"Состав заказа №{orderId}";
            }
        }

        private void LoadServiceComboBox()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    name_service,
                    name_service || ' — ' || cost_service || ' руб.' as service_display
                  from Service
                  order by name_service"
            );

            ServiceComboBox.ItemsSource = table.DefaultView;
            ServiceComboBox.DisplayMemberPath = "service_display";
            ServiceComboBox.SelectedValuePath = "name_service";
        }

        private void LoadMaterialComboBox()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    name_material,
                    name_material || ' — ' || kategory_material as material_display
                  from Material
                  order by name_material"
            );

            MaterialComboBox.ItemsSource = table.DefaultView;
            MaterialComboBox.DisplayMemberPath = "material_display";
            MaterialComboBox.SelectedValuePath = "name_material";
        }

        private void LoadOrderServices()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
                    s.name_service as ""Название услуги"",
                    s.cost_service as ""Стоимость"",
                    s.kategory_service as ""Категория""
                  from Includes i
                  join Service s on i.name_service = s.name_service
                  where i.id_order = @orderId
                  order by s.name_service",
                new NpgsqlParameter("@orderId", orderId)
            );

            ServicesDataGrid.ItemsSource = table.DefaultView;
        }

        private void LoadOrderMaterials()
        {
            DataTable table = Database.ExecuteQuery(
                @"select 
            m.name_material as ""Название материала"",
            m.kategory_material as ""Категория"",
            c.quantity_material_contains as ""Количество""
          from Contains c
          join Material m on c.name_material = m.name_material
          where c.id_order = @orderId
          order by m.name_material",
                new NpgsqlParameter("@orderId", orderId)
            );

            MaterialsDataGrid.ItemsSource = table.DefaultView;
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите услугу.",
                    "Добавление услуги",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            string serviceName = ServiceComboBox.SelectedValue.ToString() ?? "";

            bool result = Database.ExecuteNonQuery(
                @"insert into Includes (name_service, id_order)
          values (@serviceName, @orderId)",
                new NpgsqlParameter("@serviceName", serviceName),
                new NpgsqlParameter("@orderId", orderId)
            );

            if (result)
            {
                MessageBox.Show(
                    "Услуга добавлена в заказ. Чек пересчитан автоматически.",
                    "Добавление услуги",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadOrderServices();
            }
        }

        private void DeleteServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите услугу для удаления из заказа.",
                    "Удаление услуги",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)ServicesDataGrid.SelectedItem;
            string serviceName = selectedRow["Название услуги"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Удалить услугу из заказа?\n{serviceName}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    @"delete from Includes
                      where id_order = @orderId
                        and name_service = @serviceName",
                    new NpgsqlParameter("@orderId", orderId),
                    new NpgsqlParameter("@serviceName", serviceName)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Услуга удалена из заказа.",
                        "Удаление услуги",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LoadOrderServices();
                }
            }
        }

        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите материал.",
                    "Добавление материала",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            string materialName = MaterialComboBox.SelectedValue.ToString() ?? "";

            bool result = Database.ExecuteNonQuery(
                @"insert into Contains (name_material, id_order, quantity_material_contains)
          values (@materialName, @orderId, 1)",
                new NpgsqlParameter("@materialName", materialName),
                new NpgsqlParameter("@orderId", orderId)
            );

            if (result)
            {
                MessageBox.Show(
                    "Материал добавлен в заказ.",
                    "Добавление материала",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadOrderMaterials();
            }
        }

        private void DeleteMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите материал для удаления из заказа.",
                    "Удаление материала",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)MaterialsDataGrid.SelectedItem;
            string materialName = selectedRow["Название материала"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Удалить материал из заказа?\n{materialName}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    @"delete from Contains
                      where id_order = @orderId
                        and name_material = @materialName",
                    new NpgsqlParameter("@orderId", orderId),
                    new NpgsqlParameter("@materialName", materialName)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Материал удалён из заказа.",
                        "Удаление материала",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LoadOrderMaterials();
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
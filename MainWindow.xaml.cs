using HairSalonIS.Services;
using HairSalonIS.Data;
using HairSalonIS.Views;
using Npgsql;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace HairSalonIS
{
    public partial class MainWindow : Window
    {
        private string currentTable = "";

        public MainWindow()
        {
            InitializeComponent();

            Title = $"Информационная система парикмахерской — {AuthService.CurrentAdminName} ({AuthService.CurrentAdminRole})";

            ApplyRoleRestrictions();
        }

        private void LoadData(string title, string query, string tableName)
        {
            PageTitleTextBlock.Text = title;
            currentTable = tableName;

            DataTable table = Database.ExecuteQuery(query);
            MainDataGrid.ItemsSource = table.DefaultView;
        }

        private void MainDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string columnName = e.PropertyName.ToLower();

            if (columnName.Contains("id_") ||
                columnName == "company_number_admin" ||
                columnName == "number_expense" ||
                columnName == "number_shipment" ||
                columnName == "name_material")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private DateTime ConvertDbDateToDateTime(object value)
        {
            if (value is DateOnly dateOnly)
            {
                return dateOnly.ToDateTime(TimeOnly.MinValue);
            }

            return Convert.ToDateTime(value);
        }
        private TimeSpan ConvertDbTimeToTimeSpan(object value)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan;
            }

            if (value is TimeOnly timeOnly)
            {
                return timeOnly.ToTimeSpan();
            }

            return TimeSpan.Parse(value.ToString() ?? "00:00");
        }
 
        private int? ConvertDbNullableInt(object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return null;
            }

            return Convert.ToInt32(value);
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Клиенты",
                @"select 
                    id_client,
                    full_name_client as ""ФИО клиента"",
                    phone_number_client as ""Телефон""
                  from Client
                  order by full_name_client",
                "Client"
            );
        }

        private void MastersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Мастера",
                @"select 
                    id_master,
                    full_name_master as ""ФИО мастера""
                  from Master
                  order by full_name_master",
                "Master"
            );
        }

        private void ServicesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Услуги",
                @"select 
                    name_service as ""Название услуги"",
                    cost_service as ""Стоимость"",
                    kategory_service as ""Категория""
                  from Service
                  order by name_service",
                "Service"
            );
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Оформление заказов",
                @"select 
            o.id_order,
            o.company_number_admin,
            o.id_client,
            o.id_master,
            o.number_expense,
            a.admin_full_name_admin as ""Администратор"",
            c.full_name_client as ""Клиент"",
            m.full_name_master as ""Мастер"",
            o.date_and_time_order as ""Дата заказа"",
            o.recording_time_from_order as ""Время начала"",
            o.recording_time_before_order as ""Время окончания"",
            coalesce('№' || e.number_expense || ' — ' || e.cheque_expense || ' руб.', 'Чек будет сформирован после выбора услуг') as ""Чек"",
            case 
                when o.status_order = true then 'Выполнен'
                else 'Не выполнен'
            end as ""Статус""
          from ""Order"" o
          join Administrator a on o.company_number_admin = a.company_number_admin
          join Client c on o.id_client = c.id_client
          join Master m on o.id_master = m.id_master
          left join Expense e on o.number_expense = e.number_expense
          order by o.date_and_time_order desc, o.recording_time_from_order desc, o.id_order desc",
                "Order"
            );
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Расписание записей",
                @"select 
                s.id_schedule,
                s.company_number_admin,
                s.id_master,
                a.admin_full_name_admin as ""Администратор"",
                m.full_name_master as ""Мастер"",
                s.date_schedule as ""Дата"",
                s.recording_time_from_schedule as ""Время начала"",
                s.recording_time_before_schedule as ""Время окончания""
            from Schedule s
            join Administrator a on s.company_number_admin = a.company_number_admin
            join Master m on s.id_master = m.id_master
            order by s.date_schedule, s.recording_time_from_schedule",
                "Schedule"
            );
        }

        private void MaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Учёт материалов",
                @"select 
            name_material as ""Название материала"",
            kategory_material as ""Категория"",
            quantity_material as ""Количество""
          from Material
          order by name_material",
                "Material"
            );
        }

        private void SuppliersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Поставщики",
                @"select 
                    id_supplier,
                    contact_supplier as ""Контакт поставщика""
                  from Supplier
                  order by id_supplier",
                "Supplier"
            );
        }

        private void ShipmentsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData(
                "Учёт поставок",
                @"select 
            s.number_shipment,
            s.id_supplier,
            s.name_material,
            sup.contact_supplier as ""Поставщик"",
            s.name_material as ""Материал"",
            s.materials_number_shipment as ""Количество"",
            s.date_shipment as ""Дата поставки""
          from Shipment s
          join Supplier sup on s.id_supplier = sup.id_supplier
          order by s.date_shipment desc, s.number_shipment desc",
                "Shipment"
            );
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            LogService.AddLog(
                "Отчёты",
                $"Пользователь {AuthService.CurrentAdminName} открыл окно отчётов."
            );

            ReportsWindow window = new ReportsWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Введите текст для поиска.", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (currentTable == "Client")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
                        id_client,
                        full_name_client as ""ФИО клиента"",
                        phone_number_client as ""Телефон""
                      from Client
                      where full_name_client ilike @search
                      order by full_name_client",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }
            else if (currentTable == "Master")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
                        id_master,
                        full_name_master as ""ФИО мастера""
                      from Master
                      where full_name_master ilike @search
                      order by full_name_master",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }
            else if (currentTable == "Service")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
                        name_service as ""Название услуги"",
                        cost_service as ""Стоимость"",
                        kategory_service as ""Категория""
                      from Service
                      where name_service ilike @search
                         or kategory_service ilike @search
                      order by name_service",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }
            else if (currentTable == "Material")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
                        name_material as ""Название материала"",
                        kategory_material as ""Категория""
                      from Material
                      where name_material ilike @search
                         or kategory_material ilike @search
                      order by name_material",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }
            else if (currentTable == "Supplier")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
                        id_supplier,
                        contact_supplier as ""Контакт поставщика""
                      from Supplier
                      where contact_supplier ilike @search
                      order by id_supplier",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }
            else if (currentTable == "Shipment")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
                        sh.number_shipment as ""Номер поставки"",
                        sh.id_supplier,
                        sp.contact_supplier as ""Контакт поставщика"",
                        sh.date_shipment as ""Дата поставки"",
                        sh.materials_number_shipment as ""Количество материалов""
                      from Shipment sh
                      join Supplier sp on sh.id_supplier = sp.id_supplier
                      where sp.contact_supplier ilike @search
                         or cast(sh.number_shipment as text) ilike @search
                         or cast(sh.date_shipment as text) ilike @search
                      order by sh.date_shipment desc, sh.number_shipment desc",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }
            else if (currentTable == "Schedule")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
                        s.id_schedule,
                        s.company_number_admin,
                        s.id_master,
                        a.admin_full_name_admin as ""Администратор"",
                        m.full_name_master as ""Мастер"",
                        s.date_schedule as ""Дата"",
                        s.recording_time_from_schedule as ""Время начала"",
                        s.recording_time_before_schedule as ""Время окончания""
                      from Schedule s
                      join Administrator a on s.company_number_admin = a.company_number_admin
                      join Master m on s.id_master = m.id_master
                      where a.admin_full_name_admin ilike @search
                         or m.full_name_master ilike @search
                         or cast(s.date_schedule as text) ilike @search
                      order by s.date_schedule, s.recording_time_from_schedule",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }
            else if (currentTable == "Order")
            {
                DataTable table = Database.ExecuteQuery(
                    @"select 
            o.id_order,
            o.company_number_admin,
            o.id_client,
            o.id_master,
            o.number_expense,
            a.admin_full_name_admin as ""Администратор"",
            c.full_name_client as ""Клиент"",
            m.full_name_master as ""Мастер"",
            coalesce('№' || e.number_expense || ' — ' || e.cheque_expense || ' руб.', 'Без чека') as ""Чек"",
            o.date_and_time_order as ""Дата заказа"",
            case 
                when o.status_order = true then 'Выполнен'
                else 'Не выполнен'
            end as ""Статус""
          from ""Order"" o
          join Administrator a on o.company_number_admin = a.company_number_admin
          join Client c on o.id_client = c.id_client
          join Master m on o.id_master = m.id_master
          left join Expense e on o.number_expense = e.number_expense
          where a.admin_full_name_admin ilike @search
             or c.full_name_client ilike @search
             or m.full_name_master ilike @search
             or cast(o.id_order as text) ilike @search
             or cast(o.date_and_time_order as text) ilike @search
          order by o.date_and_time_order desc, o.id_order desc",
                    new NpgsqlParameter("@search", "%" + searchText + "%")
                );

                MainDataGrid.ItemsSource = table.DefaultView;
            }

            else
            {
                MessageBox.Show("Поиск для этого раздела пока не настроен.", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshCurrentTable();
        }

        private void RefreshCurrentTable()
        {
            if (currentTable == "Client")
                ClientsButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Master")
                MastersButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Service")
                ServicesButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Order")
                OrdersButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Schedule")
                ScheduleButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Material")
                MaterialsButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Supplier")
                SuppliersButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Shipment")
                ShipmentsButton_Click(this, new RoutedEventArgs());
            else if (currentTable == "Report")
                ReportsButton_Click(this, new RoutedEventArgs());
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTable == "Client")
                AddClient(sender, e);
            else if (currentTable == "Master")
                AddMaster(sender, e);
            else if (currentTable == "Service")
                AddService(sender, e);
            else if (currentTable == "Material")
                AddMaterial(sender, e);
            else if (currentTable == "Supplier")
                AddSupplier(sender, e);
            else if (currentTable == "Shipment")
                AddShipment(sender, e);
            else if (currentTable == "Schedule")
                AddSchedule(sender, e);
            else if (currentTable == "Order")
                AddOrder(sender, e);
            else
                MessageBox.Show("Добавление для этого раздела пока не реализовано.", "Добавление", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTable == "Client")
                EditClient(sender, e);
            else if (currentTable == "Master")
                EditMaster(sender, e);
            else if (currentTable == "Service")
                EditService(sender, e);
            else if (currentTable == "Material")
                EditMaterial(sender, e);
            else if (currentTable == "Supplier")
                EditSupplier(sender, e);
            else if (currentTable == "Shipment")
                EditShipment(sender, e);
            else if (currentTable == "Schedule")
                EditSchedule(sender, e);
            else if (currentTable == "Order")
                EditOrder(sender, e);
            else
                MessageBox.Show("Редактирование для этого раздела пока не реализовано.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDeletePermission())
            {
                return;
            }

            if (currentTable == "Client")
                DeleteClient(sender, e);
            else if (currentTable == "Master")
                DeleteMaster(sender, e);
            else if (currentTable == "Service")
                DeleteService(sender, e);
            else if (currentTable == "Material")
                DeleteMaterial(sender, e);
            else if (currentTable == "Supplier")
                DeleteSupplier(sender, e);
            else if (currentTable == "Shipment")
                DeleteShipment(sender, e);
            else if (currentTable == "Schedule")
                DeleteSchedule(sender, e);
            else if (currentTable == "Order")
                DeleteOrder(sender, e);
            else
                MessageBox.Show("Удаление для этого раздела пока не реализовано.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddClient(object sender, RoutedEventArgs e)
        {
            ClientWindow window = new ClientWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"insert into Client (full_name_client, phone_number_client)
                      values (@fullName, @phone)",
                    new NpgsqlParameter("@fullName", window.ClientFullName),
                    new NpgsqlParameter("@phone", window.ClientPhone)
                );

                if (result)
                {
                    MessageBox.Show("Клиент успешно добавлен.", "Добавление", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClientsButton_Click(sender, e);
                }
            }
        }

        private void EditClient(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int clientId = Convert.ToInt32(selectedRow["id_client"]);
            string fullName = selectedRow["ФИО клиента"].ToString() ?? "";
            string phone = selectedRow["Телефон"].ToString() ?? "";

            ClientWindow window = new ClientWindow(fullName, phone);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update Client
                      set full_name_client = @fullName,
                          phone_number_client = @phone
                      where id_client = @id",
                    new NpgsqlParameter("@fullName", window.ClientFullName),
                    new NpgsqlParameter("@phone", window.ClientPhone),
                    new NpgsqlParameter("@id", clientId)
                );

                if (result)
                {
                    MessageBox.Show("Данные клиента успешно изменены.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClientsButton_Click(sender, e);
                }
            }
        }

        private void DeleteClient(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int clientId = Convert.ToInt32(selectedRow["id_client"]);
            string fullName = selectedRow["ФИО клиента"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Вы действительно хотите удалить клиента:\n{fullName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    "delete from Client where id_client = @id",
                    new NpgsqlParameter("@id", clientId)
                );

                if (result)
                {
                    MessageBox.Show("Клиент успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClientsButton_Click(sender, e);
                }
            }
        }

        private void AddMaster(object sender, RoutedEventArgs e)
        {
            MasterWindow window = new MasterWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"insert into Master (full_name_master)
                      values (@fullName)",
                    new NpgsqlParameter("@fullName", window.MasterFullName)
                );

                if (result)
                {
                    MessageBox.Show("Мастер успешно добавлен.", "Добавление", MessageBoxButton.OK, MessageBoxImage.Information);
                    MastersButton_Click(sender, e);
                }
            }
        }

        private void EditMaster(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int masterId = Convert.ToInt32(selectedRow["id_master"]);
            string fullName = selectedRow["ФИО мастера"].ToString() ?? "";

            MasterWindow window = new MasterWindow(fullName);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update Master
                      set full_name_master = @fullName
                      where id_master = @id",
                    new NpgsqlParameter("@fullName", window.MasterFullName),
                    new NpgsqlParameter("@id", masterId)
                );

                if (result)
                {
                    MessageBox.Show("Данные мастера успешно изменены.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                    MastersButton_Click(sender, e);
                }
            }
        }

        private void DeleteMaster(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int masterId = Convert.ToInt32(selectedRow["id_master"]);
            string fullName = selectedRow["ФИО мастера"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Вы действительно хотите удалить мастера:\n{fullName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    "delete from Master where id_master = @id",
                    new NpgsqlParameter("@id", masterId)
                );

                if (result)
                {
                    MessageBox.Show("Мастер успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    MastersButton_Click(sender, e);
                }
            }
        }

        private void AddService(object sender, RoutedEventArgs e)
        {
            ServiceWindow window = new ServiceWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"insert into Service (name_service, cost_service, kategory_service)
                      values (@name, @cost, @category)",
                    new NpgsqlParameter("@name", window.ServiceName),
                    new NpgsqlParameter("@cost", window.ServiceCost),
                    new NpgsqlParameter("@category", window.ServiceCategory)
                );

                if (result)
                {
                    MessageBox.Show("Услуга успешно добавлена.", "Добавление", MessageBoxButton.OK, MessageBoxImage.Information);
                    ServicesButton_Click(sender, e);
                }
            }
        }

        private void EditService(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            string oldName = selectedRow["Название услуги"].ToString() ?? "";
            decimal cost = Convert.ToDecimal(selectedRow["Стоимость"]);
            string category = selectedRow["Категория"].ToString() ?? "";

            ServiceWindow window = new ServiceWindow(oldName, cost, category);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update Service
                      set name_service = @newName,
                          cost_service = @cost,
                          kategory_service = @category
                      where name_service = @oldName",
                    new NpgsqlParameter("@newName", window.ServiceName),
                    new NpgsqlParameter("@cost", window.ServiceCost),
                    new NpgsqlParameter("@category", window.ServiceCategory),
                    new NpgsqlParameter("@oldName", oldName)
                );

                if (result)
                {
                    MessageBox.Show("Данные услуги успешно изменены.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                    ServicesButton_Click(sender, e);
                }
            }
        }

        private void DeleteService(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            string serviceName = selectedRow["Название услуги"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Вы действительно хотите удалить услугу:\n{serviceName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    "delete from Service where name_service = @name",
                    new NpgsqlParameter("@name", serviceName)
                );

                if (result)
                {
                    MessageBox.Show("Услуга успешно удалена.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    ServicesButton_Click(sender, e);
                }
            }
        }

        private void AddMaterial(object sender, RoutedEventArgs e)
        {
            MaterialWindow window = new MaterialWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"insert into Material
              (name_material, kategory_material, quantity_material)
              values (@name, @category, @quantity)",
                    new NpgsqlParameter("@name", window.MaterialName),
                    new NpgsqlParameter("@category", window.MaterialCategory),
                    new NpgsqlParameter("@quantity", window.MaterialQuantity)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Материал успешно добавлен.",
                        "Добавление",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    MaterialsButton_Click(sender, e);
                }
            }
        }

        private void EditMaterial(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите материал для редактирования.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            string oldMaterialName = selectedRow["Название материала"].ToString() ?? "";
            string category = selectedRow["Категория"].ToString() ?? "";
            int quantity = Convert.ToInt32(selectedRow["Количество"]);

            MaterialWindow window = new MaterialWindow(oldMaterialName, category, quantity);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update Material
              set kategory_material = @category,
                  quantity_material = @quantity
              where name_material = @oldName",
                    new NpgsqlParameter("@category", window.MaterialCategory),
                    new NpgsqlParameter("@quantity", window.MaterialQuantity),
                    new NpgsqlParameter("@oldName", oldMaterialName)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Данные материала успешно изменены.",
                        "Редактирование",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    MaterialsButton_Click(sender, e);
                }
            }
        }

        private void DeleteMaterial(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите материал для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            string materialName = selectedRow["Название материала"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Вы действительно хотите удалить материал:\n{materialName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    "delete from Material where name_material = @name",
                    new NpgsqlParameter("@name", materialName)
                );

                if (result)
                {
                    MessageBox.Show("Материал успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    MaterialsButton_Click(sender, e);
                }
            }
        }

        private void AddSupplier(object sender, RoutedEventArgs e)
        {
            SupplierWindow window = new SupplierWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"insert into Supplier (contact_supplier)
                      values (@contact)",
                    new NpgsqlParameter("@contact", window.SupplierContact)
                );

                if (result)
                {
                    MessageBox.Show("Поставщик успешно добавлен.", "Добавление", MessageBoxButton.OK, MessageBoxImage.Information);
                    SuppliersButton_Click(sender, e);
                }
            }
        }

        private void EditSupplier(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int supplierId = Convert.ToInt32(selectedRow["id_supplier"]);
            string contact = selectedRow["Контакт поставщика"].ToString() ?? "";

            SupplierWindow window = new SupplierWindow(contact);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update Supplier
                      set contact_supplier = @contact
                      where id_supplier = @id",
                    new NpgsqlParameter("@contact", window.SupplierContact),
                    new NpgsqlParameter("@id", supplierId)
                );

                if (result)
                {
                    MessageBox.Show("Данные поставщика успешно изменены.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                    SuppliersButton_Click(sender, e);
                }
            }
        }

        private void DeleteSupplier(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int supplierId = Convert.ToInt32(selectedRow["id_supplier"]);
            string contact = selectedRow["Контакт поставщика"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Вы действительно хотите удалить поставщика с контактом:\n{contact}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    "delete from Supplier where id_supplier = @id",
                    new NpgsqlParameter("@id", supplierId)
                );

                if (result)
                {
                    MessageBox.Show("Поставщик успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    SuppliersButton_Click(sender, e);
                }
            }
        }

        private void AddShipment(object sender, RoutedEventArgs e)
        {
            ShipmentWindow window = new ShipmentWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"insert into Shipment
              (id_supplier, name_material, date_shipment, materials_number_shipment)
              values (@supplierId, @materialName, @date, @materialsNumber)",
                    new NpgsqlParameter("@supplierId", window.SupplierId),
                    new NpgsqlParameter("@materialName", window.MaterialName),
                    new NpgsqlParameter("@date", window.ShipmentDate),
                    new NpgsqlParameter("@materialsNumber", window.MaterialsNumber)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Поставка успешно добавлена. Количество материала на складе обновлено автоматически.",
                        "Добавление",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    ShipmentsButton_Click(sender, e);
                }
            }
        }

        private void EditShipment(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите поставку для редактирования.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int shipmentNumber = Convert.ToInt32(selectedRow["number_shipment"]);
            int supplierId = Convert.ToInt32(selectedRow["id_supplier"]);
            string materialName = selectedRow["name_material"].ToString() ?? "";
            DateTime shipmentDate = ConvertDbDateToDateTime(selectedRow["Дата поставки"]);
            int materialsNumber = Convert.ToInt32(selectedRow["Количество"]);

            ShipmentWindow window = new ShipmentWindow(
                supplierId,
                materialName,
                shipmentDate,
                materialsNumber
            );

            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update Shipment
              set id_supplier = @supplierId,
                  name_material = @materialName,
                  date_shipment = @date,
                  materials_number_shipment = @materialsNumber
              where number_shipment = @shipmentNumber",
                    new NpgsqlParameter("@supplierId", window.SupplierId),
                    new NpgsqlParameter("@materialName", window.MaterialName),
                    new NpgsqlParameter("@date", window.ShipmentDate),
                    new NpgsqlParameter("@materialsNumber", window.MaterialsNumber),
                    new NpgsqlParameter("@shipmentNumber", shipmentNumber)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Поставка успешно изменена. Количество материала на складе пересчитано автоматически.",
                        "Редактирование",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    ShipmentsButton_Click(sender, e);
                }
            }
        }

        private void DeleteShipment(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите поставку для удаления.",
                    "Удаление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int shipmentNumber = Convert.ToInt32(selectedRow["number_shipment"]);
            string materialName = selectedRow["Материал"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Удалить поставку материала?\n{materialName}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    @"delete from Shipment
              where number_shipment = @shipmentNumber",
                    new NpgsqlParameter("@shipmentNumber", shipmentNumber)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Поставка удалена. Количество материала на складе пересчитано автоматически.",
                        "Удаление",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    ShipmentsButton_Click(sender, e);
                }
            }
        }

        private void AddSchedule(object sender, RoutedEventArgs e)
        {
            ScheduleWindow window = new ScheduleWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"insert into Schedule 
              (recording_time_from_schedule, recording_time_before_schedule, date_schedule, company_number_admin, id_master)
              values (@timeFrom, @timeBefore, @date, @adminNumber, @masterId)",
                    new NpgsqlParameter("@timeFrom", window.TimeFrom),
                    new NpgsqlParameter("@timeBefore", window.TimeBefore),
                    new NpgsqlParameter("@date", window.ScheduleDate),
                    new NpgsqlParameter("@adminNumber", window.CompanyNumberAdmin),
                    new NpgsqlParameter("@masterId", window.MasterId)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Запись в расписании успешно добавлена.",
                        "Добавление",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    ScheduleButton_Click(sender, e);
                }
            }
        }

        private void EditSchedule(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись расписания для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int scheduleId = Convert.ToInt32(selectedRow["id_schedule"]);
            string companyNumberAdmin = selectedRow["company_number_admin"].ToString() ?? "";
            int masterId = Convert.ToInt32(selectedRow["id_master"]);
            DateTime scheduleDate = ConvertDbDateToDateTime(selectedRow["Дата"]);
            TimeSpan timeFrom = ConvertDbTimeToTimeSpan(selectedRow["Время начала"]);
            TimeSpan timeBefore = ConvertDbTimeToTimeSpan(selectedRow["Время окончания"]);

            ScheduleWindow window = new ScheduleWindow(companyNumberAdmin, masterId, scheduleDate, timeFrom, timeBefore);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update Schedule
                      set recording_time_from_schedule = @timeFrom,
                          recording_time_before_schedule = @timeBefore,
                          date_schedule = @date,
                          company_number_admin = @adminNumber,
                          id_master = @masterId
                      where id_schedule = @scheduleId",
                    new NpgsqlParameter("@timeFrom", window.TimeFrom),
                    new NpgsqlParameter("@timeBefore", window.TimeBefore),
                    new NpgsqlParameter("@date", window.ScheduleDate),
                    new NpgsqlParameter("@adminNumber", window.CompanyNumberAdmin),
                    new NpgsqlParameter("@masterId", window.MasterId),
                    new NpgsqlParameter("@scheduleId", scheduleId)
                );

                if (result)
                {
                    MessageBox.Show("Запись расписания успешно изменена.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                    ScheduleButton_Click(sender, e);
                }
            }
        }

        private void DeleteSchedule(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись расписания для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int scheduleId = Convert.ToInt32(selectedRow["id_schedule"]);
            string masterName = selectedRow["Мастер"].ToString() ?? "";
            string dateText = selectedRow["Дата"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Вы действительно хотите удалить запись расписания?\nМастер: {masterName}\nДата: {dateText}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    "delete from Schedule where id_schedule = @scheduleId",
                    new NpgsqlParameter("@scheduleId", scheduleId)
                );

                if (result)
                {
                    MessageBox.Show("Запись расписания успешно удалена.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    ScheduleButton_Click(sender, e);
                }
            }
        }
        private void AddOrder(object sender, RoutedEventArgs e)
        {
            OrderWindow window = new OrderWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                DataTable orderTable = Database.ExecuteQuery(
                    @"insert into ""Order""
              (company_number_admin, id_master, id_client, number_expense, status_order, date_and_time_order,
               recording_time_from_order, recording_time_before_order)
              values (@adminNumber, @masterId, @clientId, null, @status, @date, @timeFrom, @timeBefore)
              returning id_order",
                    new NpgsqlParameter("@adminNumber", window.CompanyNumberAdmin),
                    new NpgsqlParameter("@masterId", window.MasterId),
                    new NpgsqlParameter("@clientId", window.ClientId),
                    new NpgsqlParameter("@status", window.StatusOrder),
                    new NpgsqlParameter("@date", window.OrderDate),
                    new NpgsqlParameter("@timeFrom", window.TimeFrom),
                    new NpgsqlParameter("@timeBefore", window.TimeBefore)
                );

                if (orderTable.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "Не удалось получить номер созданного заказа.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return;
                }

                int orderId = Convert.ToInt32(orderTable.Rows[0]["id_order"]);

                foreach (SelectedServiceItem service in window.SelectedServices)
                {
                    Database.ExecuteNonQuery(
                        @"insert into Includes (name_service, id_order)
                  values (@serviceName, @orderId)",
                        new NpgsqlParameter("@serviceName", service.Name),
                        new NpgsqlParameter("@orderId", orderId)
                    );
                }

                foreach (SelectedMaterialItem material in window.SelectedMaterials)
                {
                    Database.ExecuteNonQuery(
                        @"insert into Contains (name_material, id_order, quantity_material_contains)
                  values (@materialName, @orderId, @quantity)",
                        new NpgsqlParameter("@materialName", material.Name),
                        new NpgsqlParameter("@orderId", orderId),
                        new NpgsqlParameter("@quantity", material.Quantity)
                    );
                }

                MessageBox.Show(
                    "Заказ успешно добавлен. Чек сформирован автоматически.",
                    "Добавление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LogService.AddLog(
                    "Добавление",
                    $"Пользователь {AuthService.CurrentAdminName} добавил новый заказ №{orderId}."
                );

                OrdersButton_Click(sender, e);
            }
        }

        private void EditOrder(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите заказ для редактирования.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int orderId = Convert.ToInt32(selectedRow["id_order"]);
            string companyNumberAdmin = selectedRow["company_number_admin"].ToString() ?? "";
            int clientId = Convert.ToInt32(selectedRow["id_client"]);
            int masterId = Convert.ToInt32(selectedRow["id_master"]);
            bool statusOrder = selectedRow["Статус"].ToString() == "Выполнен";
            DateTime orderDate = ConvertDbDateToDateTime(selectedRow["Дата заказа"]);
            TimeSpan timeFrom = ConvertDbTimeToTimeSpan(selectedRow["Время начала"]);
            TimeSpan timeBefore = ConvertDbTimeToTimeSpan(selectedRow["Время окончания"]);

            OrderWindow window = new OrderWindow(
                orderId,
                companyNumberAdmin,
                clientId,
                masterId,
                statusOrder,
                orderDate,
                timeFrom,
                timeBefore
            );

            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                bool result = Database.ExecuteNonQuery(
                    @"update ""Order""
              set company_number_admin = @adminNumber,
                  id_master = @masterId,
                  id_client = @clientId,
                  status_order = @status,
                  date_and_time_order = @date,
                  recording_time_from_order = @timeFrom,
                  recording_time_before_order = @timeBefore
              where id_order = @orderId",
                    new NpgsqlParameter("@adminNumber", window.CompanyNumberAdmin),
                    new NpgsqlParameter("@masterId", window.MasterId),
                    new NpgsqlParameter("@clientId", window.ClientId),
                    new NpgsqlParameter("@status", window.StatusOrder),
                    new NpgsqlParameter("@date", window.OrderDate),
                    new NpgsqlParameter("@timeFrom", window.TimeFrom),
                    new NpgsqlParameter("@timeBefore", window.TimeBefore),
                    new NpgsqlParameter("@orderId", orderId)
                );

                if (result)
                {
                    Database.ExecuteNonQuery(
                        @"delete from Includes where id_order = @orderId",
                        new NpgsqlParameter("@orderId", orderId)
                    );

                    Database.ExecuteNonQuery(
                        @"delete from Contains where id_order = @orderId",
                        new NpgsqlParameter("@orderId", orderId)
                    );

                    foreach (SelectedServiceItem service in window.SelectedServices)
                    {
                        Database.ExecuteNonQuery(
                            @"insert into Includes (name_service, id_order)
                      values (@serviceName, @orderId)",
                            new NpgsqlParameter("@serviceName", service.Name),
                            new NpgsqlParameter("@orderId", orderId)
                        );
                    }

                    foreach (SelectedMaterialItem material in window.SelectedMaterials)
                    {
                        Database.ExecuteNonQuery(
                            @"insert into Contains (name_material, id_order, quantity_material_contains)
                      values (@materialName, @orderId, @quantity)",
                            new NpgsqlParameter("@materialName", material.Name),
                            new NpgsqlParameter("@orderId", orderId),
                            new NpgsqlParameter("@quantity", material.Quantity)
                        );
                    }

                    MessageBox.Show(
                        "Данные заказа успешно изменены. Чек пересчитан автоматически.",
                        "Редактирование",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LogService.AddLog(
                        "Редактирование",
                        $"Пользователь {AuthService.CurrentAdminName} изменил заказ №{orderId}."
                    );

                    OrdersButton_Click(sender, e);
                }
            }
        }

        private void DeleteOrder(object sender, RoutedEventArgs e)
        {
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите заказ для удаления.",
                    "Удаление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int orderId = Convert.ToInt32(selectedRow["id_order"]);
            string clientName = selectedRow["Клиент"].ToString() ?? "";
            string masterName = selectedRow["Мастер"].ToString() ?? "";

            MessageBoxResult answer = MessageBox.Show(
                $"Вы действительно хотите удалить заказ №{orderId}?\nКлиент: {clientName}\nМастер: {masterName}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                bool result = Database.ExecuteNonQuery(
                    @"delete from Includes where id_order = @orderId;
              delete from Contains where id_order = @orderId;
              delete from ""Order"" where id_order = @orderId;",
                    new NpgsqlParameter("@orderId", orderId)
                );

                if (result)
                {
                    MessageBox.Show(
                        "Заказ успешно удалён.",
                        "Удаление",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LogService.AddLog(
                        "Удаление",
                        $"Пользователь {AuthService.CurrentAdminName} удалил заказ №{orderId}. Заказ автоматически перенесён в архив."
                    );

                    OrdersButton_Click(sender, e);
                }
            }
        }
        private void OrderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTable != "Order")
            {
                MessageBox.Show(
                    "Состав заказа можно открыть только в разделе Заказы.",
                    "Состав заказа",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите заказ, для которого нужно открыть состав.",
                    "Состав заказа",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            DataRowView selectedRow = (DataRowView)MainDataGrid.SelectedItem;

            int orderId = Convert.ToInt32(selectedRow["id_order"]);

            OrderDetailsWindow window = new OrderDetailsWindow(orderId);
            window.Owner = this;
            window.ShowDialog();

            OrdersButton_Click(sender, e);
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Выход",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (answer == MessageBoxResult.Yes)
            {
                LogService.AddLog(
                    "Выход",
                    $"Пользователь {AuthService.CurrentAdminName} вышел из системы."
                );

                AuthService.Logout();

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Close();
            }
        }
        private void OrderArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckArchivePermission())
            {
                return;
            }

            LogService.AddLog(
                "Архив",
                $"Пользователь {AuthService.CurrentAdminName} открыл архив удалённых заказов."
            );

            OrderArchiveWindow window = new OrderArchiveWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ApplyRoleRestrictions()
        {
            if (AuthService.CurrentAdminRole == "manager")
            {
                MessageBox.Show(
                    "Вы вошли как менеджер. Удаление данных и просмотр архива заказов будут недоступны.",
                    "Ограниченный доступ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private bool IsAdmin()
        {
            return AuthService.CurrentAdminRole == "admin";
        }

        private bool CheckDeletePermission()
        {
            if (!IsAdmin())
            {
                MessageBox.Show(
                    "У вас нет прав на удаление данных. Удаление доступно только администратору.",
                    "Недостаточно прав",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            return true;
        }

        private bool CheckArchivePermission()
        {
            if (!IsAdmin())
            {
                MessageBox.Show(
                    "У вас нет прав на просмотр архива заказов. Архив доступен только администратору.",
                    "Недостаточно прав",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            return true;
        }
        private bool CheckLogPermission()
        {
            if (!IsAdmin())
            {
                MessageBox.Show(
                    "У вас нет прав на просмотр журнала действий. Журнал доступен только администратору.",
                    "Недостаточно прав",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            return true;
        }
        private void ActionLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckLogPermission())
            {
                return;
            }

            ActionLogWindow window = new ActionLogWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
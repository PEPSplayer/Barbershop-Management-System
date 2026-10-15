using HairSalonIS.Data;
using Npgsql;
using System.Data;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class OrderArchiveWindow : Window
    {
        public OrderArchiveWindow()
        {
            InitializeComponent();

            LoadArchive();
        }

        private void LoadArchive()
        {
            DataTable table = Database.ExecuteQuery(
                @"select
                    oa.archive_id as ""Номер архива"",
                    oa.id_order as ""Номер заказа"",
                    oa.date_and_time_order as ""Дата заказа"",
                    oa.deleted_at as ""Дата удаления"",

                    coalesce(a.admin_full_name_admin, 'Администратор удалён') as ""Администратор"",
                    coalesce(c.full_name_client, 'Клиент удалён') as ""Клиент"",
                    coalesce(m.full_name_master, 'Мастер удалён') as ""Мастер"",

                    case
                        when oa.status_order = true then 'Выполнен'
                        else 'Не выполнен'
                    end as ""Статус"",

                    coalesce(oa.number_expense::text, 'Без чека') as ""Номер чека""

                  from order_archive oa
                  left join Administrator a on oa.company_number_admin = a.company_number_admin
                  left join Client c on oa.id_client = c.id_client
                  left join Master m on oa.id_master = m.id_master
                  order by oa.deleted_at desc"
            );

            ArchiveDataGrid.ItemsSource = table.DefaultView;

            InfoTextBlock.Text = $"Количество записей в архиве: {table.Rows.Count}";
        }

        private void SearchArchive(string searchText)
        {
            DataTable table = Database.ExecuteQuery(
                @"select
                    oa.archive_id as ""Номер архива"",
                    oa.id_order as ""Номер заказа"",
                    oa.date_and_time_order as ""Дата заказа"",
                    oa.deleted_at as ""Дата удаления"",

                    coalesce(a.admin_full_name_admin, 'Администратор удалён') as ""Администратор"",
                    coalesce(c.full_name_client, 'Клиент удалён') as ""Клиент"",
                    coalesce(m.full_name_master, 'Мастер удалён') as ""Мастер"",

                    case
                        when oa.status_order = true then 'Выполнен'
                        else 'Не выполнен'
                    end as ""Статус"",

                    coalesce(oa.number_expense::text, 'Без чека') as ""Номер чека""

                  from order_archive oa
                  left join Administrator a on oa.company_number_admin = a.company_number_admin
                  left join Client c on oa.id_client = c.id_client
                  left join Master m on oa.id_master = m.id_master

                  where cast(oa.id_order as text) ilike @search
                     or cast(oa.archive_id as text) ilike @search
                     or cast(oa.date_and_time_order as text) ilike @search
                     or cast(oa.deleted_at as text) ilike @search
                     or a.admin_full_name_admin ilike @search
                     or c.full_name_client ilike @search
                     or m.full_name_master ilike @search

                  order by oa.deleted_at desc",
                new NpgsqlParameter("@search", "%" + searchText + "%")
            );

            ArchiveDataGrid.ItemsSource = table.DefaultView;

            InfoTextBlock.Text = $"Найдено записей: {table.Rows.Count}";
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show(
                    "Введите текст для поиска.",
                    "Поиск",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            SearchArchive(searchText);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            LoadArchive();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
using HairSalonIS.Data;
using Npgsql;
using System.Data;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class ActionLogWindow : Window
    {
        public ActionLogWindow()
        {
            InitializeComponent();

            LoadLogs();
        }

        private void LoadLogs()
        {
            DataTable table = Database.ExecuteQuery(
                @"select
                    log_id as ""Номер"",
                    admin_full_name as ""Пользователь"",
                    role_admin as ""Роль"",
                    action_type as ""Тип действия"",
                    action_description as ""Описание"",
                    action_date as ""Дата и время""
                  from user_action_log
                  order by action_date desc, log_id desc"
            );

            LogDataGrid.ItemsSource = table.DefaultView;
            InfoTextBlock.Text = $"Количество записей в журнале: {table.Rows.Count}";
        }

        private void SearchLogs(string searchText)
        {
            DataTable table = Database.ExecuteQuery(
                @"select
                    log_id as ""Номер"",
                    admin_full_name as ""Пользователь"",
                    role_admin as ""Роль"",
                    action_type as ""Тип действия"",
                    action_description as ""Описание"",
                    action_date as ""Дата и время""
                  from user_action_log
                  where admin_full_name ilike @search
                     or role_admin ilike @search
                     or action_type ilike @search
                     or action_description ilike @search
                     or cast(action_date as text) ilike @search
                  order by action_date desc, log_id desc",
                new NpgsqlParameter("@search", "%" + searchText + "%")
            );

            LogDataGrid.ItemsSource = table.DefaultView;
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

            SearchLogs(searchText);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            LoadLogs();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
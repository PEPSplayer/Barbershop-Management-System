using HairSalonIS.Data;
using System;
using System.Data;
using System.Globalization;
using System.Windows;

namespace HairSalonIS.Views
{
    public partial class ScheduleWindow : Window
    {
        public string CompanyNumberAdmin { get; private set; } = "";
        public int MasterId { get; private set; }
        public DateTime ScheduleDate { get; private set; }
        public TimeSpan TimeFrom { get; private set; }
        public TimeSpan TimeBefore { get; private set; }

        public ScheduleWindow()
        {
            InitializeComponent();

            LoadAdministrators();
            LoadMasters();

            ScheduleDatePicker.SelectedDate = DateTime.Today;
        }

        public ScheduleWindow(
            string companyNumberAdmin,
            int masterId,
            DateTime scheduleDate,
            TimeSpan timeFrom,
            TimeSpan timeBefore)
        {
            InitializeComponent();

            LoadAdministrators();
            LoadMasters();

            AdministratorComboBox.SelectedValue = companyNumberAdmin;
            MasterComboBox.SelectedValue = masterId;
            ScheduleDatePicker.SelectedDate = scheduleDate;
            TimeFromTextBox.Text = timeFrom.ToString(@"hh\:mm");
            TimeBeforeTextBox.Text = timeBefore.ToString(@"hh\:mm");
        }

        private void LoadAdministrators()
        {
            DataTable administratorsTable = Database.ExecuteQuery(
                @"select 
                    company_number_admin,
                    admin_full_name_admin as ""ФИО администратора""
                  from Administrator
                  order by admin_full_name_admin"
            );

            AdministratorComboBox.ItemsSource = administratorsTable.DefaultView;
            AdministratorComboBox.DisplayMemberPath = "ФИО администратора";
            AdministratorComboBox.SelectedValuePath = "company_number_admin";
        }

        private void LoadMasters()
        {
            DataTable mastersTable = Database.ExecuteQuery(
                @"select 
                    id_master,
                    full_name_master as ""ФИО мастера""
                  from Master
                  order by full_name_master"
            );

            MasterComboBox.ItemsSource = mastersTable.DefaultView;
            MasterComboBox.DisplayMemberPath = "ФИО мастера";
            MasterComboBox.SelectedValuePath = "id_master";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministratorComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите администратора.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (MasterComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите мастера.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (ScheduleDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите дату.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            string timeFromText = TimeFromTextBox.Text.Trim();
            string timeBeforeText = TimeBeforeTextBox.Text.Trim();

            if (!TimeSpan.TryParseExact(timeFromText, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan timeFrom))
            {
                MessageBox.Show(
                    "Введите время начала в формате ЧЧ:ММ. Например: 10:00",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (!TimeSpan.TryParseExact(timeBeforeText, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan timeBefore))
            {
                MessageBox.Show(
                    "Введите время окончания в формате ЧЧ:ММ. Например: 11:30",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (timeBefore <= timeFrom)
            {
                MessageBox.Show(
                    "Время окончания должно быть позже времени начала.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            CompanyNumberAdmin = AdministratorComboBox.SelectedValue.ToString() ?? "";
            MasterId = Convert.ToInt32(MasterComboBox.SelectedValue);
            ScheduleDate = ScheduleDatePicker.SelectedDate.Value;
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
}
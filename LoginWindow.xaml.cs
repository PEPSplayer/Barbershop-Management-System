using HairSalonIS.Services;
using System.Windows;
using System.Windows.Input;

namespace HairSalonIS.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            LoginTextBox.Focus();

            KeyDown += LoginWindow_KeyDown;
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void TryLogin()
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            ErrorTextBlock.Text = "";

            if (string.IsNullOrWhiteSpace(login))
            {
                ErrorTextBlock.Text = "Введите логин.";
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorTextBlock.Text = "Введите пароль.";
                PasswordBox.Focus();
                return;
            }

            bool result = AuthService.Login(login, password);

            if (!result)
            {
                ErrorTextBlock.Text = "Неверный логин или пароль.";
                PasswordBox.Clear();
                PasswordBox.Focus();
                return;
            }

            LogService.AddLog(
                "Вход",
                $"Пользователь {AuthService.CurrentAdminName} выполнил вход в систему."
            );

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
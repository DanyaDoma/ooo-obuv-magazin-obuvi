using System.Windows;

using ShoeStore.Core;
using ShoeStore.Core.Models;
using ShoeStore.Core.Security;

namespace ShoeStore.App.Views;

public partial class LoginWindow : Window
{
    private readonly AuthService _authService;

    public UserInfo? CurrentUser { get; private set; }

    public LoginWindow(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        Loaded += (_, _) => LoginBox.Focus();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        SetBusy(true);

        try
        {
            var login = LoginBox.Text;
            var password = PasswordBox.Password;
            CurrentUser = await Task.Run(() => _authService.Authenticate(login, password));
            DialogResult = true;
        }
        catch (DomainException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось подключиться к базе данных. {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void Guest_Click(object sender, RoutedEventArgs e)
    {
        CurrentUser = null;
        DialogResult = true;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        LoginButton.IsEnabled = !busy;
        LoginBox.IsEnabled = !busy;
        PasswordBox.IsEnabled = !busy;
        LoginButton.Content = busy ? "Проверка…" : "Войти";
    }
}

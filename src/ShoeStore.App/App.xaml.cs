using System.Globalization;
using System.Windows;
using System.Windows.Markup;

using ShoeStore.App.Views;
using ShoeStore.Core.Data;
using ShoeStore.Core.Security;
using ShoeStore.Core.Services;

namespace ShoeStore.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("ru-RU")));

        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            var database = new Database(AppConfiguration.LoadConnectionString());
            var login = new LoginWindow(new AuthService(database));
            if (login.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var main = new MainWindow(
                login.CurrentUser,
                new UsersService(database),
                new ProductsService(database),
                new OrdersService(database),
                new ReferencesService(database));

            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка запуска",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }
}

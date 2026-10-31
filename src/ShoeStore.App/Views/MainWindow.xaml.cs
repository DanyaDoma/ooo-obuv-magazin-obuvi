using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ShoeStore.Core.Models;
using ShoeStore.Core.Security;
using ShoeStore.Core.Services;

namespace ShoeStore.App.Views;

public partial class MainWindow : Window
{
    private readonly UserInfo? _user;
    private readonly UsersService _usersService;
    private readonly ProductsService _productsService;
    private readonly OrdersService _ordersService;
    private readonly ReferencesService _referencesService;

    private References? _references;
    private bool _userFiltersReady;
    private bool _productFiltersReady;
    private int _usersCount;
    private int _productsCount;
    private int _ordersCount;

    public MainWindow(
        UserInfo? user,
        UsersService usersService,
        ProductsService productsService,
        OrdersService ordersService,
        ReferencesService referencesService)
    {
        InitializeComponent();
        _user = user;
        _usersService = usersService;
        _productsService = productsService;
        _ordersService = ordersService;
        _referencesService = referencesService;
        ConfigureRole();
    }

    private string? RoleCode => _user?.Role.Code;

    private bool CanFilterProducts => RolePermissions.CanUseProductFilters(RoleCode);

    private void ConfigureRole()
    {
        UserCaption.Text = _user is null
            ? "Гость"
            : _user.Department is null
                ? $"{_user.FullName} · {_user.Role.Name}"
                : $"{_user.FullName} · {_user.Role.Name} · {_user.Department.Name}";

        if (_user is null)
        {
            UsersTab.Visibility = Visibility.Collapsed;
            MainTabs.SelectedItem = ProductsTab;
        }

        if (!RolePermissions.CanViewOrders(RoleCode))
        {
            OrdersTab.Visibility = Visibility.Collapsed;
        }

        if (!CanFilterProducts)
        {
            ProductsFilterPanel.Visibility = Visibility.Collapsed;
        }

        if (!RolePermissions.CanManageProducts(RoleCode))
        {
            ProductActions.Visibility = Visibility.Collapsed;
        }

        if (!RolePermissions.CanManageOrders(RoleCode))
        {
            OrderActions.Visibility = Visibility.Collapsed;
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SetStatus("Загрузка данных…");
            _references = await Task.Run(() => _referencesService.Load());
            FillFilters();

            if (_user is not null)
            {
                await LoadUsersAsync();
            }

            await LoadProductsAsync();

            if (RolePermissions.CanViewOrders(RoleCode))
            {
                await LoadOrdersAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось загрузить данные. {ex.Message}");
        }
    }

    private void FillFilters()
    {
        var references = _references!;

        RoleFilter.ItemsSource = WithAll(
            references.Roles.Select(role => new LookupItem(role.Id, role.Name)),
            "Все роли");
        RoleFilter.SelectedIndex = 0;

        DepartmentFilter.ItemsSource = WithAll(references.Departments, "Все отделы");
        DepartmentFilter.SelectedIndex = 0;

        CategoryFilter.ItemsSource = WithAll(references.Categories, "Все категории");
        CategoryFilter.SelectedIndex = 0;

        SupplierFilter.ItemsSource = WithAll(references.Suppliers, "Все поставщики");
        SupplierFilter.SelectedIndex = 0;

        ManufacturerFilter.ItemsSource = WithAll(references.Manufacturers, "Все производители");
        ManufacturerFilter.SelectedIndex = 0;

        SortFilter.ItemsSource = new List<SortOption>
        {
            new("name_asc", "Название (А–Я)"),
            new("name_desc", "Название (Я–А)"),
            new("price_asc", "Цена (возрастание)"),
            new("price_desc", "Цена (убывание)"),
            new("discount_desc", "Скидка (убывание)"),
            new("stock_desc", "Остаток (убывание)")
        };
        SortFilter.SelectedIndex = 0;

        _userFiltersReady = true;
        _productFiltersReady = true;
    }

    private static List<LookupItem> WithAll(IEnumerable<LookupItem> items, string allLabel)
    {
        var list = new List<LookupItem> { new(0, allLabel) };
        list.AddRange(items);
        return list;
    }

    private async Task LoadUsersAsync()
    {
        var filter = new UserFilter(
            UserSearchBox.Text,
            SelectedId(RoleFilter),
            SelectedId(DepartmentFilter));
        var items = await Task.Run(() => _usersService.List(filter));
        UsersGrid.ItemsSource = items;
        _usersCount = items.Count;
        UpdateStatusForCurrentTab();
    }

    private async Task LoadProductsAsync()
    {
        var filter = new ProductFilter(
            CanFilterProducts ? ProductSearchBox.Text : null,
            CanFilterProducts ? SelectedId(CategoryFilter) : null,
            CanFilterProducts ? SelectedId(SupplierFilter) : null,
            CanFilterProducts ? SelectedId(ManufacturerFilter) : null,
            CanFilterProducts && DiscountOnlyCheck.IsChecked == true,
            CanFilterProducts ? (SortFilter.SelectedItem as SortOption)?.Key : null);
        var items = await Task.Run(() => _productsService.List(filter));
        ProductsGrid.ItemsSource = items;
        _productsCount = items.Count;
        UpdateStatusForCurrentTab();
    }

    private async Task LoadOrdersAsync()
    {
        var items = await Task.Run(() => _ordersService.List());
        OrdersGrid.ItemsSource = items;
        OrderItemsGrid.ItemsSource = null;
        _ordersCount = items.Count;
        UpdateStatusForCurrentTab();
    }

    private static int? SelectedId(ComboBox combo) =>
        combo.SelectedItem is LookupItem item && item.Id > 0 ? item.Id : null;

    private async void UserSearch_Click(object sender, RoutedEventArgs e) =>
        await RunAsync(LoadUsersAsync);

    private async void UserSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await RunAsync(LoadUsersAsync);
        }
    }

    private async void UserFilters_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_userFiltersReady)
        {
            await RunAsync(LoadUsersAsync);
        }
    }

    private async void ProductFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (_productFiltersReady)
        {
            await RunAsync(LoadProductsAsync);
        }
    }

    private async void ProductSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await RunAsync(LoadProductsAsync);
        }
    }

    private async void ProductFilterApply_Click(object sender, RoutedEventArgs e) =>
        await RunAsync(LoadProductsAsync);

    private async void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        if (_references is null)
        {
            return;
        }

        var window = new ProductEditWindow(_productsService, _references, null) { Owner = this };
        if (window.ShowDialog() == true)
        {
            await RunAsync(LoadProductsAsync);
        }
    }

    private async void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if (_references is null)
        {
            return;
        }

        if (ProductsGrid.SelectedItem is not Product product)
        {
            ShowError("Выберите товар для редактирования.");
            return;
        }

        var window = new ProductEditWindow(_productsService, _references, product) { Owner = this };
        if (window.ShowDialog() == true)
        {
            await RunAsync(LoadProductsAsync);
        }
    }

    private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product product)
        {
            ShowError("Выберите товар для удаления.");
            return;
        }

        var answer = MessageBox.Show(
            $"Удалить товар «{product.Name}»?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await Task.Run(() => _productsService.Delete(product.Id));
            await LoadProductsAsync();
            SetStatus("Товар удалён.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OrdersGrid.SelectedItem is not OrderSummary summary)
        {
            OrderItemsGrid.ItemsSource = null;
            return;
        }

        try
        {
            var details = await Task.Run(() => _ordersService.Get(summary.Id));
            if (OrdersGrid.SelectedItem is OrderSummary current && current.Id == details.Summary.Id)
            {
                OrderItemsGrid.ItemsSource = details.Items;
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void AddOrder_Click(object sender, RoutedEventArgs e)
    {
        if (_references is null)
        {
            return;
        }

        try
        {
            var users = await Task.Run(() => _usersService.List(new UserFilter(null, null, null)));
            var products = await Task.Run(
                () => _productsService.List(new ProductFilter(null, null, null, null, false, null)));
            var window = new OrderEditWindow(_ordersService, _references, users, products, null)
            {
                Owner = this
            };
            if (window.ShowDialog() == true)
            {
                await LoadOrdersAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void EditOrder_Click(object sender, RoutedEventArgs e)
    {
        if (_references is null)
        {
            return;
        }

        if (OrdersGrid.SelectedItem is not OrderSummary summary)
        {
            ShowError("Выберите заказ для редактирования.");
            return;
        }

        try
        {
            var details = await Task.Run(() => _ordersService.Get(summary.Id));
            var users = await Task.Run(() => _usersService.List(new UserFilter(null, null, null)));
            var products = await Task.Run(
                () => _productsService.List(new ProductFilter(null, null, null, null, false, null)));
            var window = new OrderEditWindow(_ordersService, _references, users, products, details)
            {
                Owner = this
            };
            if (window.ShowDialog() == true)
            {
                await LoadOrdersAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrdersGrid.SelectedItem is not OrderSummary summary)
        {
            ShowError("Выберите заказ для удаления.");
            return;
        }

        var answer = MessageBox.Show(
            $"Удалить заказ №{summary.Id}?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await Task.Run(() => _ordersService.Delete(summary.Id));
            await LoadOrdersAsync();
            SetStatus("Заказ удалён.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowError(string message)
    {
        SetStatus(message);
        MessageBox.Show(this, message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void SetStatus(string message) => StatusText.Text = message;

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, MainTabs) || StatusText is null)
        {
            return;
        }

        UpdateStatusForCurrentTab();
    }

    private void UpdateStatusForCurrentTab()
    {
        if (StatusText is null)
        {
            return;
        }

        StatusText.Text = ReferenceEquals(MainTabs.SelectedItem, UsersTab)
            ? $"Пользователей: {_usersCount}"
            : ReferenceEquals(MainTabs.SelectedItem, OrdersTab)
                ? $"Заказов: {_ordersCount}"
                : $"Товаров: {_productsCount}";
    }

    private sealed record SortOption(string Key, string Name);
}

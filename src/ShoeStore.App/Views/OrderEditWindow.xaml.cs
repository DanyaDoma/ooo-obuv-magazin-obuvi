using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using ShoeStore.Core;
using ShoeStore.Core.Models;
using ShoeStore.Core.Services;

namespace ShoeStore.App.Views;

public partial class OrderEditWindow : Window
{
    private readonly OrdersService _ordersService;
    private readonly OrderDetails? _order;
    private readonly ObservableCollection<OrderLineEdit> _lines = new();

    public IReadOnlyList<Product> Products { get; }

    public OrderEditWindow(
        OrdersService ordersService,
        References references,
        IReadOnlyList<UserInfo> users,
        IReadOnlyList<Product> products,
        OrderDetails? order)
    {
        InitializeComponent();
        _ordersService = ordersService;
        _order = order;
        Products = products;
        DataContext = this;

        ClientBox.ItemsSource = users;
        ClientBox.DisplayMemberPath = nameof(UserInfo.FullName);
        ClientBox.SelectedValuePath = nameof(UserInfo.Id);

        PickupPointBox.ItemsSource = references.PickupPoints;
        PickupPointBox.DisplayMemberPath = nameof(PickupPointInfo.Display);
        PickupPointBox.SelectedValuePath = nameof(PickupPointInfo.Id);

        StatusBox.ItemsSource = references.OrderStatuses;
        StatusBox.DisplayMemberPath = nameof(LookupItem.Name);
        StatusBox.SelectedValuePath = nameof(LookupItem.Id);

        ItemsGrid.ItemsSource = _lines;

        if (order is null)
        {
            TitleText.Text = "Новый заказ";
            OrderDatePicker.SelectedDate = DateTime.Today;
            StatusBox.SelectedIndex = 0;
            _lines.Add(new OrderLineEdit());
            return;
        }

        TitleText.Text = $"Заказ №{order.Summary.Id}";
        ClientBox.SelectedValue = order.Summary.UserId;
        PickupPointBox.SelectedValue = order.Summary.PickupPointId;
        StatusBox.SelectedValue = order.Summary.StatusId;
        OrderDatePicker.SelectedDate = order.Summary.OrderDate;
        DeliveryDatePicker.SelectedDate = order.Summary.DeliveryDate;
        PickupCodeBox.Text = order.Summary.PickupCode;

        foreach (var line in order.Items)
        {
            _lines.Add(new OrderLineEdit
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity
            });
        }
    }

    private void AddLine_Click(object sender, RoutedEventArgs e) =>
        _lines.Add(new OrderLineEdit());

    private void RemoveLine_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsGrid.SelectedItem is OrderLineEdit line)
        {
            _lines.Remove(line);
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        ItemsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        ItemsGrid.CommitEdit(DataGridEditingUnit.Row, true);

        if (ClientBox.SelectedValue is not long userId)
        {
            ShowError("Выберите клиента.");
            return;
        }

        if (PickupPointBox.SelectedValue is not int pickupPointId)
        {
            ShowError("Выберите пункт выдачи.");
            return;
        }

        if (StatusBox.SelectedValue is not int statusId)
        {
            ShowError("Выберите статус заказа.");
            return;
        }

        if (OrderDatePicker.SelectedDate is not DateTime orderDate)
        {
            ShowError("Укажите дату заказа.");
            return;
        }

        var items = _lines
            .Select(line => new OrderItemInput(line.ProductId, line.Quantity))
            .ToList();
        var input = new OrderInput(
            userId,
            pickupPointId,
            statusId,
            orderDate,
            DeliveryDatePicker.SelectedDate,
            PickupCodeBox.Text,
            items);

        SetBusy(true);
        try
        {
            if (_order is null)
            {
                await Task.Run(() => _ordersService.Create(input));
            }
            else
            {
                await Task.Run(() => _ordersService.Update(_order.Summary.Id, input));
            }

            DialogResult = true;
        }
        catch (DomainException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось сохранить заказ. {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        SaveButton.IsEnabled = !busy;
        SaveButton.Content = busy ? "Сохранение…" : "Сохранить";
    }
}

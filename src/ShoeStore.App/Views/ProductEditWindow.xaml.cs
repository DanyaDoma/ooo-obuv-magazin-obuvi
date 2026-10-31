using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using ShoeStore.Core;
using ShoeStore.Core.Models;
using ShoeStore.Core.Services;

namespace ShoeStore.App.Views;

public partial class ProductEditWindow : Window
{
    private readonly ProductsService _productsService;
    private readonly Product? _product;

    public ProductEditWindow(ProductsService productsService, References references, Product? product)
    {
        InitializeComponent();
        _productsService = productsService;
        _product = product;

        CategoryBox.ItemsSource = references.Categories;
        SupplierBox.ItemsSource = references.Suppliers;
        ManufacturerBox.ItemsSource = references.Manufacturers;
        UnitBox.ItemsSource = references.Units;

        if (product is null)
        {
            TitleText.Text = "Новый товар";
            DiscountBox.Text = "0";
            StockBox.Text = "0";
            return;
        }

        TitleText.Text = "Редактирование товара";
        SkuBox.Text = product.Sku;
        NameBox.Text = product.Name;
        CategoryBox.SelectedValue = product.CategoryId;
        SupplierBox.SelectedValue = product.SupplierId;
        ManufacturerBox.SelectedValue = product.ManufacturerId;
        UnitBox.SelectedValue = product.UnitId;
        PriceBox.Text = product.Price.ToString("0.00", CultureInfo.CurrentCulture);
        DiscountBox.Text = product.DiscountPercent.ToString("0.##", CultureInfo.CurrentCulture);
        StockBox.Text = product.StockQty.ToString(CultureInfo.CurrentCulture);
        DescriptionBox.Text = product.Description ?? string.Empty;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        if (CategoryBox.SelectedValue is not int categoryId)
        {
            ShowError("Выберите категорию.");
            return;
        }

        if (SupplierBox.SelectedValue is not int supplierId)
        {
            ShowError("Выберите поставщика.");
            return;
        }

        if (ManufacturerBox.SelectedValue is not int manufacturerId)
        {
            ShowError("Выберите производителя.");
            return;
        }

        if (UnitBox.SelectedValue is not int unitId)
        {
            ShowError("Выберите единицу измерения.");
            return;
        }

        if (!TryParseDecimal(PriceBox.Text, out var price))
        {
            ShowError("Укажите корректную цену.");
            return;
        }

        if (!TryParseDecimal(DiscountBox.Text, out var discount))
        {
            ShowError("Укажите корректную скидку.");
            return;
        }

        if (!int.TryParse(StockBox.Text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var stock))
        {
            ShowError("Укажите корректный остаток.");
            return;
        }

        var input = new ProductInput(
            SkuBox.Text,
            NameBox.Text,
            unitId,
            price,
            supplierId,
            manufacturerId,
            categoryId,
            discount,
            stock,
            DescriptionBox.Text);

        SetBusy(true);
        try
        {
            if (_product is null)
            {
                await Task.Run(() => _productsService.Create(input));
            }
            else
            {
                await Task.Run(() => _productsService.Update(_product.Id, input));
            }

            DialogResult = true;
        }
        catch (DomainException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось сохранить товар. {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static bool TryParseDecimal(string text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

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

using System.Text;

using MySqlConnector;

using ShoeStore.Core.Data;
using ShoeStore.Core.Models;
using ShoeStore.Core.Utils;

namespace ShoeStore.Core.Services;

public sealed class ProductsService(Database database)
{
    private const string ProductSelect = """
        SELECT p.id, p.sku, p.name,
               p.unit_id, un.name AS unit_name,
               p.price,
               p.supplier_id, sup.name AS supplier_name,
               p.manufacturer_id, man.name AS manufacturer_name,
               p.category_id, cat.name AS category_name,
               p.discount_percent, p.stock_qty, p.description
          FROM products p
          JOIN units un ON un.id = p.unit_id
          JOIN suppliers sup ON sup.id = p.supplier_id
          JOIN manufacturers man ON man.id = p.manufacturer_id
          JOIN categories cat ON cat.id = p.category_id
        """;

    private static readonly Dictionary<string, string> SortOptions = new(StringComparer.Ordinal)
    {
        ["name_asc"] = "p.name ASC, p.id ASC",
        ["name_desc"] = "p.name DESC, p.id ASC",
        ["price_asc"] = "p.price ASC, p.id ASC",
        ["price_desc"] = "p.price DESC, p.id ASC",
        ["discount_desc"] = "p.discount_percent DESC, p.price ASC, p.id ASC",
        ["stock_desc"] = "p.stock_qty DESC, p.id ASC"
    };

    public IReadOnlyList<Product> List(ProductFilter filter)
    {
        var sql = new StringBuilder(ProductSelect).Append("\n WHERE p.is_active = 1");
        using var connection = database.Open();
        using var command = connection.CreateCommand();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sql.Append(" AND (p.name LIKE @search OR CONVERT(p.sku USING utf8mb4) LIKE @search)");
            command.Parameters.AddWithValue("@search", SqlHelpers.LikeContains(filter.Search.Trim()));
        }

        if (filter.CategoryId.HasValue)
        {
            sql.Append(" AND p.category_id = @category_id");
            command.Parameters.AddWithValue("@category_id", filter.CategoryId.Value);
        }

        if (filter.SupplierId.HasValue)
        {
            sql.Append(" AND p.supplier_id = @supplier_id");
            command.Parameters.AddWithValue("@supplier_id", filter.SupplierId.Value);
        }

        if (filter.ManufacturerId.HasValue)
        {
            sql.Append(" AND p.manufacturer_id = @manufacturer_id");
            command.Parameters.AddWithValue("@manufacturer_id", filter.ManufacturerId.Value);
        }

        if (filter.DiscountOnly)
        {
            sql.Append(" AND p.discount_percent > 0");
        }

        var orderBy = SortOptions.GetValueOrDefault(filter.Sort ?? string.Empty, SortOptions["name_asc"]);
        sql.Append(" ORDER BY ").Append(orderBy).Append(" LIMIT 300");
        command.CommandText = sql.ToString();

        using var reader = command.ExecuteReader();
        var items = new List<Product>();
        while (reader.Read())
        {
            items.Add(Map(reader));
        }

        return items;
    }

    public Product Create(ProductInput input)
    {
        var data = Validate(input);
        using var connection = database.Open();
        EnsureReferences(connection, data);
        if (SkuExists(connection, data.Sku))
        {
            throw new DomainException("Артикул уже используется.");
        }

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO products
                (sku, name, unit_id, price, supplier_id, manufacturer_id, category_id,
                 discount_percent, stock_qty, description)
            VALUES
                (@sku, @name, @unit_id, @price, @supplier_id, @manufacturer_id, @category_id,
                 @discount_percent, @stock_qty, @description)
            """;
        AddProductParameters(command, data);

        try
        {
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            throw new DomainException("Артикул уже используется.");
        }

        return GetById(connection, command.LastInsertedId);
    }

    public Product Update(long id, ProductInput input)
    {
        var data = Validate(input);
        using var connection = database.Open();
        if (!ProductExists(connection, id))
        {
            throw new DomainException("Товар не найден.");
        }

        EnsureReferences(connection, data);
        if (SkuExists(connection, data.Sku, id))
        {
            throw new DomainException("Артикул уже используется.");
        }

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE products SET
                sku = @sku, name = @name, unit_id = @unit_id, price = @price,
                supplier_id = @supplier_id, manufacturer_id = @manufacturer_id,
                category_id = @category_id, discount_percent = @discount_percent,
                stock_qty = @stock_qty, description = @description
            WHERE id = @id
            """;
        AddProductParameters(command, data);
        command.Parameters.AddWithValue("@id", id);

        try
        {
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            throw new DomainException("Артикул уже используется.");
        }

        return GetById(connection, id);
    }

    public void Delete(long id)
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE products SET is_active = 0 WHERE id = @id AND is_active = 1";
        command.Parameters.AddWithValue("@id", id);
        if (command.ExecuteNonQuery() == 0)
        {
            throw new DomainException("Товар не найден.");
        }
    }

    private static ProductInput Validate(ProductInput input)
    {
        var sku = Sku.NormalizeOrThrow(input.Sku);
        var name = (input.Name ?? string.Empty).Trim();
        if (name.Length is 0 or > 255)
        {
            throw new DomainException("Наименование должно содержать от 1 до 255 символов.");
        }

        if (input.Price < 0)
        {
            throw new DomainException("Цена не может быть отрицательной.");
        }

        if (input.DiscountPercent is < 0 or > 100)
        {
            throw new DomainException("Скидка должна быть в диапазоне от 0 до 100%.");
        }

        if (input.StockQty < 0)
        {
            throw new DomainException("Остаток не может быть отрицательным.");
        }

        var description = string.IsNullOrWhiteSpace(input.Description)
            ? null
            : input.Description.Trim();
        if (description is { Length: > 65535 })
        {
            throw new DomainException("Описание слишком длинное.");
        }

        return input with { Sku = sku, Name = name, Description = description };
    }

    private static void EnsureReferences(MySqlConnection connection, ProductInput input)
    {
        EnsureReference(connection, "units", input.UnitId, "Единица измерения не найдена.");
        EnsureReference(connection, "suppliers", input.SupplierId, "Поставщик не найден.");
        EnsureReference(connection, "manufacturers", input.ManufacturerId, "Производитель не найден.");
        EnsureReference(connection, "categories", input.CategoryId, "Категория не найдена.");
    }

    private static void EnsureReference(MySqlConnection connection, string table, int id, string message)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT 1 FROM {table} WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        if (command.ExecuteScalar() is null)
        {
            throw new DomainException(message);
        }
    }

    private static bool SkuExists(MySqlConnection connection, string sku, long? excludeId = null)
    {
        using var command = connection.CreateCommand();
        command.CommandText = excludeId.HasValue
            ? "SELECT 1 FROM products WHERE sku = @sku AND id <> @id"
            : "SELECT 1 FROM products WHERE sku = @sku";
        command.Parameters.AddWithValue("@sku", sku);
        if (excludeId.HasValue)
        {
            command.Parameters.AddWithValue("@id", excludeId.Value);
        }

        return command.ExecuteScalar() is not null;
    }

    private static bool ProductExists(MySqlConnection connection, long id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM products WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        return command.ExecuteScalar() is not null;
    }

    private static void AddProductParameters(MySqlCommand command, ProductInput data)
    {
        command.Parameters.AddWithValue("@sku", data.Sku);
        command.Parameters.AddWithValue("@name", data.Name);
        command.Parameters.AddWithValue("@unit_id", data.UnitId);
        command.Parameters.AddWithValue("@price", data.Price);
        command.Parameters.AddWithValue("@supplier_id", data.SupplierId);
        command.Parameters.AddWithValue("@manufacturer_id", data.ManufacturerId);
        command.Parameters.AddWithValue("@category_id", data.CategoryId);
        command.Parameters.AddWithValue("@discount_percent", data.DiscountPercent);
        command.Parameters.AddWithValue("@stock_qty", data.StockQty);
        command.Parameters.AddWithValue("@description", (object?)data.Description ?? DBNull.Value);
    }

    private static Product GetById(MySqlConnection connection, long id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"{ProductSelect}\n WHERE p.id = @id";
        command.Parameters.AddWithValue("@id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new DomainException("Товар не найден.");
        }

        return Map(reader);
    }

    private static Product Map(MySqlDataReader reader) =>
        new(
            reader.GetInt64("id"),
            reader.GetString("sku"),
            reader.GetString("name"),
            reader.GetInt32("unit_id"),
            reader.GetString("unit_name"),
            reader.GetDecimal("price"),
            reader.GetInt32("supplier_id"),
            reader.GetString("supplier_name"),
            reader.GetInt32("manufacturer_id"),
            reader.GetString("manufacturer_name"),
            reader.GetInt32("category_id"),
            reader.GetString("category_name"),
            reader.GetDecimal("discount_percent"),
            reader.GetInt32("stock_qty"),
            reader.GetNullableString("description"));
}

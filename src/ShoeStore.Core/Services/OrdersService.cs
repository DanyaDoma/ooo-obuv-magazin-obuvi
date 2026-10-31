using MySqlConnector;

using ShoeStore.Core.Data;
using ShoeStore.Core.Models;
using ShoeStore.Core.Utils;

namespace ShoeStore.Core.Services;

public sealed class OrdersService(Database database)
{
    private const string OrderSelect = """
        SELECT o.id, o.user_id, o.pickup_point_id, o.status_id,
               o.order_date, o.delivery_date, o.pickup_code, o.total_amount,
               st.name AS status_name,
               u.full_name AS user_full_name,
               pp.postal_code, pp.city, pp.address,
               (SELECT COUNT(*) FROM order_items oi WHERE oi.order_id = o.id) AS items_count
          FROM orders o
          JOIN users u ON u.id = o.user_id
          JOIN order_statuses st ON st.id = o.status_id
          JOIN pickup_points pp ON pp.id = o.pickup_point_id
        """;

    public IReadOnlyList<OrderSummary> List()
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"{OrderSelect}\n ORDER BY o.order_date DESC, o.id DESC LIMIT 200";
        using var reader = command.ExecuteReader();
        var items = new List<OrderSummary>();
        while (reader.Read())
        {
            items.Add(MapSummary(reader));
        }

        return items;
    }

    public OrderDetails Get(long id)
    {
        using var connection = database.Open();
        OrderSummary summary;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"{OrderSelect}\n WHERE o.id = @id";
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new DomainException("Заказ не найден.");
            }

            summary = MapSummary(reader);
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT oi.product_id, oi.quantity, oi.unit_price, oi.discount_percent, oi.line_total,
                       p.sku, p.name
                  FROM order_items oi
                  JOIN products p ON p.id = oi.product_id
                 WHERE oi.order_id = @order_id
                 ORDER BY p.name ASC
                """;
            command.Parameters.AddWithValue("@order_id", id);
            using var reader = command.ExecuteReader();
            var lines = new List<OrderItemLine>();
            while (reader.Read())
            {
                lines.Add(new OrderItemLine(
                    reader.GetInt64("product_id"),
                    reader.GetString("sku"),
                    reader.GetString("name"),
                    reader.GetInt32("quantity"),
                    reader.GetDecimal("unit_price"),
                    reader.GetDecimal("discount_percent"),
                    reader.GetDecimal("line_total")));
            }

            return new OrderDetails(summary, lines);
        }
    }

    public OrderDetails Create(OrderInput input)
    {
        var data = Validate(input);
        var orderId = ExecuteInTransaction((connection, transaction) =>
        {
            EnsureReferences(connection, transaction, data);
            EnsurePickupCodeFree(connection, transaction, data.PickupCode);
            var items = ResolveItems(connection, transaction, data.Items);

            using var command = CreateCommand(connection, transaction, """
                INSERT INTO orders
                    (user_id, pickup_point_id, status_id, order_date, delivery_date, pickup_code, total_amount)
                VALUES
                    (@user_id, @pickup_point_id, @status_id, @order_date, @delivery_date, @pickup_code, @total_amount)
                """);
            AddOrderParameters(command, data);
            command.Parameters.AddWithValue("@total_amount", SumTotal(items));
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new DomainException("Код получения уже используется.");
            }

            var id = command.LastInsertedId;
            InsertItems(connection, transaction, id, items);
            return id;
        });

        return Get(orderId);
    }

    public OrderDetails Update(long id, OrderInput input)
    {
        var data = Validate(input);
        ExecuteInTransaction((connection, transaction) =>
        {
            EnsureOrderExists(connection, transaction, id);
            EnsureReferences(connection, transaction, data);
            EnsurePickupCodeFree(connection, transaction, data.PickupCode, id);
            var items = ResolveItems(connection, transaction, data.Items);

            using (var command = CreateCommand(connection, transaction, """
                UPDATE orders SET
                    user_id = @user_id, pickup_point_id = @pickup_point_id, status_id = @status_id,
                    order_date = @order_date, delivery_date = @delivery_date,
                    pickup_code = @pickup_code, total_amount = @total_amount
                WHERE id = @id
                """))
            {
                AddOrderParameters(command, data);
                command.Parameters.AddWithValue("@total_amount", SumTotal(items));
                command.Parameters.AddWithValue("@id", id);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (MySqlException ex) when (ex.Number == 1062)
                {
                    throw new DomainException("Код получения уже используется.");
                }
            }

            using (var command = CreateCommand(connection, transaction,
                       "DELETE FROM order_items WHERE order_id = @order_id"))
            {
                command.Parameters.AddWithValue("@order_id", id);
                command.ExecuteNonQuery();
            }

            InsertItems(connection, transaction, id, items);
            return 0;
        });

        return Get(id);
    }

    public void Delete(long id)
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM orders WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        if (command.ExecuteNonQuery() == 0)
        {
            throw new DomainException("Заказ не найден.");
        }
    }

    private static OrderInput Validate(OrderInput input)
    {
        var code = (input.PickupCode ?? string.Empty).Trim();
        if (code.Length is 0 or > 6 || !code.All(char.IsAsciiLetterOrDigit))
        {
            throw new DomainException("Код получения должен содержать от 1 до 6 латинских букв или цифр.");
        }

        var orderDate = input.OrderDate.Date;
        if (orderDate == default)
        {
            throw new DomainException("Укажите дату заказа.");
        }

        var deliveryDate = input.DeliveryDate?.Date;
        if (deliveryDate < orderDate)
        {
            throw new DomainException("Дата доставки не может быть раньше даты заказа.");
        }

        return input with { PickupCode = code, OrderDate = orderDate, DeliveryDate = deliveryDate };
    }

    private static void EnsureReferences(
        MySqlConnection connection,
        MySqlTransaction transaction,
        OrderInput input)
    {
        EnsureReference(connection, transaction, "users", input.UserId, "Клиент не найден.");
        EnsureReference(connection, transaction, "pickup_points", input.PickupPointId, "Пункт выдачи не найден.");
        EnsureReference(connection, transaction, "order_statuses", input.StatusId, "Статус заказа не найден.");
    }

    private static void EnsureReference(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string table,
        long id,
        string message)
    {
        using var command = CreateCommand(connection, transaction, $"SELECT 1 FROM {table} WHERE id = @id");
        command.Parameters.AddWithValue("@id", id);
        if (command.ExecuteScalar() is null)
        {
            throw new DomainException(message);
        }
    }

    private static void EnsureOrderExists(
        MySqlConnection connection,
        MySqlTransaction transaction,
        long id)
    {
        using var command = CreateCommand(connection, transaction, "SELECT 1 FROM orders WHERE id = @id");
        command.Parameters.AddWithValue("@id", id);
        if (command.ExecuteScalar() is null)
        {
            throw new DomainException("Заказ не найден.");
        }
    }

    private static void EnsurePickupCodeFree(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string code,
        long? excludeId = null)
    {
        using var command = CreateCommand(connection, transaction,
            excludeId.HasValue
                ? "SELECT 1 FROM orders WHERE pickup_code = @code AND id <> @id"
                : "SELECT 1 FROM orders WHERE pickup_code = @code");
        command.Parameters.AddWithValue("@code", code);
        if (excludeId.HasValue)
        {
            command.Parameters.AddWithValue("@id", excludeId.Value);
        }

        if (command.ExecuteScalar() is not null)
        {
            throw new DomainException("Код получения уже используется.");
        }
    }

    private static IReadOnlyList<ResolvedItem> ResolveItems(
        MySqlConnection connection,
        MySqlTransaction transaction,
        IReadOnlyList<OrderItemInput> items)
    {
        if (items.Count == 0)
        {
            throw new DomainException("Добавьте хотя бы один товар.");
        }

        if (items.Any(item => item.ProductId <= 0))
        {
            throw new DomainException("Выберите товар для каждой позиции.");
        }

        if (items.Any(item => item.Quantity <= 0))
        {
            throw new DomainException("Количество должно быть больше нуля.");
        }

        if (items.Select(item => item.ProductId).Distinct().Count() != items.Count)
        {
            throw new DomainException("Товар не должен повторяться в заказе.");
        }

        using var command = CreateCommand(connection, transaction, string.Empty);
        var placeholders = new List<string>();
        for (var i = 0; i < items.Count; i++)
        {
            var name = $"@product_{i}";
            placeholders.Add(name);
            command.Parameters.AddWithValue(name, items[i].ProductId);
        }

        command.CommandText = $"""
            SELECT id, price, discount_percent
              FROM products
             WHERE id IN ({string.Join(", ", placeholders)}) AND is_active = 1
            """;

        var products = new Dictionary<long, (decimal Price, decimal Discount)>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                products[reader.GetInt64("id")] =
                    (reader.GetDecimal("price"), reader.GetDecimal("discount_percent"));
            }
        }

        var missing = items
            .Select(item => item.ProductId)
            .Where(id => !products.ContainsKey(id))
            .ToList();
        if (missing.Count > 0)
        {
            throw new DomainException(
                $"Товары не найдены или скрыты: {string.Join(", ", missing)}.");
        }

        return items.Select(item =>
        {
            var product = products[item.ProductId];
            var lineTotal = SqlHelpers.RoundMoney(
                item.Quantity * product.Price * (1 - product.Discount / 100m));
            return new ResolvedItem(
                item.ProductId,
                item.Quantity,
                product.Price,
                product.Discount,
                lineTotal);
        }).ToList();
    }

    private static decimal SumTotal(IEnumerable<ResolvedItem> items) =>
        SqlHelpers.RoundMoney(items.Sum(item => item.LineTotal));

    private static void InsertItems(
        MySqlConnection connection,
        MySqlTransaction transaction,
        long orderId,
        IEnumerable<ResolvedItem> items)
    {
        using var command = CreateCommand(connection, transaction, """
            INSERT INTO order_items
                (order_id, product_id, quantity, unit_price, discount_percent, line_total)
            VALUES
                (@order_id, @product_id, @quantity, @unit_price, @discount_percent, @line_total)
            """);
        command.Parameters.AddWithValue("@order_id", orderId);
        var productId = command.Parameters.Add("@product_id", MySqlDbType.Int64);
        var quantity = command.Parameters.Add("@quantity", MySqlDbType.Int32);
        var unitPrice = command.Parameters.Add("@unit_price", MySqlDbType.Decimal);
        var discount = command.Parameters.Add("@discount_percent", MySqlDbType.Decimal);
        var lineTotal = command.Parameters.Add("@line_total", MySqlDbType.Decimal);

        foreach (var item in items)
        {
            productId.Value = item.ProductId;
            quantity.Value = item.Quantity;
            unitPrice.Value = item.UnitPrice;
            discount.Value = item.DiscountPercent;
            lineTotal.Value = item.LineTotal;
            command.ExecuteNonQuery();
        }
    }

    private static MySqlCommand CreateCommand(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }

    private static void AddOrderParameters(MySqlCommand command, OrderInput data)
    {
        command.Parameters.AddWithValue("@user_id", data.UserId);
        command.Parameters.AddWithValue("@pickup_point_id", data.PickupPointId);
        command.Parameters.AddWithValue("@status_id", data.StatusId);
        command.Parameters.AddWithValue("@order_date", data.OrderDate);
        command.Parameters.AddWithValue("@delivery_date", (object?)data.DeliveryDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@pickup_code", data.PickupCode);
    }

    private T ExecuteInTransaction<T>(Func<MySqlConnection, MySqlTransaction, T> action)
    {
        using var connection = database.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var result = action(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static OrderSummary MapSummary(MySqlDataReader reader) =>
        new(
            reader.GetInt64("id"),
            reader.GetInt64("user_id"),
            reader.GetInt32("pickup_point_id"),
            reader.GetInt32("status_id"),
            reader.GetDateTime("order_date"),
            reader.GetNullableDateTime("delivery_date"),
            reader.GetString("pickup_code"),
            reader.GetDecimal("total_amount"),
            reader.GetString("status_name"),
            reader.GetString("user_full_name"),
            new PickupPointInfo(
                reader.GetInt32("pickup_point_id"),
                reader.GetString("postal_code"),
                reader.GetString("city"),
                reader.GetString("address")),
            reader.GetInt32("items_count"));

    private sealed record ResolvedItem(
        long ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal DiscountPercent,
        decimal LineTotal);
}

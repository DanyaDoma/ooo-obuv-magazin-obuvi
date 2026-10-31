using ShoeStore.Core;
using ShoeStore.Core.Data;
using ShoeStore.Core.Models;
using ShoeStore.Core.Security;
using ShoeStore.Core.Services;
using ShoeStore.Core.Utils;

namespace ShoeStore.Tests;

public sealed class RealDatabaseTests
{
    private static Database? TryCreateDatabase()
    {
        var connectionString = Environment.GetEnvironmentVariable("SHOE_STORE_TEST_CONNECTION");
        return string.IsNullOrWhiteSpace(connectionString) ? null : new Database(connectionString);
    }

    [Fact]
    public void SeedData_HasExpectedCounts()
    {
        var database = TryCreateDatabase();
        if (database is null) return;

        Assert.Equal(10, new UsersService(database).List(new UserFilter(null, null, null)).Count);
        Assert.Equal(30, new ProductsService(database)
            .List(new ProductFilter(null, null, null, null, false, null)).Count);
        Assert.Equal(10, new OrdersService(database).List().Count);
        Assert.Equal(36, new ReferencesService(database).Load().PickupPoints.Count);
    }

    [Fact]
    public void Admin_CanAuthenticateWithRealHash()
    {
        var database = TryCreateDatabase();
        if (database is null) return;

        var auth = new AuthService(database);
        var admin = auth.Authenticate("94d5ous@gmail.com", "uzWC67");
        Assert.Equal("admin", admin.Role.Code);

        Assert.Throws<DomainException>(() => auth.Authenticate("94d5ous@gmail.com", "wrong"));
    }

    [Fact]
    public void SqlInjection_DoesNotBypassLoginOrSearch()
    {
        var database = TryCreateDatabase();
        if (database is null) return;

        var auth = new AuthService(database);
        Assert.Throws<DomainException>(() =>
            auth.Authenticate("94d5ous@gmail.com' OR 1=1 --", "uzWC67"));

        var users = new UsersService(database);
        Assert.Empty(users.List(new UserFilter("' OR 1=1 --", null, null)));
        Assert.Empty(users.List(new UserFilter("%", null, null)));
    }

    [Fact]
    public void Users_SearchAndFiltersWork()
    {
        var database = TryCreateDatabase();
        if (database is null) return;

        var users = new UsersService(database);
        Assert.Single(users.List(new UserFilter("Никифорова", null, null)));
        Assert.Equal(3, users.List(new UserFilter(null, 1, null)).Count);
        Assert.Equal(3, users.List(new UserFilter(null, null, 2)).Count);
    }

    [Fact]
    public void Products_FiltersAndSortingWork()
    {
        var database = TryCreateDatabase();
        if (database is null) return;

        var products = new ProductsService(database);
        var discounted = products.List(new ProductFilter(null, null, null, null, true, null));
        Assert.NotEmpty(discounted);
        Assert.All(discounted, product => Assert.True(product.DiscountPercent > 0));

        var sorted = products.List(new ProductFilter(null, null, null, null, false, "price_asc"));
        Assert.True(sorted.Zip(sorted.Skip(1)).All(pair => pair.First.Price <= pair.Second.Price));

        var first = sorted[0];
        var bySku = products.List(new ProductFilter(first.Sku, null, null, null, false, null));
        Assert.Contains(bySku, product => product.Id == first.Id);
    }

    [Fact]
    public void Product_CrudRoundTrip()
    {
        var database = TryCreateDatabase();
        if (database is null) return;

        var products = new ProductsService(database);
        var references = new ReferencesService(database).Load();
        var input = new ProductInput(
            "T" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            "Тестовый товар",
            references.Units[0].Id,
            1000m,
            references.Suppliers[0].Id,
            references.Manufacturers[0].Id,
            references.Categories[0].Id,
            10m,
            5,
            "Тестовое описание");

        Product? created = null;
        try
        {
            created = products.Create(input);
            Assert.Equal(900m, created.FinalPrice);

            var updated = products.Update(created.Id, input with
            {
                Name = "Тестовый товар 2",
                Price = 2000m,
                DiscountPercent = 0m
            });
            Assert.Equal(2000m, updated.FinalPrice);
            Assert.Contains(
                products.List(new ProductFilter(null, null, null, null, false, null)),
                product => product.Id == created.Id);

            products.Delete(created.Id);
            Assert.DoesNotContain(
                products.List(new ProductFilter(null, null, null, null, false, null)),
                product => product.Id == created.Id);
        }
        finally
        {
            if (created is not null)
            {
                using var connection = database.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM products WHERE id = @id";
                command.Parameters.AddWithValue("@id", created.Id);
                command.ExecuteNonQuery();
            }
        }
    }

    [Fact]
    public void Order_CrudRoundTrip()
    {
        var database = TryCreateDatabase();
        if (database is null) return;

        var orders = new OrdersService(database);
        var users = new UsersService(database).List(new UserFilter(null, null, null));
        var products = new ProductsService(database)
            .List(new ProductFilter(null, null, null, null, false, null));
        var references = new ReferencesService(database).Load();

        var client = users.First(user => user.Role.Code == Role.ClientCode);
        var product = products[0];
        var pickupCode = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var orderDate = new DateTime(2026, 10, 31);

        OrderDetails? created = null;
        try
        {
            created = orders.Create(new OrderInput(
                client.Id,
                references.PickupPoints[0].Id,
                references.OrderStatuses[0].Id,
                orderDate,
                null,
                pickupCode,
                new[] { new OrderItemInput(product.Id, 2) }));

            var line = Assert.Single(created.Items);
            Assert.Equal(2, line.Quantity);
            Assert.Equal(
                SqlHelpers.RoundMoney(2 * product.Price * (1 - product.DiscountPercent / 100m)),
                created.Summary.TotalAmount);

            var updated = orders.Update(created.Summary.Id, new OrderInput(
                client.Id,
                references.PickupPoints[1].Id,
                references.OrderStatuses[1].Id,
                orderDate,
                orderDate.AddDays(5),
                pickupCode,
                new[] { new OrderItemInput(product.Id, 3) }));

            Assert.Equal(3, Assert.Single(updated.Items).Quantity);
            Assert.Equal(references.PickupPoints[1].Id, updated.Summary.PickupPointId);

            orders.Delete(updated.Summary.Id);
            Assert.Throws<DomainException>(() => orders.Get(updated.Summary.Id));
            created = null;
        }
        finally
        {
            if (created is not null)
            {
                try
                {
                    orders.Delete(created.Summary.Id);
                }
                catch
                {
                    // заказ уже удалён
                }
            }
        }
    }
}

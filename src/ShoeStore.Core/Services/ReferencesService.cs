using MySqlConnector;

using ShoeStore.Core.Data;
using ShoeStore.Core.Models;

namespace ShoeStore.Core.Services;

public sealed class ReferencesService(Database database)
{
    public References Load()
    {
        using var connection = database.Open();
        return new References(
            ReadLookups(connection, "SELECT id, name FROM categories ORDER BY name"),
            ReadLookups(connection, "SELECT id, name FROM suppliers ORDER BY name"),
            ReadLookups(connection, "SELECT id, name FROM manufacturers ORDER BY name"),
            ReadLookups(connection, "SELECT id, name FROM units ORDER BY name"),
            ReadLookups(connection, "SELECT id, name FROM order_statuses ORDER BY id"),
            ReadPickupPoints(connection),
            ReadRoles(connection),
            ReadLookups(connection, "SELECT id, name FROM departments ORDER BY name"));
    }

    private static List<LookupItem> ReadLookups(MySqlConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        var items = new List<LookupItem>();
        while (reader.Read())
        {
            items.Add(new LookupItem(reader.GetInt32("id"), reader.GetString("name")));
        }

        return items;
    }

    private static List<PickupPointInfo> ReadPickupPoints(MySqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, postal_code, city, address
              FROM pickup_points
             ORDER BY id
            """;
        using var reader = command.ExecuteReader();
        var items = new List<PickupPointInfo>();
        while (reader.Read())
        {
            items.Add(new PickupPointInfo(
                reader.GetInt32("id"),
                reader.GetString("postal_code"),
                reader.GetString("city"),
                reader.GetString("address")));
        }

        return items;
    }

    private static List<Role> ReadRoles(MySqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, code, name FROM roles ORDER BY id";
        using var reader = command.ExecuteReader();
        var items = new List<Role>();
        while (reader.Read())
        {
            items.Add(new Role(
                reader.GetInt32("id"),
                reader.GetString("code"),
                reader.GetString("name")));
        }

        return items;
    }
}

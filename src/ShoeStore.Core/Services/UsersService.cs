using System.Text;

using MySqlConnector;

using ShoeStore.Core.Data;
using ShoeStore.Core.Models;
using ShoeStore.Core.Utils;

namespace ShoeStore.Core.Services;

public sealed class UsersService(Database database)
{
    public IReadOnlyList<UserInfo> List(UserFilter filter)
    {
        var sql = new StringBuilder("""
            SELECT u.id, u.full_name, u.login,
                   r.id AS role_id, r.code AS role_code, r.name AS role_name,
                   d.id AS department_id, d.name AS department_name
              FROM users u
              JOIN roles r ON r.id = u.role_id
              LEFT JOIN departments d ON d.id = u.department_id
            """);

        var conditions = new List<string>();
        using var connection = database.Open();
        using var command = connection.CreateCommand();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            conditions.Add("u.full_name LIKE @search");
            command.Parameters.AddWithValue("@search", SqlHelpers.LikeContains(filter.Search.Trim()));
        }

        if (filter.RoleId.HasValue)
        {
            conditions.Add("u.role_id = @role_id");
            command.Parameters.AddWithValue("@role_id", filter.RoleId.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            conditions.Add("u.department_id = @department_id");
            command.Parameters.AddWithValue("@department_id", filter.DepartmentId.Value);
        }

        if (conditions.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", conditions));
        }

        sql.Append(" ORDER BY u.full_name ASC, u.id ASC");
        command.CommandText = sql.ToString();

        using var reader = command.ExecuteReader();
        var items = new List<UserInfo>();
        while (reader.Read())
        {
            var department = reader.GetNullableInt32("department_id") is { } departmentId
                ? new Department(departmentId, reader.GetString("department_name"))
                : null;

            items.Add(new UserInfo(
                reader.GetInt64("id"),
                reader.GetString("full_name"),
                reader.GetString("login"),
                new Role(
                    reader.GetInt32("role_id"),
                    reader.GetString("role_code"),
                    reader.GetString("role_name")),
                department));
        }

        return items;
    }
}

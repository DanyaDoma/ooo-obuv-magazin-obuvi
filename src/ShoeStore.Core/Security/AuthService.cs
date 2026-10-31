using ShoeStore.Core.Data;
using ShoeStore.Core.Models;

namespace ShoeStore.Core.Security;

public sealed class AuthService(Database database)
{
    private static readonly Lazy<string> DummyHash =
        new(() => BCrypt.Net.BCrypt.HashPassword("shoe-store-dummy-password", 12));

    public UserInfo Authenticate(string login, string password)
    {
        var normalizedLogin = (login ?? string.Empty).Trim();
        if (normalizedLogin.Length == 0 || string.IsNullOrEmpty(password))
        {
            throw new DomainException("Введите логин и пароль.");
        }

        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT u.id, u.full_name, u.login, u.password_hash,
                   r.id AS role_id, r.code AS role_code, r.name AS role_name,
                   d.id AS department_id, d.name AS department_name
              FROM users u
              JOIN roles r ON r.id = u.role_id
              LEFT JOIN departments d ON d.id = u.department_id
             WHERE u.login = @login
            """;
        command.Parameters.AddWithValue("@login", normalizedLogin);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            BCrypt.Net.BCrypt.Verify(password, DummyHash.Value);
            throw new DomainException("Неверный логин или пароль.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, reader.GetString("password_hash")))
        {
            throw new DomainException("Неверный логин или пароль.");
        }

        var role = new Role(
            reader.GetInt32("role_id"),
            reader.GetString("role_code"),
            reader.GetString("role_name"));

        var department = reader.GetNullableInt32("department_id") is { } departmentId
            ? new Department(departmentId, reader.GetString("department_name"))
            : null;

        return new UserInfo(
            reader.GetInt64("id"),
            reader.GetString("full_name"),
            reader.GetString("login"),
            role,
            department);
    }
}

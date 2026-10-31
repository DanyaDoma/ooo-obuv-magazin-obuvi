using System.Text.Json;

namespace ShoeStore.Core.Data;

public static class AppConfiguration
{
    public static string LoadConnectionString()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                "Не найден appsettings.json рядом с исполняемым файлом.");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        if (!root.TryGetProperty("ConnectionStrings", out var connections) ||
            !connections.TryGetProperty("ShoeStore", out var connectionString))
        {
            throw new InvalidOperationException(
                "В appsettings.json не задана строка подключения ConnectionStrings:ShoeStore.");
        }

        return connectionString.GetString()
               ?? throw new InvalidOperationException(
                   "Строка подключения ConnectionStrings:ShoeStore пуста.");
    }
}

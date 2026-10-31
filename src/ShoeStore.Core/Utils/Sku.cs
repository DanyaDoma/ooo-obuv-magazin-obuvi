using System.Text;

namespace ShoeStore.Core.Utils;

public static class Sku
{
    private static readonly Dictionary<char, char> CyrillicToLatin = new()
    {
        ['А'] = 'A', ['В'] = 'B', ['С'] = 'C', ['Е'] = 'E', ['Ё'] = 'E', ['Н'] = 'H',
        ['І'] = 'I', ['Ј'] = 'J', ['К'] = 'K', ['М'] = 'M', ['О'] = 'O', ['Р'] = 'P',
        ['Ѕ'] = 'S', ['Т'] = 'T', ['У'] = 'Y', ['Х'] = 'X', ['Ԛ'] = 'Q', ['Ԝ'] = 'W',
        ['Ӏ'] = 'I'
    };

    public static string Normalize(string? raw)
    {
        var value = (raw ?? string.Empty).Normalize(NormalizationForm.FormC).Trim().ToUpperInvariant();
        var chars = value.Select(ch => CyrillicToLatin.TryGetValue(ch, out var latin) ? latin : ch);
        return new string(chars.ToArray());
    }

    public static string NormalizeOrThrow(string? raw)
    {
        var sku = Normalize(raw);
        var valid = sku.Length is >= 4 and <= 32 &&
                    sku.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9');
        if (!valid)
        {
            throw new DomainException("Артикул должен содержать 4–32 латинских буквы или цифры.");
        }

        return sku;
    }
}

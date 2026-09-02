using System.Text;

namespace PebbleJar.Extensions;

public static class Extensions
{
    public static string ToDebugString(this object obj)
    {
        var result = new StringBuilder();
        var name = obj.GetType().Name;
        var properties = obj.GetType().GetProperties();

        result.Append($"[{name}]\n");

        foreach (var property in properties)
        {
            result.Append($"{property.Name}: {property.GetValue(obj)}");
        }

        return result.ToString();
    }

    public static string ToDebugString<T>(this IEnumerable<T> obj)
    {
        var result = new StringBuilder();
        var type = obj.GetType();
        var count = type.GetProperty("Count")?.GetValue(obj);
        var capacity = type.GetProperty("Capacity")?.GetValue(obj);

        result.Append($"[{type.Name}<{typeof(T).Name}>]({count}/{capacity})\n");

        foreach (var item in obj)
        {
            result.Append($"{item?.ToDebugString()}\n");
        }

        return result.ToString();
    }

    public static void Dump(this object obj)
    {
        Console.WriteLine($"{ToDebugString(obj)}");
    }
}


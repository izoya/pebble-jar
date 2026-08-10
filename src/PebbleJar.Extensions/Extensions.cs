namespace PebbleJar.Extensions;

public static class Extensions
{
    public static void Dump(this object obj)
    {
        var name = obj.GetType().Name;
        var properties = obj.GetType().GetProperties();

        Console.WriteLine($"[{name}]");

        foreach (var property in properties)
        {
            Console.WriteLine($"{property.Name}: {property.GetValue(obj)}");
        }
    }

    public static void Dump<T>(this IEnumerable<T> obj)
    {
        var type = obj.GetType();
        var count = type.GetProperty("Count")?.GetValue(obj);
        var capacity = type.GetProperty("Capacity")?.GetValue(obj);

        Console.WriteLine($"[{type.Name}<{typeof(T).Name}>]({count}/{capacity})");
        foreach (var item in obj)
        {
            item?.Dump();
            Console.WriteLine();
        }
    }
}


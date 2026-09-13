using PebbleJar.Application.Queries;
using PebbleJar.Domain;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PebbleJar.Api.Serialization;

public class GroupingKeyJsonConverter : JsonConverter<GroupingKey>
{
    public override GroupingKey? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var grouping = ParseEnumName<TransactionGrouping>(ReadString(root, "type"));
        var value = ReadString(root, "value");

        return grouping switch
        {
            TransactionGrouping.Day or
            TransactionGrouping.Week or
            TransactionGrouping.Fortnight or
            TransactionGrouping.Month or
            TransactionGrouping.Year => DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date)
                ? new GroupingKey(grouping, date)
                : throw new JsonException("Expected a date in yyyy-MM-dd format."),
            TransactionGrouping.TransactionType => new GroupingKey(grouping, ParseEnumName<TransactionType>(value)),
            TransactionGrouping.TransactionCategory => new GroupingKey(grouping, ParseEnumName<TransactionCategory>(value)),
            _ => throw new JsonException("Unsupported grouping type.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        GroupingKey value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", value.Key.ToString());
        writer.WriteString("value", value.Value.Match(
            (DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (TransactionType type) => type.ToString(),
            (TransactionCategory category) => category.ToString()));
        writer.WriteEndObject();
    }

    private static string ReadString(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(name, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"`{name}` must be a string.");
        }

        return property.GetString()!;
    }

    private static T ParseEnumName<T>(string value)
        where T : struct, Enum
    {
        if (!Enum.TryParse<T>(value, out var result)
            || !Enum.IsDefined(result)
            || result.ToString() != value)
        {
            throw new JsonException($"Unknown {typeof(T).Name}: '{value}.");
        }

        return result;
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PebbleJar.Extensions.Types
{
    [DebuggerDisplay("[REDACTED]")]
    [TypeConverter(typeof(SecretStringTypeConverter))]
    [JsonConverter(typeof(SecretStringJsonConverter))]
    public sealed class SecretString(string Value)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _value = Value;

        public override string ToString()
        {
            return "[REDACTED]";
        }

        public string Reveal()
        {
            return _value;
        }
    }

    public sealed class SecretStringJsonConverter : JsonConverter<SecretString>
    {
        public override SecretString Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new JsonException("SecretString cannot be read from JSON.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            SecretString value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue("[REDACTED]");
        }
    }

    public sealed class SecretStringTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(
            ITypeDescriptorContext? context,
            Type sourceType)
        {
            return sourceType == typeof(string)
                || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object value)
        {
            return value is string text
                ? new SecretString(text)
                : base.ConvertFrom(context, culture, value);
        }
    }
}

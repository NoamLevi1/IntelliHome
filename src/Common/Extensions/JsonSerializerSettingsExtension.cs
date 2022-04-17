using Newtonsoft.Json;

namespace IntelliHome.Common;

public static class JsonSerializerSettingsExtension
{
    public static JsonSerializerSettings ConfigureCommon(this JsonSerializerSettings settings)
    {
        settings.TypeNameHandling = TypeNameHandling.Objects;

        settings.Converters.Add(new ObjectStringJsonConverter<HttpMethod>(method => method.ToString(), str => new HttpMethod(str)));
        settings.Converters.Add(new ObjectStringJsonConverter<Memory<byte>>(memory => Convert.ToBase64String(memory.Span), base64 => Convert.FromBase64String(base64)));
        settings.Converters.Add(new ObjectStringJsonConverter<ReadOnlyMemory<byte>>(memory => Convert.ToBase64String(memory.Span), base64 => Convert.FromBase64String(base64)));

        settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;

        return settings;
    }

    private sealed class ObjectStringJsonConverter<TObject> : JsonConverter
    {
        private readonly Func<TObject, string> _serializer;
        private readonly Func<string, TObject> _deserializer;

        public ObjectStringJsonConverter(
            Func<TObject, string> serializer,
            Func<string, TObject> deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                serializer.Serialize(writer, _serializer((TObject) value));
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return default;
            }

            var str = serializer.Deserialize<string>(reader);
            return str is null
                ? default(object?)
                : _deserializer(str);
        }

        public override bool CanConvert(Type objectType) =>
            typeof(TObject).IsAssignableFrom(objectType);
    }
}
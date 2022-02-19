using Newtonsoft.Json;

namespace IntelliHome.Common;

public static class JsonSerializerSettingsExtension
{
    public static JsonSerializerSettings ConfigureCommon(this JsonSerializerSettings settings)
    {
        settings.TypeNameHandling = TypeNameHandling.Objects;

        settings.Converters.Add(new HttpContentJsonConverter());

        return settings;
    }

    private class HttpContentJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                serializer.Serialize(writer, ((HttpContent) value).ReadAsByteArrayAsync().Await());
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var bytes = serializer.Deserialize<byte[]>(reader);

            return bytes == null
                ? null
                : new ByteArrayContent(bytes);
        }

        public override bool CanConvert(Type objectType) => 
            typeof(HttpContent).IsAssignableFrom(objectType);
    }
}
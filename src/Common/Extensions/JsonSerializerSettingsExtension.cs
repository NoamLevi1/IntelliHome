using Newtonsoft.Json;

namespace IntelliHome.Common;

public static class JsonSerializerSettingsExtension
{
    public static JsonSerializerSettings ConfigureCommon(this JsonSerializerSettings settings)
    {
        settings.TypeNameHandling = TypeNameHandling.Objects;

        return settings;
    }
}
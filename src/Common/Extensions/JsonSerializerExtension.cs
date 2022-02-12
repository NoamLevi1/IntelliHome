using Newtonsoft.Json;

namespace IntelliHome.Common;

public static class JsonSerializerExtension
{
    public static string SerializeToString(this JsonSerializer jsonSerializer, object obj)
    {
        using var stringWriter = new StringWriter();
        jsonSerializer.Serialize(stringWriter, obj);
        return stringWriter.ToString();
    }
}
using IntelliHome.Common;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace IntelliHome.Cli;

public sealed class YamlSerializer
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public YamlSerializer()
    {
        _serializer = new SerializerBuilder().WithTagMapping(YamlFileReference.TagName, typeof(YamlFileReference)).Build();
        _deserializer = new DeserializerBuilder().WithTagMapping(YamlFileReference.TagName, typeof(YamlFileReference)).Build();
    }

    public TItem Deserialize<TItem>(string yaml)
    {
        Ensure.NotNullOrWhiteSpace(yaml);

        return _deserializer.Deserialize<TItem>(yaml);
    }

    public string Serialize(object item)
    {
        Ensure.NotNull(item);

        return _serializer.Serialize(item);
    }

    private sealed class YamlFileReference : IYamlConvertible
    {
        public const string TagName = "!include";

        private string? _fileName;

        public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            var scalar = parser.Consume<Scalar>();
            _fileName = scalar.Value;
        }

        public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            emitter.Emit(
                new Scalar(
                    new AnchorName(),
                    TagName,
                    _fileName ?? string.Empty,
                    ScalarStyle.Plain,
                    false,
                    false));
        }
    }
}
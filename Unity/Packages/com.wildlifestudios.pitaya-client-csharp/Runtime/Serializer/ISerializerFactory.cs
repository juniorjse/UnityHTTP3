using Newtonsoft.Json;

namespace Wildlife.PitayaCSharp.Serializer
{
    public interface ISerializerFactory
    {
        IPitayaSerializer CreateJsonSerializer();
        IPitayaSerializer CreateProtobufSerializer(ProtobufSerializer.SerializationFormat format);
    }

    public class SerializerFactory : ISerializerFactory
    {
        readonly JsonSerializerSettings _settings;

        public SerializerFactory() : this(new JsonSerializerSettings()) {}
        
        public SerializerFactory(JsonSerializerSettings settings)
        {
            _settings = settings;
        }
        
        public IPitayaSerializer CreateJsonSerializer()
        {
            return new JsonSerializer(_settings);
        }
        
        public IPitayaSerializer CreateProtobufSerializer(ProtobufSerializer.SerializationFormat format)
        {
            return new ProtobufSerializer(format);
        }
        
    }
}
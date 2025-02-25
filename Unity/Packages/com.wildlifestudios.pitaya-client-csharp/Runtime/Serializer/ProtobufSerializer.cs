using System;
using System.Text;
using Google.Protobuf;
using Newtonsoft.Json;

namespace Wildlife.PitayaCSharp.Serializer
{
    public class ProtobufSerializer : IPitayaSerializer
    {
        public enum SerializationFormat
        {        
            [JsonProperty("json")]
            Json,
            [JsonProperty("protobuf")]
            Protobuf
        }
        readonly SerializationFormat _format;

        public ProtobufSerializer(SerializationFormat format)
        {
            _format = format;
        }
        
        public byte[] Encode(object message)
        {
            byte[] buffer = {};
            switch (_format)
            {
                case SerializationFormat.Protobuf:
                    buffer = ((IMessage) message).ToByteArray();
                    break;
                case SerializationFormat.Json:
                    var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
                    var jsonString = jsonFormatter.Format((IMessage)message);
                    buffer = Encoding.UTF8.GetBytes(jsonString);
                    break;
                default:
                    throw new Exception("Undefined SerializationFormat");
            }

            return buffer;
        }
        
        public T Decode<T>(byte[] buffer)
        {
            IMessage res = (IMessage) Activator.CreateInstance(typeof(T));
            switch (_format)
            {
                case SerializationFormat.Protobuf:
                    res.MergeFrom(buffer);
                    break;
                case SerializationFormat.Json:
                    var stringified = Encoding.UTF8.GetString(buffer);
                    res = JsonParser.Default.Parse(stringified, res.Descriptor);
                    break;
                default:
                    throw new Exception("Undefined SerializationFormat");
            }

            return (T)res;
        }
    }
}

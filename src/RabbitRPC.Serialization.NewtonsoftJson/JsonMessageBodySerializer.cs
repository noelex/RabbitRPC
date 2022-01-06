using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RabbitRPC.Serialization.NewtonsoftJson
{
    internal class JsonMessageBodySerializer : IMessageBodySerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
        };

        private ReadOnlyMemory<byte> DoSerialize<T>(T obj)
        {
            var str=JsonConvert.SerializeObject(obj, _settings);
            return Encoding.UTF8.GetBytes(str);
        }

        public ReadOnlyMemory<byte> Serialize(IRequestMessageBody body) => DoSerialize(body);

        public ReadOnlyMemory<byte> Serialize(IResponseMessageBody body) => DoSerialize(body);

        public ReadOnlyMemory<byte> Serialize<T>(T body) => DoSerialize(body);

        public IRequestMessageBody DeserializeRequest(ReadOnlyMemory<byte> data)
        {
            var str = Encoding.UTF8.GetString(data.Span);
            return JsonConvert.DeserializeObject<JsonRequestBody>(str, _settings)!;
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> data, Type targetType)
        {
            var str = Encoding.UTF8.GetString(data.Span);
            return JsonConvert.DeserializeObject<T>(str, _settings)!;
        }

        public IResponseMessageBody DeserializeResponse(ReadOnlyMemory<byte> data)
        {
            var str=Encoding.UTF8.GetString(data.Span);
            return JsonConvert.DeserializeObject<JsonResponseBody>(str, _settings)!;
        }

        public object? DeserializeWithTypeInfo(ReadOnlyMemory<byte> data)
        {
            var str = Encoding.UTF8.GetString(data.Span);
            return JsonConvert.DeserializeObject(str);
        }

        public ReadOnlyMemory<byte> SerializeWithTypeInfo(object? obj)
        {
            var str=JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(str);
        }
    }
}

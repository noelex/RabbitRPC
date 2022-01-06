using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RabbitRPC.Serialization
{
    public interface IMessageBodySerializer
    {
        ReadOnlyMemory<byte> Serialize(IRequestMessageBody body);

        ReadOnlyMemory<byte> Serialize(IResponseMessageBody body);

        ReadOnlyMemory<byte> Serialize<T>(T body);

        IRequestMessageBody DeserializeRequest(ReadOnlyMemory<byte> data);

        IResponseMessageBody DeserializeResponse(ReadOnlyMemory<byte> data);

        T Deserialize<T>(ReadOnlyMemory<byte> data, Type targetType);

        object? DeserializeWithTypeInfo(ReadOnlyMemory<byte> data);

        ReadOnlyMemory<byte> SerializeWithTypeInfo(object? obj);
    }
}

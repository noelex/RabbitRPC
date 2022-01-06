using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RabbitRPC.Serialization.MessagePack
{
    internal class MessagePackMessageBodySerializer : IMessageBodySerializer
    {
        private static readonly MessagePackSerializerOptions _options =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithResolver(CompositeResolver.Create(MessagePackExceptionResolver.Instance, ContractlessStandardResolverAllowPrivate.Instance));

        //ContractlessStandardResolverAllowPrivate

        private ReadOnlyMemory<byte> DoSerialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj!.GetType(), obj, _options);
        }

        public ReadOnlyMemory<byte> Serialize(IRequestMessageBody body) => DoSerialize(body);

        public ReadOnlyMemory<byte> Serialize(IResponseMessageBody body) => DoSerialize(body);

        public ReadOnlyMemory<byte> Serialize<T>(T body) => DoSerialize(body);

        public IRequestMessageBody DeserializeRequest(ReadOnlyMemory<byte> data)
        {
            return MessagePackSerializer.Deserialize<MessagePackRequestBody>(new System.Buffers.ReadOnlySequence<byte>(data), _options);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> data, Type targetType)
        {
            return MessagePackSerializer.Deserialize<T>(new System.Buffers.ReadOnlySequence<byte>(data), _options);
        }

        public IResponseMessageBody DeserializeResponse(ReadOnlyMemory<byte> data)
        {
            return MessagePackSerializer.Deserialize<MessagePackResponseBody>(new System.Buffers.ReadOnlySequence<byte>(data), _options);
        }

        public object? DeserializeWithTypeInfo(ReadOnlyMemory<byte> data)
        {
            return MessagePackSerializer.Typeless.Deserialize(new System.Buffers.ReadOnlySequence<byte>(data));
        }

        public ReadOnlyMemory<byte> SerializeWithTypeInfo(object? obj)
        {
            return MessagePackSerializer.Typeless.Serialize(obj);
        }
    }
}

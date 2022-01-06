using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MessagePack;
using MessagePack.Formatters;

namespace RabbitRPC.Serialization.MessagePack
{
    /// <summary>
    /// Manages serialization of any <see cref="Exception"/>-derived type that follows standard <see cref="SerializableAttribute"/> rules.
    /// </summary>
    /// <remarks>
    /// A serializable class will:
    /// 1. Derive from <see cref="Exception"/>
    /// 2. Be attributed with <see cref="SerializableAttribute"/>
    /// 3. Declare a constructor with a signature of (<see cref="SerializationInfo"/>, <see cref="StreamingContext"/>).
    /// </remarks>
    internal class MessagePackExceptionResolver : IFormatterResolver
    {
        internal static readonly MessagePackExceptionResolver Instance = new MessagePackExceptionResolver();

        private MessagePackExceptionResolver()
        {
        }

        public IMessagePackFormatter<T>? GetFormatter<T>() => Cache<T>.Formatter;

        private static class Cache<T>
        {
            internal static readonly IMessagePackFormatter<T>? Formatter;

            static Cache()
            {
                if (typeof(Exception).IsAssignableFrom(typeof(T)) && typeof(T).GetCustomAttribute<SerializableAttribute>() is object)
                {
                    Formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(typeof(ExceptionFormatter<>).MakeGenericType(typeof(T)))!;
                }
            }
        }

        private class ExceptionFormatter<T> : IMessagePackFormatter<T>
            where T : Exception
        {
            private static readonly IFormatterConverter FormatterConverter = new FormatterConverter();

            private static readonly Type[] DeserializingConstructorParameterTypes = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };

            private static StreamingContext Context => new StreamingContext(StreamingContextStates.Remoting);

            public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil())
                {
                    return null!;
                }

                var info = new SerializationInfo(typeof(T), FormatterConverter);
                int memberCount = reader.ReadMapHeader();
                for (int i = 0; i < memberCount; i++)
                {
                    string name = reader.ReadString();
                    object value = TypelessFormatter.Instance.Deserialize(ref reader, options);
                    info.AddValue(name, value);
                }

                string? runtimeTypeName = info.GetString("ClassName");
                if (runtimeTypeName is null)
                {
                    throw new MessagePackSerializationException("ClassName was not found in the serialized data.");
                }

                Type? runtimeType = Type.GetType(runtimeTypeName);
                if (runtimeType is null)
                {
                    throw new MessagePackSerializationException($"{runtimeTypeName} type could not be loaded.");
                }

                // Sanity/security check: ensure the runtime type derives from the expected type.
                if (!typeof(T).IsAssignableFrom(runtimeType))
                {
                    throw new MessagePackSerializationException($"{runtimeTypeName} does not derive from {typeof(T).FullName}.");
                }

                EnsureSerializableAttribute(runtimeType);

                ConstructorInfo? ctor = FindDeserializingConstructor(runtimeType);
                if (ctor is null)
                {
                    throw new MessagePackSerializationException($"{runtimeType.FullName} does not declare a deserializing constructor with signature ({string.Join(", ", DeserializingConstructorParameterTypes.Select(t => t.FullName))}).");
                }

                return (T)ctor.Invoke(new object?[] { info, Context });
            }

            public void Serialize(ref MessagePackWriter writer, T? value, MessagePackSerializerOptions options)
            {
                if (value is null)
                {
                    writer.WriteNil();
                    return;
                }

                EnsureSerializableAttribute(value.GetType());

                var info = new SerializationInfo(typeof(T), FormatterConverter);
                value.GetObjectData(info, Context);
                writer.WriteMapHeader(info.MemberCount);
                foreach (SerializationEntry element in info)
                {
                    writer.Write(element.Name);
                    TypelessFormatter.Instance.Serialize(ref writer, element.Value, options);
                }
            }

            private static void EnsureSerializableAttribute(Type runtimeType)
            {
                // If the value being serialized isn't exactly our generic type argument, then make sure this actual object
                // is attributed as Serializable. Otherwise, the exception may not be serializing all its data.
                if (typeof(T) != runtimeType.GetType() && typeof(T).GetCustomAttribute<SerializableAttribute>() is null)
                {
                    throw new MessagePackSerializationException($"{typeof(T).FullName} is not marked with the {typeof(SerializableAttribute).FullName}.");
                }
            }

            private static ConstructorInfo? FindDeserializingConstructor(Type runtimeType) => runtimeType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, DeserializingConstructorParameterTypes, null);
        }
    }
}


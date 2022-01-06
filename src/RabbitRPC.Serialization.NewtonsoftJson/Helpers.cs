using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RabbitRPC.Serialization.NewtonsoftJson
{
    internal static class Helpers
    {
        public static object? EnsureObjectType(object? obj, Type targetType)
        {
            if (obj != null)
            {
                var sourceType = obj.GetType();

                var targetTypeConverter = TypeDescriptor.GetConverter(targetType);
                if (targetTypeConverter.CanConvertFrom(sourceType))
                {
                    return targetTypeConverter.ConvertFrom(obj);
                }

                if (targetType.IsEnum)
                {
                    return Enum.ToObject(targetType, obj);
                }

                if (sourceType.IsPrimitive && targetType != sourceType)
                {
                    return Convert.ChangeType(obj, targetType);
                }
            }

            return obj;
        }
    }
}

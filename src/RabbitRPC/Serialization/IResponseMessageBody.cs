using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace RabbitRPC.Serialization
{
    public interface IResponseMessageBody
    {
        void SetReturnValue(object? returnValue);

        object? GetReturnValue(Type targetType);

        Exception? Exception { get; set; }

        bool IsCanceled { get; set; }
    }
}

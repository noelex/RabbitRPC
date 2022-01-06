using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Serialization
{
    public interface IRequestMessageBody
    {
        object? GetParameter(int position, string parameName, Type targetType);

        void SetParameter(int position, string parameName, object? parameter);
    }
}

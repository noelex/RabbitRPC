using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RabbitRPC.Serialization.MessagePack
{
    [DataContract]
    class MessagePackRequestBody : IRequestMessageBody
    {
        [DataMember]
        private Dictionary<string, object?> _parameters = new Dictionary<string, object?>();

        public object? GetParameter(int position, string parameName, Type targetType) => _parameters[parameName];

        public void SetParameter(int position, string parameName, object? parameter) => _parameters[parameName] = parameter;
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Text;

namespace RabbitRPC.Serialization.MessagePack
{
    [DataContract]
    class MessagePackResponseBody : IResponseMessageBody
    {
        [DataMember]
        private bool _isCanceled;

        [DataMember]
        private object? _returnValue;

        [DataMember]
        private Exception? _exception;

        [DataMember]
        private Dictionary<string,object?> _byRefParameters=new Dictionary<string,object?>();

        public Exception? Exception { get => _exception; set => _exception = value; }

        public bool IsCancelled { get => _isCanceled; set => _isCanceled = value; }

        public object? GetByRefParameter(int position, string paramName, Type targetType)
        {
            return _byRefParameters[paramName];
        }

        public object? GetReturnValue(Type targetType)
        {
            return _returnValue;
        }

        public void SetByRefParameter(int position, string paramName, object? value)
        {
            _byRefParameters[paramName] = value;
        }

        public void SetReturnValue(object? returnValue)
        {
            _returnValue= returnValue;
        }
    }
}

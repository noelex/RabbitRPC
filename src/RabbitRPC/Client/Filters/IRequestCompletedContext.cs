using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IRequestCompletedContext
    {
        bool Canceled { get; set; }

        RequestStatus Status { get; set; }

        bool ExceptionHandled { get; set; }

        Exception? Exception { get; set; }

        object? Result { get; set; }
    }
}

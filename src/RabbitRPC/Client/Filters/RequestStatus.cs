using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public enum RequestStatus
    {
        Sucess,
        Aborted,
        TimedOut,
        ServerError,
        ClientError
    }
}

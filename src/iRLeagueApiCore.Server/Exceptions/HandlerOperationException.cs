using System.Runtime.Serialization;

namespace iRLeagueApiCore.Server.Exceptions;

public class HandlerOperationException : InvalidOperationException
{
    public HandlerOperationException()
    {
    }

    public HandlerOperationException(string message) : base(message)
    {
    }

    public HandlerOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected HandlerOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

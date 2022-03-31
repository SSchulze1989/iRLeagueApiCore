using System;
using System.Runtime.Serialization;

namespace iRLeagueApiCore.Server.Exceptions
{
    public class ResourceNotFoundException : Exception
    {
        public string ResourceName { get; }
        public ResourceNotFoundException() : this("Requested resource was not found")
        {
        }

        public ResourceNotFoundException(string message) : base(message)
        {
        }

        public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

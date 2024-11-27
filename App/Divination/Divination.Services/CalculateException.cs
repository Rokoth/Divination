using System;
using System.Runtime.Serialization;

namespace Divination.Services
{
    internal class CalculateException : Exception
    {
        public CalculateException()
        {
        }

        public CalculateException(string message) : base(message)
        {
        }

        public CalculateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CalculateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
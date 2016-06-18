using System;

namespace Beta.Platform.Exceptions
{
    public class ProcessorJammedException : Exception
    {
        public ProcessorJammedException(string message)
            : base(message)
        {
        }
    }
}

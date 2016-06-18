using System;

namespace Beta.Platform.Exceptions
{
    public class CompilerPleasingException : Exception
    {
        public CompilerPleasingException()
            : base("This is exception was meant to appease the compiler. If you're seeing this then I screwed up.")
        {
        }
    }
}

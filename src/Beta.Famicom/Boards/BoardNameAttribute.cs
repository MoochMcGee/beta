using System;

namespace Beta.Famicom.Boards
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BoardNameAttribute : Attribute
    {
        public string Pattern { get; }

        public BoardNameAttribute(string pattern)
        {
            this.Pattern = pattern;
        }
    }
}

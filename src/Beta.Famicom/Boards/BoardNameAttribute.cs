using System;

namespace Beta.Famicom.Boards
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BoardNameAttribute : Attribute
    {
        public string Name { get; }

        public BoardNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}

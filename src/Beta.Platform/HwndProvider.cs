using System;

namespace Beta.Platform
{
    public sealed class HwndProvider
    {
        private readonly IntPtr handle;

        public HwndProvider(IntPtr handle)
        {
            this.handle = handle;
        }

        public IntPtr GetHandle() => handle;
    }
}

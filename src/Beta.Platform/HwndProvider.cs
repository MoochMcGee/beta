using System;

namespace Beta.Platform
{
    public sealed class HwndProvider : IHwndProvider
    {
        private readonly IntPtr handle;

        public HwndProvider(IntPtr handle)
        {
            this.handle = handle;
        }

        public IntPtr GetHandle() => handle;
    }
}

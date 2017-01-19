using System;
using Beta.Platform.Configuration;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Beta.Platform.Video
{
    public sealed class VideoBackend : IVideoBackend
    {
        private readonly IntPtr handle;
        private readonly int width;
        private readonly int height;
        private readonly int[][] screen;

        private Device device;
        private DeviceContext context;
        private RenderTargetView renderTargetView;
        private SwapChain swapChain;
        private Texture2D texture;

        public VideoBackend(HwndProvider hwndProvider, ConfigurationFile config)
        {
            this.handle = hwndProvider.GetHandle();
            this.width = config.Video.Width;
            this.height = config.Video.Height;
            this.screen = Utility.CreateArray<int>(height, width);
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                Flags = SwapChainFlags.None,
                IsWindowed = true,
                ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                OutputHandle = handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDescription, out device, out swapChain);
            context = device.ImmediateContext;

            texture = new Texture2D(device, new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = 0
            });

            using (var resource = swapChain.GetBackBuffer<Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(device, resource);
            }

            context.OutputMerger.SetRenderTargets(renderTargetView);
        }

        public void Render()
        {
            UpdateTexture();

            swapChain.Present(1, PresentFlags.None);
        }

        private void UpdateTexture()
        {
            DataStream stream;
            var source = context.MapSubresource(texture, 0, 0, MapMode.WriteDiscard, MapFlags.None, out stream);

            var strideSize = source.RowPitch - (width * sizeof(int));
            if (strideSize != 0)
            {
                var stride = new byte[strideSize];

                foreach (var raster in screen)
                {
                    stream.WriteRange(raster, 0, raster.Length);
                    stream.Write(stride, 0, stride.Length);
                }
            }
            else
            {
                foreach (var raster in screen)
                {
                    stream.WriteRange(raster, 0, raster.Length);
                }
            }

            context.UnmapSubresource(texture, 0);
            context.UpdateSubresource(source, renderTargetView.Resource);
        }

        public int[] GetRaster(int line)
        {
            return screen[line];
        }
    }
}

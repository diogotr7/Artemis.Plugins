using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Ambilight.PropertyGroups;
using SkiaSharp;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Usage = Vortice.Direct3D11.Usage;

namespace Artemis.Plugins.LayerBrushes.Ambilight
{
    // This is the layer brush, the plugin feature has provided it to Artemis via a descriptor
    // Artemis may create multiple instances of it, one instance for each profile element (folder/layer) it is applied to
    public class PluginLayerBrush : LayerBrush<MainPropertyGroup>
    {
        private static readonly FeatureLevel[] s_featureLevels = new[]
        {
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1,
        };

        private IDXGIOutputDuplication duplication;
        private ID3D11Texture2D stagingTexture;
        private ID3D11Device device;
        private SKBitmap skBitmap = new SKBitmap(1920 / 2, 1080 / 2);
        double time;
        private ID3D11Texture2D smallerTexture;
        private ID3D11ShaderResourceView smallerTextureView;

        public override void EnableLayerBrush()
        {
            var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            var adapter = factory.GetAdapter1(0);

            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.None, s_featureLevels, out device);

            var output = adapter.GetOutput(0);
            var output1 = output.QueryInterface<IDXGIOutput1>();

            var bounds = output1.Description.DesktopCoordinates;
            var width = bounds.Right - bounds.Left;
            var height = bounds.Bottom - bounds.Top;

            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width / 2,
                Height = height / 2,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = Usage.Staging
            };
            stagingTexture = device.CreateTexture2D(textureDesc);

            var smallerTextureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.GenerateMips,
                MipLevels = 4,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = Usage.Default
            };

            smallerTexture = device.CreateTexture2D(smallerTextureDesc);
            smallerTextureView = device.CreateShaderResourceView(smallerTexture);

            duplication = output1.DuplicateOutput(device);
            
            Thread.Sleep(100);
        }

        public override void DisableLayerBrush()
        {
        }

        public override void Update(double deltaTime)
        {
            //if (time < 1)
            //{
            //    time += deltaTime;
            //    return;
            //}
            //else
            //{
            //    time = 0;
            //}\
            IDXGIResource screenResource;
            OutduplFrameInfo frameInfo;

            duplication.AcquireNextFrame(500, out frameInfo, out screenResource);

            using (var tempTexture = screenResource.QueryInterface<ID3D11Texture2D>())
                device.ImmediateContext.CopySubresourceRegion(smallerTexture, 0, 0, 0, 0, tempTexture, 0);

            device.ImmediateContext.GenerateMips(smallerTextureView);
            device.ImmediateContext.CopySubresourceRegion(stagingTexture, 0, 0, 0, 0, smallerTexture, 1);

            var dataBox = device.ImmediateContext.Map(stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

            ProcessDataIntoSKBitmap(dataBox);

            device.ImmediateContext.Unmap(stagingTexture, 0);

            screenResource.Dispose();
            duplication?.ReleaseFrame();
        }

        private void ProcessDataIntoSKBitmap(MappedSubresource dataBox)
        {
            int width = dataBox.RowPitch / 4;
            int height = dataBox.DepthPitch / width / 4;

            var skInfo = new SKImageInfo
            {
                ColorType = SKColorType.Bgra8888,
                AlphaType = SKAlphaType.Premul,
                Height = height,
                Width = width
            };
            var skPixmap = new SKPixmap(skInfo, dataBox.DataPointer);
            skBitmap.InstallPixels(skPixmap);
        }

        public override void Render(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
            canvas.DrawBitmap(skBitmap.Resize(new SKImageInfo((int)bounds.Width, (int)bounds.Height), SKFilterQuality.Medium), 0, 0);
        }
    }
}
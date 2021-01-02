using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Ambilight.PropertyGroups;
using SkiaSharp;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

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
        private ID3D11Texture2D texture;
        private ID3D11Device device;
        private SKBitmap skBitmap = new SKBitmap();
        double time;

        public override void EnableLayerBrush()
        {
            var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            var adapter = factory.GetAdapter(0);
            var output = adapter.GetOutput(0);
            var output1 = output.QueryInterface<IDXGIOutput1>();
            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.Debug, s_featureLevels, out device);

            var bounds = output1.Description.DesktopCoordinates;
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = bounds.Right - bounds.Left,
                Height = bounds.Bottom - bounds.Top,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = Vortice.Direct3D11.Usage.Staging
            };

            duplication = output1.DuplicateOutput(device);
            texture = device.CreateTexture2D(textureDesc);

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
            //}

            duplication.AcquireNextFrame(500, out var frameInfo, out var desktopResource);
            using (desktopResource)
            {
                using var tempTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
                device.ImmediateContext.CopyResource(texture, tempTexture);
            }

            duplication?.ReleaseFrame();

            var dataBox = device.ImmediateContext.Map(texture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

            ProcessDataIntoSKBitmap(dataBox);
        }

        private void ProcessDataIntoSKBitmap(MappedSubresource dataBox)
        {
            var skInfo = new SKImageInfo
            {
                ColorType = SKColorType.Bgra8888,
                AlphaType = SKAlphaType.Premul,
                Height = 1080,
                Width = 1920
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
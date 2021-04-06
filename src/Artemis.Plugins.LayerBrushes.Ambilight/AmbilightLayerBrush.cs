using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Ambilight.PropertyGroups;
using SkiaSharp;
using System;
using System.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Usage = Vortice.Direct3D11.Usage;

namespace Artemis.Plugins.LayerBrushes.Ambilight
{
    public class AmbilightLayerBrush : PerLedLayerBrush<AmbilightPropertyGroup>
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
        private readonly object pixmapLock = new object();
        private SKPixmap pixmap;
        private IDXGIFactory1 factory;
        private IDXGIOutputDuplication duplication;
        private ID3D11Texture2D stagingTexture;
        private ID3D11Device device;
        private ID3D11Texture2D smallerTexture;
        private ID3D11ShaderResourceView smallerTextureView;
        private System.Timers.Timer releaseLastFrameTimer;
        private bool release = false;

        public override void EnableLayerBrush()
        {
            factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();

            StartDesktopDuplicator(0, 0);
            Thread.Sleep(100);

            releaseLastFrameTimer = new System.Timers.Timer(5000);
            releaseLastFrameTimer.Elapsed += ReleaseLastFrameTimer_Elapsed;
        }

        private void ReleaseLastFrameTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (release)
            {
                duplication.ReleaseFrame();
                release = false;
            }
        }

        public override void DisableLayerBrush()
        {
            releaseLastFrameTimer.Elapsed -= ReleaseLastFrameTimer_Elapsed;
            releaseLastFrameTimer.Dispose();
            StopDesktopDuplicator();
        }

        public override void Update(double deltaTime)
        {
            releaseLastFrameTimer.Stop();
            releaseLastFrameTimer.Start();
            IDXGIResource screenResource;
            OutduplFrameInfo frameInfo;
            try
            {
                if (release)
                {
                    duplication.ReleaseFrame();
                    release = false;
                }

                duplication.AcquireNextFrame(500, out frameInfo, out screenResource);

                using (var tempTexture = screenResource.QueryInterface<ID3D11Texture2D>())
                    device.ImmediateContext.CopySubresourceRegion(smallerTexture, 0, 0, 0, 0, tempTexture, 0);

                device.ImmediateContext.GenerateMips(smallerTextureView);
                device.ImmediateContext.CopySubresourceRegion(stagingTexture, 0, 0, 0, 0, smallerTexture, 1);

                var dataBox = device.ImmediateContext.Map(stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

                ProcessDataIntoSKPixmap(dataBox);

                device.ImmediateContext.Unmap(stagingTexture, 0);

                screenResource.Dispose();
                release = true;
                //duplication.ReleaseFrame();
            }
            catch (SharpGen.Runtime.SharpGenException e)
            {
                if (e.ResultCode == Vortice.DXGI.ResultCode.AccessLost)
                {
                    StartDesktopDuplicator(0, 0);
                }
                else if (e.ResultCode == Vortice.DXGI.ResultCode.WaitTimeout)
                {
                    //ignore
                }
                else
                {

                }
                //throw;
            }
        }

        private void ProcessDataIntoSKPixmap(MappedSubresource dataBox)
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

            lock (pixmapLock)
            {
                pixmap = new SKPixmap(skInfo, dataBox.DataPointer);
            }
        }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            const int sampleSize = 9;
            const int sampleDepth = 3;

            lock (pixmapLock)
            {
                var renderBounds = Layer.Bounds;
                var widthScale = pixmap.Width / renderBounds.Width;
                var heightScale = pixmap.Height / renderBounds.Height;
                int x = (int)Math.Max((renderPoint.X * widthScale), 0);
                int y = (int)Math.Max((renderPoint.Y * heightScale), 0);
                int width = (int)(led.Rectangle.Width * widthScale);
                int height = (int)(led.Rectangle.Height * heightScale);

                int verticalSteps = height / (sampleDepth - 1);
                int horizontalSteps = width / (sampleDepth - 1);

                int a = 0, r = 0, g = 0, b = 0;
                for (int horizontalStep = 0; horizontalStep < sampleDepth; horizontalStep++)
                {
                    for (int verticalStep = 0; verticalStep < sampleDepth; verticalStep++)
                    {
                        var bruhX = x + horizontalSteps * horizontalStep;
                        var bruhY = y + verticalSteps * verticalStep;
                        SKColor color = pixmap.GetPixelColor(
                            Math.Min(bruhX, pixmap.Width - 1),
                            Math.Min(bruhY, pixmap.Height - 1)
                            );
                        r += color.Red;
                        g += color.Green;
                        b += color.Blue;
                        a += color.Alpha;
                    }
                }
                return new SKColor((byte)(r / sampleSize), (byte)(g / sampleSize), (byte)(b / sampleSize));
            }
        }

        //adapted from https://stackoverflow.com/questions/24064837/resizing-a-dxgi-resource-or-texture2d-in-sharpdx
        private void StartDesktopDuplicator(int adapterId, int outputId)
        {
            var adapter = factory.GetAdapter1(adapterId);

            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.Debug, s_featureLevels, out device);

            var output = adapter.GetOutput(outputId);
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
        }

        private void StopDesktopDuplicator()
        {
            device?.Dispose();
            stagingTexture?.Dispose();
            smallerTexture?.Dispose();
            smallerTextureView?.Dispose();
            duplication.Dispose();
        }
    }
}
using Artemis.Core.DataModelExpansions;
using SkiaSharp;
using System;
using System.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma.DataModelExpansion
{
    public class ChromaDataModelExpansion : DataModelExpansion<ChromaDataModel>
    {
        private readonly ChromaPluginService _chroma;
        public ChromaDataModelExpansion(ChromaPluginService chroma)
        {
            _chroma = chroma;
        }

        public override void Enable()
        {
            _chroma.MatrixUpdated += UpdateMatrix;
            _chroma.AppListUpdated += UpdateAppList;
        }

        public override void Disable()
        {
            _chroma.MatrixUpdated -= UpdateMatrix;
            _chroma.AppListUpdated -= UpdateAppList;
        }

        public override void Update(double deltaTime)
        {

        }

        private void UpdateAppList(object sender, EventArgs e)
        {
            DataModel.CurrentApplication = _chroma.CurrentApp;
            DataModel.ApplicationList = _chroma.Apps;
            DataModel.PidList = _chroma.Pids;
        }

        private void UpdateMatrix(object sender, RzDeviceType e)
        {
            switch (e)
            {
                case RzDeviceType.Mousepad:
                    if (_chroma.Matrices.TryGetValue(RzDeviceType.Mousepad, out SKColor[,] m1))
                    {
                        DataModel.Mousepad = m1.Cast<SKColor>().ToArray();
                    }
                    break;
                case RzDeviceType.Mouse:

                    if (_chroma.Matrices.TryGetValue(RzDeviceType.Mouse, out SKColor[,] m2))
                    {
                        DataModel.Mouse = m2.Cast<SKColor>().ToArray();
                    }
                    break;
                case RzDeviceType.Keypad:

                    if (_chroma.Matrices.TryGetValue(RzDeviceType.Keypad, out SKColor[,] m3))
                    {
                        DataModel.Keypad = m3.Cast<SKColor>().ToArray();
                    }
                    break;
                case RzDeviceType.Keyboard:
                    if (_chroma.Matrices.TryGetValue(RzDeviceType.Keyboard, out SKColor[,] m4))
                    {
                        DataModel.Keyboard = m4.Cast<SKColor>().ToArray();
                    }
                    break;
                case RzDeviceType.Headset:
                    if (_chroma.Matrices.TryGetValue(RzDeviceType.Headset, out SKColor[,] m5))
                    {
                        DataModel.Headset = m5.Cast<SKColor>().ToArray();
                    }
                    break;
                case RzDeviceType.ChromaLink:
                    if (_chroma.Matrices.TryGetValue(RzDeviceType.ChromaLink, out SKColor[,] m7))
                    {
                        DataModel.ChromaLink = m7.Cast<SKColor>().ToArray();
                    }
                    break;
            }
        }
    }
}

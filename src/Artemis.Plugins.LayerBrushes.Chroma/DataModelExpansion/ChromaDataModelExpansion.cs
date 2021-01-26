using Artemis.Core.DataModelExpansions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.LayerBrushes.Chroma.DataModelExpansion
{
    public class ChromaDataModelExpansion : DataModelExpansion<ChromaDataModel>
    {
        private readonly ChromaPluginService _chroma;
        public ChromaDataModelExpansion(ChromaPluginService chroma)
        {
            _chroma = chroma;
        }

        public override void Disable()
        {
           // throw new NotImplementedException();
        }

        public override void Enable()
        {
            //throw new NotImplementedException();
        }

        public override void Update(double deltaTime)
        {
            DataModel.CurrentApplication = _chroma.CurrentApp;
            DataModel.ApplicationList = _chroma.Apps;
            DataModel.PidList = _chroma.Pids;

            if (_chroma.Matrices.TryGetValue(RzDeviceType.Mousepad, out var m1))
            {
                DataModel.Mousepad = m1.Cast<SKColor>().ToArray();
            }

            if (_chroma.Matrices.TryGetValue(RzDeviceType.Mouse, out var m2))
            {
                DataModel.Mouse = m2.Cast<SKColor>().ToArray();
            }

            if (_chroma.Matrices.TryGetValue(RzDeviceType.Keypad, out var m3))
            {
                DataModel.Keypad = m3.Cast<SKColor>().ToArray();
            }

            if (_chroma.Matrices.TryGetValue(RzDeviceType.Keyboard, out var m4))
            {
                DataModel.Keyboard = m4.Cast<SKColor>().ToArray();
            }

            if (_chroma.Matrices.TryGetValue(RzDeviceType.Headset, out var m5))
            {
                DataModel.Headset = m5.Cast<SKColor>().ToArray();
            }

            if (_chroma.Matrices.TryGetValue(RzDeviceType.ChromaLink, out var m7))
            {
                DataModel.ChromaLink = m7.Cast<SKColor>().ToArray();
            }
        }
    }
}

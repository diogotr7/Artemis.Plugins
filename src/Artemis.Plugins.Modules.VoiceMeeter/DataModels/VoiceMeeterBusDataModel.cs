using Artemis.Core.Modules;
using System;

namespace Artemis.Plugins.Modules.VoiceMeeter.DataModels
{
    public class VoiceMeeterBusDataModel : DataModel
    {
        private readonly int _index;

        public VoiceMeeterBusDataModel(int idx)
        {
            _index = idx;
        }

        internal void Update()
        {
        }
    }
}
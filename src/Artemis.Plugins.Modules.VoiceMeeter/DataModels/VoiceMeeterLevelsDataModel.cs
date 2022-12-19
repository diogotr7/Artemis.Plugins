using Artemis.Core.Modules;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.VoiceMeeter.DataModels
{
    public class VoiceMeeterLevelsDataModel : DataModel 
    {
        public VoiceMeeterInputLevelsDataModel InputLevels { get; } = new();
        public VoiceMeeterOutputLevelsDataModel OutputLevels { get; } = new();
    }

    public class VoiceMeeterInputLevelsDataModel : DataModel
    {
        
    }

    public class VoiceMeeterOutputLevelsDataModel : DataModel
    {

    }

    public class VoiceMeeterLevelDataModel : DataModel
    {
        private readonly DynamicChild<float>[] _children;
        private readonly int _type;
        private readonly int _index;

        public float Value => _children.Select(x => x.Value).Average();

        public VoiceMeeterLevelDataModel(int type, int index, int count)
        {
            _type = type;
            _index = index;
            _children = new DynamicChild<float>[count];
            for (int i = 0; i < count; i++)
                _children[i] = AddDynamicChild(i.ToString(), 0f, $"Level {i + 1}");
        }
        
        public void Update()
        {
            for (int i = 0; i < _children.Length; i++)
            {
                var result = VoiceMeeterRemote.GetLevel(_type, _index + 0, out var val);

                if (result != 0)
                    throw new Exception();

                _children[i].Value = val;
            }
        }
    }
}

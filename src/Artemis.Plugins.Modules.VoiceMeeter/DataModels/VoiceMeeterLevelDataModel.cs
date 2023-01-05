using Artemis.Core.Modules;
using System;
using System.Linq;

namespace Artemis.Plugins.Modules.VoiceMeeter.DataModels;

public class VoiceMeeterLevelDataModel : DataModel
{
    private readonly DynamicChild<float>[] _children;
    private readonly int _type;
    private readonly int _index;

    public float Value => _children.Select(x => x.Value).Average();

    public VoiceMeeterLevelDataModel(int type, int index, int channels)
    {
        _type = type;
        _index = index;
        _children = new DynamicChild<float>[channels];
        for (int i = 0; i < channels; i++)
            _children[i] = AddDynamicChild(i.ToString(), 0f, $"Channel {i + 1}");
    }

    public void Update()
    {
        for (int i = 0; i < _children.Length; i++)
        {
            var result = VoiceMeeterRemote.GetLevel(_type, _index + i, out var val);

            if (result != 0)
                throw new Exception();

            _children[i].Value = val;
        }
    }
}

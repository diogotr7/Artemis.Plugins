using Artemis.Core.DataModelExpansions;
using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Aida64.DataModels
{
    public class Aida64DataModel : DataModel
    {

    }

    public class AidaElementDataModel : DataModel
    {
        public string Value { get; set; }

        public AidaElementDataModel(string val)
        {
            Value = val;
        }
    }

    public class AidaFloatElementDataModel : DataModel
    {
        public float Value { get; set; }
        public AidaFloatElementDataModel(float val)
        {
            Value = val;
        }
    }
}
using Artemis.Core;
using Artemis.Core.Modules;
using SkiaSharp;
using Artemis.Plugins.Modules.Fallout4.DataModels;

namespace Artemis.Plugins.Modules.Fallout4
{
    public class Fallout4Module : ProfileModule<Fallout4DataModel>
    {
        public override void EnablePlugin()
        {
            DisplayName = "Fallout 4";
            DisplayIcon = "ToyBrickPlus";
            DefaultPriorityCategory = ModulePriorityCategory.Application;
            UpdateDuringActivationOverride = false;
        }

        public override void DisablePlugin()
        {
        }

        public override void ModuleActivated(bool isOverride)
        {
        }

        public override void ModuleDeactivated(bool isOverride)
        {
        }

        public override void Update(double deltaTime)
        {
        }

        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo) { }
    }
}
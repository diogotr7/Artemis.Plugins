using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.LeagueOfLegendsConfigurationDialog;
using Artemis.UI.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class PluginBootstrapper : IPluginBootstrapper
    {
        public void Disable(Plugin plugin)
        {
        }

        public void Enable(Plugin plugin)
        {
            plugin.ConfigurationDialog = new PluginConfigurationDialog<LeagueOfLegendsConfigurationDialogViewModel>();
        }
    }
}

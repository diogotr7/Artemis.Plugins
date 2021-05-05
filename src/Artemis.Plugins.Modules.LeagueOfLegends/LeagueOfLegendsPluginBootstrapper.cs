using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.LeagueOfLegendsConfigurationDialog;
using Artemis.UI.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class LeagueOfLegendsPluginBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            plugin.ConfigurationDialog = new PluginConfigurationDialog<LeagueOfLegendsConfigurationDialogViewModel>();
        }
    }
}

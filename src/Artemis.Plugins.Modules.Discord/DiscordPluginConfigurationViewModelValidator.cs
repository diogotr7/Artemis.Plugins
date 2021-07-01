using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordPluginConfigurationViewModelValidator : AbstractValidator<DiscordPluginConfigurationViewModel>
    {
        public DiscordPluginConfigurationViewModelValidator()
        {
            RuleFor(vm => vm.ClientId).Length(18).WithMessage("Client id must be 18 characters.");
            RuleFor(vm => vm.ClientSecret).Length(32).WithMessage("Client id must be 32 characters.");
        }
    }
}

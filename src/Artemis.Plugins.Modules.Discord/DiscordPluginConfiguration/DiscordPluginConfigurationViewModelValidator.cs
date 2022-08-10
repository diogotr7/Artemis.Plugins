using FluentValidation;
using System.Linq;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordPluginConfigurationViewModelValidator : AbstractValidator<DiscordPluginConfigurationViewModel>
    {
        public DiscordPluginConfigurationViewModelValidator()
        {
            RuleFor(vm => vm.ClientId).Must(x => x.All(c => char.IsDigit(c))).WithMessage("Client id must be only number characters.");
            RuleFor(vm => vm.ClientSecret).NotEmpty().WithMessage("Client secret must not be empty.");
        }
    }
}

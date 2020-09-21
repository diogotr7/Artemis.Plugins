using Artemis.Core;
using FluentValidation;
using Stylet;

namespace Artemis.Plugins.Devices.OpenRGB
{
    public class OpenRGBConfigurationDialogViewModel : PluginConfigurationViewModel
    {
        private readonly PluginSetting<string> _ipAddress;
        private readonly PluginSetting<int> _port;

        public OpenRGBConfigurationDialogViewModel(Plugin plugin, PluginSettings settings, IModelValidator<OpenRGBConfigurationDialogViewModel> validator) : base(plugin, validator)
        {
            _ipAddress = settings.GetSetting("IpAddress", "127.0.0.1");
            _port = settings.GetSetting("Port", 6742);

            Port = _port.Value;

            var ips = _ipAddress.Value.Split('.');

            Ip1 = int.Parse(ips[0]);
            Ip2 = int.Parse(ips[1]);
            Ip3 = int.Parse(ips[2]);
            Ip4 = int.Parse(ips[3]);
        }

        public int Port { get; set; }
        public int Ip1 { get; set; }
        public int Ip2 { get; set; }
        public int Ip3 { get; set; }
        public int Ip4 { get; set; }

        public void SaveChanges()
        {
            Validate();
            if (HasErrors)
                return;

            _port.Value = Port;
            _port.Save();

            _ipAddress.Value = $"{Ip1}.{Ip2}.{Ip3}.{Ip4}";
            _ipAddress.Save();
            RequestClose();
        }

        public void Cancel()
        {
            RequestClose();
        }
    }

    public class IpPortValidator : AbstractValidator<OpenRGBConfigurationDialogViewModel>
    {
        public IpPortValidator()
        {
            RuleFor(vm => vm.Port).ExclusiveBetween(1024, 65535);
            RuleFor(vm => vm.Ip1).InclusiveBetween(0, 255);
            RuleFor(vm => vm.Ip2).InclusiveBetween(0, 255);
            RuleFor(vm => vm.Ip3).InclusiveBetween(0, 255);
            RuleFor(vm => vm.Ip4).InclusiveBetween(0, 255);
            RuleFor(vm => vm.Ip4).InclusiveBetween(0, 255);
        }
    }
}
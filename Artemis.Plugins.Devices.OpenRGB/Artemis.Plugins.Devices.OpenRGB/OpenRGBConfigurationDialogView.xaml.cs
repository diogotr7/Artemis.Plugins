using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Artemis.Plugins.Devices.OpenRGB
{
    /// <summary>
    /// Interaction logic for OpenRGBConfigurationDialogView.xaml
    /// </summary>
    public partial class OpenRGBConfigurationDialogView : UserControl
    {
        private readonly Regex numberRegex = new Regex("[^0-9]+");

        public OpenRGBConfigurationDialogView()
        {
            InitializeComponent();
        }

        private void ValidateNumberInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = numberRegex.IsMatch(e.Text);
        }
    }
}

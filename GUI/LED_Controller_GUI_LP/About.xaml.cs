using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TPS9266xEvaluationModule
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            labelProductName.Content = "TPS92682 EVM and Bench";
            labelRevs.Content = "GUI Rev: " + Globals.swVersion + ", FW Rev: " + Globals.fwVersion + ", DLL Rev: " + Globals.dllVersion;
            labelCopyright.Content = "Copyright Texas Instruments Inc.";
            labelCompanyName.Content = "Texas Instruments Inc.";

            textBoxDescription.Text = "LAUNCHPAD TPS92682 Evaluation and Bench Test GUI";
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class AboutAll : Window
    {
        public AboutAll()
        {
            InitializeComponent();

            labelProductName.Content = "TPS9266x Evaluation Module GUI";
            labelRevs.Content = "GUI Rev: " + Globals.swVersion + ", FW Rev: " + Globals.fwVersion + ", DLL Rev: " + Globals.dllVersion;
            labelCopyright.Content = "Copyright Texas Instruments Inc.";
            labelCompanyName.Content = "Texas Instruments Inc.";

            textBoxDescription.Text = "TPS9266x Evaluation Module GUI\n" + "Current Culture: " + Thread.CurrentThread.CurrentCulture + ", Installed Culture: " + CultureInfo.InstalledUICulture.Name + ", Region: " + RegionInfo.CurrentRegion.ThreeLetterISORegionName + ", Input: " + InputLanguageManager.Current.CurrentInputLanguage + ", Display: " + CultureInfo.InstalledUICulture.DisplayName;

#if CSTMR || DEBUG || UPDATER_TEST || FAIL_ANAL
            textBoxDescription.Text += ". Sw(s):";
#endif

#if CSTMR
            textBoxDescription.Text += " - CSTMR";
#endif

#if DEBUG
            textBoxDescription.Text += " - DEBUG";
#endif

#if UPDATER_TEST
            textBoxDescription.Text += " - UPDATER_TEST";
#endif

#if FAIL_ANAL  // both CSTMR and FAIL_ANAL need to be true for failure analysis mode; add FA to end of assembly name a change name in sign tool
            textBoxDescription.Text += " - FA";
#endif
            textBoxDescription.Text += ".";
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

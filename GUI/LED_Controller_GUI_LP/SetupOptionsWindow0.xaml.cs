using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
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
    /// Interaction logic for SetupOptionsWindow0.xaml
    /// </summary>
    public partial class SetupOptionsWindow0 : Window
    {
        private bool proceed = false;
        private DataTable dataTbl;
        private byte currentDevice;

        public SetupOptionsWindow0(DataTable dt)
        {
            InitializeComponent();

            comboBoxEVM.SelectionChanged -= comboBoxEVM_SelectionChanged;
            ObservableCollection<string> comboItemsEVM = new ObservableCollection<string>();  // this will set the watchdog on the MCU
            for(int i = 0; i < Globals.EVMS.Count(); i++)
                comboItemsEVM.Add(Globals.EVMS[i]);
            comboBoxEVM.ItemsSource = comboItemsEVM;
            comboBoxEVM.SelectedIndex = Properties.Settings.Default.comboBoxEVM;
            comboBoxEVM.SelectionChanged += comboBoxEVM_SelectionChanged;

            for (int i = 0; i < Properties.Settings.Default.maxDevices + 1; i++)
                comboBoxNumDevices.Items.Add(i);
            comboBoxNumDevices.SelectedIndex = 0;// Properties.Settings.Default.comboBoxNumSelectedIndex;

            ObservableCollection<string> comboItemsAddress = new ObservableCollection<string>();
            for (int i = 0; i < Properties.Settings.Default.maxDevices; i++)
                comboItemsAddress.Add(i.ToString());
            comboBoxAddress.ItemsSource = comboItemsAddress;
            comboBoxAddress.SelectedIndex = 0;

            groupBoxAddressSetup.Visibility = Visibility.Hidden;
            comboBoxNumDevices.Visibility = Visibility.Hidden;
            labelNumberDevices.Visibility = Visibility.Hidden;

            dataTbl = dt;
            currentDevice = 1;
            labelDeviceAdded.Content = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!proceed)
                Environment.Exit(0);
        }

        private void buttonAddDevice_Click(object sender, RoutedEventArgs e)
        {
            DataRow dr = dataTbl.NewRow();
            dr["Device"] = comboBoxDevice.SelectedItem.ToString();
            dr["Address"] = byte.Parse(comboBoxAddress.SelectedItem.ToString(), CultureInfo.InvariantCulture);

            for (int i = 0; i < dataTbl.Rows.Count; i++)
            {
                if ((byte)dataTbl.Rows[i][1] == byte.Parse(comboBoxAddress.SelectedItem.ToString(), CultureInfo.InvariantCulture))
                {
                    MessageBox.Show(this, "Address " + comboBoxAddress.SelectedItem.ToString() + " has been already selected.\nPlease select another address.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            dataTbl.Rows.Add(dr);

            if (currentDevice == Globals.userNumDevices)
            {
                proceed = true;
                checkBoxShowSetup_Checked(this, null);
                Close();
            }
            labelDeviceAdded.Content = comboBoxDevice.SelectedItem.ToString() + " added at address " + byte.Parse(comboBoxAddress.SelectedItem.ToString(), CultureInfo.InvariantCulture);
            currentDevice++;
            //    comboBoxAddress.SelectedIndex = currentDevice - 1;
        }

        private void checkBoxShowSetup_Checked(object sender, RoutedEventArgs e)
        {
            if (!proceed)
                return;
            if (checkBoxShowSetup.IsChecked == true)
                Properties.Settings.Default.setMeUpHolder = false;
            else
                Properties.Settings.Default.setMeUpHolder = true;
        }

        private void comboBoxEVM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxNumDevices.SelectedIndex = 0;
            Globals.userSelectedEVM = Properties.Settings.Default.comboBoxNumSelectedIndex = (byte)comboBoxEVM.SelectedIndex;
            dataTbl.Clear();  // clear the data table

            groupBoxAddressSetup.Visibility = Visibility.Hidden;
            Globals.userSelectedEVM = Properties.Settings.Default.userSelectedEVM = (byte)comboBoxEVM.SelectedIndex;

            ObservableCollection<string> comboItemsDevice = new ObservableCollection<string>();
            if (Globals.userSelectedEVM == 1)  // 518
            {
                comboItemsDevice.Add("518");
                Globals.userNumDevices = 1;
                comboBoxNumDevices.Visibility = Visibility.Hidden;
                labelNumberDevices.Visibility = Visibility.Hidden;
                groupBoxAddressSetup.Visibility = Visibility.Visible;
            }
            else if (Globals.userSelectedEVM == 2)
            {
                comboItemsDevice.Add("520");  // 520
                Globals.userNumDevices = 1;
                comboBoxNumDevices.Visibility = Visibility.Hidden;
                labelNumberDevices.Visibility = Visibility.Hidden;
                groupBoxAddressSetup.Visibility = Visibility.Visible;
            }
            else if ((Globals.userSelectedEVM == 3) || (Globals.userSelectedEVM == 4))  // 520 - 682
                comboItemsDevice.Add("520");


            if (Globals.userSelectedEVM > 2)  // 682s
            {
                comboItemsDevice.Add("682");
                comboBoxNumDevices.Visibility = Visibility.Visible;
                labelNumberDevices.Visibility = Visibility.Visible;
            } 
            if (Globals.userSelectedEVM > 7)  // custom
            {
                comboItemsDevice.Add("518");
                comboItemsDevice.Add("520");
                comboBoxNumDevices.Visibility = Visibility.Visible;
                labelNumberDevices.Visibility = Visibility.Visible;
            }
            comboBoxDevice.ItemsSource = comboItemsDevice;
            comboBoxDevice.SelectedIndex = 0;
        }

        private void comboBoxNumDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateComboBoxNumDevices();
        }

        private void comboBoxNumDevices_DropDownClosed(object sender, EventArgs e)
        {
            updateComboBoxNumDevices();
        }

        private void updateComboBoxNumDevices()
        {
            if (byte.Parse(comboBoxNumDevices.SelectedItem.ToString(), CultureInfo.InvariantCulture) == 0)
            {
                groupBoxAddressSetup.Visibility = Visibility.Hidden;
                return;
            }
            groupBoxAddressSetup.Visibility = Visibility.Visible;
            Globals.userNumDevices = byte.Parse(comboBoxNumDevices.SelectedItem.ToString(), CultureInfo.InvariantCulture);
            //        Properties.Settings.Default.comboBoxNumSelectedIndex = (byte)comboBoxNumDevices.SelectedIndex;
        }

        private void comboBoxAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            labelDeviceAdded.Content = "";
        }

        private void comboBoxDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            labelDeviceAdded.Content = "";
        }
    }
}
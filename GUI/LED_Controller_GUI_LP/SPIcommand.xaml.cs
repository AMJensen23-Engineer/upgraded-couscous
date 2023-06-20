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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TPS9266xEvaluationModule
{
    /// <summary>
    /// Interaction logic for SPIcommand.xaml
    /// </summary>
    public partial class SPIcommand : UserControl
    {
        private bool addressOK = false;
        private bool writeDataOK = false;

        private MainWindow mainWindow = null;  // get the MainWindow object

        public SPIcommand(MainWindow master)
        {
            InitializeComponent();

            mainWindow = master;

            mainWindow.updateStatus("");
            checkBoxWrite.IsChecked = Properties.Settings.Default.checkBoxWrite;
            textBoxWriteData.IsEnabled = Properties.Settings.Default.checkBoxWrite;

            radioButton518.IsChecked = Properties.Settings.Default.radioButton518;
            radioButton520.IsChecked = Properties.Settings.Default.radioButton520;
            radioButton682.IsChecked = Properties.Settings.Default.radioButton682;

            radioButtonSingle.IsChecked = Properties.Settings.Default.radioButtonSingle;
            radioButtonStar.IsChecked = Properties.Settings.Default.radioButtonStar;
            radioButtonDaisy.IsChecked = Properties.Settings.Default.radioButtonDaisy;

            comboBoxAddress.SelectionChanged -= comboBoxAddress_SelectionChanged;
            for (int i = 0; i < 9; i++)
                comboBoxAddress.Items.Add(i);
            comboBoxAddress.SelectionChanged += comboBoxAddress_SelectionChanged;

            //        comboBoxAddress.SelectedIndex = Properties.Settings.Default.comboBoxAddress;

            // buttonSPIpower.Background = Brushes.Red;

            groupBoxAddress.IsEnabled = false;
            groupBoxDevice.IsEnabled = false;
            labelSPIFreq.Visibility = Visibility.Hidden;
        }

        private void buttonSendCommand_Click(object sender, RoutedEventArgs e)
        {
            if (!addressOK || (!writeDataOK && (bool)checkBoxWrite.IsChecked))
                return;

            byte registerValue;
            string registerData = textBoxRegAddress.Text;
            if (registerData.Contains("0x"))
                registerValue = byte.Parse(registerData.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            else
                registerValue = byte.Parse(registerData, CultureInfo.InvariantCulture);

            UInt16 dataValue = 0;
            if ((bool)checkBoxWrite.IsChecked)
            {
                string writeData = textBoxWriteData.Text;
                if (writeData.Contains("0x"))
                    dataValue = UInt16.Parse(writeData.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                else
                    dataValue = UInt16.Parse(writeData, CultureInfo.InvariantCulture);
            }

            SPIcommadReturnData scrd = new SPIcommadReturnData();
            if (radioButton518.IsChecked == true)
            {
                dataValue &= 0x01FF;
                scrd = Globals.mcuCommand.sendSPI((bool)checkBoxWrite.IsChecked, registerValue, dataValue, byte.Parse(comboBoxAddress.SelectedItem.ToString(), CultureInfo.InvariantCulture));
            }
            else  // radioButton520.IsChecked or radioButton682.IsChecked
            {
                dataValue &= 0x00FF;
                for (int i = 0; i < 5; i++)
                {
                    scrd = Globals.mcuCommand.sendSPI((bool)checkBoxWrite.IsChecked, registerValue, (byte)dataValue, byte.Parse(comboBoxAddress.SelectedItem.ToString(), CultureInfo.InvariantCulture));
                    mainWindow.updateStatus("MISO: Transfer " + (i + 1).ToString() + " = " + scrd.returnData + "\n");
                    string trimmedString = scrd.returnData.Remove(0, 2);
                    if ((!(bool)checkBoxWrite.IsChecked) && ((UInt16.Parse(trimmedString, NumberStyles.HexNumber, CultureInfo.InvariantCulture) & 0x6000) == 0x6000))
                        break;
                    else if (((bool)checkBoxWrite.IsChecked) && ((UInt16.Parse(trimmedString, NumberStyles.HexNumber, CultureInfo.InvariantCulture) & 0x4000) == 0x4000))
                        break;
                }
            }

            textBoxDataRead.Text = scrd.returnData;  // high byte = status, low byte = data
            if (radioButton518.IsChecked == true)
            {
                if (!(bool)checkBoxWrite.IsChecked)
                {
                    // Decode this as a read return packet
                    if ((scrd.assembledReturn & 0x8000) == 0x8000)  // The SPE bit
                        mainWindow.updateStatus("SPI Error bit set on read response.\n");

                    if ((scrd.assembledReturn & 0x6000) != 0x6000)  // // The 2 reserved bits
                        mainWindow.updateStatus("SPI Read Response reserved bits were not both '1'.\n");

                    if ((scrd.assembledReturn & 0x1000) == 0x1000)  // The power cycled bit                   
                        mainWindow.updateStatus("TPS92518 PC bit is '1'.\n");  // PC bit is set
                    else
                        mainWindow.updateStatus("TPS92518 PC bit is '0'.\n");  // PC bit is clear

                    if ((scrd.assembledReturn & 0x0800) == 0x0800)
                        mainWindow.updateStatus("TPS92518 LED2 BOOTUV bit is '1'.\n");  // LED2 BOOTUV ERROR bit is set
                    else
                        mainWindow.updateStatus("TPS92518 LED2 BOOTUV bit is '0'.\n");  // LED2 BOOTUV ERROR bit is clear

                    if ((scrd.assembledReturn & 0x0400) == 0x0400)
                        mainWindow.updateStatus("TPS92518 LED1 BOOTUV bit is '1'.\n");  // LED1 BOOTUV ERROR bit is set
                    else
                        mainWindow.updateStatus("TPS92518 LED1 BOOTUV bit is '0'.\n");  // LED1 BOOTUV ERROR bit is cleat 

                    if ((scrd.assembledReturn & 0x0200) == 0x0200)  // THERMAL WARNING                   
                        mainWindow.updateStatus("TPS92518 TW bit is '1'.\n");  // THERMAL WARNING bit is set
                    else
                        mainWindow.updateStatus("TPS92518 TW bit is '0'.\n");  // THERMAL WARNING bit is clear

                    mainWindow.updateStatus("TPS92518 read data: 0x" + ((scrd.assembledReturn & 0x001FF).ToString("X3")) + ".\n");  // Finally, the data returned
                }
                else
                {
                    // Decode this as a write return packet and / or error
                    if (scrd.assembledReturn == 0x8000)
                        mainWindow.updateStatus("TPS92518 reporting SPI write error on previous transaction.\n");  // The previous write command had a SPI error
                    else
                    {
                        // The previous write was valid                    
                        if ((scrd.assembledReturn & 0x8000) != 0x0000)  // Decode the SPE bit -- should always be 0?                        
                            mainWindow.updateStatus("ERROR The SPE bit was set in a Write Response frame.\n");  // The SPE bit is set, but this is a write response; not to spec

                        if ((scrd.assembledReturn & 0x4000) != 0x4000)  // Decode the CMD bit -- should always be 1?                   
                            mainWindow.updateStatus("ERROR The CMD bit was cleared in a Write Response frame.\n");  // The CMD bit is cleared indicating response to a read; not to spec

                        if ((scrd.assembledReturn & 0xC000) == 0x4000)// Otherwise, we are good
                            mainWindow.updateStatus(scrd.info);
                    }
                }
            }
            else if (radioButton520.IsChecked == true)
            {
                if (!(bool)checkBoxWrite.IsChecked)
                {
                    // Decode this as a read return packet
                    if ((scrd.assembledReturn & 0x8000) == 0x8000)  // The SPE bit
                        mainWindow.updateStatus("SPI Error bit set on read response.\n");

                    if ((scrd.assembledReturn & 0x7C00) != 0x6000)  // The 5 reserved bits
                        mainWindow.updateStatus("SPI Read Response reserved bits were not '1100'.\n");

                    if ((scrd.assembledReturn & 0x0200) == 0x0200)  // The power cycled bit                   
                        mainWindow.updateStatus("TPS92520 PC bit is '1'.\n");  // PC bit is set
                    else
                        mainWindow.updateStatus("TPS92520 PC bit is '0'.\n");  // PC bit is clear;

                    if ((scrd.assembledReturn & 0x0100) == 0x0100)  // THERMAL WARNING                   
                        mainWindow.updateStatus("TPS92520 TW bit is '1'.\n");  // THERMAL WARNING bit is set
                    else
                        mainWindow.updateStatus("TPS92520 TW bit is '0'.\n");  // THERMAL WARNING bit is clear;

                    mainWindow.updateStatus("TPS92520 read data: 0x" + ((scrd.assembledReturn & 0x00FF).ToString("X2")) + ".\n");  // Finally, the data returned
                }
                else
                {
                    // Decode this as a write return packet and / or error
                    if (scrd.assembledReturn == 0x8000)
                        mainWindow.updateStatus("TPS92520 reporting SPI write error on previous transaction.\n");  // The previous write command had a SPI error
                    else
                    {
                        // The previous write was valid                    
                        if ((scrd.assembledReturn & 0x8000) != 0x0000)  // Decode the SPE bit -- should always be 0?                        
                            mainWindow.updateStatus("ERROR The SPE bit was set in a Write Response frame.\n");  // The SPE bit is set, but this is a write response; not to spec

                        if ((scrd.assembledReturn & 0x4000) != 0x4000)  // Decode the CMD bit -- should always be 1?                   
                            mainWindow.updateStatus("ERROR The CMD bit was cleared in a Write Response frame.\n");  // The CMD bit is cleared indicating response to a read; not to spec

                        if ((scrd.assembledReturn & 0xC000) == 0x4000)// Otherwise, we are good
                            mainWindow.updateStatus(scrd.info);
                    }
                }
            }
            else  // 682
            {
                if (!(bool)checkBoxWrite.IsChecked)
                {
                    // Decode this as a read return packet
                    if ((scrd.assembledReturn & 0x8000) == 0x8000)  // The SPE bit
                        mainWindow.updateStatus("SPI Error bit set on read response.\n");

                    if ((scrd.assembledReturn & 0x7800) != 0x6000)  // The 4 reserved bits
                        mainWindow.updateStatus("SPI Read Response reserved bits were not '1100'.\n");

                    if ((scrd.assembledReturn & 0x0400) == 0x0400)  // The RT Open Fault bit                    
                        mainWindow.updateStatus("TPS92682 RTO bit is '1'.\n");  // RTO bit is set
                    else
                        mainWindow.updateStatus("TPS92682 RTO bit is '0'.\n");  // RTO bit is clear

                    if ((scrd.assembledReturn & 0x0200) == 0x0200)  // The power cycled bit                   
                        mainWindow.updateStatus("TPS92682 PC bit is '1'.\n");  // PC bit is set
                    else
                        mainWindow.updateStatus("TPS92682 PC bit is '0'.\n");  // PC bit is clear;

                    if ((scrd.assembledReturn & 0x0100) == 0x0100)  // THERMAL WARNING                   
                        mainWindow.updateStatus("TPS92682 TW bit is '1'.\n");  // THERMAL WARNING bit is set
                    else
                        mainWindow.updateStatus("TPS92682 TW bit is '0'.\n");  // THERMAL WARNING bit is clear;

                    mainWindow.updateStatus("TPS92682 read data: 0x" + ((scrd.assembledReturn & 0x00FF).ToString("X2")) + ".\n");  // Finally, the data returned
                }
                else
                {
                    // Decode this as a write return packet and / or error
                    if (scrd.assembledReturn == 0x8000)
                        mainWindow.updateStatus("TPS92682 reporting SPI write error on previous transaction.\n");  // The previous write command had a SPI error
                    else
                    {
                        // The previous write was valid                    
                        if ((scrd.assembledReturn & 0x8000) != 0x0000)  // Decode the SPE bit -- should always be 0?                        
                            mainWindow.updateStatus("ERROR The SPE bit was set in a Write Response frame.\n");  // The SPE bit is set, but this is a write response; not to spec

                        if ((scrd.assembledReturn & 0x4000) != 0x4000)  // Decode the CMD bit -- should always be 1?                   
                            mainWindow.updateStatus("ERROR The CMD bit was cleared in a Write Response frame.\n");  // The CMD bit is cleared indicating response to a read; not to spec

                        if ((scrd.assembledReturn & 0xC000) == 0x4000)// Otherwise, we are good
                            mainWindow.updateStatus(scrd.info + "\n");
                    }
                }
            }
        }

        private void checkBoxWrite_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)checkBoxWrite.IsChecked)
                textBoxWriteData.IsEnabled = true;
            else
                textBoxWriteData.IsEnabled = false;
        }

        private void textBoxRegAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBoxRegAddress.LostFocus -= textBoxRegAddress_LostFocus;
                changeRegisterAddress();
                textBoxRegAddress.LostFocus += textBoxRegAddress_LostFocus;
            }
        }

        private void textBoxRegAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            changeRegisterAddress();
        }

        private void changeRegisterAddress()
        {
            if (textBoxRegAddress.Text == "")
                return;

            byte addrValue;
            bool passedParse = false;
            string registerData = textBoxRegAddress.Text;
            if (registerData.Contains("0x"))
                passedParse = byte.TryParse(registerData.Substring(2), NumberStyles.AllowHexSpecifier, null, out addrValue);
            else
                passedParse = byte.TryParse(registerData, out addrValue);

            if (!passedParse)
            {
                addressOK = false;
                textBoxRegAddress.Text = "";
                MessageBox.Show("Please enter a valid register address", "SPI Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if ((addrValue < 0) || (addrValue > 0x3F))
            {
                addressOK = false;
                textBoxRegAddress.Text = "";
                MessageBox.Show("Please enter a register address in range 0x00 - 0x3F(63)", "SPI Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            addressOK = true;
        }

        private void textBoxWriteData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBoxWriteData.LostFocus -= textBoxWriteData_LostFocus;
                changeWriteData();
                textBoxWriteData.LostFocus += textBoxWriteData_LostFocus;
            }
        }

        private void textBoxWriteData_LostFocus(object sender, RoutedEventArgs e)
        {
            changeWriteData();
        }

        private void changeWriteData()
        {
            if (textBoxWriteData.Text == "")
                return;

            byte dataValue;
            string writeData = textBoxWriteData.Text;
            bool passedParse = false;
            if (writeData.Contains("0x"))
                passedParse = byte.TryParse(writeData.Substring(2), NumberStyles.AllowHexSpecifier, null, out dataValue);
            else
                passedParse = byte.TryParse(writeData, out dataValue);

            if (!passedParse)
            {
                writeDataOK = false;
                textBoxWriteData.Text = "";
                MessageBox.Show("Please enter a valid data value", "SPI Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if ((dataValue < 0) || (dataValue > 0xFF))
            {
                addressOK = false;
                textBoxWriteData.Text = "";
                MessageBox.Show("Please enter a data value in range 0x00 - 0xFF(255)", "SPI Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            writeDataOK = true;
        }

        private void buttonStatus_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.updateStatus(String.Empty);  // clear status
        }

        private void radioButton518_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.radioButton518 = (bool)radioButton518.IsChecked;
            Properties.Settings.Default.radioButton520 = false;
            Properties.Settings.Default.radioButton682 = false;
        }

        private void radioButton520_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.radioButton518 = false;
            Properties.Settings.Default.radioButton520 = (bool)radioButton520.IsChecked;
            Properties.Settings.Default.radioButton682 = false;
        }

        private void radioButton682_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.radioButton518 = false;
            Properties.Settings.Default.radioButton520 = false;
            Properties.Settings.Default.radioButton682 = (bool)radioButton682.IsChecked;
        }        

        private void radioButtonSingle_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.radioButtonSingle = (bool)radioButtonSingle.IsChecked;
            Properties.Settings.Default.radioButtonStar = false;
            Properties.Settings.Default.radioButtonDaisy = false;
        }

        private void radioButtonStar_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.radioButtonStar = (bool)radioButtonStar.IsChecked;
            Properties.Settings.Default.radioButtonSingle = false;
            Properties.Settings.Default.radioButtonDaisy = false;
        }

        private void radioButtonDaisy_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.radioButtonDaisy = (bool)radioButtonDaisy.IsChecked;
            Properties.Settings.Default.radioButtonSingle = false;
            Properties.Settings.Default.radioButtonStar = false;
        }

        private void comboBoxAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.comboBoxAddress = comboBoxAddress.SelectedIndex;
            mainWindow.selectSlave(comboBoxAddress.SelectedIndex);
        }
    }
}

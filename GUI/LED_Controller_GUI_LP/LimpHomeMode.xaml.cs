using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
    /// Interaction logic for LimpHomeMode.xaml
    /// </summary>
    public partial class LimpHomeMode : Window
    {
        private byte myAddress;
        private int tabIndex;
        private bool limpModeActive = false;

        public LimpHomeMode(byte parentDeviceAddress, bool initialRun, int tabIndexSavedParams)
        {
            InitializeComponent();

            tabIndex = tabIndexSavedParams;
            myAddress = parentDeviceAddress;

            initSetup();
            tabIndex = tabIndexSavedParams;

     //       if (initialRun)
        //        resetControls();
       //     else
         //       restoreCustomSettings();

            updateAll();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveCustomSettings();
        }

        private void initSetup()
        {
            eventsOff();


            textBlockAnalogCurrentChannel1.Text = "";
            textBlockOnTimeChannel1.Text = "";
            textBlockPwmChannel1.Text = "";
            checkBoxEnableChannel1.IsChecked = false;  // default at power up
            checkBoxPwmChannel1.IsChecked = false;
            checkBoxPwm100Channel1.IsChecked = false;
            checkBoxLowFetChannel1.IsChecked = false;
            checkBoxHighFetChannel1.IsChecked = false;
            checkBoxThermalResponseChannel1.IsChecked = false;
            sliderAnalogCurrentChannel1.Value = 0;  
            sliderOnTimeChannel1.Value = 7;
            sliderPwmChannel1.Value = 0;

            textBlockAnalogCurrentChannel2.Text = "";
            textBlockOnTimeChannel2.Text = "";
            textBlockPwmChannel2.Text = "";
            checkBoxEnableChannel2.IsChecked = false;  // default at power up
            checkBoxPwmChannel2.IsChecked = false;
            checkBoxPwm100Channel2.IsChecked = false;
            checkBoxLowFetChannel2.IsChecked = false;
            checkBoxHighFetChannel2.IsChecked = false;
            checkBoxThermalResponseChannel2.IsChecked = false;
            sliderAnalogCurrentChannel2.Value = 0;
            sliderOnTimeChannel2.Value = 7;
            sliderPwmChannel2.Value = 0;

            textBlockLhiDevice.Text = "";
            ObservableCollection<string> comboItemsFaultTimer = new ObservableCollection<string>();  // both
            comboItemsFaultTimer.Add("4");
            comboItemsFaultTimer.Add("8");
            comboItemsFaultTimer.Add("16");
            comboItemsFaultTimer.Add("32");
            comboBoxFaultTimer.ItemsSource = comboItemsFaultTimer;
            comboBoxFaultTimer.SelectedIndex = 0;  // default value
            checkBoxLhiReference.IsChecked = false;

            //           sliderAnalogCurrentChannel1.IsEnabled = false;
            //         sliderAnalogCurrentChannel2.IsEnabled = false;
            textBlockLhifiltDevice.Text = "";

            eventsOn();
        }

        private void resetControls()
        {
            checkBoxPwmChannel1_Checked(this, null);  // channel 1
            checkBoxPwm100Channel1_Checked(this, null);
            checkBoxLowFetChannel1_Checked(this, null);
            checkBoxHighFetChannel1_Checked(this, null);
            checkBoxThermalResponseChannel1_Checked(this, null);
            sliderAnalogCurrentChannel1_ValueChanged(this, null);
            sliderOnTimeChannel1_ValueChanged(this, null);
            sliderPwmChannel1_ValueChanged(this, null);

            checkBoxPwmChannel2_Checked(this, null);  // channel 2
            checkBoxPwm100Channel2_Checked(this, null);
            checkBoxLowFetChannel2_Checked(this, null);
            checkBoxHighFetChannel2_Checked(this, null);
            checkBoxThermalResponseChannel2_Checked(this, null);
            sliderAnalogCurrentChannel2_ValueChanged(this, null);
            sliderOnTimeChannel2_ValueChanged(this, null);
            sliderPwmChannel2_ValueChanged(this, null);

            comboBoxFaultTimer_SelectionChanged(this, null); // both
            checkBoxLhiReference_Checked(this, null);

            checkBoxEnableChannel1_Checked(this, null);  // call the enable last
            checkBoxEnableChannel2_Checked(this, null);
        }

        public void restoreCustomSettings()
        {
            eventsOff();

            if (Properties.Settings.Default.lmCheckBox0_1[tabIndex] == "T")  // channel 1 set saved values
                checkBoxEnableChannel1.IsChecked = true;
            else
                checkBoxEnableChannel1.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox1_1[tabIndex] == "T")
                checkBoxPwmChannel1.IsChecked = true;
            else
                checkBoxPwmChannel1.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox5_1[tabIndex] == "T")
                checkBoxPwm100Channel1.IsChecked = true;
            else
                checkBoxPwm100Channel1.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox2_1[tabIndex] == "T")
                checkBoxLowFetChannel1.IsChecked = true;
            else
                checkBoxLowFetChannel1.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox3_1[tabIndex] == "T")
                checkBoxHighFetChannel1.IsChecked = true;
            else
                checkBoxHighFetChannel1.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox4_1[tabIndex] == "T")
                checkBoxThermalResponseChannel1.IsChecked = true;
            else
                checkBoxThermalResponseChannel1.IsChecked = false;

            sliderAnalogCurrentChannel1.Value = UInt16.Parse(Properties.Settings.Default.lmSlider0_1[tabIndex], CultureInfo.InvariantCulture);
            sliderOnTimeChannel1.Value = byte.Parse(Properties.Settings.Default.lmSlider1_1[tabIndex], CultureInfo.InvariantCulture);
            sliderPwmChannel1.Value = UInt16.Parse(Properties.Settings.Default.lmSlider2_1[tabIndex], CultureInfo.InvariantCulture);


            if (Properties.Settings.Default.lmCheckBox0_2[tabIndex] == "T")  // channel 2 set saved values
                checkBoxEnableChannel2.IsChecked = true;
            else
                checkBoxEnableChannel2.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox1_2[tabIndex] == "T")
                checkBoxPwmChannel2.IsChecked = true;
            else
                checkBoxPwmChannel2.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox5_2[tabIndex] == "T")
                checkBoxPwm100Channel2.IsChecked = true;
            else
                checkBoxPwm100Channel2.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox2_2[tabIndex] == "T")
                checkBoxLowFetChannel2.IsChecked = true;
            else
                checkBoxLowFetChannel2.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox3_2[tabIndex] == "T")
                checkBoxHighFetChannel2.IsChecked = true;
            else
                checkBoxHighFetChannel2.IsChecked = false;

            if (Properties.Settings.Default.lmCheckBox4_2[tabIndex] == "T")
                checkBoxThermalResponseChannel2.IsChecked = true;
            else
                checkBoxThermalResponseChannel2.IsChecked = false;

            sliderAnalogCurrentChannel2.Value = UInt16.Parse(Properties.Settings.Default.lmSlider0_2[tabIndex], CultureInfo.InvariantCulture);
            sliderOnTimeChannel2.Value = byte.Parse(Properties.Settings.Default.lmSlider1_2[tabIndex], CultureInfo.InvariantCulture);
            sliderPwmChannel2.Value = UInt16.Parse(Properties.Settings.Default.lmSlider2_2[tabIndex], CultureInfo.InvariantCulture);

            comboBoxFaultTimer.SelectedIndex = byte.Parse(Properties.Settings.Default.lmCombo0[tabIndex], CultureInfo.InvariantCulture);  // both
            if (Properties.Settings.Default.lmCheckBox0[tabIndex] == "T")
                checkBoxLhiReference.IsChecked = true;
            else
                checkBoxLhiReference.IsChecked = false;

            resetControls();
            eventsOn();
        }

        public void saveCustomSettings()
        {
            if (checkBoxEnableChannel1.IsChecked == true)  // channel 1
                Properties.Settings.Default.lmCheckBox0_1[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox0_1[tabIndex] = "F";

            if (checkBoxPwmChannel1.IsChecked == true)
                Properties.Settings.Default.lmCheckBox1_1[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox1_1[tabIndex] = "F";

            if (checkBoxPwm100Channel1.IsChecked == true)
                Properties.Settings.Default.lmCheckBox5_1[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox5_1[tabIndex] = "F";

            if (checkBoxLowFetChannel1.IsChecked == true)
                Properties.Settings.Default.lmCheckBox2_1[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox2_1[tabIndex] = "F";

            if (checkBoxHighFetChannel1.IsChecked == true)
                Properties.Settings.Default.lmCheckBox3_1[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox3_1[tabIndex] = "F";

            if (checkBoxThermalResponseChannel1.IsChecked == true)
                Properties.Settings.Default.lmCheckBox4_1[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox4_1[tabIndex] = "F";

            Properties.Settings.Default.lmSlider0_1[tabIndex] = sliderAnalogCurrentChannel1.Value.ToString("0");  // save integer only
            Properties.Settings.Default.lmSlider1_1[tabIndex] = sliderOnTimeChannel1.Value.ToString("0");
            Properties.Settings.Default.lmSlider2_1[tabIndex] = sliderPwmChannel1.Value.ToString("0");

            if (checkBoxEnableChannel2.IsChecked == true)  // channel 2
                Properties.Settings.Default.lmCheckBox0_2[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox0_2[tabIndex] = "F";

            if (checkBoxPwmChannel2.IsChecked == true)
                Properties.Settings.Default.lmCheckBox1_2[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox1_2[tabIndex] = "F";

            if (checkBoxPwm100Channel2.IsChecked == true)
                Properties.Settings.Default.lmCheckBox5_2[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox5_2[tabIndex] = "F";

            if (checkBoxLowFetChannel2.IsChecked == true)
                Properties.Settings.Default.lmCheckBox2_2[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox2_2[tabIndex] = "F";

            if (checkBoxHighFetChannel2.IsChecked == true)
                Properties.Settings.Default.lmCheckBox3_2[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox3_2[tabIndex] = "F";

            if (checkBoxThermalResponseChannel2.IsChecked == true)
                Properties.Settings.Default.lmCheckBox4_2[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox4_2[tabIndex] = "F";

            Properties.Settings.Default.lmSlider0_2[tabIndex] = sliderAnalogCurrentChannel2.Value.ToString("0");  // save integer only
            Properties.Settings.Default.lmSlider1_2[tabIndex] = sliderOnTimeChannel2.Value.ToString("0");
            Properties.Settings.Default.lmSlider2_2[tabIndex] = sliderPwmChannel2.Value.ToString("0");

            Properties.Settings.Default.lmCombo0[tabIndex] = comboBoxFaultTimer.SelectedIndex.ToString();  // both
            if (checkBoxLhiReference.IsChecked == true)
                Properties.Settings.Default.lmCheckBox0[tabIndex] = "T";
            else
                Properties.Settings.Default.lmCheckBox0[tabIndex] = "F";
        }

        private void eventsOff()
        {
            checkBoxEnableChannel1.Checked -= checkBoxEnableChannel1_Checked;  // channel 1 turn off event
            checkBoxEnableChannel1.Unchecked -= checkBoxEnableChannel1_Checked;
            checkBoxPwmChannel1.Checked -= checkBoxPwmChannel1_Checked;
            checkBoxPwmChannel1.Unchecked -= checkBoxPwmChannel1_Checked;
            checkBoxPwm100Channel1.Checked -= checkBoxPwm100Channel1_Checked;
            checkBoxPwm100Channel1.Unchecked -= checkBoxPwm100Channel1_Checked;
            checkBoxLowFetChannel1.Checked -= checkBoxLowFetChannel1_Checked;
            checkBoxLowFetChannel1.Unchecked -= checkBoxLowFetChannel1_Checked;
            checkBoxHighFetChannel1.Checked -= checkBoxHighFetChannel1_Checked;
            checkBoxHighFetChannel1.Unchecked -= checkBoxHighFetChannel1_Checked;
            checkBoxThermalResponseChannel1.Checked -= checkBoxThermalResponseChannel1_Checked;
            checkBoxThermalResponseChannel1.Unchecked -= checkBoxThermalResponseChannel1_Checked;
            sliderAnalogCurrentChannel1.ValueChanged -= sliderAnalogCurrentChannel1_ValueChanged;
            sliderOnTimeChannel1.ValueChanged -= sliderOnTimeChannel1_ValueChanged;
            sliderPwmChannel1.ValueChanged -= sliderPwmChannel1_ValueChanged;

            checkBoxEnableChannel2.Checked -= checkBoxEnableChannel2_Checked;  // channel 2 turn off event
            checkBoxEnableChannel2.Unchecked -= checkBoxEnableChannel2_Checked;
            checkBoxPwmChannel2.Checked -= checkBoxPwmChannel2_Checked;
            checkBoxPwmChannel2.Unchecked -= checkBoxPwmChannel2_Checked;
            checkBoxLowFetChannel2.Checked -= checkBoxLowFetChannel2_Checked;
            checkBoxLowFetChannel2.Unchecked -= checkBoxLowFetChannel2_Checked;
            checkBoxHighFetChannel2.Checked -= checkBoxHighFetChannel2_Checked;
            checkBoxHighFetChannel2.Unchecked -= checkBoxHighFetChannel2_Checked;
            checkBoxThermalResponseChannel2.Checked -= checkBoxThermalResponseChannel2_Checked;
            checkBoxThermalResponseChannel2.Unchecked -= checkBoxThermalResponseChannel2_Checked;
            sliderAnalogCurrentChannel2.ValueChanged -= sliderAnalogCurrentChannel2_ValueChanged;
            sliderOnTimeChannel2.ValueChanged -= sliderOnTimeChannel2_ValueChanged;
            sliderPwmChannel2.ValueChanged -= sliderPwmChannel2_ValueChanged;

            comboBoxFaultTimer.SelectionChanged -= comboBoxFaultTimer_SelectionChanged;  // both
            checkBoxLhiReference.Checked -= checkBoxLhiReference_Checked;
            checkBoxLhiReference.Unchecked -= checkBoxLhiReference_Checked;
        }

        private void eventsOn()
        {
            checkBoxEnableChannel1.Checked += checkBoxEnableChannel1_Checked;  // channel 1 restore event
            checkBoxEnableChannel1.Unchecked += checkBoxEnableChannel1_Checked;
            checkBoxPwmChannel1.Checked += checkBoxPwmChannel1_Checked;
            checkBoxPwmChannel1.Unchecked += checkBoxPwmChannel1_Checked;
            checkBoxPwm100Channel1.Checked += checkBoxPwm100Channel1_Checked;
            checkBoxPwm100Channel1.Unchecked += checkBoxPwm100Channel1_Checked;
            checkBoxLowFetChannel1.Checked += checkBoxLowFetChannel1_Checked;
            checkBoxLowFetChannel1.Unchecked += checkBoxLowFetChannel1_Checked;
            checkBoxHighFetChannel1.Checked += checkBoxHighFetChannel1_Checked;
            checkBoxHighFetChannel1.Unchecked += checkBoxHighFetChannel1_Checked;
            checkBoxThermalResponseChannel1.Checked += checkBoxThermalResponseChannel1_Checked;
            checkBoxThermalResponseChannel1.Unchecked += checkBoxThermalResponseChannel1_Checked;
            sliderAnalogCurrentChannel1.ValueChanged += sliderAnalogCurrentChannel1_ValueChanged;
            sliderOnTimeChannel1.ValueChanged += sliderOnTimeChannel1_ValueChanged;
            sliderPwmChannel1.ValueChanged += sliderPwmChannel1_ValueChanged;

            checkBoxEnableChannel2.Checked += checkBoxEnableChannel2_Checked;  // channel 2 turn off event
            checkBoxEnableChannel2.Unchecked += checkBoxEnableChannel2_Checked;
            checkBoxPwmChannel2.Checked += checkBoxPwmChannel2_Checked;
            checkBoxPwmChannel2.Unchecked += checkBoxPwmChannel2_Checked;
            checkBoxLowFetChannel2.Checked += checkBoxLowFetChannel2_Checked;
            checkBoxLowFetChannel2.Unchecked += checkBoxLowFetChannel2_Checked;
            checkBoxHighFetChannel2.Checked += checkBoxHighFetChannel2_Checked;
            checkBoxHighFetChannel2.Unchecked += checkBoxHighFetChannel2_Checked;
            checkBoxThermalResponseChannel2.Checked += checkBoxThermalResponseChannel2_Checked;
            checkBoxThermalResponseChannel2.Unchecked += checkBoxThermalResponseChannel2_Checked;
            sliderAnalogCurrentChannel2.ValueChanged += sliderAnalogCurrentChannel2_ValueChanged;
            sliderOnTimeChannel2.ValueChanged += sliderOnTimeChannel2_ValueChanged;
            sliderPwmChannel2.ValueChanged += sliderPwmChannel2_ValueChanged;

            comboBoxFaultTimer.SelectionChanged += comboBoxFaultTimer_SelectionChanged;  // both
            checkBoxLhiReference.Checked += checkBoxLhiReference_Checked;
            checkBoxLhiReference.Unchecked += checkBoxLhiReference_Checked;
        }

        void updateAll()
        {
//            readStatusRegs();  // update status regs
    //        updateADCs();      // display ADCs
      //      currentTmpC();     // show temp C
        }

        public void resetLimpHomeMode()
        {
            initSetup();
            resetControls();
     //       updateAll();
        }

        // channel 1
        private void checkBoxEnableChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((bool)checkBoxEnableChannel1.IsChecked)
                currentValue |= 0x01;  // If we want it on, set the LSbit
            else
                currentValue &= 0xFE;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxPwmChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((bool)checkBoxPwmChannel1.IsChecked)
            {
                currentValue |= 0x02;
                sliderPwmChannel1.Visibility = Visibility.Visible;
                textBlockPwmChannel1.Visibility = Visibility.Visible;
                labelPwmChannel1.Visibility = Visibility.Visible;
            }
            else
            {
                currentValue &= 0xFD;
                sliderPwmChannel1.Visibility = Visibility.Hidden;
                textBlockPwmChannel1.Visibility = Visibility.Hidden;
                labelPwmChannel1.Visibility = Visibility.Hidden;
            }

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxPwm100Channel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((bool)checkBoxPwm100Channel1.IsChecked)
                currentValue |= 0x04;
            else
                currentValue &= 0xFB;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxLowFetChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG2);

            if ((bool)checkBoxLowFetChannel1.IsChecked)
                currentValue |= 0x01;  // If we want it on, set the LSbit
            else
                currentValue &= 0xFE;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxHighFetChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG2);

            if ((bool)checkBoxHighFetChannel1.IsChecked)
                currentValue |= 0x02;
            else
                currentValue &= 0xFD;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxThermalResponseChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG2);

            if ((bool)checkBoxThermalResponseChannel1.IsChecked)
                currentValue |= 0x04;
            else
                currentValue &= 0xFB;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG2, currentValue, myAddress);  // write it out
        }

        private void sliderAnalogCurrentChannel1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockAnalogCurrentChannel1.Text = ((UInt16)sliderAnalogCurrentChannel1.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderAnalogCurrentChannel1.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LH1IADJH, (byte)((integerValue >> 2) & 0xFF), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.LH1IADJL, (byte)(integerValue & 0x03), myAddress);  // write it out low                       
        }

        private void sliderOnTimeChannel1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockOnTimeChannel1.Text = ((ushort)sliderOnTimeChannel1.Value).ToString(CultureInfo.InvariantCulture);
            byte integerValue = byte.Parse(sliderOnTimeChannel1.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LH1TON, (byte)(integerValue & 0x3F), myAddress);  // write it out
        }

        private void sliderPwmChannel1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockPwmChannel1.Text = ((UInt16)sliderPwmChannel1.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderPwmChannel1.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);
            
            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCH1PWMH, (byte)((integerValue >> 8) & 0x03), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCH1PWML, (byte)(integerValue & 0xFF), myAddress);  // write it out low
        }

        // channel 2
        private void checkBoxEnableChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((bool)checkBoxEnableChannel2.IsChecked)
                currentValue |= 0x08;  // If we want it on, set the LSbit
            else
                currentValue &= 0xF7;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxPwmChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((bool)checkBoxPwmChannel2.IsChecked)
            {
                currentValue |= 0x10;
                sliderPwmChannel2.Visibility = Visibility.Visible;
                textBlockPwmChannel2.Visibility = Visibility.Visible;
                labelPwmChannel2.Visibility = Visibility.Visible;
            }
            else
            {
                currentValue &= 0xEF;
                sliderPwmChannel2.Visibility = Visibility.Hidden;
                textBlockPwmChannel2.Visibility = Visibility.Hidden;
                labelPwmChannel2.Visibility = Visibility.Hidden;
            }

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxPwm100Channel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((bool)checkBoxPwm100Channel2.IsChecked)
                currentValue |= 0x20;
            else
                currentValue &= 0xDF;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxLowFetChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG2);

            if ((bool)checkBoxLowFetChannel2.IsChecked)
                currentValue |= 0x08;  // If we want it on, set the LSbit
            else
                currentValue &= 0xF7;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxHighFetChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG2);

            if ((bool)checkBoxHighFetChannel2.IsChecked)
                currentValue |= 0x10;
            else
                currentValue &= 0xEF;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxThermalResponseChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG2);

            if ((bool)checkBoxThermalResponseChannel2.IsChecked)
                currentValue |= 0x20;
            else
                currentValue &= 0xDF;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG2, currentValue, myAddress);  // write it out
        }

        private void sliderAnalogCurrentChannel2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockAnalogCurrentChannel2.Text = ((UInt16)sliderAnalogCurrentChannel2.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderAnalogCurrentChannel2.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LH2IADJH, (byte)((integerValue >> 2) & 0xFF), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.LH2IADJL, (byte)(integerValue & 0x03), myAddress);  // write it out low
        }

        private void sliderOnTimeChannel2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockOnTimeChannel2.Text = ((ushort)sliderOnTimeChannel2.Value).ToString(CultureInfo.InvariantCulture);
            byte integerValue = byte.Parse(sliderOnTimeChannel2.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LH2TON, (byte)(integerValue & 0x3F), myAddress);  // write it out
        }

        private void sliderPwmChannel2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockPwmChannel2.Text = ((UInt16)sliderPwmChannel2.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderPwmChannel2.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCH2PWMH, (byte)((integerValue >> 8) & 0x03), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCH2PWML, (byte)(integerValue & 0xFF), myAddress);  // write it out low
        }    

        private void comboBoxFaultTimer_SelectionChanged(object sender, SelectionChangedEventArgs e)  // both channels
        {
            byte currentValue = (byte)(currentRegValue((byte)Registers.LHCFG2) & 0x3F);
            currentValue |= (byte)((comboBoxFaultTimer.SelectedIndex << 6) & 0xC0);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxLhiReference_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((bool)checkBoxLhiReference.IsChecked)
            {
                currentValue &= 0xBF;
//                sliderAnalogCurrentChannel1.IsEnabled = false;
  //              sliderAnalogCurrentChannel2.IsEnabled = false;
            }
            else
            {
                currentValue |= 0x40;
    //            sliderAnalogCurrentChannel1.IsEnabled = true;
      //          sliderAnalogCurrentChannel2.IsEnabled = true;                             
            }

            readLHI();

            Globals.mcuCommand.sendSPI(true, (byte)Registers.LHCFG1, currentValue, myAddress);  // write it out
        }

        public void readLHI()
        {
            byte currentValue1 = (byte)(currentRegValue((byte)Registers.LHIL) & 0x03);
            byte currentValue2 = currentRegValue((byte)Registers.LHIH);

            textBlockLhiDevice.Text = ((UInt16)(currentValue1 | (currentValue2 << 2))).ToString();
        }

        private byte currentRegValue(byte reg)
        {
            SPIcommadReturnData scrd = new SPIcommadReturnData();

            Globals.mcuCommand.sendSPI(false, reg, 0, myAddress);
            Globals.mcuCommand.sendSPI(false, reg, 0, myAddress);
            scrd = Globals.mcuCommand.sendSPI(false, reg, 0, myAddress);  // Now send it again to get the data back

            return (byte)(scrd.assembledReturn & 0xFF);
        }

        public void buttonReadAll_Clicked()
        {
            eventsOff();

            byte currentValue = currentRegValue((byte)Registers.LHCFG1);

            if ((currentValue & 0x01) == 0x01)
                checkBoxEnableChannel1.IsChecked = true;
            else
                checkBoxEnableChannel1.IsChecked = false;

            if ((currentValue & 0x02) == 0x02)
                checkBoxPwmChannel1.IsChecked = true;
            else
                checkBoxPwmChannel1.IsChecked = false;

            if ((currentValue & 0x04) == 0x04)
                checkBoxPwm100Channel1.IsChecked = true;
            else
                checkBoxPwm100Channel1.IsChecked = false;

            if ((currentValue & 0x08) == 0x08)
                checkBoxEnableChannel2.IsChecked = true;
            else
                checkBoxEnableChannel2.IsChecked = false;

            if ((currentValue & 0x10) == 0x10)
                checkBoxPwmChannel2.IsChecked = true;
            else
                checkBoxPwmChannel2.IsChecked = false;

            if ((currentValue & 0x20) == 0x20)
                checkBoxPwm100Channel2.IsChecked = true;
            else
                checkBoxPwm100Channel2.IsChecked = false;

            if ((currentValue & 0x40) == 0x40)
            {
                checkBoxLhiReference.IsChecked = true;
  //              sliderAnalogCurrentChannel1.IsEnabled = true;
    //            sliderAnalogCurrentChannel2.IsEnabled = true;
            }
            else
            {
                checkBoxLhiReference.IsChecked = false;
      //          sliderAnalogCurrentChannel1.IsEnabled = false;
        //        sliderAnalogCurrentChannel2.IsEnabled = false;
            }

            currentValue = currentRegValue((byte)Registers.LHCFG2);

            if ((currentValue & 0x01) == 0x01)
                checkBoxLowFetChannel1.IsChecked = true;
            else
                checkBoxLowFetChannel1.IsChecked = false;

            if ((currentValue & 0x02) == 0x02)
                checkBoxHighFetChannel1.IsChecked = true;
            else
                checkBoxHighFetChannel1.IsChecked = false;

            if ((currentValue & 0x04) == 0x04)
                checkBoxThermalResponseChannel1.IsChecked = true;
            else
                checkBoxThermalResponseChannel1.IsChecked = false;

            if ((currentValue & 0x08) == 0x08)
                checkBoxLowFetChannel2.IsChecked = true;
            else
                checkBoxLowFetChannel2.IsChecked = false;

            if ((currentValue & 0x10) == 0x10)
                checkBoxHighFetChannel2.IsChecked = true;
            else
                checkBoxHighFetChannel2.IsChecked = false;

            if ((currentValue & 0x20) == 0x20)
                checkBoxThermalResponseChannel2.IsChecked = true;
            else
                checkBoxThermalResponseChannel2.IsChecked = false;

            currentValue = currentRegValue((byte)Registers.LH1IADJL);
            UInt16 currentValue2 = (UInt16)(currentValue | (currentRegValue((byte)Registers.LH1IADJH) << 2));
            sliderAnalogCurrentChannel1.Value = currentValue2;
            textBlockAnalogCurrentChannel1.Text = currentValue2.ToString();

            currentValue = currentRegValue((byte)Registers.LH1TON);
            sliderOnTimeChannel1.Value = currentValue;
            textBlockOnTimeChannel1.Text = currentValue.ToString();

            if (sliderPwmChannel1.Visibility == Visibility.Visible)
            {
                currentValue = currentRegValue((byte)Registers.LHCH1PWML);
                currentValue2 = (UInt16)(currentValue | ((currentRegValue((byte)Registers.LHCH1PWMH) & 0x03) << 8));
                sliderPwmChannel1.Value = currentValue2;
                textBlockPwmChannel1.Text = currentValue2.ToString();
            }

            currentValue = currentRegValue((byte)Registers.LH2IADJL);
            currentValue2 = (UInt16)(currentValue | (currentRegValue((byte)Registers.LH2IADJH) << 2));
            sliderAnalogCurrentChannel2.Value = currentValue2;
            textBlockAnalogCurrentChannel2.Text = currentValue2.ToString();

            currentValue = currentRegValue((byte)Registers.LH2TON);
            sliderOnTimeChannel2.Value = currentValue;
            textBlockOnTimeChannel2.Text = currentValue.ToString();

            if (sliderPwmChannel2.Visibility == Visibility.Visible)
            {
                currentValue = currentRegValue((byte)Registers.LHCH2PWML);
                currentValue2 = (UInt16)(currentValue | ((currentRegValue((byte)Registers.LHCH2PWMH) & 0x03) << 8));
                sliderPwmChannel2.Value = currentValue2;
                textBlockPwmChannel2.Text = currentValue2.ToString();
            }

            comboBoxFaultTimer.SelectedIndex = (currentRegValue((byte)Registers.LHCFG2) & 0xC0) >> 6;

            currentValue2 = (UInt16)(currentRegValue((byte)Registers.LHIFILTH) << 2);
            currentValue = (byte)(currentRegValue((byte)Registers.LHIFILTL) & 0x03);
            currentValue2 |= (UInt16)currentValue;
            textBlockLhifiltDevice.Text = currentValue2.ToString();

            readLHI();

            eventsOn();
        }

        private void buttonLimpMode_Click(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.SYSCFG1);

            if (!limpModeActive)
            {
                buttonLimpMode.Content = "Limp Mode Off";
                limpModeActive = true;
                currentValue |= 0x20;
            }

            else
            {
                buttonLimpMode.Content = "Limp Mode On";
                limpModeActive = false;
                currentValue &= 0xDF;
            }

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
        }


}
}

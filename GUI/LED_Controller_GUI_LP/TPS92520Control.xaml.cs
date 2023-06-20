using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TPS9266xEvaluationModule
{
    /// <summary>
    /// Interaction logic for TPS92520Control.xaml
    /// </summary>
    public enum Registers : byte  // outside of class so limp home object can utilize
    {
        SYSCFG1 = 0x00,
        SYSCFG2 = 0x01,
        CMWTAP = 0x02,
        STATUS1 = 0x03,
        STATUS2 = 0x04,
        STATUS3 = 0x05,
        TWLMT = 0x06,
        SLEEP = 0x07,
        CH1IADJL = 0x08,
        CH1IADJH = 0x09,
        CH2IADJL = 0x0A,
        CH2IADJH = 0x0B,
        PWMDIV = 0x0C,
        CH1PWML = 0x0D,
        CH1PWMH = 0x0E,
        CH2PWML = 0x0F,
        CH2PWMH = 0x10,
        CH1TON = 0x11,
        CH2TON = 0x12,
        CH1VIN = 0x13,
        CH1VLED = 0x14,
        CH1VLEDON = 0x15,
        CH1VLEDOFF = 0x16,
        CH2VIN = 0x17,
        CH2VLED = 0x18,
        CH2VLEDON = 0x19,
        CH2VLEDOFF = 0x1A,
        TEMPL = 0x1B,
        TEMPH = 0x1C,
        V5D = 0x1D,
        LHCFG1 = 0x1E,
        LHCFG2 = 0x1F,
        LHIL = 0x20,
        LHIH = 0x21,
        LHIFILTL = 0x22,
        LHIFILTH = 0x23,
        LH1IADJL = 0x24,
        LH1IADJH = 0x25,
        LH2IADJL = 0x26,
        LH2IADJH = 0x27,
        LHCH1PWML = 0x28,
        LHCH1PWMH = 0x29,
        LHCH2PWML = 0x2A,
        LHCH2PWMH = 0x2B,
        LH1TON = 0x2C,
        LH2TON = 0x2D,
        RESET = 0x2E,
        PASSWD = 0x3E,
        TESTMODE = 0x3F
    };

    public partial class TPS92520Control : UserControl
    {
        public bool limpModeActive = false;
        public byte myAddress;
        private System.Timers.Timer update520Timer;
        private LimpHomeMode limpHomeMode;
        private bool initialRun;
        private bool timerOn = false;
        private int tabIndexSavedParams;
    //    private Cursor _previousCursor;
        private bool sliderInitValues1 = false;
        private bool sliderInitValues2 = false;
        private const double ANAOLOG_CURRENT_MIN = 100;
        private const double ON_TIME_MIN = 9;
        private const double PWM_MIN = 5;
        private bool CUSTOMER_CODE = false;
        private MainWindow mainWindow = null;  // get the MainWindow object
        private const byte NUM_REGS = 0x2F;

        public TPS92520Control(byte thisAddress, IndividualTabSettings savedTabControlSettings, bool firstTime, int tabIndex, MainWindow master)
        {
            InitializeComponent();
            mainWindow = master;

            groupBoxWatchDog.Visibility = Visibility.Hidden;
            if (CUSTOMER_CODE)
            {
                checkBoxLowFetChannel1.Visibility = Visibility.Hidden;
                checkBoxHighFetChannel1.Visibility = Visibility.Hidden;
                checkBoxThermalResponseChannel1.Visibility = Visibility.Hidden;
                checkBoxLowFetChannel2.Visibility = Visibility.Hidden;
                checkBoxHighFetChannel2.Visibility = Visibility.Hidden;
                checkBoxThermalResponseChannel2.Visibility = Visibility.Hidden;

                buttonLimpHome.Visibility = Visibility.Hidden;
                buttonSleepDevice.Visibility = Visibility.Hidden;
                labelViolationsDevice.Visibility = Visibility.Hidden;
                textBlockWatchdogViolations.Visibility = Visibility.Hidden;
                master.mcuc.Visibility = Visibility.Hidden;
                master.groupBoxMCUcontrol.Visibility = Visibility.Hidden;
                checkBoxEnableChannel1.Content = "Enable (Vin < 48V at start up)";
                checkBoxEnableChannel2.Content = "Enable (Vin < 48V at start up)";
            }

  //          _previousCursor = Mouse.OverrideCursor;

            labelFpin.Visibility = Visibility.Hidden;
            textBlockPcDeviceFltPin.Visibility = Visibility.Hidden;

            update520Timer = new System.Timers.Timer();
            update520Timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            update520Timer.Interval = 500;  // auto update timer; can't make this faster, takes too long to update all the controls

            myAddress = thisAddress;
            tabIndexSavedParams = tabIndex;  // pass this to the limp mode object to save params properly
            initSetup();

            initialRun = firstTime;  // feels like the very first time...

            if (Globals.userSelectedEVM == 3)  // EVM = "TPS92520, TPS92682 - LPP129"
            {
                labelFpin.Visibility = Visibility.Hidden; // Visibility.Visible; new long board broke these
                textBlockPcDeviceFltPin.Visibility = Visibility.Hidden;
            }
        }

        private void initSetup()
        {
            textBlockAnalogCurrentChannel1.Text = "";
            textBlockOnTimeChannel1.Text = "";
            textBlockPwmChannel1.Text = "";
            textBlockCompovChannel1.Text = "";  // clear the status text boxes here so I can see them when in the xml editor
            textBlockShortChannel1.Text = "";   // channel 1
            textBlockHslimChannel1.Text = "";
            textBlockLslimChannel1.Text = "";
            textBlockOffminChannel1.Text = "";
            textBlockBstuvChannel1.Text = "";
            textBlockTpChannel1.Text = "";
            textBlockStatusChannel1.Text = "";
            textBlockVinAdcChannel1.Text = "";
            textBlockVinVoltsChannel1.Text = "";
            textBlockLedAdcChannel1.Text = "";
            textBlockLedVoltsChannel1.Text = "";
            textBlockLastOnAdcChannel1.Text = "";
            textBlockLastOnVoltsChannel1.Text = "";
            textBlockLastOffAdcChannel1.Text = "";
            textBlockLastOffVoltsChannel1.Text = "";
            textBlockPcDeviceFltPin.Text = "";
            checkBoxEnableChannel1.Checked -= checkBoxEnableChannel1_Checked;
            checkBoxEnableChannel1.IsChecked = false;  // default settings from spec.
            checkBoxEnableChannel1.Checked += checkBoxEnableChannel1_Checked;
            checkBoxPwmChannel1.Checked -= checkBoxPwmChannel1_Checked;
            checkBoxPwmChannel1.IsChecked = false;  // default settings from spec.
            checkBoxPwmChannel1.Checked += checkBoxPwmChannel1_Checked;
            checkBoxLowFetChannel1.Checked -= checkBoxLowFetChannel1_Checked;
            checkBoxLowFetChannel1.IsChecked = false;  // default settings from spec.
            checkBoxLowFetChannel1.Checked += checkBoxLowFetChannel1_Checked;
            checkBoxHighFetChannel1.Checked -= checkBoxHighFetChannel1_Checked;
            checkBoxHighFetChannel1.IsChecked = false;  // default settings from spec.
            checkBoxHighFetChannel1.Checked += checkBoxHighFetChannel1_Checked;
            checkBoxThermalResponseChannel1.Checked -= checkBoxThermalResponseChannel1_Checked;
            checkBoxThermalResponseChannel1.IsChecked = false;  // default settings from spec.
            checkBoxThermalResponseChannel1.Checked += checkBoxThermalResponseChannel1_Checked;
            sliderAnalogCurrentChannel1.ValueChanged -= sliderAnalogCurrentChannel1_ValueChanged;
            sliderAnalogCurrentChannel1.Value = 0;  // default at power up
            sliderAnalogCurrentChannel1.ValueChanged += sliderAnalogCurrentChannel1_ValueChanged;
            sliderOnTimeChannel1.ValueChanged -= sliderOnTimeChannel1_ValueChanged;
            sliderOnTimeChannel1.Value = 7;  // default at power up
            sliderOnTimeChannel1.ValueChanged += sliderOnTimeChannel1_ValueChanged;
            textBlockAnalogCurrentChannel1.Text = sliderAnalogCurrentChannel1.Value.ToString(CultureInfo.InvariantCulture);
            textBlockOnTimeChannel1.Text = sliderOnTimeChannel1.Value.ToString(CultureInfo.InvariantCulture);



            textBlockAnalogCurrentChannel2.Text = "";
            textBlockOnTimeChannel2.Text = "";
            textBlockPwmChannel2.Text = "";
            textBlockCompovChannel2.Text = "";  // channel 2
            textBlockShortChannel2.Text = "";
            textBlockHslimChannel2.Text = "";
            textBlockLslimChannel2.Text = "";
            textBlockOffminChannel2.Text = "";
            textBlockBstuvChannel2.Text = "";
            textBlockTpChannel2.Text = "";
            textBlockStatusChannel2.Text = "";
            textBlockVinAdcChannel2.Text = "";
            textBlockVinVoltsChannel2.Text = "";
            textBlockLedAdcChannel2.Text = "";
            textBlockLedVoltsChannel2.Text = "";
            textBlockLastOnAdcChannel2.Text = "";
            textBlockLastOnVoltsChannel2.Text = "";
            textBlockLastOffAdcChannel2.Text = "";
            textBlockLastOffVoltsChannel2.Text = "";
            checkBoxEnableChannel2.Checked -= checkBoxEnableChannel2_Checked;
            checkBoxEnableChannel2.IsChecked = false;  // default settings from spec.
            checkBoxEnableChannel2.Checked += checkBoxEnableChannel2_Checked;
            checkBoxPwmChannel2.Checked -= checkBoxPwmChannel2_Checked;
            checkBoxPwmChannel2.IsChecked = false;  // default settings from spec.
            checkBoxPwmChannel2.Checked += checkBoxPwmChannel2_Checked;
            checkBoxLowFetChannel2.Checked -= checkBoxLowFetChannel2_Checked;
            checkBoxLowFetChannel2.IsChecked = false;  // default settings from spec.
            checkBoxLowFetChannel2.Checked += checkBoxLowFetChannel2_Checked;
            checkBoxHighFetChannel2.Checked -= checkBoxHighFetChannel2_Checked;
            checkBoxHighFetChannel2.IsChecked = false;  // default settings from spec.
            checkBoxHighFetChannel2.Checked += checkBoxHighFetChannel2_Checked;
            checkBoxThermalResponseChannel2.Checked -= checkBoxThermalResponseChannel2_Checked;
            checkBoxThermalResponseChannel2.IsChecked = false;  // default settings from spec.
            checkBoxThermalResponseChannel2.Checked += checkBoxThermalResponseChannel2_Checked;
            sliderAnalogCurrentChannel2.ValueChanged -= sliderAnalogCurrentChannel2_ValueChanged;
            sliderAnalogCurrentChannel2.Value = 0;  // default at power up
            sliderAnalogCurrentChannel2.ValueChanged += sliderAnalogCurrentChannel2_ValueChanged;
            sliderOnTimeChannel2.ValueChanged -= sliderOnTimeChannel2_ValueChanged;
            sliderOnTimeChannel2.Value = 7;  // default at power up
            sliderOnTimeChannel2.ValueChanged += sliderOnTimeChannel2_ValueChanged;
            textBlockAnalogCurrentChannel2.Text = sliderAnalogCurrentChannel2.Value.ToString(CultureInfo.InvariantCulture);
            textBlockOnTimeChannel2.Text = sliderOnTimeChannel2.Value.ToString(CultureInfo.InvariantCulture);

            textBlockV5dAdcDevice.Text = "";
            textBlockV5dVoltsDevice.Text = "";
            textBlockTempCDevice.Text = "";
            textBlockReg0x00.Text = "";
            textBlockWatchdogViolations.Text = "0";  // device
            textBlockPcDevice.Text = "";
            textBlockTwDevice.Text = "";
            textBlockV5auvDevice.Text = "";
            textBlockStandAloneDevice.Text = "";
            textBoxTwLimitDevice.Text = "0x8A";  // default

            checkBoxPwmPhase.Checked -= checkBoxPwmPhase_Checked;
            checkBoxPwmPhase.IsChecked = false;
            checkBoxPwmPhase.Checked += checkBoxPwmPhase_Checked;

            comboBoxFaultTimer.SelectionChanged -= comboBoxFaultTimer_SelectionChanged;
            ObservableCollection<string> comboItemsFaultTimer = new ObservableCollection<string>();
            comboItemsFaultTimer.Add("4");
            comboItemsFaultTimer.Add("8");
            comboItemsFaultTimer.Add("16");
            comboItemsFaultTimer.Add("32");
            comboBoxFaultTimer.ItemsSource = comboItemsFaultTimer;
            comboBoxFaultTimer.SelectedIndex = 0;  // does not need to be hardware set, this is the part's default value
            comboBoxFaultTimer.SelectionChanged += comboBoxFaultTimer_SelectionChanged;

            comboBoxWatchDogTimer.SelectionChanged -= comboBoxWatchDogTimer_SelectionChanged;
            ObservableCollection<string> comboItemsWatchDogTimer = new ObservableCollection<string>();
            comboItemsWatchDogTimer.Add("6.5");
            comboItemsWatchDogTimer.Add("13.1");
            comboItemsWatchDogTimer.Add("26.2");
            comboItemsWatchDogTimer.Add("52.4");
            comboItemsWatchDogTimer.Add("104.8");
            comboItemsWatchDogTimer.Add("209.7");
            comboItemsWatchDogTimer.Add("419.4");
            comboItemsWatchDogTimer.Add("838.9");
            comboItemsWatchDogTimer.Add("1670");
            comboBoxWatchDogTimer.ItemsSource = comboItemsWatchDogTimer;
            comboBoxWatchDogTimer.SelectionChanged += comboBoxWatchDogTimer_SelectionChanged;
            comboBoxWatchDogTimer.SelectedIndex = 8;  // does not need to be hardware set, this is the part's default value            

            comboBoxPwmDividerDevice.SelectionChanged -= comboBoxPwmDividerDevice_SelectionChanged;
            ObservableCollection<string> comboItemsPwmDiv = new ObservableCollection<string>();
            comboItemsPwmDiv.Add("1395");
            comboItemsPwmDiv.Add("1221");
            comboItemsPwmDiv.Add("977");
            comboItemsPwmDiv.Add("814");
            comboItemsPwmDiv.Add("610");
            comboItemsPwmDiv.Add("407");
            if (!CUSTOMER_CODE)
            {
                comboItemsPwmDiv.Add("199");
                comboItemsPwmDiv.Add("100");
            }
            comboBoxPwmDividerDevice.ItemsSource = comboItemsPwmDiv;
            comboBoxPwmDividerDevice.SelectedIndex = 4;  // does not need to be hardware set, this is the part's default value
            comboBoxPwmDividerDevice.SelectionChanged += comboBoxPwmDividerDevice_SelectionChanged;
        }

        public void restoreCustomSettings(IndividualTabSettings savedTabControlSettings)
        {
            controlEventsOff();

            // set control
            checkBoxEnableChannel1.IsChecked = savedTabControlSettings.Enable1;  // channel 1
            checkBoxPwmChannel1.IsChecked = savedTabControlSettings.Enable2;
            checkBoxLowFetChannel1.IsChecked = savedTabControlSettings.Enable5;
            checkBoxHighFetChannel1.IsChecked = savedTabControlSettings.Enable6;
            checkBoxThermalResponseChannel1.IsChecked = savedTabControlSettings.Enable11;
            sliderAnalogCurrentChannel1.Value = savedTabControlSettings.Slider0_1;
            sliderOnTimeChannel1.Value = savedTabControlSettings.Slider1_1;
            sliderPwmChannel1.Value = savedTabControlSettings.Slider2_1;

            checkBoxEnableChannel2.IsChecked = savedTabControlSettings.Enable7;  // channel 2
            checkBoxPwmChannel2.IsChecked = savedTabControlSettings.Enable8;
            checkBoxLowFetChannel2.IsChecked = savedTabControlSettings.Enable9;
            checkBoxHighFetChannel2.IsChecked = savedTabControlSettings.Enable10;
            checkBoxThermalResponseChannel2.IsChecked = savedTabControlSettings.Enable12;
            sliderAnalogCurrentChannel2.Value = savedTabControlSettings.Slider0_2;
            sliderOnTimeChannel2.Value = savedTabControlSettings.Slider1_2;
            sliderPwmChannel2.Value = savedTabControlSettings.Slider2_2;

            comboBoxFaultTimer.SelectedIndex = savedTabControlSettings.Combo0;
            comboBoxWatchDogTimer.SelectedIndex = savedTabControlSettings.Combo1;
            comboBoxPwmDividerDevice.SelectedIndex = savedTabControlSettings.Combo2;
            textBoxTwLimitDevice.Text = savedTabControlSettings.Text1;
            checkBoxPwmPhase.IsChecked = savedTabControlSettings.Enable13;
            comboBoxPwmDividerDevice.SelectedIndex = savedTabControlSettings.Combo2;
            controlEventsOn();           
            resetControls();  // set control and register bits
        }

        private void controlEventsOff()
        {
            // turn off event
            checkBoxEnableChannel1.Checked -= checkBoxEnableChannel1_Checked;  // channel 1
            checkBoxEnableChannel1.Unchecked -= checkBoxEnableChannel1_Checked;  // channel 1
            checkBoxPwmChannel1.Checked -= checkBoxPwmChannel1_Checked;
            checkBoxPwmChannel1.Unchecked -= checkBoxPwmChannel1_Checked;
            checkBoxLowFetChannel1.Checked -= checkBoxLowFetChannel1_Checked;
            checkBoxLowFetChannel1.Unchecked -= checkBoxLowFetChannel1_Checked;
            checkBoxHighFetChannel1.Checked -= checkBoxHighFetChannel1_Checked;
            checkBoxHighFetChannel1.Unchecked -= checkBoxHighFetChannel1_Checked;
            checkBoxThermalResponseChannel1.Checked -= checkBoxThermalResponseChannel1_Checked;
            checkBoxThermalResponseChannel1.Unchecked -= checkBoxThermalResponseChannel1_Checked;
            sliderAnalogCurrentChannel1.ValueChanged -= sliderAnalogCurrentChannel1_ValueChanged;
            sliderOnTimeChannel1.ValueChanged -= sliderOnTimeChannel1_ValueChanged;
            sliderPwmChannel1.ValueChanged -= sliderPwmChannel1_ValueChanged;

            checkBoxEnableChannel2.Checked -= checkBoxEnableChannel2_Checked;  // channel 2
            checkBoxEnableChannel2.Unchecked -= checkBoxEnableChannel2_Checked;  // channel 2
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

            comboBoxFaultTimer.SelectionChanged -= comboBoxFaultTimer_SelectionChanged;
            comboBoxWatchDogTimer.SelectionChanged -= comboBoxWatchDogTimer_SelectionChanged;
            comboBoxPwmDividerDevice.SelectionChanged -= comboBoxPwmDividerDevice_SelectionChanged;
            checkBoxPwmPhase.Checked -= checkBoxPwmPhase_Checked;
            checkBoxPwmPhase.Unchecked -= checkBoxPwmPhase_Checked;
        }

        private void controlEventsOn()
        {
            // turn on event
            checkBoxEnableChannel1.Checked += checkBoxEnableChannel1_Checked;  // channel 1
            checkBoxEnableChannel1.Unchecked += checkBoxEnableChannel1_Checked;  // channel 1
            checkBoxPwmChannel1.Checked += checkBoxPwmChannel1_Checked;
            checkBoxPwmChannel1.Unchecked += checkBoxPwmChannel1_Checked;
            checkBoxLowFetChannel1.Checked += checkBoxLowFetChannel1_Checked;
            checkBoxLowFetChannel1.Unchecked += checkBoxLowFetChannel1_Checked;
            checkBoxHighFetChannel1.Checked += checkBoxHighFetChannel1_Checked;
            checkBoxHighFetChannel1.Unchecked += checkBoxHighFetChannel1_Checked;
            checkBoxThermalResponseChannel1.Checked += checkBoxThermalResponseChannel1_Checked;
            checkBoxThermalResponseChannel1.Unchecked += checkBoxThermalResponseChannel1_Checked;
            sliderAnalogCurrentChannel1.ValueChanged += sliderAnalogCurrentChannel1_ValueChanged;
            sliderOnTimeChannel1.ValueChanged += sliderOnTimeChannel1_ValueChanged;
            sliderPwmChannel1.ValueChanged += sliderPwmChannel1_ValueChanged;

            checkBoxEnableChannel2.Checked += checkBoxEnableChannel2_Checked;  // channel 2
            checkBoxEnableChannel2.Unchecked += checkBoxEnableChannel2_Checked;  // channel 2
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

            comboBoxFaultTimer.SelectionChanged += comboBoxFaultTimer_SelectionChanged;
            comboBoxWatchDogTimer.SelectionChanged += comboBoxWatchDogTimer_SelectionChanged;
            comboBoxPwmDividerDevice.SelectionChanged += comboBoxPwmDividerDevice_SelectionChanged;
            checkBoxPwmPhase.Checked += checkBoxPwmPhase_Checked;
            checkBoxPwmPhase.Unchecked += checkBoxPwmPhase_Checked;
        }

        public void saveCustomSettings(byte tabIndex)
        {
            var TabControlSettings = ConfigurationManager.GetSection("TabControlSettings") as TPS9266xEvaluationModule.TabControlSettings;
            IndividualTabSettings savedTabControlSettings = TabControlSettings.SelectedTab("Tab" + tabIndex);

            savedTabControlSettings.currentlySelectedTab += tabIndex;

            savedTabControlSettings.Enable1 = (bool)checkBoxEnableChannel1.IsChecked;  // channel 1
            savedTabControlSettings.Enable2 = (bool)checkBoxPwmChannel1.IsChecked;
            savedTabControlSettings.Enable5 = (bool)checkBoxLowFetChannel1.IsChecked;
            savedTabControlSettings.Enable6 = (bool)checkBoxHighFetChannel1.IsChecked;
            savedTabControlSettings.Enable11 = (bool)checkBoxThermalResponseChannel1.IsChecked;
            savedTabControlSettings.Slider0_1 = (UInt16)sliderAnalogCurrentChannel1.Value;
            savedTabControlSettings.Slider1_1 = (byte)sliderOnTimeChannel1.Value;
            savedTabControlSettings.Slider2_1 = (UInt16)sliderPwmChannel1.Value;

            savedTabControlSettings.Enable7 = (bool)checkBoxEnableChannel2.IsChecked;  // channel 2
            savedTabControlSettings.Enable8 = (bool)checkBoxPwmChannel2.IsChecked;
            savedTabControlSettings.Enable9 = (bool)checkBoxLowFetChannel2.IsChecked;
            savedTabControlSettings.Enable10 = (bool)checkBoxHighFetChannel2.IsChecked;
            savedTabControlSettings.Enable12 = (bool)checkBoxThermalResponseChannel2.IsChecked;
            savedTabControlSettings.Slider0_2 = (UInt16)sliderAnalogCurrentChannel2.Value;
            savedTabControlSettings.Slider1_2 = (byte)sliderOnTimeChannel2.Value;
            savedTabControlSettings.Slider2_2 = (UInt16)sliderPwmChannel2.Value;

            savedTabControlSettings.Combo0 = (byte)comboBoxFaultTimer.SelectedIndex;
            savedTabControlSettings.Combo1 = (byte)comboBoxWatchDogTimer.SelectedIndex;
            savedTabControlSettings.Combo2 = (byte)comboBoxPwmDividerDevice.SelectedIndex;
            savedTabControlSettings.Text1 = textBoxTwLimitDevice.Text;
            savedTabControlSettings.Enable13 = (bool)checkBoxPwmPhase.IsChecked;
        }

        private void resetControls()
        {
    //        checkBoxEnableChannel1_Checked(this, null);  // channel 1
            checkBoxPwmChannel1_Checked(this, null);
            checkBoxLowFetChannel1_Checked(this, null);
            checkBoxHighFetChannel1_Checked(this, null);
            checkBoxThermalResponseChannel1_Checked(this, null);
            sliderAnalogCurrentChannel1_ValueChanged(this, null);
            sliderOnTimeChannel1_ValueChanged(this, null);
            sliderPwmChannel1_ValueChanged(this, null);
            checkBoxPwmPhase_Checked(this, null);

     //       checkBoxEnableChannel2_Checked(this, null);  // channel 2
            checkBoxPwmChannel2_Checked(this, null);
            checkBoxLowFetChannel2_Checked(this, null);
            checkBoxHighFetChannel2_Checked(this, null);
            checkBoxThermalResponseChannel2_Checked(this, null);
            sliderAnalogCurrentChannel2_ValueChanged(this, null);
            sliderOnTimeChannel2_ValueChanged(this, null);
            sliderPwmChannel2_ValueChanged(this, null);

            comboBoxFaultTimer_SelectionChanged(this, null);
            comboBoxWatchDogTimer_SelectionChanged(this, null);
            comboBoxPwmDividerDevice_SelectionChanged(this, null);
            setTwLimit();
            buttonSleepDevice.Content = "Sleep On";

            checkBoxEnableChannel1_Checked(this, null);  // set enables after all others are set
            checkBoxEnableChannel2_Checked(this, null);
        }      

        // channel 1
        private void checkBoxEnableChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

            if ((bool)checkBoxEnableChannel1.IsChecked)
            {
                currentValue |= 0x01;  // If we want it on, set the LSbit
                if (sliderInitValues1 == false)
                    initChannelsDevice1();
            }
            else
                currentValue &= 0xFE;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
        }

        public void initChannelsDevice1()
        {
 //           if (sliderInitValues1 == false)
   //         {
                sliderAnalogCurrentChannel1.Value = ANAOLOG_CURRENT_MIN;
                sliderOnTimeChannel1.Value = ON_TIME_MIN;
      //          sliderPwmChannel1.Value = PWM_MIN;
                sliderAnalogCurrentChannel1_ValueChanged(this, null);
                sliderOnTimeChannel1_ValueChanged(this, null);
        //        sliderPwmChannel1_ValueChanged(this, null);
                sliderInitValues1 = true;
     //       }
        }

        private void checkBoxPwmChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

            if ((bool)checkBoxPwmChannel1.IsChecked)
                currentValue |= 0x02;
            else
                currentValue &= 0xFD;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxLowFetChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.SYSCFG2);

            if ((bool)checkBoxLowFetChannel1.IsChecked)
                currentValue |= 0x01;  // If we want it on, set the LSbit
            else
                currentValue &= 0xFE;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxHighFetChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.SYSCFG2);

            if ((bool)checkBoxHighFetChannel1.IsChecked)
                currentValue |= 0x02;
            else
                currentValue &= 0xFD;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxThermalResponseChannel1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.SYSCFG2);

            if ((bool)checkBoxThermalResponseChannel1.IsChecked)
                currentValue |= 0x04;
            else
                currentValue &= 0xFB;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG2, currentValue, myAddress);  // write it out
        }

        private void sliderAnalogCurrentChannel1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
  //          if (sliderAnalogCurrentChannel1.Value > ANAOLOG_CURRENT_MIN)
    //            sliderAnalogCurrentChannel1.Value = ANAOLOG_CURRENT_MIN;
     //       sliderInitValues1 = true;
            textBlockAnalogCurrentChannel1.Text = ((UInt16)sliderAnalogCurrentChannel1.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderAnalogCurrentChannel1.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1IADJH, (byte)((integerValue >> 2) & 0xFF), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1IADJL, (byte)(integerValue & 0x03), myAddress);  // write it out low
        }

        private void sliderOnTimeChannel1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
    //        if (sliderOnTimeChannel1.Value > ON_TIME_MIN)
      //          sliderOnTimeChannel1.Value = ON_TIME_MIN;
  //          sliderInitValues1 = true;
            textBlockOnTimeChannel1.Text = ((ushort)sliderOnTimeChannel1.Value).ToString(CultureInfo.InvariantCulture);
            byte integerValue = byte.Parse(sliderOnTimeChannel1.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1TON, (byte)(integerValue & 0x3F), myAddress);  // write it out
        }

        private void sliderPwmChannel1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //           if (sliderPwmChannel1.Value < PWM_MIN)
            //             sliderPwmChannel1.Value = PWM_MIN;
      //      sliderInitValues1 = true;
            textBlockPwmChannel1.Text = ((UInt16)sliderPwmChannel1.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderPwmChannel1.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);
         
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1PWMH, (byte)((integerValue >> 8) & 0x03), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1PWML, (byte)(integerValue & 0xFF), myAddress);  // write it out low
        }

        // channel 2
        private void checkBoxEnableChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

            if ((bool)checkBoxEnableChannel2.IsChecked)
            {
                currentValue |= 0x04;
                if (sliderInitValues2 == false)
                    initChannelsDevice2();
            }
            else
                currentValue &= 0xFB;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
        }

        public void initChannelsDevice2()
        {
   //         if (sliderInitValues1 == false)
     //       {
                sliderAnalogCurrentChannel2.Value = ANAOLOG_CURRENT_MIN;
                sliderOnTimeChannel2.Value = ON_TIME_MIN;
       //         sliderPwmChannel2.Value = PWM_MIN;
                sliderAnalogCurrentChannel2_ValueChanged(this, null);
                sliderOnTimeChannel2_ValueChanged(this, null);
            //    sliderPwmChannel2_ValueChanged(this, null);
                sliderInitValues1 = true;
       //     }
        }

        private void checkBoxPwmChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

            if ((bool)checkBoxPwmChannel2.IsChecked)
                currentValue |= 0x08;
            else
                currentValue &= 0xF7;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxLowFetChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG2);

            if ((bool)checkBoxLowFetChannel2.IsChecked)
                currentValue |= 0x08;  // If we want it on, set the LSbit
            else
                currentValue &= 0xF7;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxHighFetChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.SYSCFG2);

            if ((bool)checkBoxHighFetChannel2.IsChecked)
                currentValue |= 0x10;
            else
                currentValue &= 0xEF;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG2, currentValue, myAddress);  // write it out
        }

        private void checkBoxThermalResponseChannel2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.SYSCFG2);

            if ((bool)checkBoxThermalResponseChannel1.IsChecked)
                currentValue |= 0x20;
            else
                currentValue &= 0xDF;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG2, currentValue, myAddress);  // write it out
        }

        private void sliderAnalogCurrentChannel2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
//            if (sliderAnalogCurrentChannel2.Value < ANAOLOG_CURRENT_MIN)
  //              sliderAnalogCurrentChannel2.Value = ANAOLOG_CURRENT_MIN;
   //         sliderInitValues2 = true;
            textBlockAnalogCurrentChannel2.Text = ((UInt16)sliderAnalogCurrentChannel2.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderAnalogCurrentChannel2.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2IADJH, (byte)((integerValue >> 2) & 0xFF), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2IADJL, (byte)(integerValue & 0x03), myAddress);  // write it out low         
        }

        private void sliderOnTimeChannel2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
 //           if (sliderOnTimeChannel2.Value < ON_TIME_MIN)
   //            sliderOnTimeChannel2.Value = ON_TIME_MIN;
   //         sliderInitValues2 = true;
            textBlockOnTimeChannel2.Text = ((ushort)sliderOnTimeChannel2.Value).ToString(CultureInfo.InvariantCulture);
            byte integerValue = byte.Parse(sliderOnTimeChannel2.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2TON, (byte)(integerValue & 0x3F), myAddress);  // write it out
        }

        private void sliderPwmChannel2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //       if (sliderPwmChannel2.Value < PWM_MIN) && (initialRun)
            //         sliderPwmChannel2.Value = PWM_MIN;
  //          sliderInitValues2 = true;
            textBlockPwmChannel2.Text = ((UInt16)sliderPwmChannel2.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderPwmChannel2.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);
            
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2PWMH, (byte)((integerValue >> 8) & 0x03), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2PWML, (byte)(integerValue & 0xFF), myAddress);  // write it out low
        }

        private void buttonLimpHome_Click(object sender, RoutedEventArgs e)
        {
            if (!limpModeActive)
                limpModeActive = true;
            else
                limpModeActive = false;

            limpModeWindow(limpModeActive);
        }

        private void limpModeWindowClosed(object sender, EventArgs e)
        {
            limpHomeMode.Closed -= new EventHandler(limpModeWindowClosed);
            buttonLimpHome_Click(this, null);
        }

        public void limpModeWindow(bool enableLimpMode)
        {
            try
            {
                if (enableLimpMode)
                {
                    if (limpHomeMode == null)
                    {
                        limpHomeMode = new LimpHomeMode(myAddress, initialRun, tabIndexSavedParams);
                        limpHomeMode.Title += ": Device Address " + myAddress;
                        limpHomeMode.Closed += new EventHandler(limpModeWindowClosed);
                        limpHomeMode.Show();
                        Dispatcher.Run();
                    }
                    else
                        limpModeWindowHide(false);
                }
                else
                {
                    if (limpHomeMode != null)
                    {
                        limpHomeMode.Closed -= new EventHandler(limpModeWindowClosed);  // call event handler only if "X" to close window was pushed
                        limpHomeMode.Close();
                        limpHomeMode.Closed += new EventHandler(limpModeWindowClosed);
                        limpHomeMode = null;
                    }
                }
            }
            catch (ThreadAbortException)
            {
#if DEBUG
                MessageBox.Show("Error LT: ", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                limpHomeMode.Close();
                Dispatcher.InvokeShutdown();
            }
        }

        public void limpModeWindowHide(bool hide)
        {
            if (limpHomeMode != null)
            {
                if(hide)
                    limpHomeMode.Visibility = Visibility.Collapsed;
                else
                    limpHomeMode.Visibility = Visibility.Visible;
            }
        }

        private void buttonResetFaults_Click(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

            currentValue |= 0x80;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
            readStatusRegs();
        }

        private void comboBoxFaultTimer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            byte currentValue = (byte)(currentRegValue((byte)Registers.SYSCFG2) & 0x3F);
            currentValue |= (byte)((comboBoxFaultTimer.SelectedIndex << 6) & 0xC0);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG2, currentValue, myAddress);  // write it out
        }

        private void comboBoxWatchDogTimer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            byte currentValue = (byte)(currentRegValue((byte)Registers.CMWTAP) & 0xF0);

            currentValue |= (byte)(comboBoxWatchDogTimer.SelectedIndex & 0x0F);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CMWTAP, currentValue, myAddress);  // write it out
        }

        public void comboBoxWatchDogTimer_SelectionChanged(byte newIndex)
        {
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CMWTAP, newIndex, myAddress);  // write it out
        }

        private void readStatusRegs()
        {
            try  // in case we lose comms during the read
            {
                byte statusReg1 = currentRegValue((byte)Registers.STATUS1);
                byte statusReg2 = currentRegValue((byte)Registers.STATUS2);
                byte statusReg3 = currentRegValue((byte)Registers.STATUS3);

                // status 1
                if ((statusReg1 & 0x01) == 0x01)  // CH1COMPOV
                    textBlockCompovChannel1.Background = Brushes.Red;
                else
                    textBlockCompovChannel1.Background = Brushes.Green;

                if ((statusReg1 & 0x02) == 0x02)  // CH1SHORT
                    textBlockShortChannel1.Background = Brushes.Red;
                else
                    textBlockShortChannel1.Background = Brushes.Green;

                if ((statusReg1 & 0x04) == 0x04)  // CH1HSILIM
                    textBlockHslimChannel1.Background = Brushes.Red;
                else
                    textBlockHslimChannel1.Background = Brushes.Green;

                if ((statusReg1 & 0x08) == 0x08)  // CH1LSILIM
                    textBlockLslimChannel1.Background = Brushes.Red;
                else
                    textBlockLslimChannel1.Background = Brushes.Green;

                if ((statusReg1 & 0x10) == 0x10)  // CH2COMPOV
                    textBlockCompovChannel2.Background = Brushes.Red;
                else
                    textBlockCompovChannel2.Background = Brushes.Green;

                if ((statusReg1 & 0x20) == 0x20)  // CH2SHORT
                    textBlockShortChannel2.Background = Brushes.Red;
                else
                    textBlockShortChannel2.Background = Brushes.Green;

                if ((statusReg1 & 0x40) == 0x40)  // CH2HSILIM
                    textBlockHslimChannel2.Background = Brushes.Red;
                else
                    textBlockHslimChannel2.Background = Brushes.Green;

                if ((statusReg1 & 0x80) == 0x80)  // CH2LSILIM
                    textBlockLslimChannel2.Background = Brushes.Red;
                else
                    textBlockLslimChannel2.Background = Brushes.Green;

                // status 2
                if ((statusReg2 & 0x01) == 0x01)  // CH1TOFFMIN
                    textBlockOffminChannel1.Background = Brushes.Red;
                else
                    textBlockOffminChannel1.Background = Brushes.Green;

                if ((statusReg2 & 0x02) == 0x02)  // CH1BSTUV
                    textBlockBstuvChannel1.Background = Brushes.Red;
                else
                    textBlockBstuvChannel1.Background = Brushes.Green;

                if ((statusReg2 & 0x04) == 0x04)  // CH1TP
                    textBlockTpChannel1.Background = Brushes.Red;
                else
                    textBlockTpChannel1.Background = Brushes.Green;

                if ((statusReg2 & 0x08) == 0x08)  // CH2TOFFMIN
                    textBlockOffminChannel2.Background = Brushes.Red;
                else
                    textBlockOffminChannel2.Background = Brushes.Green;

                if ((statusReg2 & 0x10) == 0x10)  // CH2BSTUV
                    textBlockBstuvChannel2.Background = Brushes.Red;
                else
                    textBlockBstuvChannel2.Background = Brushes.Green;

                if ((statusReg2 & 0x20) == 0x20)  // CH2TP
                    textBlockTpChannel2.Background = Brushes.Red;
                else
                    textBlockTpChannel2.Background = Brushes.Green;

                // status 3
                if ((statusReg3 & 0x01) == 0x01)  // CH1STATUS
                    textBlockStatusChannel1.Background = Brushes.Red;
                else
                    textBlockStatusChannel1.Background = Brushes.Green;

                if ((statusReg3 & 0x02) == 0x02)  // CH2STATUS
                    textBlockStatusChannel2.Background = Brushes.Red;
                else
                    textBlockStatusChannel2.Background = Brushes.Green;

                if ((statusReg3 & 0x04) == 0x04)  // PC
                    textBlockPcDevice.Background = Brushes.Red;
                else
                    textBlockPcDevice.Background = Brushes.Green;

                if ((statusReg3 & 0x08) == 0x08)  // TW
                    textBlockTwDevice.Background = Brushes.Red;
                else
                    textBlockTwDevice.Background = Brushes.Green;

                textBlockWatchdogViolations.Text = ((statusReg3 & 0x30) >> 4).ToString();  // number of watchdog violations

                if ((statusReg3 & 0x40) == 0x40)  // TW
                    textBlockV5auvDevice.Background = Brushes.Red;
                else
                    textBlockV5auvDevice.Background = Brushes.Green;

                if ((statusReg3 & 0x80) == 0x80)  // Standalone
                    textBlockStandAloneDevice.Background = Brushes.Red;
                else
                    textBlockStandAloneDevice.Background = Brushes.Green; 

                if (textBlockPcDeviceFltPin.Visibility == Visibility.Visible) 
                {
                    bool? pinStatus = null;
                    if(myAddress == 1)
                        pinStatus = mainWindow.ac.currentGPIOpinState("PP5");  // We are actively using EVM = "TPS92520, TPS92682 - LPP129"
                    else if (myAddress == 2)
                        pinStatus = mainWindow.ac.currentGPIOpinState("PH2");  // We are actively using EVM = "TPS92520, TPS92682 - LPP129"
                    if (pinStatus == null)  // there was an error getting the pin status
                    {
                        labelFpin.Visibility = Visibility.Hidden;
                        textBlockPcDeviceFltPin.Visibility = Visibility.Hidden;
                    }
                    else if ((bool)pinStatus)  // fault
                        textBlockPcDeviceFltPin.Background = Brushes.Red;
                    else  // fault
                        textBlockPcDeviceFltPin.Background = Brushes.Green;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LU: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private byte currentRegValue(byte reg)
        {
            SPIcommadReturnData scrd = new SPIcommadReturnData();
            Globals.mcuCommand.sendSPI(false, reg, 0, myAddress);
            scrd = Globals.mcuCommand.sendSPI(false, reg, 0, myAddress);  // Now send it again to get the data back

            return (byte)(scrd.assembledReturn & 0xFF);
        }

        private void setTwLimit()
        {
            try
            {
                byte twLimit;

                if (textBoxTwLimitDevice.Text.Contains("0x"))
                    twLimit = byte.Parse(textBoxTwLimitDevice.Text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                else
                    twLimit = byte.Parse(textBoxTwLimitDevice.Text, CultureInfo.InvariantCulture);

                textBlockTempCDevice.Text = (((twLimit * 4) * .7168) - 271.5).ToString("0");  // x 4 to account for bit shift of 2 in register value
                Globals.mcuCommand.sendSPI(true, (byte)Registers.TWLMT, twLimit, myAddress);  // write it out
            }
            catch (Exception ex)
            {
                textBlockTempCDevice.Text = "";
                MessageBox.Show("Please enter a valid value for Thermal Warning Limit (0 - 255)", "Thermal Warning Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void textBoxTwLimitDevice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBoxTwLimitDevice.LostFocus -= textBoxTwLimitDevice_LostFocus;  // remove event
                setTwLimit();
                textBoxTwLimitDevice.LostFocus += textBoxTwLimitDevice_LostFocus;  // add it back
            }
        }

        private void textBoxTwLimitDevice_LostFocus(object sender, RoutedEventArgs e)
        {
            setTwLimit();
        }

        void updateAll()
        {
            if (timerOn)
                update520Timer.Enabled = false;
                        
            if (buttonUpdateAllDevice.Content.ToString().Contains("Off"))
            {
                if (textBlockReg0x00.Text == "0xFF")
                {                   
                    buttonUpdateAllDevice_Click(this, null);
                }
            }
            readStatusRegs();  // update status regs
            updateADCs();      // display ADCs
            currentTmpC();     // show temp C

            if (limpHomeMode != null)  // limp mode LHI update
                limpHomeMode.readLHI();

            if (timerOn)
                update520Timer.Enabled = true;

            textBlockReg0x00.Text = "0x" + currentRegValue((byte)Registers.SYSCFG1).ToString("X2");
        }

        void updateADCs()
        {
            vinChannelX((byte)Registers.CH1VIN, 1);  // channel 1
            ledChannelX((byte)Registers.CH1VLED, 1);
            lastOnChannelX((byte)Registers.CH1VLEDON, 1);
            lastOffChannelX((byte)Registers.CH1VLEDOFF, 1);

            vinChannelX((byte)Registers.CH2VIN, 2);  // channel 2
            ledChannelX((byte)Registers.CH2VLED, 2);
            lastOnChannelX((byte)Registers.CH2VLEDON, 2);
            lastOffChannelX((byte)Registers.CH2VLEDOFF, 2);

            v5dDevice((byte)Registers.V5D);

    //        lastOnChannelX((byte)Registers.CH1VLEDON, 1);
      //      lastOnChannelX((byte)Registers.CH2VLEDON, 2);
        }

        private void adcVoltsDisplay(byte register, TextBlock tbDAC, TextBlock tbVolts, double coeff1, double coeff2)
        {
            UInt16 currentValue = (UInt16)(currentRegValue(register) & 0xFF);  // get current value
            tbDAC.Text = currentValue.ToString();
            tbVolts.Text = ((currentValue * coeff1) / coeff2).ToString("0.0");
        }

        private void vinChannelX(byte register, int channel)
        {
            if(channel == 1)
                adcVoltsDisplay(register, textBlockVinAdcChannel1, textBlockVinVoltsChannel1, .009725, .037);
            else
                adcVoltsDisplay(register, textBlockVinAdcChannel2, textBlockVinVoltsChannel2, .009725, .037);
        }

        private void ledChannelX(byte register, int channel)
        {
            if (channel == 1)
                adcVoltsDisplay(register, textBlockLedAdcChannel1, textBlockLedVoltsChannel1, .009725, .037);
            else
                adcVoltsDisplay(register, textBlockLedAdcChannel2, textBlockLedVoltsChannel2, .009725, .037);
        }

        private void lastOnChannelX(byte register, int channel)
        {
            if (channel == 1)
                adcVoltsDisplay(register, textBlockLastOnAdcChannel1, textBlockLastOnVoltsChannel1, .009725, .037);
            else
                adcVoltsDisplay(register, textBlockLastOnAdcChannel2, textBlockLastOnVoltsChannel2, .009725, .037);
        }

        private void lastOffChannelX(byte register, int channel)
        {
            if (channel == 1)
                adcVoltsDisplay(register, textBlockLastOffAdcChannel1, textBlockLastOffVoltsChannel1, .009725, .037);
            else
                adcVoltsDisplay(register, textBlockLastOffAdcChannel2, textBlockLastOffVoltsChannel2, .009725, .037);
        }

        private void currentTmpC()
        {
            UInt16 tempL = currentRegValue((byte)Registers.TEMPL);
            UInt16 tempH = currentRegValue((byte)Registers.TEMPH);

            Int16 tempTotal = (Int16)((tempL & 0x0003) | ((tempH & 0xFF) << 2));
            textBlockCurrentTempCDevice.Text = ((tempTotal * .7168) - 271.51).ToString("0");
        }

        private void v5dDevice(byte register)
        {
            adcVoltsDisplay(register, textBlockV5dAdcDevice, textBlockV5dVoltsDevice, .009725, .45);
        }

        private void buttonSleepDevice_Click(object sender, RoutedEventArgs e)
        {
            if (buttonSleepDevice.Content.ToString().Contains("On"))
            {
                buttonSleepDevice.Content = "Sleep Off";
                Globals.mcuCommand.sendSPI(true, (byte)Registers.SLEEP, 0x01, myAddress);  // write it out
            }
            else
            {
                buttonSleepDevice.Content = "Sleep On";
                Globals.mcuCommand.sendSPI(true, (byte)Registers.SLEEP, 0x00, myAddress);  // write it out
            }
        }

        private void buttonResetDevice_Click(object sender, RoutedEventArgs e)
        {
            Globals.mcuCommand.sendSPI(true, (byte)Registers.RESET, 0xC3, myAddress);  // write it out
        }

        private void comboBoxPwmDividerDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            byte currentValue = (byte)(currentRegValue((byte)Registers.PWMDIV) & 0xF8);

            currentValue |= (byte)(byte.Parse(comboBoxPwmDividerDevice.SelectedIndex.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) & 0x07);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.PWMDIV, currentValue, myAddress);  // write it out
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try  // here just in case user shuts down the app with the timer running...
            {
                Dispatcher.Invoke(() =>  // thread safe call to UI thread that contains the control
                {
                    updateAll();
                });
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LV: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public void startMostRecentTimer()
        {
            buttonUpdateAllDevice.Content = "Update Off";
            update520Timer.Enabled = true;
            timerOn = true;
        }

        public void stopMostRecentTimer()
        {
            buttonUpdateAllDevice.Content = "Update On";
            update520Timer.Enabled = false;
            timerOn = false;
        }

        private void buttonUpdateAllDevice_Click(object sender, RoutedEventArgs e)
        {
            if (buttonUpdateAllDevice.Content.ToString().Contains("On"))
                startMostRecentTimer();
            else
                stopMostRecentTimer();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            return;
        }

        public void buttonReadAll_Click(object sender, RoutedEventArgs e)
        {
    //        Mouse.OverrideCursor = Cursors.Wait;  // set hourglass

            controlEventsOff();
            if (timerOn)
                update520Timer.Enabled = false;
            
            try  // here if we lose comms during the read sequence
            {
                byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

                if ((currentValue & 0x01) == 0x01)
                    checkBoxEnableChannel1.IsChecked = true;
                else
                    checkBoxEnableChannel1.IsChecked = false;

                if ((currentValue & 0x02) == 0x02)
                    checkBoxPwmChannel1.IsChecked = true;
                else
                    checkBoxPwmChannel1.IsChecked = false;

                if ((currentValue & 0x04) == 0x04)
                    checkBoxEnableChannel2.IsChecked = true;
                else
                    checkBoxEnableChannel2.IsChecked = false;

                if ((currentValue & 0x08) == 0x08)
                    checkBoxPwmChannel2.IsChecked = true;
                else
                    checkBoxPwmChannel2.IsChecked = false;

                currentValue = currentRegValue((byte)Registers.SYSCFG2);

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

                currentValue = currentRegValue((byte)Registers.CH1IADJL);
                UInt16 currentValue2 = (UInt16)(currentValue | (currentRegValue((byte)Registers.CH1IADJH) << 2));
                sliderAnalogCurrentChannel1.Value = currentValue2;
                textBlockAnalogCurrentChannel1.Text = currentValue2.ToString();

                currentValue = currentRegValue((byte)Registers.CH1TON);
                sliderOnTimeChannel1.Value = currentValue;
                textBlockOnTimeChannel1.Text = currentValue.ToString();

                currentValue = currentRegValue((byte)Registers.CH1PWML);
                currentValue2 = (UInt16)(currentValue | (currentRegValue((byte)Registers.CH1PWMH) << 8));
                sliderPwmChannel1.Value = currentValue2;
                textBlockPwmChannel1.Text = currentValue2.ToString();

                currentValue = currentRegValue((byte)Registers.CH2IADJL);
                currentValue2 = (UInt16)(currentValue | (currentRegValue((byte)Registers.CH2IADJH) << 2));
                sliderAnalogCurrentChannel2.Value = currentValue2;
                textBlockAnalogCurrentChannel2.Text = currentValue2.ToString();

                currentValue = currentRegValue((byte)Registers.CH2TON);
                sliderOnTimeChannel2.Value = currentValue;
                textBlockOnTimeChannel2.Text = currentValue.ToString();

                currentValue = currentRegValue((byte)Registers.CH2PWML);
                currentValue2 = (UInt16)(currentValue | (currentRegValue((byte)Registers.CH2PWMH) << 8));
                sliderPwmChannel2.Value = currentValue2;
                textBlockPwmChannel2.Text = currentValue2.ToString();

                comboBoxFaultTimer.SelectedIndex = (currentRegValue((byte)Registers.SYSCFG2) & 0xC0) >> 6;
                comboBoxWatchDogTimer.SelectedIndex = currentRegValue((byte)Registers.CMWTAP) & 0x0F;
                comboBoxPwmDividerDevice.SelectedIndex = currentRegValue((byte)Registers.PWMDIV) & 0x07;

                currentValue = currentRegValue((byte)Registers.SLEEP);
                if (currentValue == 0x01)
                    buttonSleepDevice.Content = "Sleep Off";
                else
                    buttonSleepDevice.Content = "Sleep On";

                updateAll();

                textBoxTwLimitDevice.Text = "0x8A";
                setTwLimit();

                if (limpHomeMode != null)  // limp mode LHI update
                    limpHomeMode.buttonReadAll_Clicked();

      //          Mouse.OverrideCursor = _previousCursor;
                controlEventsOn();
                if (timerOn)
                    update520Timer.Enabled = true;

                textBlockReg0x00.Text = "0x" + currentRegValue((byte)Registers.SYSCFG1).ToString("X2");
            }  
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LW: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //           Mouse.OverrideCursor = _previousCursor;
                controlEventsOn();
                if (timerOn)
                    update520Timer.Enabled = true;

                textBlockReg0x00.Text = "0x" + currentRegValue((byte)Registers.SYSCFG1).ToString("X2");
            }
        }

        private void buttonUpdateAllDevice_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTip tooltip = new ToolTip();

            if (buttonUpdateAllDevice.Content.ToString().Contains("On"))
                buttonUpdateAllDevice.ToolTip = "Updating is Off";
            else
                buttonUpdateAllDevice.ToolTip = "Updating is On";
        }

        private void buttonSleepDevice_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTip tooltip = new ToolTip();

            if (buttonSleepDevice.Content.ToString().Contains("On"))
                buttonSleepDevice.ToolTip = "Sleep Mode is Off";
            else
                buttonSleepDevice.ToolTip = "Sleep Mode is On";
        }

        public void readRegStatusReg3()
        {
            Globals.mcuCommand.sendSPI(false, (byte) Registers.STATUS3, 0x00, myAddress);  // write it out
        }

        public void disable520Watchdog()
        {
            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, 0x00, myAddress);  // write it out
        }
        public void enable520Watchdog()
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

            currentValue |= 0x10;
            
            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
        }

        public void readZero()
        {
            Globals.mcuCommand.sendSPI(false, (byte)Registers.SYSCFG1, 0x00, myAddress);  // write it out
        }

        private void ButtonRegDump_Click(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            data.Add("\nRegister Value " + DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture));
            for (byte i = 0; i < NUM_REGS; i++)
            {
                byte currentValue = currentRegValue(i);
                data.Add("0x" + i.ToString("X") + " 0x" + currentValue.ToString("X2"));
            }

            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            using (StreamWriter sw = File.AppendText(path + "\\regDump520_" + myAddress.ToString() + ".txt"))
            {
                string report = string.Join(Environment.NewLine, data.Select(array => string.Join(" ", array)));
                sw.WriteLine(report);
            }
        }

        private void checkBoxPwmPhase_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue = currentRegValue((byte)Registers.SYSCFG1);

            if ((bool)checkBoxPwmPhase.IsChecked)
                currentValue |= 0x40;
            else
                currentValue &= 0xBF;

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SYSCFG1, currentValue, myAddress);  // write it out
        }

        private void Grid520Control_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!mainWindow.usbRemoved && !Globals.updateMode)
            {
                if (this.myAddress == 1)
                    mainWindow.selectSlave(1);
                else
                    mainWindow.selectSlave(2);
            }
        }
    }
}

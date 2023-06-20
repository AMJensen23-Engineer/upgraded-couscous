using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TPS9266xEvaluationModule
{
    /// <summary>
    /// Interaction logic for TPS92518Control.xaml
    /// </summary>
    public partial class TPS92518Control : UserControl
    {
        public const double RSENSE_DEFAULT = .15;  // constants 518
        private const UInt16 SCALAR_KILO = 1000;
        private const UInt32 SCALAR_MEGA = 1000000;
        private const double SCALAR_NANO = 0.000000001;
        private const double OFF_TIME_COEFF_00 = 2.035;
        private const double MAX_OFF_TIME_COEFF = (251 * 0.000000001);
        private const double VLED_COEFF = .26;

        private byte myAddress;
        private UInt16 led1MostRecent;
        private UInt16 led2MostRecent;
        private double buck1PeakA;
        private double buck2PeakA;
        private double buck1OffT_Est;
        private double buck2OffT_Est;
        private double buck1MaxOffEst;
        private double buck2MaxOffEst;
        private Utilities utils;
        private Timer aTimer;
        private bool timerOn = false;

        private const byte MIN_BUCK_OFF = 10;
        private const byte MIN_MAX_OFF = 15;

        private enum Registers : byte
        {
            CONTROL = 0x00,
            STATUS = 0x01,
            THERM_WARN_LIMIT = 0x02,
            LED1_PKTH_DAC = 0x03,
            LED2_PKTH_DAC = 0x04,
            LED1_TOFF_DAC = 0x05,
            LED2_TOFF_DAC = 0x06,
            LED1_MAXOFF_DAC = 0x07,
            LED2_MAXOFF_DAC = 0x08,
            VTHERM = 0x09,
            LED1_MOST_RECENT = 0x0A,
            LED1_LAST_ON = 0x0B,
            LED1_LAST_OFF = 0x0C,
            LED2_MOST_RECENT = 0x0D,
            LED2_LAST_ON = 0x0E,
            LED2_LAST_OFF = 0x0F,
            RESET = 0x10
        }

        public TPS92518Control(byte thisAddress, IndividualTabSettings savedTabControlSettings, bool initialRun)
        {
            InitializeComponent();

            aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 500;  // auto update timer

            utils = new Utilities();
            textBoxBuck1Rsense.Text = RSENSE_DEFAULT.ToString("0.00");
            textBoxBuck2Rsense.Text = RSENSE_DEFAULT.ToString("0.00");
            textBlockBuck1LastOn.Text = "";
            textBlockBuck1LastOnVolts.Text = "";
            textBlockBuck2LastOn.Text = "";
            textBlockBuck2LastOnVolts.Text = "";
            textBlockBuck1LastOff.Text = "";
            textBlockBuck1LastOffVolts.Text = "";
            textBlockBuck2LastOff.Text = "";
            textBlockBuck2LastOffVolts.Text = "";
            textBlockStatusReg.Text = "";
            textBlockSpiError.Text = "";
            textBlockThermWarning.Text = "";
            textBlockLed1.Text = "";
            textBlockLed2.Text = "";
            textBlockPowerCycled.Text = "";

            myAddress = thisAddress;

            if (!initialRun)
                restoreCustomSettings(savedTabControlSettings);
        }

        public void restoreCustomSettings(IndividualTabSettings savedTabControlSettings)
        {
            checkBoxBuck1Enable.Checked -= checkBoxBuck1Enable_Checked;
            checkBoxBuck2Enable.Checked -= checkBoxBuck2Enable_Checked;
 
            sliderBuck1PeakV.ValueChanged -= sliderBuck1PeakV_ValueChanged;
            sliderBuck2PeakV.ValueChanged -= sliderBuck2PeakV_ValueChanged;
            sliderBuck1OffTime.ValueChanged -= sliderBuck1OffTime_ValueChanged;
            sliderBuck2OffTime.ValueChanged -= sliderBuck2OffTime_ValueChanged;
            sliderBuck1MaxOff.ValueChanged -= sliderBuck1MaxOff_ValueChanged;
            sliderBuck2MaxOff.ValueChanged -= sliderBuck2MaxOff_ValueChanged;

            checkBoxBuck1Enable.IsChecked = savedTabControlSettings.Enable1;
            checkBoxBuck2Enable.IsChecked = savedTabControlSettings.Enable2;
            sliderBuck1PeakV.Value = savedTabControlSettings.Slider0_1;
            sliderBuck2PeakV.Value = savedTabControlSettings.Slider0_1;
            sliderBuck1OffTime.Value = savedTabControlSettings.Slider1_1;
            sliderBuck2OffTime.Value = savedTabControlSettings.Slider1_2;
            sliderBuck1MaxOff.Value = savedTabControlSettings.Slider2_1;
            sliderBuck2MaxOff.Value = savedTabControlSettings.Slider2_2;

            checkBoxBuck1Enable.Checked += checkBoxBuck1Enable_Checked;
            checkBoxBuck2Enable.Checked += checkBoxBuck2Enable_Checked;
            sliderBuck1PeakV.ValueChanged += sliderBuck1PeakV_ValueChanged;
            sliderBuck2PeakV.ValueChanged += sliderBuck2PeakV_ValueChanged;
            sliderBuck1OffTime.ValueChanged += sliderBuck1OffTime_ValueChanged;
            sliderBuck2OffTime.ValueChanged += sliderBuck2OffTime_ValueChanged;
            sliderBuck1MaxOff.ValueChanged += sliderBuck1MaxOff_ValueChanged;
            sliderBuck2MaxOff.ValueChanged += sliderBuck2MaxOff_ValueChanged;

            sliderBuck1PeakV_ValueChanged(null, null);  // set the sliders before enabling the bucks
            sliderBuck2PeakV_ValueChanged(null, null);
            sliderBuck1OffTime_ValueChanged(null, null);
            sliderBuck2OffTime_ValueChanged(null, null);
            sliderBuck1MaxOff_ValueChanged(null, null);
            sliderBuck2MaxOff_ValueChanged(null, null);

            checkBoxBuck1Enable_Checked(null, null);  // set enables
            checkBoxBuck2Enable_Checked(null, null);

            buttonAllButtsUpdate_Click(null, null);
        }

        public void saveCustomSettings(byte tabIndex)
        {
            var TabControlSettings = ConfigurationManager.GetSection("TabControlSettings") as TPS9266xEvaluationModule.TabControlSettings;
            IndividualTabSettings savedTabControlSettings = TabControlSettings.SelectedTab("Tab" + tabIndex);

            savedTabControlSettings.currentlySelectedTab += tabIndex;
    //        savedTabControlSettings.Device = "518";
      //      savedTabControlSettings.Address = myAddress;
            savedTabControlSettings.Enable1 = (bool)checkBoxBuck1Enable.IsChecked;
            savedTabControlSettings.Enable2 = (bool)checkBoxBuck2Enable.IsChecked;
            savedTabControlSettings.Slider0_1 = (byte)sliderBuck1PeakV.Value;
            savedTabControlSettings.Slider0_2 = (byte)sliderBuck2PeakV.Value;
            savedTabControlSettings.Slider1_1 = (byte)sliderBuck1OffTime.Value;
            savedTabControlSettings.Slider1_2 = (byte)sliderBuck2OffTime.Value;
            savedTabControlSettings.Slider2_1 = (byte)sliderBuck1MaxOff.Value;
            savedTabControlSettings.Slider2_2 = (byte)sliderBuck2MaxOff.Value;
        }

        private void checkBoxBuck1Enable_Checked(object sender, RoutedEventArgs e)
        {
            UInt16 currentValue = currentRegValue((byte)Registers.CONTROL);  // get current value

            if ((bool)checkBoxBuck1Enable.IsChecked)
                currentValue |= 0x0015;  // If we want it on, set the LSbit; Enable LED1, VLED1 sampling, VTHERM sampling
            else               
                currentValue &= 0xFFFE;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CONTROL, currentValue, myAddress);  // Now do the write
        }

        private void checkBoxBuck2Enable_Checked(object sender, RoutedEventArgs e)
        {
            UInt16 currentValue = currentRegValue((byte)Registers.CONTROL);  // get current value

            if ((bool)checkBoxBuck2Enable.IsChecked)
                currentValue |= 0x001A;  // If we want it on, set bit 2; Enable LED1, VLED1 sampling, VTHERM sampling
            else
                currentValue &= 0xFFFD;  // Off means clear bit 2

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CONTROL, currentValue, myAddress);  // Now do the write
        }

        private UInt16 currentRegValue(byte reg)
        {
            SPIcommadReturnData scrd = new SPIcommadReturnData();

            Globals.mcuCommand.sendSPI(false, reg, (UInt16)0, myAddress);  // Send a read response
            scrd = Globals.mcuCommand.sendSPI(false, reg, (UInt16)0, myAddress);  // Now send it again to get the data back

            return (UInt16)(scrd.assembledReturn & 0x01FF);
        }

        private void buttonRestoreSettings_Click(object sender, RoutedEventArgs e)
        {
            var TabControlSettings = ConfigurationManager.GetSection("TabControlSettings") as TPS9266xEvaluationModule.TabControlSettings;
            restoreCustomSettings(TabControlSettings.SelectedTab("Tab" + myAddress));
        }

        #region BuckPeakV

        private void changeBuckPeakV(byte whichBuck, byte updateValue)
        {
            if (whichBuck == 1)
            {
                Globals.mcuCommand.sendSPI(true, (byte)Registers.LED1_PKTH_DAC, (UInt16)updateValue, myAddress);
                Buck1PeakCurrent = getBuckxPeakA(updateValue, double.Parse(textBoxBuck1Rsense.Text, CultureInfo.InvariantCulture));
            }
            else if (whichBuck == 2)
            {
                Globals.mcuCommand.sendSPI(true, (byte)Registers.LED2_PKTH_DAC, (UInt16)updateValue, myAddress);
                Buck2PeakCurrent = getBuckxPeakA(updateValue, double.Parse(textBoxBuck2Rsense.Text, CultureInfo.InvariantCulture));
            }
        }

        public double getBuckxPeakA(byte peakDac, double rsense)
        {
            if (rsense == 0)
                return ((double)peakDac / SCALAR_KILO) / RSENSE_DEFAULT;
            else
                return ((double)peakDac / SCALAR_KILO) / rsense;  // return Amps
        }

        #region Buck1PeakV     

        public double Buck1PeakCurrent  // just in case we want this value in the future LMS
        {
            get { return buck1PeakA; }
            set
            {
                buck1PeakA = value;
                textBlockBuck1PeakA.Text = utils.sciNotToTime(value, ref labelPeakThresholdAmps1);
                labelPeakThresholdAmps1.Content += "A*";
            }
        }

        private void sliderBuck1PeakV_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBoxBuck1PeakV.Text = ((byte)sliderBuck1PeakV.Value).ToString();
            changeBuckPeakV(1, (byte)sliderBuck1PeakV.Value);
        }

        private void textBoxBuck1PeakV_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (Int16.Parse(textBoxBuck1PeakV.Text, CultureInfo.InvariantCulture) < 0)
                        textBoxBuck1PeakV.Text = "0";
                    else if (Int16.Parse(textBoxBuck1PeakV.Text, CultureInfo.InvariantCulture) > 255)
                        textBoxBuck1PeakV.Text = "255";

                    sliderBuck1PeakV.Value = byte.Parse(textBoxBuck1PeakV.Text, CultureInfo.InvariantCulture);
                    textBoxBuck1PeakV.CaretIndex = textBoxBuck1PeakV.Text.Length;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LG: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck1PeakV.Text = "0";  // if an invalid entry was made default to 0
                sliderBuck1PeakV.Value = 0;
                textBoxBuck1PeakV.CaretIndex = textBoxBuck1PeakV.Text.Length;
            }
        }

        private void textBoxBuck1PeakV_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int16.Parse(textBoxBuck1PeakV.Text, CultureInfo.InvariantCulture) < 0)
                    textBoxBuck1PeakV.Text = "0";
                else if (Int16.Parse(textBoxBuck1PeakV.Text, CultureInfo.InvariantCulture) > 255)
                    textBoxBuck1PeakV.Text = "255";

                sliderBuck1PeakV.Value = byte.Parse(textBoxBuck1PeakV.Text, CultureInfo.InvariantCulture);
             }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LH: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck1PeakV.Text = "0";  // if an invalid entry was made default to 0
                sliderBuck1PeakV.Value = 0;
            }
        }

#endregion Buck1PeakV

#region Buck2PeakV

        public double Buck2PeakCurrent  // just in case we want this value in the future LMS
        {
            get { return buck2PeakA; }
            set
            {
                buck2PeakA = value;
                textBoxBuck2PeakA.Text = utils.sciNotToTime(value, ref labelPeakThresholdAmps2);
                labelPeakThresholdAmps2.Content += "A*";
            }
        }

        private void sliderBuck2PeakV_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBoxBuck2PeakV.Text = ((byte)sliderBuck2PeakV.Value).ToString();
            changeBuckPeakV(2, (byte)sliderBuck2PeakV.Value);
        }

        private void textBoxBuck2PeakV_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (Int16.Parse(textBoxBuck2PeakV.Text, CultureInfo.InvariantCulture) < 0)
                        textBoxBuck2PeakV.Text = "0";
                    else if (Int16.Parse(textBoxBuck2PeakV.Text, CultureInfo.InvariantCulture) > 255)
                        textBoxBuck2PeakV.Text = "255";

                    sliderBuck2PeakV.Value = byte.Parse(textBoxBuck2PeakV.Text, CultureInfo.InvariantCulture);
                    textBoxBuck2PeakV.CaretIndex = textBoxBuck2PeakV.Text.Length;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LI: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck2PeakV.Text = "0";  // if an invalid entry was made default to 0
                sliderBuck2PeakV.Value = 0;
                textBoxBuck2PeakV.CaretIndex = textBoxBuck2PeakV.Text.Length;
            }
        }

        private void textBoxBuck2PeakV_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int16.Parse(textBoxBuck2PeakV.Text, CultureInfo.InvariantCulture) < 0)
                    textBoxBuck2PeakV.Text = "0";
                else if (Int16.Parse(textBoxBuck2PeakV.Text, CultureInfo.InvariantCulture) > 255)
                    textBoxBuck2PeakV.Text = "255";

                sliderBuck2PeakV.Value = byte.Parse(textBoxBuck2PeakV.Text, CultureInfo.InvariantCulture);
                textBoxBuck2PeakV.CaretIndex = textBoxBuck2PeakV.Text.Length;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LJ: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck2PeakV.Text = "0";  // if an invalid entry was made default to 0
                sliderBuck2PeakV.Value = 0;
                textBoxBuck2PeakV.CaretIndex = textBoxBuck2PeakV.Text.Length;
            }
        }

#endregion Buck2PeakV
#endregion BuckPeakV

#region BuckOffTime
        private void changeBuckOffTime(byte whichBuck, byte updateValue, bool writeReg)
        {       
            if (whichBuck == 1)
            {
                if(writeReg)
                    Globals.mcuCommand.sendSPI(true, (byte)Registers.LED1_TOFF_DAC, (UInt16)updateValue, myAddress);
                UInt16 currentValue = currentRegValue((byte)Registers.LED1_MOST_RECENT);  // get current value
                Led1MostRecent = currentValue;  // update Led1MostRecent

                 currentValue &= 0xFF;
                 Buck1OffTimeEst = getBuckxOffTimeEst(updateValue, (byte)currentValue);
            }
            else if (whichBuck == 2)
            {
                if (writeReg)
                    Globals.mcuCommand.sendSPI(true, (byte)Registers.LED2_TOFF_DAC, (UInt16)updateValue, myAddress);
                UInt16 currentValue = currentRegValue((byte)Registers.LED2_MOST_RECENT);  // get current value
                Led2MostRecent = currentValue;  // update Led2MostRecent

                currentValue &= 0xFF;
                Buck2OffTimeEst = getBuckxOffTimeEst(updateValue, (byte)currentValue);
            }
        }

        public double getBuckxOffTimeEst(byte tOffDac, byte VledDac)
        {
            double denom = ((double)VledDac * VLED_COEFF * OFF_TIME_COEFF_00 * (double)SCALAR_MEGA);
            if (denom == 0)
                return 0;
            return (double)(tOffDac) / denom;  // return = secs;
        }

#region Buck1OffTime

        public double Buck1OffTimeEst
        {
            get { return buck1OffT_Est; }
            set
            {
                buck1OffT_Est = value;
                textBlockBuck1OffTimeEst.Text = utils.sciNotToTime(value, ref labelBuck1OffTimeEstSecs);
                labelBuck1OffTimeEstSecs.Content += "S*";
            }
        }

        private void sliderBuck1OffTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderBuck1OffTime.Value < MIN_BUCK_OFF)
                sliderBuck1OffTime.Value = MIN_BUCK_OFF;

            textBoxBuck1OffTime.Text = ((byte)sliderBuck1OffTime.Value).ToString();
            changeBuckOffTime(1, (byte)sliderBuck1OffTime.Value, true);
        }

        private void textBoxBuck1OffTime_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (Int16.Parse(textBoxBuck1OffTime.Text, CultureInfo.InvariantCulture) < MIN_BUCK_OFF)
                        textBoxBuck1OffTime.Text = MIN_BUCK_OFF.ToString();
                    else if (Int16.Parse(textBoxBuck1OffTime.Text, CultureInfo.InvariantCulture) > 255)
                        textBoxBuck1OffTime.Text = "255";

                    sliderBuck1OffTime.Value = byte.Parse(textBoxBuck1OffTime.Text, CultureInfo.InvariantCulture);
                    textBoxBuck1OffTime.CaretIndex = textBoxBuck1OffTime.Text.Length;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LK: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck1OffTime.Text = MIN_BUCK_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck1OffTime.Value = MIN_BUCK_OFF;
                textBoxBuck1OffTime.CaretIndex = textBoxBuck1OffTime.Text.Length;
            }
        }

        private void textBoxBuck1OffTime_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int16.Parse(textBoxBuck1OffTime.Text, CultureInfo.InvariantCulture) < MIN_BUCK_OFF)
                    textBoxBuck1OffTime.Text = MIN_BUCK_OFF.ToString();
                else if (Int16.Parse(textBoxBuck1OffTime.Text, CultureInfo.InvariantCulture) > 255)
                    textBoxBuck1OffTime.Text = "255";

                sliderBuck1OffTime.Value = byte.Parse(textBoxBuck1OffTime.Text, CultureInfo.InvariantCulture);
                textBoxBuck1OffTime.CaretIndex = textBoxBuck1OffTime.Text.Length;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LL: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck1OffTime.Text = MIN_BUCK_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck1OffTime.Value = MIN_BUCK_OFF;
                textBoxBuck1OffTime.CaretIndex = textBoxBuck1OffTime.Text.Length;
            }
        }

#endregion Buck1OffTime

#region Buck2OffTime

        public double Buck2OffTimeEst
        {
            get { return buck2OffT_Est; }
            set
            {
                buck2OffT_Est = value;
                textBlockBuck2OffTimeEst.Text = utils.sciNotToTime(value, ref labelBuck2OffTimeEstSecs);
                labelBuck2OffTimeEstSecs.Content += "S*";
            }
        }

        private void sliderBuck2OffTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderBuck2OffTime.Value < MIN_BUCK_OFF)
                sliderBuck2OffTime.Value = MIN_BUCK_OFF;

            textBoxBuck2OffTime.Text = ((byte)sliderBuck2OffTime.Value).ToString();
            changeBuckOffTime(2, (byte)sliderBuck2OffTime.Value, true);
        }

        private void textBoxBuck2OffTime_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (Int16.Parse(textBoxBuck2OffTime.Text, CultureInfo.InvariantCulture) < MIN_BUCK_OFF)
                        textBoxBuck2OffTime.Text = MIN_BUCK_OFF.ToString();
                    else if (Int16.Parse(textBoxBuck2OffTime.Text, CultureInfo.InvariantCulture) > 255)
                        textBoxBuck2OffTime.Text = "255";

                    sliderBuck2OffTime.Value = byte.Parse(textBoxBuck2OffTime.Text, CultureInfo.InvariantCulture);
                    textBoxBuck2OffTime.CaretIndex = textBoxBuck2OffTime.Text.Length;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LM: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck2OffTime.Text = MIN_BUCK_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck2OffTime.Value = MIN_BUCK_OFF;
                textBoxBuck2OffTime.CaretIndex = textBoxBuck2OffTime.Text.Length;
            }
        }

        private void textBoxBuck2OffTime_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int16.Parse(textBoxBuck2OffTime.Text, CultureInfo.InvariantCulture) < MIN_BUCK_OFF)
                    textBoxBuck2OffTime.Text = MIN_BUCK_OFF.ToString();
                else if (Int16.Parse(textBoxBuck2OffTime.Text, CultureInfo.InvariantCulture) > 255)
                    textBoxBuck2OffTime.Text = "255";

                sliderBuck2OffTime.Value = byte.Parse(textBoxBuck2OffTime.Text, CultureInfo.InvariantCulture);
                textBoxBuck2OffTime.CaretIndex = textBoxBuck2OffTime.Text.Length;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LN: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck2OffTime.Text = MIN_BUCK_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck2OffTime.Value = MIN_BUCK_OFF;
                textBoxBuck2OffTime.CaretIndex = textBoxBuck2OffTime.Text.Length;
            }
        }

#endregion Buck2OffTime
#endregion BuckOffTime

#region BuckMaxOff

        private void changeBuckMaxOff(byte whichBuck, byte updateValue)
        {
            if (whichBuck == 1)
            {
                Globals.mcuCommand.sendSPI(true, (byte)Registers.LED1_MAXOFF_DAC, (UInt16)updateValue, myAddress);
                Buck1MaxOffEst = getBoxBuckMaxOffEst(updateValue);  // update off time value estimate
            }
            else if (whichBuck == 2)
            {
                Globals.mcuCommand.sendSPI(true, (byte)Registers.LED2_MAXOFF_DAC, (UInt16)updateValue, myAddress);
                Buck2MaxOffEst = getBoxBuckMaxOffEst(updateValue);  // update off time value estimate
            }
        }

        public double getBoxBuckMaxOffEst(byte tMaxOffDac)
        {
            return (double)tMaxOffDac * MAX_OFF_TIME_COEFF;  // return = secs;
        }

#region Buck1MaxOff

        public double Buck1MaxOffEst
        {
            get { return buck1MaxOffEst; }
            set
            {
                buck1MaxOffEst = value;
                textBlockBuck1MaxOffEst.Text = utils.sciNotToTime(value, ref labelBuck1MaxOffTimeEstSecs);
                labelBuck1MaxOffTimeEstSecs.Content += "S*";
            }
        }

        private void sliderBuck1MaxOff_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderBuck1MaxOff.Value < MIN_MAX_OFF)
                sliderBuck1MaxOff.Value = MIN_MAX_OFF;

            textBoxBuck1MaxOff.Text = ((byte)sliderBuck1MaxOff.Value).ToString();
            changeBuckMaxOff(1, byte.Parse(textBoxBuck1MaxOff.Text, CultureInfo.InvariantCulture));
        }

        private void textBoxBuck1MaxOff_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (Int16.Parse(textBoxBuck1MaxOff.Text, CultureInfo.InvariantCulture) < MIN_MAX_OFF)
                        textBoxBuck1MaxOff.Text = MIN_MAX_OFF.ToString();
                    else if (Int16.Parse(textBoxBuck1MaxOff.Text, CultureInfo.InvariantCulture) > 255)
                        textBoxBuck1MaxOff.Text = "255";

                    sliderBuck1MaxOff.Value = byte.Parse(textBoxBuck1MaxOff.Text, CultureInfo.InvariantCulture);
                    textBoxBuck1MaxOff.CaretIndex = textBoxBuck1MaxOff.Text.Length;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LO: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck1MaxOff.Text = MIN_MAX_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck1MaxOff.Value = MIN_MAX_OFF;
                textBoxBuck1MaxOff.CaretIndex = textBoxBuck1MaxOff.Text.Length;
            }
        }

        private void textBoxBuck1MaxOff_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int16.Parse(textBoxBuck1MaxOff.Text, CultureInfo.InvariantCulture) < MIN_MAX_OFF)
                    textBoxBuck1MaxOff.Text = MIN_MAX_OFF.ToString();
                else if (Int16.Parse(textBoxBuck1MaxOff.Text, CultureInfo.InvariantCulture) > 255)
                    textBoxBuck1MaxOff.Text = "255";

                sliderBuck1MaxOff.Value = byte.Parse(textBoxBuck1MaxOff.Text, CultureInfo.InvariantCulture);
                textBoxBuck1MaxOff.CaretIndex = textBoxBuck1MaxOff.Text.Length;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LP: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck1MaxOff.Text = MIN_MAX_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck1MaxOff.Value = MIN_MAX_OFF;
                textBoxBuck1MaxOff.CaretIndex = textBoxBuck1MaxOff.Text.Length;
            }
        }

#endregion Buck1MaxOff

#region Buck2MaxOff

        public double Buck2MaxOffEst
        {
            get { return buck2MaxOffEst; }
            set
            {
                buck2MaxOffEst = value;
                textBlockBuck2MaxOffEst.Text = utils.sciNotToTime(value, ref labelBuck2MaxOffTimeEstSecs);
                labelBuck2MaxOffTimeEstSecs.Content += "S*";
            }
        }

        private void sliderBuck2MaxOff_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderBuck2MaxOff.Value < MIN_MAX_OFF)
                sliderBuck2MaxOff.Value = MIN_MAX_OFF;

            textBoxBuck2MaxOff.Text = ((byte)sliderBuck2MaxOff.Value).ToString(CultureInfo.InvariantCulture);
            changeBuckMaxOff(2, byte.Parse(textBoxBuck2MaxOff.Text, CultureInfo.InvariantCulture));
        }

        private void textBoxBuck2MaxOff_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (Int16.Parse(textBoxBuck2MaxOff.Text, CultureInfo.InvariantCulture) < MIN_MAX_OFF)
                        textBoxBuck2MaxOff.Text = MIN_MAX_OFF.ToString();
                    else if (Int16.Parse(textBoxBuck2MaxOff.Text, CultureInfo.InvariantCulture) > 255)
                        textBoxBuck2MaxOff.Text = "255";

                    sliderBuck2MaxOff.Value = byte.Parse(textBoxBuck2MaxOff.Text, CultureInfo.InvariantCulture);
                    textBoxBuck2MaxOff.CaretIndex = textBoxBuck2MaxOff.Text.Length;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LQ: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck2MaxOff.Text = MIN_MAX_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck2MaxOff.Value = MIN_MAX_OFF;
                textBoxBuck2MaxOff.CaretIndex = textBoxBuck2MaxOff.Text.Length;
            }
        }

        private void textBoxBuck2MaxOff_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Int16.Parse(textBoxBuck2MaxOff.Text, CultureInfo.InvariantCulture) < 0)
                    textBoxBuck2MaxOff.Text = MIN_MAX_OFF.ToString();
                else if (Int16.Parse(textBoxBuck2MaxOff.Text, CultureInfo.InvariantCulture) > 255)
                    textBoxBuck2MaxOff.Text = "255";

                sliderBuck2MaxOff.Value = byte.Parse(textBoxBuck2MaxOff.Text, CultureInfo.InvariantCulture);
                textBoxBuck2MaxOff.CaretIndex = textBoxBuck2MaxOff.Text.Length;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LR: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                textBoxBuck2MaxOff.Text = MIN_MAX_OFF.ToString();  // if an invalid entry was made default to 0
                sliderBuck2MaxOff.Value = MIN_MAX_OFF;
                textBoxBuck2MaxOff.CaretIndex = textBoxBuck2MaxOff.Text.Length;
            }
        }

#endregion Buck2MaxOff
#endregion BuckMaxOff

        public UInt16 Led1MostRecent
        {
            get { return led1MostRecent; }
            set
            {
                if ((value & 0x0100) == 0x0100)  // If the PWM bit is high...
                    textBlockBuck1PwmStatus.Text = "True";
                else
                    textBlockBuck1PwmStatus.Text = "False";

                value &= 0x00FF;  // Hang on to just the data portion now
                led1MostRecent = value;
                textBlockLED1MostRecentDAC.Text = Convert.ToString(value);  // Write the read value to the box
                double result = ((double)value * 0.26);  // And the converted value;
                textBlockLED1MostRecentVolts.Text = result.ToString("0.0");
            }
        }

        public UInt16 Led2MostRecent
        {
            get { return led2MostRecent; }
            set
            {
                if ((value & 0x0100) == 0x0100)  // If the PWM bit is high...
                    textBlockBuck2PwmStatus.Text = "True";
                else
                    textBlockBuck2PwmStatus.Text = "False";

                value &= 0x00FF;  // Hang on to just the data portion now
                led2MostRecent = value;
                textBlockLED2MostRecentDAC.Text = Convert.ToString(value);  // Write the read value to the box
                double result = ((double)value * 0.26);  // And the converted value;
                textBlockLED2MostRecentVolts.Text = result.ToString("0.0");
            }
        }

        private void buttonBuck1LastOn_Click(object sender, RoutedEventArgs e)
        {
            buckxLastOnLastOff((byte)Registers.LED1_LAST_ON, textBlockBuck1LastOn, textBlockBuck1LastOnVolts);
        }

        private void buttonBuck2LastOn_Click(object sender, RoutedEventArgs e)
        {
            buckxLastOnLastOff((byte)Registers.LED2_LAST_ON, textBlockBuck2LastOn, textBlockBuck2LastOnVolts);
        }

        private void buttonBuck1LastOff_Click(object sender, RoutedEventArgs e)
        {
            buckxLastOnLastOff((byte)Registers.LED1_LAST_OFF, textBlockBuck1LastOff, textBlockBuck1LastOffVolts);
        }

        private void buttonBuck2LastOff_Click(object sender, RoutedEventArgs e)
        {
            buckxLastOnLastOff((byte)Registers.LED2_LAST_OFF, textBlockBuck2LastOff, textBlockBuck2LastOffVolts);
        }

        private void buckxLastOnLastOff(byte register, TextBlock tbDAC, TextBlock tbVolts)
        {
            UInt16 currentValue = (UInt16)(currentRegValue(register) & 0xFF);  // get current value
            tbDAC.Text = currentValue.ToString();
            tbVolts.Text = (currentValue * VLED_COEFF).ToString("0.0");
        }

        private void buttonVthermUpdate_Click(object sender, RoutedEventArgs e)
        {
            UInt16 currentValue = currentRegValue((byte)Registers.VTHERM);  // get current value

            if ((currentValue == 0x0000) || (currentValue == 0x01FF))  // bucks not on....
            {
                textBoxVthermDAC.Text = "";
                textBoxVthermTemp.Text = "";
            }
            else
            {
                textBoxVthermDAC.Text = currentValue.ToString();
                double result = ((2.439 * (double)currentValue) - 293.5);
                textBoxVthermTemp.Text = result.ToString("0");
            }
        }

        private void buttonStatus_Click(object sender, RoutedEventArgs e)
        {
            byte currentValue = (byte)(currentRegValue((byte)Registers.STATUS) & 0x1F);  // get current value

            byte lastValue;
            byte newValue;
            do
            {
                lastValue = (byte)(Globals.mcuCommand.sendSPI(false, (byte)Registers.STATUS, (UInt16)0, myAddress).assembledReturn & 0x1F);
                newValue = (byte)(Globals.mcuCommand.sendSPI(false, (byte)Registers.STATUS, (UInt16)0, myAddress).assembledReturn & 0x1F);
            } while (lastValue != newValue) ;

            currentValue = lastValue;
            textBlockStatusReg.Text = "0x" + currentValue.ToString("X2");

            textBlockSpiError.Background = new SolidColorBrush(Colors.Green);
            textBlockThermWarning.Background = new SolidColorBrush(Colors.Green);
            textBlockLed1.Background = new SolidColorBrush(Colors.Green);
            textBlockLed2.Background = new SolidColorBrush(Colors.Green);
            textBlockPowerCycled.Background = new SolidColorBrush(Colors.Green);

            if ((currentValue & 0x01) == 0x01)  // SPI_ERROR -- Bit 0
                textBlockSpiError.Background = new SolidColorBrush(Colors.Red);
            if ((currentValue & 0x02) == 0x02)  // THERMAL_WARNING -- Bit 1
                textBlockThermWarning.Background = new SolidColorBrush(Colors.Red);
            if ((currentValue & 0x04) == 0x04)  // LED1_BOOTUV_ERROR -- Bit 2
                textBlockLed1.Background = new SolidColorBrush(Colors.Red);
            if ((currentValue & 0x08) == 0x08)  // LED2_BOOTUV_ERROR -- Bit 3
                textBlockLed2.Background = new SolidColorBrush(Colors.Red);
            if ((currentValue & 0x10) == 0x10)  // POWER_CYCLED -- Bit 4
                textBlockPowerCycled.Background = new SolidColorBrush(Colors.Red);
        }

        private void buttonAllButtsUpdate_Click(object sender, RoutedEventArgs e)
        {
            changeBuckOffTime(1, 0, false);
            changeBuckOffTime(2, 0, false);
            buttonVthermUpdate_Click(null, null);  // get temp
            buttonBuck1LastOn_Click(null, null);
            buttonBuck2LastOn_Click(null, null);
            buttonBuck1LastOff_Click(null, null);
            buttonBuck2LastOff_Click(null, null);
            buttonStatus_Click(null, null);  // get status
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try  // here just in case user shuts down the app with the timer running...
            {
                Dispatcher.Invoke(() =>  // thread safe call to UI thread that contains the control
                {
                    buttonAllButtsUpdate_Click(this, null);
                });
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LS: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public void startMostRecentTimer()
        {
            buttonUpdateAllDevice.Content = "Update Off";
            aTimer.Enabled = true;
            timerOn = true;
        }

        public void stopMostRecentTimer()
        {
            buttonUpdateAllDevice.Content = "Update On";
            aTimer.Enabled = false;
            timerOn = false;
        }

        private void buttonUpdateAllDevice_Click(object sender, RoutedEventArgs e)
        {
            if (buttonUpdateAllDevice.Content.ToString().Contains("On"))
                startMostRecentTimer();
            else
                stopMostRecentTimer();
        }

        private void userControl518_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible == true)
            {
                if (timerOn)
                    aTimer.Enabled = true;
            }
            else
            {
                if (timerOn)
                    aTimer.Enabled = false;
            }
        }
    }
}

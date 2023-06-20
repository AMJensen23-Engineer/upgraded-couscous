using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    /// Interaction logic for TPS92682Control.xaml
    /// </summary>
    public partial class TPS92682Control : UserControl
    {
        private byte myAddress;
  //      private Cursor _previousCursor;
        private MainWindow mainWindow = null;  // get the MainWindow object;
        private const byte NUM_REGS = 0x26;

        private enum Registers : byte
        {
            EN = 0x00,
            CFG1 = 0x01,
            SWDIV = 0x03,
            ISLOPE = 0x04,
            FM = 0x05,
            SOFTSTART = 0x06,
            CH1IADJ  = 0x07,
            CH2IADJ = 0x08,
            PWMDIV = 0x09,
            CH1PWML = 0x0A,
            CH1PWMH = 0x0B,
            CH2PWML = 0x0C,
            CH2PWMH = 0x0D,
            FLT1 = 0x11,
            FLT2 = 0x12
        }

        public TPS92682Control(MainWindow master, byte thisAddress, IndividualTabSettings savedTabControlSettings, bool initialRun)
        {
            InitializeComponent();

//            mainWindow = master;
 //           _previousCursor = Mouse.OverrideCursor;

            myAddress = thisAddress;
            
            comboboxChan1ClockDiv.SelectionChanged -= comboboxChan1ClockDiv_SelectionChanged;
            ObservableCollection<string> comboItemsClockDiv = new ObservableCollection<string>();
            comboItemsClockDiv.Add("2"); comboItemsClockDiv.Add("4"); comboItemsClockDiv.Add("8");
            comboboxChan1ClockDiv.ItemsSource = comboItemsClockDiv;
            comboboxChan1ClockDiv.SelectedIndex = 0;
            comboboxChan1ClockDiv.SelectionChanged += comboboxChan1ClockDiv_SelectionChanged;

            comboboxChan2ClockDiv.SelectionChanged -= comboboxChan2ClockDiv_SelectionChanged;
            comboboxChan2ClockDiv.ItemsSource = comboItemsClockDiv;
            comboboxChan2ClockDiv.SelectedIndex = 0;
            comboboxChan2ClockDiv.SelectionChanged += comboboxChan2ClockDiv_SelectionChanged;

            comboBoxSoftStartCh1.SelectionChanged -= comboBoxSoftStartCh1_SelectionChanged;
            ObservableCollection<string> comboItemsSoftStart = new ObservableCollection<string>();
            comboItemsSoftStart.Add("0"); comboItemsSoftStart.Add("2"); comboItemsSoftStart.Add("4"); comboItemsSoftStart.Add("6");
            comboItemsSoftStart.Add("8"); comboItemsSoftStart.Add("12"); comboItemsSoftStart.Add("16"); comboItemsSoftStart.Add("20");
            comboItemsSoftStart.Add("26"); comboItemsSoftStart.Add("32"); comboItemsSoftStart.Add("38"); comboItemsSoftStart.Add("46");
            comboItemsSoftStart.Add("54"); comboItemsSoftStart.Add("64"); comboItemsSoftStart.Add("80"); comboItemsSoftStart.Add("100");
            comboBoxSoftStartCh1.ItemsSource = comboItemsSoftStart;
            comboBoxSoftStartCh1.SelectedIndex = 7;  // does not need to be hardware set, this is the part's default value
            comboBoxSoftStartCh1.SelectionChanged += comboBoxSoftStartCh1_SelectionChanged;

            comboBoxSoftStartCh2.SelectionChanged -= comboBoxSoftStartCh2_SelectionChanged;
            comboBoxSoftStartCh2.ItemsSource = comboItemsSoftStart;
            comboBoxSoftStartCh2.SelectedIndex = 7;  // does not need to be hardware set, this is the part's default value
            comboBoxSoftStartCh2.SelectionChanged += comboBoxSoftStartCh2_SelectionChanged;

            comboBoxPWMclock.SelectionChanged -= comboBoxPWMclock_SelectionChanged;
            ObservableCollection<string> comboItemsPWMclock = new ObservableCollection<string>();
            for (int i = 1; i < 9; i++)
                comboItemsPWMclock.Add(i.ToString());
            comboBoxPWMclock.ItemsSource = comboItemsPWMclock;
            comboBoxPWMclock.SelectedIndex = 1;
            comboBoxPWMclock.SelectionChanged += comboBoxPWMclock_SelectionChanged;

            comboBoxFMmag.SelectionChanged -= comboBoxFMmag_SelectionChanged;
            ObservableCollection<string> comboItemFMmag = new ObservableCollection<string>();
            comboItemFMmag.Add("0"); comboItemFMmag.Add("3.8"); comboItemFMmag.Add("7.5"); comboItemFMmag.Add("15");
            comboBoxFMmag.ItemsSource = comboItemFMmag;
            comboBoxFMmag.SelectedIndex = 0;
            comboBoxFMmag.SelectionChanged += comboBoxFMmag_SelectionChanged;

            comboBoxFMfreek.SelectionChanged -= comboBoxFMfreek_SelectionChanged;
            ObservableCollection<string> comboItemFMfreek = new ObservableCollection<string>();
            comboItemFMfreek.Add("4096"); comboItemFMfreek.Add("3584"); comboItemFMfreek.Add("3072"); comboItemFMfreek.Add("2560");
            comboItemFMfreek.Add("2048"); comboItemFMfreek.Add("1536"); comboItemFMfreek.Add("1024"); comboItemFMfreek.Add("512");
            comboItemFMfreek.Add("256"); comboItemFMfreek.Add("128"); comboItemFMfreek.Add("64"); comboItemFMfreek.Add("32");
            comboItemFMfreek.Add("16"); comboItemFMfreek.Add("8"); comboItemFMfreek.Add("0");
            comboBoxFMfreek.ItemsSource = comboItemFMfreek;
            comboBoxFMfreek.SelectedIndex = 5;
            comboBoxFMfreek.SelectionChanged += comboBoxFMfreek_SelectionChanged;

            textBlockChan1VIadj.Text = "";
            textBlockChan1PWMdootie.Text = "";
            textBlockChan1Slope.Text = "";
            textBlockChan2VIadj.Text = "";
            textBlockChan2PWMdootie.Text = "";
            textBlockChan2Slope.Text = "";

            textBlockChan1OV.Text = "";
            textBlockChan1UV.Text = "";
            textBlockChan1OC.Text = "";
            textBlockChan1UC.Text = "";
            textBlockChan1ILIM.Text = "";
            textBlockChan1ISO.Text = "";

            textBlockTW.Text = "";
            textBlockPC.Text = "";
            textBlockRTO.Text = "";

            textBlockChan2OV.Text = "";
            textBlockChan2UV.Text = "";
            textBlockChan2OC.Text = "";
            textBlockChan2UC.Text = "";
            textBlockChan2ILIM.Text = "";
            textBlockChan2ISO.Text = "";

            myAddress = thisAddress;

            sliderChan1Slope.ValueChanged -= sliderChan1Slope_ValueChanged;
            sliderChan2Slope.ValueChanged -= sliderChan2Slope_ValueChanged;
            sliderChan1Slope.Value = 5;  // does not need to be hardware set, this is the part's default value
            sliderChan2Slope.Value = 5;  // does not need to be hardware set, this is the part's default value
            textBlockChan1Slope.Text = sliderChan1Slope.Value.ToString();
            textBlockChan2Slope.Text = sliderChan2Slope.Value.ToString();
            sliderChan1Slope.ValueChanged += sliderChan1Slope_ValueChanged;
            sliderChan2Slope.ValueChanged += sliderChan2Slope_ValueChanged;

#if (CSTMR && FAIL_ANAL)
            if (!initialRun)
                restoreCustomSettings(savedTabControlSettings);
#endif

            if (Globals.EVMS[Globals.userSelectedEVM].Contains("CV"))  // 682 CV EVM selected
            {
                checkBoxCV1.IsChecked = true;                
                checkBoxCV2.IsChecked = true;
                checkBoxDualPhase.IsChecked = true;
            }

            if (Globals.userSelectedEVM == 4) // matrix EVM
            {
                checkBoxCV1.IsEnabled = false;
                comboboxChan1ClockDiv.IsEnabled = false;
                comboBoxSoftStartCh1.IsEnabled = false;
                sliderChan1VIadj.IsEnabled = true;
                sliderChan1PWMdootie.IsEnabled = false;
                sliderChan1Slope.IsEnabled = false;

                groupBoxConfig.IsEnabled = false;
                groupBoxFM.IsEnabled = false;
                groupChan2.IsEnabled = false;
            }

            if ((Globals.userSelectedEVM > 4) && (Globals.userSelectedEVM < 8))  // make 520 content go away using these EVMs
            {
                groupBox520.Visibility = Visibility.Hidden;
                mainWindow.groupBoxWatchdog.Visibility = Visibility.Hidden;
            }
        }

        public void restoreCustomSettings(IndividualTabSettings savedTabControlSettings)
        {
            controlEventsOff();

            checkBoxBuck1Enable.IsChecked = savedTabControlSettings.Enable1;
            checkBoxBuck2Enable.IsChecked = savedTabControlSettings.Enable2;
            checkBoxCV1.IsChecked = savedTabControlSettings.Enable3;
            checkBoxCV2.IsChecked = savedTabControlSettings.Enable4;
            sliderChan1VIadj.Value = savedTabControlSettings.Slider1_1;
            sliderChan2VIadj.Value = savedTabControlSettings.Slider1_2;
            sliderChan1PWMdootie.Value = savedTabControlSettings.Slider2_1;
            sliderChan2PWMdootie.Value = savedTabControlSettings.Slider2_2;
            sliderChan1Slope.Value = savedTabControlSettings.Slider3_1;
            sliderChan2Slope.Value = savedTabControlSettings.Slider3_2;
            comboboxChan1ClockDiv.SelectedIndex = savedTabControlSettings.Combo0_1;
            comboboxChan2ClockDiv.SelectedIndex = savedTabControlSettings.Combo0_2;
            comboBoxSoftStartCh1.SelectedIndex = savedTabControlSettings.Combo1_1;
            comboBoxSoftStartCh2.SelectedIndex = savedTabControlSettings.Combo1_2;
            comboBoxPWMclock.SelectedIndex = savedTabControlSettings.Combo0;
            comboBoxFMmag.SelectedIndex = savedTabControlSettings.Combo1;
            comboBoxFMfreek.SelectedIndex = savedTabControlSettings.Combo2;
            checkBoxDualPhase.IsChecked = savedTabControlSettings.Enable5;
            checkBoxIntPwm.IsChecked = savedTabControlSettings.Enable6;

            controlEventsOn();
            resetControls();  // set control and register bits
        }

        private void controlEventsOff()
        {
            checkBoxBuck1Enable.Checked -= checkBoxBuck1Enable_Checked;
            checkBoxBuck1Enable.Unchecked -= checkBoxBuck1Enable_Checked;
            checkBoxBuck2Enable.Checked -= checkBoxBuck2Enable_Checked;
            checkBoxBuck2Enable.Unchecked -= checkBoxBuck2Enable_Checked;
            checkBoxCV1.Checked -= checkBoxCV1_Checked;
            checkBoxCV1.Unchecked -= checkBoxCV1_Checked;
            checkBoxCV2.Checked -= checkBoxCV2_Checked;
            checkBoxCV2.Unchecked -= checkBoxCV2_Checked;
            sliderChan1VIadj.ValueChanged -= sliderChan1VIadj_ValueChanged;
            sliderChan2VIadj.ValueChanged -= sliderChan2VIadj_ValueChanged;
            sliderChan1PWMdootie.ValueChanged -= sliderChan1PWMdootie_ValueChanged;
            sliderChan2PWMdootie.ValueChanged -= sliderChan2PWMdootie_ValueChanged;
            sliderChan1Slope.ValueChanged -= sliderChan1Slope_ValueChanged;
            sliderChan2Slope.ValueChanged -= sliderChan2Slope_ValueChanged;
            comboboxChan1ClockDiv.SelectionChanged -= comboboxChan1ClockDiv_SelectionChanged;
            comboboxChan2ClockDiv.SelectionChanged -= comboboxChan2ClockDiv_SelectionChanged;
            comboBoxSoftStartCh1.SelectionChanged -= comboBoxSoftStartCh1_SelectionChanged;
            comboBoxSoftStartCh2.SelectionChanged -= comboBoxSoftStartCh2_SelectionChanged;
            comboBoxPWMclock.SelectionChanged -= comboBoxPWMclock_SelectionChanged;
            comboBoxFMmag.SelectionChanged -= comboBoxFMmag_SelectionChanged;
            comboBoxFMfreek.SelectionChanged -= comboBoxFMfreek_SelectionChanged;
            checkBoxDualPhase.Checked -= checkBoxDualPhase_Checked;
            checkBoxDualPhase.Unchecked -= checkBoxDualPhase_Checked;
            checkBoxIntPwm.Checked -= checkBoxIntPwm_Checked;
            checkBoxIntPwm.Unchecked -= checkBoxIntPwm_Checked;
        }

        private void controlEventsOn()
        {
            checkBoxBuck1Enable.Checked += checkBoxBuck1Enable_Checked;
            checkBoxBuck1Enable.Unchecked += checkBoxBuck1Enable_Checked;
            checkBoxBuck2Enable.Checked += checkBoxBuck2Enable_Checked;
            checkBoxBuck2Enable.Unchecked += checkBoxBuck2Enable_Checked;
            checkBoxCV1.Checked += checkBoxCV1_Checked;
            checkBoxCV1.Unchecked += checkBoxCV1_Checked;
            checkBoxCV2.Checked += checkBoxCV2_Checked;
            checkBoxCV2.Unchecked += checkBoxCV2_Checked;
            sliderChan1VIadj.ValueChanged += sliderChan1VIadj_ValueChanged;
            sliderChan2VIadj.ValueChanged += sliderChan2VIadj_ValueChanged;
            sliderChan1PWMdootie.ValueChanged += sliderChan1PWMdootie_ValueChanged;
            sliderChan2PWMdootie.ValueChanged += sliderChan2PWMdootie_ValueChanged;
            sliderChan1Slope.ValueChanged += sliderChan1Slope_ValueChanged;
            sliderChan2Slope.ValueChanged += sliderChan2Slope_ValueChanged;
            comboboxChan1ClockDiv.SelectionChanged += comboboxChan1ClockDiv_SelectionChanged;
            comboboxChan2ClockDiv.SelectionChanged += comboboxChan2ClockDiv_SelectionChanged;
            comboBoxSoftStartCh1.SelectionChanged += comboBoxSoftStartCh1_SelectionChanged;
            comboBoxSoftStartCh2.SelectionChanged += comboBoxSoftStartCh2_SelectionChanged;
            comboBoxPWMclock.SelectionChanged += comboBoxPWMclock_SelectionChanged;
            comboBoxFMmag.SelectionChanged += comboBoxFMmag_SelectionChanged;
            comboBoxFMfreek.SelectionChanged += comboBoxFMfreek_SelectionChanged;
            checkBoxDualPhase.Checked += checkBoxDualPhase_Checked;
            checkBoxDualPhase.Unchecked += checkBoxDualPhase_Checked;
            checkBoxIntPwm.Checked += checkBoxIntPwm_Checked;
            checkBoxIntPwm.Unchecked += checkBoxIntPwm_Checked;
        }

        public void saveCustomSettings(byte tabIndex)
        {
            var TabControlSettings = ConfigurationManager.GetSection("TabControlSettings") as TPS9266xEvaluationModule.TabControlSettings;
            IndividualTabSettings savedTabControlSettings = TabControlSettings.SelectedTab("Tab" + tabIndex);

            savedTabControlSettings.currentlySelectedTab += tabIndex;
            savedTabControlSettings.Enable1 = (bool)checkBoxBuck1Enable.IsChecked;
            savedTabControlSettings.Enable2 = (bool)checkBoxBuck2Enable.IsChecked;
            savedTabControlSettings.Enable3 = (bool)checkBoxCV1.IsChecked;
            savedTabControlSettings.Enable4 = (bool)checkBoxCV2.IsChecked;
            savedTabControlSettings.Slider1_1 = (byte)sliderChan1VIadj.Value;
            savedTabControlSettings.Slider1_2 = (byte)sliderChan2VIadj.Value;
            savedTabControlSettings.Slider2_1 = (UInt16)sliderChan1PWMdootie.Value;
            savedTabControlSettings.Slider2_2 = (UInt16)sliderChan2PWMdootie.Value;
            savedTabControlSettings.Slider3_1 = (byte)sliderChan1Slope.Value;
            savedTabControlSettings.Slider3_2 = (byte)sliderChan2Slope.Value;
            savedTabControlSettings.Combo0_1 = (byte)comboboxChan1ClockDiv.SelectedIndex;
            savedTabControlSettings.Combo0_2 = (byte)comboboxChan2ClockDiv.SelectedIndex;
            savedTabControlSettings.Combo1_1 = (byte)comboBoxSoftStartCh1.SelectedIndex;
            savedTabControlSettings.Combo1_2 = (byte)comboBoxSoftStartCh2.SelectedIndex;
            savedTabControlSettings.Combo0 = (byte)comboBoxPWMclock.SelectedIndex;
            savedTabControlSettings.Combo1 = (byte)comboBoxFMmag.SelectedIndex;
            savedTabControlSettings.Combo2 = (byte)comboBoxFMfreek.SelectedIndex;
            savedTabControlSettings.Enable5 = (bool)checkBoxDualPhase.IsChecked;
            savedTabControlSettings.Enable6 = (bool)checkBoxIntPwm.IsChecked;
        }

        private void resetControls()
        {
            checkBoxCV1_Checked(null, null);
            checkBoxCV2_Checked(null, null);
            sliderChan1VIadj_ValueChanged(null, null);
            sliderChan2VIadj_ValueChanged(null, null);
            sliderChan1PWMdootie_ValueChanged(null, null);
            sliderChan2PWMdootie_ValueChanged(null, null);
            sliderChan1Slope_ValueChanged(null, null);
            sliderChan2Slope_ValueChanged(null, null);
            comboboxChan1ClockDiv_SelectionChanged(null, null);
            comboboxChan2ClockDiv_SelectionChanged(null, null);
            comboBoxSoftStartCh1_SelectionChanged(null, null);
            comboBoxSoftStartCh2_SelectionChanged(null, null);
            comboBoxPWMclock_SelectionChanged(null, null);
            comboBoxFMmag_SelectionChanged(null, null);
            comboBoxFMfreek_SelectionChanged(null, null);
            checkBoxDualPhase_Checked(null, null);
            checkBoxIntPwm_Checked(null, null);

            checkBoxBuck1Enable_Checked(null, null);  // set enables after all others are set
            checkBoxBuck2Enable_Checked(null, null);
        }

        private void checkBoxBuck1Enable_Checked(object sender, RoutedEventArgs e)  // We will handle this with R-M-W
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.EN);

            if ((bool)checkBoxBuck1Enable.IsChecked)
                currentValue |= 0x01;  // If we want it on, set the LSbit
            else
                currentValue &= 0xFE;  // Off means clear the LSbit

            Globals.mcuCommand.sendSPI(true, (byte)Registers.EN, currentValue, myAddress);  // write it out
        }

        private void checkBoxBuck2Enable_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.EN);

            if ((bool)checkBoxBuck2Enable.IsChecked)
                currentValue |= 0x02;  // If we want it on, set bit 2
            else
                currentValue &= 0xFD;  // Off means clear bit 2

            Globals.mcuCommand.sendSPI(true, (byte)Registers.EN, currentValue, myAddress);  // write it out
        }

        private void checkBoxCV1_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.CFG1);

            if ((bool)checkBoxCV1.IsChecked)
                currentValue |= 0x01;  // If we want it on, set bit 1
            else
                currentValue &= 0xFE;  // Off means clear bit 1

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxCV2_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.CFG1);

            if ((bool)checkBoxCV2.IsChecked)
                currentValue |= 0x02;  // If we want it on, set bit 2
            else
                currentValue &= 0xFD;  // Off means clear bit 2

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxDualPhase_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.CFG1);

            if ((bool)checkBoxDualPhase.IsChecked)
            {
                currentValue |= 0x20;  // If we want it on, set bit 5

                comboboxChan1ClockDiv_SelectionChanged(null, null);  // clock dividers are the same now
                comboboxChan2ClockDiv.IsEnabled = false;

                sliderChan1VIadj_ValueChanged(null, null);  // VI adjs are the same now
                textBlockChan2VIadj.IsEnabled = false;
                sliderChan2VIadj.IsEnabled = false;

                sliderChan1PWMdootie_ValueChanged(null, null);  // PWMs are the same now
                textBlockChan2PWMdootie.IsEnabled = false;
                sliderChan2PWMdootie.IsEnabled = false;
                comboBoxSoftStartCh2.IsEnabled = false;
            }
            else
            {
                currentValue &= 0xDF;  // Off means clear bit 5

                comboboxChan2ClockDiv.IsEnabled = true;  // turn em all back on

                textBlockChan2VIadj.IsEnabled = true;
                sliderChan2VIadj.IsEnabled = true;

                textBlockChan2PWMdootie.IsEnabled = true;
                sliderChan2PWMdootie.IsEnabled = true;
                comboBoxSoftStartCh2.IsEnabled = true;
            }

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CFG1, currentValue, myAddress);  // write it out
        }

        private void checkBoxIntPwm_Checked(object sender, RoutedEventArgs e)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.CFG1);

            if ((bool)checkBoxIntPwm.IsChecked)
                currentValue |= 0x40;  // If we want it on, set bit 6
            else
                currentValue &= 0xBF;  // Off means clear bit 6

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CFG1, currentValue, myAddress);  // write it out
        }

        private void comboboxChan1ClockDiv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeClockDiv(comboboxChan1ClockDiv);

            if ((bool)checkBoxDualPhase.IsChecked)
                comboboxChan2ClockDiv.SelectedIndex = comboboxChan1ClockDiv.SelectedIndex;
        }

        private void comboboxChan2ClockDiv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeClockDiv(comboboxChan2ClockDiv);
        }

        private void changeClockDiv(ComboBox comboB)
        {
            byte currentValue;

            if (comboB == comboboxChan1ClockDiv)
            {
                currentValue = (byte)(currentRegValue((byte)Registers.SWDIV) & 0x0C);
                currentValue |= byte.Parse(comboB.SelectedIndex.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            }
            else
            {
                currentValue = (byte)(currentRegValue((byte)Registers.SWDIV) & 0x03);
                currentValue |= (byte)(byte.Parse(comboB.SelectedIndex.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) << 2);
            }                

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SWDIV, currentValue, myAddress);  // write it out
        }

        private void comboBoxSoftStartCh1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeSoftStart(comboBoxSoftStartCh1);
        }

        private void comboBoxSoftStartCh2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeSoftStart(comboBoxSoftStartCh2);
        }

        private void changeSoftStart(ComboBox comboB)
        {
            byte currentValue;
            currentValue = currentRegValue((byte)Registers.SOFTSTART);

            if (comboB == comboBoxSoftStartCh2)
            {
                currentValue &= 0x0F;
                currentValue |= (byte)(comboB.SelectedIndex << 4);
            }
            else
            {
                currentValue &= 0xF0;
                currentValue |= (byte)comboB.SelectedIndex;
            }

            Globals.mcuCommand.sendSPI(true, (byte)Registers.SOFTSTART, currentValue, myAddress);  // write it out
        }

        private void sliderChan1VIadj_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockChan1VIadj.Text = ((byte)sliderChan1VIadj.Value).ToString();
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1IADJ, (byte)sliderChan1VIadj.Value, myAddress);  // write it out

            if ((bool)checkBoxDualPhase.IsChecked)
            {
                Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2IADJ, (byte)sliderChan1VIadj.Value, myAddress);  // write it out; do not need to set it, 682 does that....
                textBlockChan2VIadj.Text = textBlockChan1VIadj.Text;
                sliderChan2VIadj.Value = sliderChan1VIadj.Value;
            }           
        }

        private void sliderChan2VIadj_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockChan2VIadj.Text = ((byte)sliderChan2VIadj.Value).ToString();
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2IADJ, (byte)sliderChan2VIadj.Value, myAddress);  // write it out
        }

        private void comboBoxPWMclock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.mcuCommand.sendSPI(true, (byte)Registers.PWMDIV, (byte)comboBoxPWMclock.SelectedIndex, myAddress);  // write it out
        }

        private void comboBoxFMmag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            byte currentValue = (byte)(currentRegValue((byte)Registers.FM) & 0x0F);
            currentValue |= (byte)((comboBoxFMmag.SelectedIndex << 4) & 0x30);
            Globals.mcuCommand.sendSPI(true, (byte)Registers.FM, currentValue, myAddress);  // write it out
        }

        private void comboBoxFMfreek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            byte currentValue = (byte)(currentRegValue((byte)Registers.FM) & 0x30);
            currentValue |= (byte)(comboBoxFMfreek.SelectedIndex & 0x0F);
            Globals.mcuCommand.sendSPI(true, (byte)Registers.FM, currentValue, myAddress);  // write it out
        }

        private void sliderChan1PWMdootie_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockChan1PWMdootie.Text = ((ushort)sliderChan1PWMdootie.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderChan1PWMdootie.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1PWMH, (byte)((integerValue >> 8) & 0x03), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH1PWML, (byte)(integerValue & 0xFF), myAddress);  // write it out low         

            if ((bool)checkBoxDualPhase.IsChecked)
            {
                textBlockChan2PWMdootie.Text = textBlockChan1PWMdootie.Text;
                sliderChan2PWMdootie.Value = sliderChan1PWMdootie.Value;
            }
        }

        private void sliderChan2PWMdootie_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockChan2PWMdootie.Text = ((ushort)sliderChan2PWMdootie.Value).ToString(CultureInfo.InvariantCulture);
            UInt16 integerValue = UInt16.Parse(sliderChan2PWMdootie.Value.ToString(CultureInfo.InvariantCulture).Split('.')[0], CultureInfo.InvariantCulture);

            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2PWMH, (byte)((integerValue >> 8) & 0x03), myAddress);  // write it out high
            Globals.mcuCommand.sendSPI(true, (byte)Registers.CH2PWML, (byte)(integerValue & 0xFF), myAddress);  // write it out low         
        }

        private void sliderChan1Slope_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockChan1Slope.Text = ((ushort)sliderChan1Slope.Value).ToString();

            byte currentValue = (byte)(currentRegValue((byte)Registers.ISLOPE) & 0xF0);
            currentValue |= (byte)((int)sliderChan1Slope.Value & 0x0F);
            Globals.mcuCommand.sendSPI(true, (byte)Registers.ISLOPE, currentValue, myAddress);
        }

        private void sliderChan2Slope_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlockChan2Slope.Text = ((ushort)sliderChan2Slope.Value).ToString();

            byte currentValue = (byte)(currentRegValue((byte)Registers.ISLOPE) & 0x0F);
            currentValue |= (byte)(((int)sliderChan2Slope.Value << 4) & 0xF0);
            Globals.mcuCommand.sendSPI(true, (byte)Registers.ISLOPE, currentValue, myAddress);
        }

        private byte currentRegValue(byte reg)
        {
            SPIcommadReturnData scrd = new SPIcommadReturnData();

            Globals.mcuCommand.sendSPI(false, reg, 0, myAddress);  // Send a read response
            scrd = Globals.mcuCommand.sendSPI(false, reg, 0, myAddress);  // Now send it again to get the data back

            return (byte)(scrd.assembledReturn & 0xFF);
        }

        private void buttonReadFaults_Click(object sender, RoutedEventArgs e)
        {
            byte faultReg1 = currentRegValue((byte)Registers.FLT1);
            byte faultReg2 = currentRegValue((byte)Registers.FLT2);

#if CSTMR
            return;
#endif

            if ((faultReg1 & 0x01) == 0x01)  // UV channel 1
                textBlockChan1UV.Background = Brushes.Red;
            else
                textBlockChan1UV.Background = Brushes.Green;

            if ((faultReg1 & 0x02) == 0x02)  // UV channel 2
                textBlockChan2UV.Background = Brushes.Red;
            else
                textBlockChan2UV.Background = Brushes.Green;

            if ((faultReg1 & 0x04) == 0x04)  // OV channel 1
                textBlockChan1OV.Background = Brushes.Red;
            else
                textBlockChan1OV.Background = Brushes.Green;

            if ((faultReg1 & 0x08) == 0x08)  // OV channel 2
                textBlockChan2OV.Background = Brushes.Red;
            else
                textBlockChan2OV.Background = Brushes.Green;

            if ((faultReg1 & 0x10) == 0x10)  // TW
                textBlockTW.Background = Brushes.Red;
            else
                textBlockTW.Background = Brushes.Green;

            if ((faultReg1 & 0x20) == 0x20)  // PC
                textBlockPC.Background = Brushes.Red;
            else
                textBlockPC.Background = Brushes.Green;

            if ((faultReg1 & 0x80) == 0x80)  // RTO
                textBlockRTO.Background = Brushes.Red;
            else
                textBlockRTO.Background = Brushes.Green;

            if ((faultReg2 & 0x01) == 0x01)  // ISO channel 1
                textBlockChan1ISO.Background = Brushes.Red;
            else
                textBlockChan1ISO.Background = Brushes.Green;

            if ((faultReg2 & 0x02) == 0x02)  // ISO channel 2
                textBlockChan2ISO.Background = Brushes.Red;
            else
                textBlockChan2ISO.Background = Brushes.Green;

            if ((faultReg2 & 0x04) == 0x04)  // ILIM channel 1
                textBlockChan1ILIM.Background = Brushes.Red;
            else
                textBlockChan1ILIM.Background = Brushes.Green;

            if ((faultReg2 & 0x08) == 0x08)  // ILIM channel 2
                textBlockChan2ILIM.Background = Brushes.Red;
            else
                textBlockChan2ILIM.Background = Brushes.Green;

            if ((faultReg2 & 0x10) == 0x10)  // OC channel 1
                textBlockChan1OC.Background = Brushes.Red;
            else
                textBlockChan1OC.Background = Brushes.Green;

            if ((faultReg2 & 0x20) == 0x20)  // OC channel 2
                textBlockChan2OC.Background = Brushes.Red;
            else
                textBlockChan2OC.Background = Brushes.Green;

            if ((faultReg2 & 0x40) == 0x40)  // UC channel 1
                textBlockChan1UC.Background = Brushes.Red;
            else
                textBlockChan1UC.Background = Brushes.Green;

            if ((faultReg2 & 0x80) == 0x80)  // UC channel 2
                textBlockChan2UC.Background = Brushes.Red;
            else
                textBlockChan2UC.Background = Brushes.Green;
        }

        public void buttonInit520_Click(object sender, RoutedEventArgs e)
        {
            buttonReadFaults_Click(null, null);

            checkBoxBuck1Enable.IsChecked = false;
            checkBoxBuck2Enable.IsChecked = false;
            checkBoxCV1.IsChecked = true;
            checkBoxCV2.IsChecked = true;
            sliderChan1VIadj.Value = 255;
            sliderChan1PWMdootie.Value = 0;
            sliderChan1Slope.Value = 7;
            sliderChan2Slope.Value = 7;
            comboboxChan1ClockDiv.SelectedIndex = 1;  // 4
            comboBoxSoftStartCh1.SelectedIndex = 15;  // 100
            comboBoxSoftStartCh2.SelectedIndex = 15;
//            comboBoxPWMclock.SelectedIndex = savedTabControlSettings.Combo0;
  //          comboBoxFMmag.SelectedIndex = savedTabControlSettings.Combo1;
            checkBoxDualPhase.IsChecked = true;
            //      checkBoxIntPwm.IsChecked = savedTabControlSettings.Enable6;

#if CSTMR
            checkBoxBuck1Enable.IsChecked = true;
#endif
        }

        public void buttonReadAll_Click(object sender, RoutedEventArgs e)
        {
  //          Mouse.OverrideCursor = Cursors.Wait;  // set hourglass

            try
            {
                byte currentValue = currentRegValue((byte)Registers.EN);
                if ((currentValue & 0x01) == 0x01)
                    checkBoxBuck1Enable.IsChecked = true;
                else
                    checkBoxBuck1Enable.IsChecked = false;
                if ((currentValue & 0x02) == 0x02)
                    checkBoxBuck2Enable.IsChecked = true;
                else
                    checkBoxBuck2Enable.IsChecked = false;

                currentValue = currentRegValue((byte)Registers.CFG1);
                if ((currentValue & 0x01) == 0x01)
                    checkBoxCV1.IsChecked = true;
                else
                    checkBoxCV1.IsChecked = false;
                if ((currentValue & 0x02) == 0x02)
                    checkBoxCV2.IsChecked = true;
                else
                    checkBoxCV2.IsChecked = false;
                if ((currentValue & 0x20) == 0x20)
                    checkBoxDualPhase.IsChecked = true;
                else
                    checkBoxDualPhase.IsChecked = false;
                if ((currentValue & 0x40) == 0x40)
                    checkBoxIntPwm.IsChecked = true;
                else
                    checkBoxIntPwm.IsChecked = false;

                currentValue = currentRegValue((byte)Registers.SWDIV);
                comboboxChan1ClockDiv.SelectedIndex = currentValue & 0x03;
                comboboxChan2ClockDiv.SelectedIndex = (currentValue & 0x0C) >> 2;

                currentValue = currentRegValue((byte)Registers.FM);
                comboBoxFMfreek.SelectedIndex = currentValue & 0x0F;
                comboBoxFMmag.SelectedIndex = (currentValue & 0x30) >> 4;

                currentValue = currentRegValue((byte)Registers.SOFTSTART);
                comboBoxSoftStartCh1.SelectedIndex = currentValue & 0x0F;
                comboBoxSoftStartCh1.SelectedIndex = currentValue & (0xF0 >> 4);

                currentValue = currentRegValue((byte)Registers.CH1IADJ);
                sliderChan1VIadj.Value = currentValue;
                textBlockChan1VIadj.Text = sliderChan1VIadj.Value.ToString();

                currentValue = currentRegValue((byte)Registers.CH2IADJ);
                sliderChan2VIadj.Value = currentValue;
                textBlockChan2VIadj.Text = sliderChan2VIadj.Value.ToString();

                currentValue = currentRegValue((byte)Registers.PWMDIV);
                comboBoxPWMclock.SelectedIndex = currentValue;

                currentValue = currentRegValue((byte)Registers.CH1PWML);
                UInt16 oredValue = (UInt16)(((currentRegValue((byte)Registers.CH1PWMH) & 0x03) << 8) | currentValue);
                sliderChan1PWMdootie.Value = oredValue;
                textBlockChan1PWMdootie.Text = sliderChan1PWMdootie.Value.ToString();

                currentValue = currentRegValue((byte)Registers.CH2PWML);
                oredValue = (UInt16)(((currentRegValue((byte)Registers.CH2PWMH) & 0x03) << 8) | currentValue);
                sliderChan2PWMdootie.Value = oredValue;
                textBlockChan2PWMdootie.Text = sliderChan2PWMdootie.Value.ToString();

                currentValue = currentRegValue((byte)Registers.ISLOPE);
                sliderChan1Slope.Value = currentValue & 0x0F;
                textBlockChan1Slope.Text = sliderChan1Slope.Value.ToString();
                sliderChan2Slope.Value = (currentValue & 0xF0) >> 4;
                textBlockChan2Slope.Text = sliderChan2Slope.Value.ToString();
                
                buttonReadFaults_Click(null, null);

        //        Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LM2: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //              Mouse.OverrideCursor = _previousCursor;
            }
        }

        private void userControl682_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!mainWindow.usbRemoved && !Globals.updateMode)
                buttonReadFaults_Click(null, null);
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
            using (StreamWriter sw = File.AppendText(path + "\\regDump682_" + myAddress.ToString() + ".txt"))
            {
                string report = string.Join(Environment.NewLine, data.Select(array => string.Join(" ", array)));
                sw.WriteLine(report);
            }
        }
    }
}

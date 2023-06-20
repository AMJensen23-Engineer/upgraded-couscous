using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
    public class lmmDevice
    {
        public string address { get; set; }
        public byte device { get; set; }
        public string deviceName { get; set; }
    }

    /// <summary>
    /// Interaction logic for TPS92622Control.xaml
    /// </summary>
 
    public partial class TPS92662Control : UserControl
    {        
        private List<lmmDevice> lmmDeviceList = new List<lmmDevice>();
        private List<String> regList = new List<string>();
        private List<String> sortList663 = new List<string>();
        private List<String> sortList664 = new List<string>();
        private List<String> sortList664MTP_0_1_4_5 = new List<string>();
        private List<String> sortListBoth_BF = new List<string>();
        private RegMap regMap664;
        private RegMap regMap665;
        private RegMap regMap667;
        private Pixel pix;
        private MainWindow mainWindow = null;               // get the MainWindow object
        private UInt16[] lmmChToLMMAddr = new UInt16[LMMS_TOTAL];	// Which LMM address is associated with each buck channel?
        private UInt16 lmmPhaseShift;
        private UInt16 lmmDutyCycle;
        public byte currentLMMselected;
        private byte currentRegister;
        private byte currentCommand;
  //      private bool badCRConWrite = false;

        private const byte BROADCAST_ID = 0x1F;
        private const byte LMMS_TOTAL = 32;
//        private bool firstInit = false;

        public TPS92662Control(MainWindow master)
        {
            InitializeComponent();
            mainWindow = master;

            //       regList.Sort();
            //       regList.Sort();
            //       regList.OrderBy(

            checkBoxNoBusWalk.IsChecked = Properties.Settings.Default.checkBoxNoBusWalk;

            checkBoxAckEnable.Checked -= CheckBoxAckEnable_Checked;
            checkBoxAckEnable.IsChecked = Properties.Settings.Default.checkBoxAckEnable;

            lmmPhaseShift = 85;
            lmmDutyCycle = 0;

            comboBoxMTPconfig.Foreground = Brushes.Gray;
            checkBoxbadCRC.Foreground = Brushes.Gray;
            groupBox664_.IsEnabled = false;

            if (Properties.Settings.Default.baudRate == 1000000)
                radioButton1Mbps.IsChecked = true;
            else
                radioButton500Kbps.IsChecked = true;

  //          for (int i = 0; i < 8; i++)
    //            comboBoxCMWTAP.Items.Add(i);

            for (int i = 0; i < LMMS_TOTAL; i++)
                comboBoxBusWalkDevices.Items.Add(i);
            comboBoxBusWalkDevices.SelectedIndex = Properties.Settings.Default.comboBoxBusWalkDevices;

            string[] mtpConfigModes = new string[17] { "VOLATILE", "MTP_Mode_0 (0b0000)", "MTP_Mode_1 (0b0001)", "MTP_Mode_2 (0b0010)", "MTP_Mode_3 (0b0011)", "MTP_Mode_4 (0b0100)", "MTP_Mode_5 (0b0101)", "MTP_Mode_6 (0b0110)", "MTP_Mode_7 (0b0111)", "MTP_Mode_8 (0b1000)",
            "MTP_Mode_9 (0b1001)", "MTP_Mode_A (0b1010)", "MTP_Mode_B (0b1011)", "MTP_Mode_C (0b1100)", "MTP_Mode_D (0b1101)", "MTP_Mode_E (0b1110)", "MTP_Mode_F (0b1111)" };
            for (int i = 0; i < mtpConfigModes.Count(); i++)
                comboBoxMTPconfig.Items.Add(mtpConfigModes[i]);

            groupBoxRegister.IsEnabled = false;
#if !DEBUG
            groupBoxLMM.IsEnabled = false;         
            buttonRegMap.IsEnabled = false;
            buttonPixel.IsEnabled = false;
            checkBoxPhase.Foreground = Brushes.Gray;
            checkBoxAllChannel.Foreground = Brushes.Gray;
            textBlockDutyCycle.Foreground = Brushes.Gray;
#endif
            textBoxPhase.Text = "0";
            textBlockDutyCycle.Text = "0";

            sortList663.Add("All");
            sortList663.Add("MPHASE");
            sortList663.Add("LPHASE");
            sortList663.Add("DPHASE");
            sortList663.Add("MWIDTH");
            sortList663.Add("LWIDTH");
            sortList663.Add("DWIDTH");
            sortList663.Add("DEF");
            sortList663.Add("SLEW");
            sortList663.Add("I2C");
            sortList663.Add("ADC");

            sortList664.Add("All");
            sortList664.Add("MTP");
            sortList664.Add("SLEW");
            sortList664.Add("BANK_WIDTH");
            sortList664.Add("WIDTH");
            sortList664.Add("BANK_PHASE");
            sortList664.Add("PHASE");
            sortList664.Add("BANK");
            sortList664.Add("LED");
            sortList664.Add("SYS");
            sortList664.Add("ADC");
            sortList664.Add("FLT");
            sortList664.Add("FAULT");
            sortList664.Add("VLD");
            sortList664.Add("LOT");

            sortList664MTP_0_1_4_5.Add("All");
            sortList664MTP_0_1_4_5.Add("DEF");
            sortList664MTP_0_1_4_5.Add("CA");
            sortList664MTP_0_1_4_5.Add("CB");
            sortList664MTP_0_1_4_5.Add("CD");
            sortList664MTP_0_1_4_5.Add("CE");
            sortList664MTP_0_1_4_5.Add("PC");
            sortList664MTP_0_1_4_5.Add("CRC");
            sortList664MTP_0_1_4_5.Add("PHASE");

            sortListBoth_BF.Add("All");

            textBlockDutyCycle.Text = "0";
            textBoxPhase.Text = "0";
//#if CSTMR
//            checkBoxNoBusWalk.Visibility = Visibility.Hidden;
//#if !FAIL_ANAL  // both CSTMR (always TRUE!) and FAIL_ANAL need to be true for failure analysis mode; add FA to end of assembly name a change name in sign tool
//            buttonTestMode664.Visibility = Visibility.Hidden;
//            textboxRegOverride.Text = "";
//            textboxRegOverride.Visibility = Visibility.Hidden;
//            buttonClearReg.Visibility = Visibility.Hidden;
//            radioButton500Kbps.Visibility = Visibility.Hidden;
//            radioButton1Mbps.IsChecked = true;
//#else  // FAIL_ANAL = true
//            buttonPixel.Visibility = Visibility.Hidden;
//            textBlockDutyCycle.Text = "0";
//#endif
//            buttonFSBF.Visibility = Visibility.Hidden;
//            sliderPhase.Visibility = Visibility.Hidden;
//            labelPhase1.Visibility = Visibility.Hidden;
//            checkBoxbadCRC.Visibility = Visibility.Collapsed;
//            comboBoxSort.Visibility = Visibility.Collapsed;
//            labelSort.Visibility = Visibility.Collapsed;
//#endif

#if (CSTMR && !FAIL_ANAL)
            checkBoxNoBusWalk.Visibility = Visibility.Hidden;
            buttonTestMode664.Visibility = Visibility.Hidden;
            textboxRegOverride.Text = "";
            textboxRegOverride.Visibility = Visibility.Hidden;
            buttonClearReg.Visibility = Visibility.Hidden;
   //         radioButton500Kbps.Visibility = Visibility.Hidden;
            radioButton1Mbps.IsChecked = true;
            buttonFSBF.Visibility = Visibility.Hidden;
            sliderPhase.Visibility = Visibility.Hidden;
            labelPhase1.Visibility = Visibility.Hidden;
            checkBoxbadCRC.Visibility = Visibility.Collapsed;
            comboBoxSort.Visibility = Visibility.Collapsed;
            labelSort.Visibility = Visibility.Collapsed;
            checkBoxAckEnable.Foreground = Brushes.Gray;

            groupBox664_.IsEnabled = false;
            buttonResetReg0.Visibility = Visibility.Hidden;
            buttonProgMTP.Visibility = Visibility.Hidden;
            buttonRestoreVol.Visibility = Visibility.Hidden;
            checkBoxMTPNoRw.Visibility = Visibility.Hidden;
            comboBoxMTPconfig.Visibility = Visibility.Hidden;
            labelMTPconfig.Visibility = Visibility.Hidden;
            checkBoxMTPNoRw.Visibility = Visibility.Hidden;
#elif (CSTMR && FAIL_ANAL)
            buttonPixel.Visibility = Visibility.Hidden;
            textBlockDutyCycle.Text = "0";
#endif

            //      comboBoxSort.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = sortList664 });
            textBoxWriteDelay.Text = Properties.Settings.Default.textBoxWriteDelay;
        }

        public void updateStatus(string statusText)
        {
            textBoxOutput.AppendText(statusText);
            textBoxOutput.CaretIndex = textBoxOutput.Text.Length;
            textBoxOutput.ScrollToEnd();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Closing += window_Closing;
        }

        void window_Closing(object sender, global::System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.comboBoxBusWalkDevices = (byte)comboBoxBusWalkDevices.SelectedIndex;
            Properties.Settings.Default.Save();

            if (buttonWatchDog.Content.ToString() == "Stop WD")
                Globals.mcuCommand.stopUARTtimer(currentLMMselected);  // shut off the watchdog if running

            if (regMap664 != null)
            {
                regMap664.Close();
                regMap664 = null;
            }

            if (regMap665 != null)
            {
                regMap665.Close();
                regMap665 = null;
            }

            if (regMap667 != null)
            {
                regMap667.Close();
                regMap667 = null;
            }

            if (pix != null)
            {
                pix.Close();
                pix = null;
            }
        }

        private string setDeviceName(byte device)
        {
            // 0x92 = 662, 0x93 = 663, 0x94 = 662A, 0x95 = 663A, 0x96 = 664, 0x97 = 665, 0x98 = 667
            string returnDevice = "";

            switch (device)
            {
                case 0x92:
                    returnDevice = "662";
                    break;

                case 0x93:
                    returnDevice = "663";
                    break;

                case 0x94:
                    returnDevice = "662A";
                    break;

                case 0x95:
                    returnDevice = "663A";
                    break;

                case 0x96:
                    returnDevice = "664";
                    break;

                case 0x97:
                    returnDevice = "665";
                    break;

                case 0x98:
                    returnDevice = "667";
                    break;

                default:
                    break;
            }

            return returnDevice;
        }

        private void Button662Init_Click(object sender, RoutedEventArgs e)
        {
            Cursor _previousCursor = Mouse.OverrideCursor;
            try
            {               
                Mouse.OverrideCursor = Cursors.Wait;  // set hourglass

                //          string status = "";
                //          Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, BROADCAST_ID, (byte)RegAddr.SYSCFG, BitConverter.GetBytes(0x00), ref status);
                //        Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, BROADCAST_ID, (byte)RegAddr.ADCID, BitConverter.GetBytes(0x00), ref status);

                UInt32 lmmsPresent = 0;
                byte numLMMsFound = 0;
                lmmDeviceList.Clear();

                comboBox66XDevices.SelectionChanged -= ComboBox66XDevices_SelectionChanged;
                ObservableCollection<string> emptyList = new ObservableCollection<string>();
                comboBox66XDevices.ItemsSource = emptyList;
                comboBox66XDevices.SelectionChanged += ComboBox66XDevices_SelectionChanged;

                buttonPixel.IsEnabled = false;

                groupBox664_.IsEnabled = false;
                buttonResetReg0.Visibility = Visibility.Visible;
                buttonProgMTP.Visibility = Visibility.Visible;
                buttonRestoreVol.Visibility = Visibility.Visible;
                checkBoxMTPNoRw.Visibility = Visibility.Visible;
                comboBoxMTPconfig.Visibility = Visibility.Visible;
                labelMTPconfig.Visibility = Visibility.Visible;
                checkBoxMTPNoRw.Visibility = Visibility.Visible;
#if !DEBUG
                string readStatus = "";

                if (!(bool)checkBoxNoBusWalk.IsChecked)
                {
                    Globals.mcuCommand.lmmCommsReset();  // bus must be reset if we ping a non existant part. LMM bus will tri state

                    byte numDevices = Convert.ToByte(comboBoxBusWalkDevices.SelectedValue.ToString());
                    for (byte lmm = 0; lmm <= numDevices; lmm++)
                    {
                        if (Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, lmm, (byte)LMMRegConfigs.RegAddr.ICID, ref readStatus, false, null) == 3)
                        {
                            if ((Globals.uartBuf[0] == 0x92) || (Globals.uartBuf[0] == 0x93) || (Globals.uartBuf[0] == 0x94) || (Globals.uartBuf[0] == 0x95) || (Globals.uartBuf[0] == 0x96) || (Globals.uartBuf[0] == 0x97) || (Globals.uartBuf[0] == 0x98))  // We got something back; is it valid? 0x92 = 662, 0x93 = 663, 0x94 = 662A, 0x95 = 663A, 0x96 = 664, 0x97 = 665, 0x98 = 667
                            {
                                lmmsPresent |= (byte)(1 << lmm);  // This is a valid LMM                            
                                lmmDeviceList.Add(new lmmDevice { address = lmm.ToString(), device = Globals.uartBuf[0], deviceName = setDeviceName(Globals.uartBuf[0]) });
                                numLMMsFound++;
#if CSTMR
                                if (Globals.uartBuf[0] > 0x95)
                                {
                                    currentLMMselected = lmm;
                                    ButtonFSBF_Click(this, null);
                                }
#endif
                            }
                            updateStatus(readStatus + "\n");
                        }
                        else
                            Globals.mcuCommand.lmmCommsReset();  // bus must be reset if we ping a non existant part. LMM bus will tri state
                    }
                }
                else
                {
                    for (byte lmm = 0; lmm < comboBoxBusWalkDevices.SelectedIndex + 1; lmm++)
                    {                        
                        lmmsPresent |= (byte)(1 << lmm);  // This is a valid LMM
                        lmmDeviceList.Add(new lmmDevice { address = lmm.ToString(), device = 0x96, deviceName = "664" });
                        numLMMsFound++;
                        updateStatus(readStatus + "\n");
                    }
                }
#else
                lmmsPresent |= (byte)(1 << 0);  // This is a valid LMM
                                              //             lmmChToLMMAddr[numLMMsFound++] = lmm;
            lmmDeviceList.Add(new lmmDevice { address = "0", device = 0x96 });
            numLMMsFound++;
#endif
                lmmDeviceList.Add(new lmmDevice { address = "BF", device = 0xFF, deviceName = "" });  // add bus write with 664 regs
                if (numLMMsFound > 0)
                {
                    checkBoxAllChannel.IsEnabled = true;
                    comboBox66XDevices.SelectionChanged -= ComboBox66XDevices_SelectionChanged;
                    ObservableCollection<string> comboItemsLMMsFound = new ObservableCollection<string>();
                    for (int i = 0; i < numLMMsFound + 1; i++)
                    {
                        if(lmmDeviceList[i].address != "BF")
                            comboItemsLMMsFound.Add(lmmDeviceList[i].address + " = " + lmmDeviceList[i].deviceName + "-0x" + lmmDeviceList[i].device.ToString("X"));
                        else
                            comboItemsLMMsFound.Add(lmmDeviceList[i].address + " = All");
                    }
                    comboBox66XDevices.ItemsSource = comboItemsLMMsFound;
                    if (Properties.Settings.Default.comboBox66XDevices < numLMMsFound)
                        comboBox66XDevices.SelectedIndex = Properties.Settings.Default.comboBox66XDevices;
                    else
                        comboBox66XDevices.SelectedIndex = 0;

                    string[] tokens = comboBox66XDevices.SelectedItem.ToString().Split(' ');
                    currentLMMselected = byte.Parse(tokens[0], CultureInfo.InvariantCulture);
                    comboBox66XDevices.SelectionChanged += ComboBox66XDevices_SelectionChanged;

                    if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device > 0x95)
                    {
                        comboBoxMTPconfig.Foreground = Brushes.Black;
                        groupBox664_.IsEnabled = true;
                        checkBoxPhase.Content = 64;
                    }

     //               comboBoxCMWTAP.SelectedIndex = Properties.Settings.Default.comboBoxCMWTAP;
                    comboBoxMTPconfig.SelectedIndex = 0;  // Properties.Settings.Default.mtpConfigModes;

                    groupBoxLMM.IsEnabled = true;
                    groupBoxRegister.IsEnabled = true;
                    buttonRegMap.IsEnabled = true;
                    if(pix == null)
                        buttonPixel.IsEnabled = true;
                    checkBoxPhase.Foreground = Brushes.Black;
                    checkBoxAllChannel.Foreground = Brushes.Black;
                    comboBoxRegister.Foreground = Brushes.Black;
                    comboBoxCommand.Foreground = Brushes.Black;
                    comboBoxSort.Foreground = Brushes.Black;
     //               comboBoxCMWTAP.Foreground = Brushes.Black;
                    textBlockDutyCycle.Foreground = Brushes.Black;
                    checkBoxbadCRC.Foreground = Brushes.Black;
                    checkBoxAckEnable.Foreground = Brushes.Black;

                    if (numLMMsFound > 1)
                    {
                        bool foundOld = false;
                        bool foundNew = false;
                        for (int i = 0; i < lmmDeviceList.Count; i++)  // only enable all option if all are new or all are old
                        {
                            if (lmmDeviceList[i].device < 0x96)
                                foundOld = true;
                            else if (lmmDeviceList[i].device > 0x95)
                                foundNew = true;

                            if (foundOld && foundNew)
                            {
                                Globals.bothGenDevicesPresent = true;
                                checkBoxAllChannel.IsEnabled = false;
                                break;
                            }
                        }
                    }
                    else
                        checkBoxAllChannel.IsEnabled = false;

                    if (checkBoxAllChannel.IsEnabled == true)
                        checkBoxAllChannel.IsChecked = Properties.Settings.Default.checkBoxAllChannel;
                    checkBoxPhase.IsChecked = Properties.Settings.Default.checkBoxPhase;


#if !FAIL_ANAL
                    if ((!(bool)checkBoxNoBusWalk.IsChecked) && (Globals.bothGenDevicesPresent == false))
                    {
                        BuckPhaseShift = Properties.Settings.Default.BuckPhaseShift;  // restore previous settings if all is well
                        LMMdutyCycle = Properties.Settings.Default.LMMdutyCycle;
                    }
#endif

                    comboBoxSort.SelectionChanged -= ComboBoxSort_SelectionChanged;
                    if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
                        comboBoxSort.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = sortList663 });
                    else
                        comboBoxSort.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = sortList664 });
                    comboBoxSort.SelectedIndex = 0;  // this has to be 0; in case user had different device vs. last power up // Properties.Settings.Default.comboBoxSort;
                    comboBoxSort.SelectionChanged += ComboBoxSort_SelectionChanged;
                    loadComboBoxRegCmds("All");

                    CheckBoxAckEnable_Checked(this, null);
                    checkBoxAckEnable.Checked += CheckBoxAckEnable_Checked;

                    //              Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, 0, (byte)RegAddr.PARLED, BitConverter.GetBytes(0x02), ref status);
                }
                else
                {
                    MessageBox.Show("No LMM(s) found during 66X initialization...", "66X Intialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    emptyList = new ObservableCollection<string>();
                    comboBox66XDevices.ItemsSource = emptyList;
                    groupBoxLMM.IsEnabled = false;
                    groupBoxRegister.IsEnabled = false;
                    checkBoxPhase.Foreground = Brushes.Gray;
                    checkBoxAllChannel.Foreground = Brushes.Gray;
                    comboBoxRegister.Foreground = Brushes.Gray;
        //            comboBoxCMWTAP.Foreground = Brushes.Gray;
                    comboBoxCommand.Foreground = Brushes.Gray;
                    comboBoxSort.Foreground = Brushes.Gray;
                    textBlockDutyCycle.Foreground = Brushes.Gray;
                    Mouse.OverrideCursor = _previousCursor;
                    checkBoxbadCRC.Foreground = Brushes.Gray;
                    return;
                }

                Mouse.OverrideCursor = _previousCursor;
                //          mainWindow.w662.Focus();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = _previousCursor;
                comboBox66XDevices.SelectionChanged -= ComboBox66XDevices_SelectionChanged;
                ObservableCollection<string> emptyList = new ObservableCollection<string>();
                comboBox66XDevices.ItemsSource = emptyList;
                comboBox66XDevices.SelectionChanged += ComboBox66XDevices_SelectionChanged;
                groupBoxLMM.IsEnabled = false;
                groupBoxRegister.IsEnabled = false;
                checkBoxPhase.Foreground = Brushes.Gray;
                checkBoxAllChannel.Foreground = Brushes.Gray;
                comboBoxRegister.Foreground = Brushes.Gray;
      //          comboBoxCMWTAP.Foreground = Brushes.Gray;
                comboBoxCommand.Foreground = Brushes.Gray;
                comboBoxSort.Foreground = Brushes.Gray;
                checkBoxbadCRC.Foreground = Brushes.Gray;
                MessageBox.Show("Fatal error during 66X initialization...\n" + ex.Message, "66X Intialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void loadComboBoxRegCmds(string sortString)
        {
            if ((comboBox66XDevices.SelectedItem.ToString().Contains("BF")) && (!Globals.bothGenDevicesPresent))  // use last loaded registers from whatever device that may be....
                return;

            if (regList != null)
                regList.Clear();
            comboBoxRegister.SelectionChanged -= ComboBoxRegister_SelectionChanged;
            ObservableCollection<string> comboItemsRegs = new ObservableCollection<string>();

            if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
            {
                for (int i = 0; i < LMMRegConfigs.regs.Length; i++)
                {
                    if(sortString == "All")  // if((string)comboBoxSort.SelectedItem == "All") //if ((string)comboBoxSort.Items.CurrentItem == "All")
                        regList.Add(LMMRegConfigs.regs[i]);
                    else if (LMMRegConfigs.regs[i].Contains(sortString))
                    {
                        regList.Add(LMMRegConfigs.regs[i]);
                    }
                }               
            }
            else if ((lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x96)  && (!comboBox66XDevices.SelectedItem.ToString().Contains("BF")))  // BF has device value of 0xFF
            {
                buttonResetReg0.Visibility = Visibility.Visible;
                buttonProgMTP.Visibility = Visibility.Visible;
                buttonRestoreVol.Visibility = Visibility.Visible;
                checkBoxMTPNoRw.Visibility = Visibility.Visible;
                comboBoxMTPconfig.Visibility = Visibility.Visible;
                labelMTPconfig.Visibility = Visibility.Visible;
                checkBoxMTPNoRw.Visibility = Visibility.Visible;
                if (comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664[i]);
                        else if (LMMRegConfigs.regs664[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664[i]);
                        }
                    }
                }
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_0")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_1")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_4")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_5")))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_0_1_4_5.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_0_1_4_5[i]);
                        else if (LMMRegConfigs.regs664MTP_0_1_4_5[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_0_1_4_5[i]);
                        }
                    }
                }
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_2")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_6")))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_2_6.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_2_6[i]);
                        else if (LMMRegConfigs.regs664MTP_2_6[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_2_6[i]);
                        }
                    }
                }
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_3")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_7")))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_3_7.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_3_7[i]);
                        else if (LMMRegConfigs.regs664MTP_3_7[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_3_7[i]);
                        }
                    }
                }
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_8")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_9")))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_8_9.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_8_9[i]);
                        else if (LMMRegConfigs.regs664MTP_8_9[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_8_9[i]);
                        }
                    }
                }
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_A"))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_10.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_10[i]);
                        else if (LMMRegConfigs.regs664MTP_10[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_10[i]);
                        }
                    }
                }
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_B"))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_11.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_11[i]);
                        else if (LMMRegConfigs.regs664MTP_11[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_11[i]);
                        }
                    }
                }
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_C")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_D")))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_12_13.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_12_13[i]);
                        else if (LMMRegConfigs.regs664MTP_12_13[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_12_13[i]);
                        }
                    }
                }
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_E"))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_14.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_14[i]);
                        else if (LMMRegConfigs.regs664MTP_14[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_14[i]);
                        }
                    }
                }
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_F"))
                {
                    for (int i = 0; i < LMMRegConfigs.regs664MTP_15.Length; i++)
                    {
                        if (sortString == "All")
                            regList.Add(LMMRegConfigs.regs664MTP_15[i]);
                        else if (LMMRegConfigs.regs664MTP_15[i].Contains(sortString))
                        {
                            regList.Add(LMMRegConfigs.regs664MTP_15[i]);
                        }
                    }
                }
            }
            else if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x97)  // 665
            {
                buttonResetReg0.Visibility = Visibility.Hidden;
                buttonProgMTP.Visibility = Visibility.Hidden;
                buttonRestoreVol.Visibility = Visibility.Hidden;
                checkBoxMTPNoRw.Visibility = Visibility.Hidden;
                comboBoxMTPconfig.Visibility = Visibility.Hidden;
                labelMTPconfig.Visibility = Visibility.Hidden;
                checkBoxMTPNoRw.Visibility = Visibility.Hidden;
                for (int i = 0; i < LMMRegConfigs.regs665.Length; i++)
                {
                    if (sortString == "All")  // if((string)comboBoxSort.SelectedItem == "All") //if ((string)comboBoxSort.Items.CurrentItem == "All")
                        regList.Add(LMMRegConfigs.regs665[i]);
                    else if (LMMRegConfigs.regs665[i].Contains(sortString))
                    {
                        regList.Add(LMMRegConfigs.regs665[i]);
                    }
                }
            }
            else if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x98)  // 667
            {
                buttonResetReg0.Visibility = Visibility.Hidden;
                buttonProgMTP.Visibility = Visibility.Hidden;
                buttonRestoreVol.Visibility = Visibility.Hidden;
                checkBoxMTPNoRw.Visibility = Visibility.Hidden;
                comboBoxMTPconfig.Visibility = Visibility.Hidden;
                labelMTPconfig.Visibility = Visibility.Hidden;
                checkBoxMTPNoRw.Visibility = Visibility.Hidden;
                for (int i = 0; i < LMMRegConfigs.regs667.Length; i++)
                {
                    if (sortString == "All")  // if((string)comboBoxSort.SelectedItem == "All") //if ((string)comboBoxSort.Items.CurrentItem == "All")
                        regList.Add(LMMRegConfigs.regs667[i]);
                    else if (LMMRegConfigs.regs667[i].Contains(sortString))
                    {
                        regList.Add(LMMRegConfigs.regs667[i]);
                    }
                }
            }
            else if ((comboBox66XDevices.SelectedItem.ToString().Contains("BF")) && (Globals.bothGenDevicesPresent))
            {
                for (int i = 0; i < LMMRegConfigs.regsBoth_BF.Length; i++)
                {
                    if (sortString == "All")  // if((string)comboBoxSort.SelectedItem == "All") //if ((string)comboBoxSort.Items.CurrentItem == "All")
                        regList.Add(LMMRegConfigs.regsBoth_BF[i]);
                    else if (LMMRegConfigs.regsBoth_BF[i].Contains(sortString))
                    {
                        regList.Add(LMMRegConfigs.regsBoth_BF[i]);
                    }
                }
            }

            //          comboBoxRegister.ItemsSource = comboItemsRegs;
            BindingOperations.ClearBinding(comboBoxRegister, ItemsControl.ItemsSourceProperty);
            comboBoxRegister.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = regList });

            if (sortString == "All")   // if ((string)comboBoxSort.SelectedItem == "All")
            {
                int regIndex = 0;
                if (comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegister);
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_0")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_1")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_4")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_5")))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_0_1_4_5);
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_2")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_6")))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_2_6);
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_3")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_7")))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_3_7);
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_8")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_9")))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_8_9);
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_A"))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_10);
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_B"))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_11);
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_C")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_D")))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_12_13);
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_E"))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_14);
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_F"))
                    regIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_15);

                if (regIndex < 0)
                    comboBoxRegister.SelectedIndex = 0;
                else
                {
                    if (comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegister);
                    else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_0")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_1")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_4")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_5")))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_0_1_4_5);
                    else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_2")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_6")))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_2_6);
                    else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_3")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_7")))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_3_7);
                    else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_8")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_9")))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_8_9);
                    else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_A"))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_10);
                    else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_B"))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_11);
                    else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_C")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_D")))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_12_13);
                    else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_E"))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_14);
                    else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_F"))
                        comboBoxRegister.SelectedIndex = comboBoxRegister.Items.IndexOf(Properties.Settings.Default.currentlySelectedRegisterMTP_15);
                }
            }
            else
                comboBoxRegister.SelectedIndex = 0;

            if (comboBoxRegister.SelectedValue != null)
                currentRegister = Convert.ToByte(((string)comboBoxRegister.SelectedValue).Substring(Math.Max(0, ((string)comboBoxRegister.SelectedValue).Length - 2)), 16);

            comboBoxRegister.SelectionChanged += ComboBoxRegister_SelectionChanged;
            ComboBoxRegister_SelectionChanged(this, null);

            comboBoxCommand.SelectionChanged -= ComboBoxCommand_SelectionChanged;
            ObservableCollection<string> comboItemsCommands = new ObservableCollection<string>();
            if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
            {
                for (int i = 0; i < LMMRegConfigs.cmds.Length; i++)
                    comboItemsCommands.Add(LMMRegConfigs.cmds[i]);
            }
            else
            {
                for (int i = 0; i < LMMRegConfigs.cmds664.Length; i++)
                    comboItemsCommands.Add(LMMRegConfigs.cmds664[i]);
            }
            comboBoxCommand.ItemsSource = comboItemsCommands;

            if (Properties.Settings.Default.currentCommand > comboBoxCommand.Items.Count)
                Properties.Settings.Default.currentCommand = 0;
            comboBoxCommand.SelectedIndex = Properties.Settings.Default.currentCommand;

            if (comboBoxCommand.SelectedValue != null)
            {
                currentCommand = Convert.ToByte(((string)comboBoxCommand.SelectedValue).Substring(Math.Max(0, ((string)comboBoxCommand.SelectedValue).Length - 2)), 16);
                comboBoxCommand.SelectionChanged += ComboBoxCommand_SelectionChanged;
                ComboBoxCommand_SelectionChanged(this, null);
            }
        }

        private void setPhase()
        {
            UInt16 newPhaseShift = UInt16.Parse(textBoxPhase.Text, CultureInfo.InvariantCulture);

            if ((newPhaseShift < 0) || (newPhaseShift > 1023))
            {
                MessageBox.Show("Please enter a DAC value of 0-1023", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BuckPhaseShift = lmmPhaseShift;
            }
            else
                BuckPhaseShift = newPhaseShift;  // Good value entered, save it
        }

        private void SliderPhase_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            checkBoxPhase.IsChecked = false;
            setPhase((UInt16)sliderPhase.Value);
        }

        private void setPhase(UInt16 valFromSlider)
        {
            textBoxPhase.Text = valFromSlider.ToString();

            BuckPhaseShift = valFromSlider;  // Good value entered, save it
        }

        public UInt16 BuckPhaseShift  // treating this like a full Buck since we are writing all the LEDs on the desired buck
        {
            get { return lmmPhaseShift; }
            set
            {
                lmmPhaseShift = value;
                textBoxPhase.Text = value.ToString();

                if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
                {
                    if (checkBoxAllChannel.IsChecked == true)
                        writeLMMphaseShift(lmmPhaseShift, BROADCAST_ID);
                    else
                        writeLMMphaseShift(lmmPhaseShift, currentLMMselected);
                }
                else
                {
                    if (checkBoxAllChannel.IsChecked == true)
                        writeLMMphaseShift664(lmmPhaseShift, BROADCAST_ID);
                    else
                        writeLMMphaseShift664(lmmPhaseShift, currentLMMselected);
                }

                Properties.Settings.Default.BuckPhaseShift = (byte)BuckPhaseShift;

                sliderPhase.ValueChanged -= SliderPhase_ValueChanged;
                sliderPhase.Value = BuckPhaseShift;
                sliderPhase.ValueChanged += SliderPhase_ValueChanged;
            }
        }

        private void parseMPHASE01LValues(UInt16[] tenBitVals, byte[] eightBitRegs)
        {
            if ((eightBitRegs.Length != 16) || (tenBitVals.Length != 12))
                MessageBox.Show("Error in parseMPHASE01LValues.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                eightBitRegs[0] = (byte)(tenBitVals[0] & 0x00FF);
                eightBitRegs[1] = (byte)(tenBitVals[1] & 0x00FF);
                eightBitRegs[2] = (byte)(tenBitVals[2] & 0x00FF);
                eightBitRegs[3] = 0x00;
                eightBitRegs[3] = (byte)(
                                        (((tenBitVals[0] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[1] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[2] & 0x0300) >> 8) << 4)
                                        );

                eightBitRegs[4] = (byte)(tenBitVals[3] & 0x00FF);
                eightBitRegs[5] = (byte)(tenBitVals[4] & 0x00FF);
                eightBitRegs[6] = (byte)(tenBitVals[5] & 0x00FF);
                eightBitRegs[7] = 0x00;
                eightBitRegs[7] = (byte)(
                                        (((tenBitVals[3] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[4] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[5] & 0x0300) >> 8) << 4)
                                        );

                eightBitRegs[8] = (byte)(tenBitVals[6] & 0x00FF);
                eightBitRegs[9] = (byte)(tenBitVals[7] & 0x00FF);
                eightBitRegs[10] = (byte)(tenBitVals[8] & 0x00FF);
                eightBitRegs[11] = 0x00;
                eightBitRegs[11] = (byte)(
                                        (((tenBitVals[6] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[7] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[8] & 0x0300) >> 8) << 4)
                                        );

                eightBitRegs[12] = (byte)(tenBitVals[9] & 0x00FF);
                eightBitRegs[13] = (byte)(tenBitVals[10] & 0x00FF);
                eightBitRegs[14] = (byte)(tenBitVals[11] & 0x00FF);
                eightBitRegs[15] = 0x00;
                eightBitRegs[15] = (byte)(
                                        (((tenBitVals[9] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[10] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[11] & 0x0300) >> 8) << 4)
                                        );
            }
        }

        public void writeLMMphaseShift(UInt16 phaseShiftValue, byte lmm)
        {
            UInt16 totalPhaseShift;
            UInt16[] phaseValues = new UInt16[12];
            byte[] registerValues = new byte[16];

            totalPhaseShift = 0;  // Get the desired phase shift and initialize the accumulator

            for (byte i = 0; i < 12; i++)  // Calculate the 10-bit phase values
            {
                phaseValues[i] = totalPhaseShift;  // Assign values
                totalPhaseShift += phaseShiftValue;  // Increment by selected amount
                if (totalPhaseShift > 1023)
                    totalPhaseShift -= 1024;  // Handle wrapping
            }

            parseMPHASE01LValues(phaseValues, registerValues);  // Parse from 10-bit storage to 8-bit register
            string status = "";
            Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE16, lmm, (byte)LMMRegConfigs.RegAddr.MPHASE01L, registerValues, ref status, false, (bool)checkBoxAckEnable.IsChecked);
            updateStatus(status + "\n");
        }

        private void parseMPHASE01LValues664(UInt16[] tenBitVals, byte[] eightBitRegs)
        {
            if ((eightBitRegs.Length != 20) || (tenBitVals.Length != 16))
                MessageBox.Show("Error in parseMPHASE01LValues664.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                eightBitRegs[0] = (byte)(tenBitVals[0] & 0x00FF);  // 0x3D
                eightBitRegs[1] = (byte)(tenBitVals[1] & 0x00FF);  // 0x3E
                eightBitRegs[2] = (byte)(tenBitVals[2] & 0x00FF);  // 0x3F
                eightBitRegs[3] = (byte)(tenBitVals[3] & 0x00FF);  // 0x40
                eightBitRegs[4] = 0x00;                            // 0x41
                eightBitRegs[4] = (byte)(
                                        (((tenBitVals[0] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[1] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[2] & 0x0300) >> 8) << 4) |
                                        (((tenBitVals[3] & 0x0300) >> 8) << 6)
                                        );

                eightBitRegs[5] = (byte)(tenBitVals[4] & 0x00FF);
                eightBitRegs[6] = (byte)(tenBitVals[5] & 0x00FF);
                eightBitRegs[7] = (byte)(tenBitVals[6] & 0x00FF);
                eightBitRegs[8] = (byte)(tenBitVals[7] & 0x00FF);
                eightBitRegs[9] = 0x00;
                eightBitRegs[9] = (byte)(
                                        (((tenBitVals[4] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[5] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[6] & 0x0300) >> 8) << 4) |
                                        (((tenBitVals[7] & 0x0300) >> 8) << 6)
                                        );

                eightBitRegs[10] = (byte)(tenBitVals[8] & 0x00FF);
                eightBitRegs[11] = (byte)(tenBitVals[9] & 0x00FF);
                eightBitRegs[12] = (byte)(tenBitVals[10] & 0x00FF);
                eightBitRegs[13] = (byte)(tenBitVals[11] & 0x00FF);
                eightBitRegs[14] = 0x00;
                eightBitRegs[14] = (byte)(
                                        (((tenBitVals[8] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[9] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[10] & 0x0300) >> 8) << 4) |
                                        (((tenBitVals[11] & 0x0300) >> 8) << 6)
                                        );

                eightBitRegs[15] = (byte)(tenBitVals[12] & 0x00FF);
                eightBitRegs[16] = (byte)(tenBitVals[13] & 0x00FF);
                eightBitRegs[17] = (byte)(tenBitVals[14] & 0x00FF);
                eightBitRegs[18] = (byte)(tenBitVals[15] & 0x00FF);
                eightBitRegs[19] = 0x00;
                eightBitRegs[19] = (byte)(
                                        (((tenBitVals[12] & 0x0300) >> 8) << 0) |
                                        (((tenBitVals[13] & 0x0300) >> 8) << 2) |
                                        (((tenBitVals[14] & 0x0300) >> 8) << 4) |
                                        (((tenBitVals[15] & 0x0300) >> 8) << 6)
                                        );
            }
        }

        public UInt16 LMMphase
        {
            get { return lmmDutyCycle; }
            set
            {
                lmmDutyCycle = value;
                sliderDutyCycle.Value = value;
                textBlockDutyCycle.Text = value.ToString();

                if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
                {
                    if (checkBoxAllChannel.IsChecked == true)
                        writeLMMwidth(BROADCAST_ID, value, (byte)LMMRegConfigs.RegAddr.MWIDTH01L);  // if all checked
                    else
                        writeLMMwidth(currentLMMselected, value, (byte)LMMRegConfigs.RegAddr.MWIDTH01L);
                }
                else
                {
                    if (checkBoxAllChannel.IsChecked == true)
                        writeLMMwidth664(BROADCAST_ID, value, (byte)LMMRegConfigs.RegAddr664.BANK_WIDTH01L);  // if all checked
                    else
                        writeLMMwidth664(currentLMMselected, value, (byte)LMMRegConfigs.RegAddr664.BANK_WIDTH01L);
                }

                Properties.Settings.Default.LMMdutyCycle = (byte)LMMdutyCycle;
            }
        }

        public void writeLMMphaseShift664(UInt16 phaseShiftValue, byte lmm)
        {
            UInt16 totalPhaseShift;
            UInt16[] phaseValues = new UInt16[16];
            byte[] registerValues = new byte[20];

            totalPhaseShift = 0;  // Get the desired phase shift and initialize the accumulator

            for (byte i = 0; i < 16; i++)  // Calculate the 10-bit phase values
            {
                phaseValues[i] = totalPhaseShift;  // Assign values
                totalPhaseShift += phaseShiftValue;  // Increment by selected amount
                if (totalPhaseShift > 1023)
                    totalPhaseShift -= 1024;  // Handle wrapping
            }

            parseMPHASE01LValues664(phaseValues, registerValues);  // Parse from 10-bit storage to 8-bit register
            string status = "";
            Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE20, lmm, (byte)LMMRegConfigs.RegAddr664.BANK_PHASE01L, registerValues, ref status, false, (bool)checkBoxAckEnable.IsChecked);
            updateStatus(status + "\n");
        }

        public UInt16 LMMdutyCycle
        {
            get { return lmmDutyCycle; }
            set
            {
                lmmDutyCycle = value;
                sliderDutyCycle.Value = value;
                textBlockDutyCycle.Text = value.ToString();

                if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
                {
                    if (checkBoxAllChannel.IsChecked == true)
                        writeLMMwidth(BROADCAST_ID, value, (byte)LMMRegConfigs.RegAddr.MWIDTH01L);  // if all checked
                    else
                        writeLMMwidth(currentLMMselected, value, (byte)LMMRegConfigs.RegAddr.MWIDTH01L);
                }
                else
                {
                    if (checkBoxAllChannel.IsChecked == true)
                        writeLMMwidth664(BROADCAST_ID, value, (byte)LMMRegConfigs.RegAddr664.BANK_WIDTH01L);  // if all checked
                    else
                        writeLMMwidth664(currentLMMselected, value, (byte)LMMRegConfigs.RegAddr664.BANK_WIDTH01L);
                }

                Properties.Settings.Default.LMMdutyCycle = (byte)LMMdutyCycle;
            }
        }

        private void SliderDutyCycle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LMMdutyCycle = (UInt16)sliderDutyCycle.Value;
        }

        public void writeLMMwidth(byte lmm, UInt16 widthValue, byte register)
        {
            try
            {
                UInt16[] dutyCycleValues = new UInt16[12];
                byte[] registerValues = new byte[16];

                for (byte i = 0; i < 12; i++)  // Calculate the 10-bit phase values
                    dutyCycleValues[i] = widthValue;  // Assign values
               
                parseMPHASE01LValues(dutyCycleValues, registerValues);  // Parse from 10-bit storage to 8-bit register
                string status = "";
                Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE16, lmm, register, registerValues, ref status, false, (bool)checkBoxAckEnable.IsChecked);
                updateStatus(status + "\n");
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LX: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ;  // if user shuts down the main window with a task running (example: TI logo) this will handle the exception
            }
        }

        public void writeLMMwidth664(byte lmm, UInt16 widthValue, byte register)
        {
            try
            {
                UInt16[] dutyCycleValues = new UInt16[16];
                byte[] registerValues = new byte[20];

                for (byte i = 0; i < 16; i++)  // Calculate the 10-bit phase values
                    dutyCycleValues[i] = widthValue;  // Assign values

                parseMPHASE01LValues664(dutyCycleValues, registerValues);  // Parse from 10-bit storage to 8-bit register
                string status = "";
                Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE20, lmm, register, registerValues, ref status, false, (bool)checkBoxAckEnable.IsChecked);
                updateStatus(status + "\n");
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LY: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ;  // if user shuts down the main window with a task running (example: TI logo) this will handle the exception
            }
        }

        private void ComboBox66XDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // possible BF situations
            if ((comboBox66XDevices.SelectedItem.ToString().Contains("BF")) && (comboBoxMTPconfig.SelectedValue.ToString().Contains("Mode")))  // use last loaded sort from whatever device that may be....
            {
                MessageBox.Show("Device \"BF\" not available while in MTP_Mode.", "Device Error", MessageBoxButton.OK, MessageBoxImage.Error);
                comboBox66XDevices.SelectionChanged -= ComboBox66XDevices_SelectionChanged;
                comboBox66XDevices.SelectedIndex = Properties.Settings.Default.comboBox66XDevices;
                comboBox66XDevices.SelectionChanged += ComboBox66XDevices_SelectionChanged;
                return;
            }

            // all other scenarios
            if (comboBox66XDevices.Items.Count > 0)
            {
                comboBoxSort.SelectionChanged -= ComboBoxSort_SelectionChanged;
                BindingOperations.ClearBinding(comboBoxSort, ItemsControl.ItemsSourceProperty);
                if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
                {
                    comboBoxMTPconfig.Foreground = Brushes.Gray;
                    comboBoxSort.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = sortList663 });
                    checkBoxPhase.Content = 85;
                    string[] tokens = comboBox66XDevices.SelectedItem.ToString().Split(' ');
                    currentLMMselected = byte.Parse(tokens[0], CultureInfo.InvariantCulture);
                    Properties.Settings.Default.comboBox66XDevices = (byte)comboBox66XDevices.SelectedIndex;
                }
                else if((lmmDeviceList[comboBox66XDevices.SelectedIndex].device > 0x95) && (!comboBox66XDevices.SelectedItem.ToString().Contains("BF")))  // BF has device value of 0xFF
                {
             //       comboBoxMTPconfig.Foreground = Brushes.Black;
                    groupBox664_.IsEnabled = true;
                    comboBoxSort.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = sortList664 });
                    checkBoxPhase.Content = 64;
                    string[] tokens = comboBox66XDevices.SelectedItem.ToString().Split(' ');
                    currentLMMselected = byte.Parse(tokens[0], CultureInfo.InvariantCulture);
                    Properties.Settings.Default.comboBox66XDevices = (byte)comboBox66XDevices.SelectedIndex;
                }
                else if ((comboBox66XDevices.SelectedItem.ToString().Contains("BF")) && (Globals.bothGenDevicesPresent))
                {
                    currentLMMselected = BROADCAST_ID;
                    comboBoxMTPconfig.Foreground = Brushes.Gray;
     //               groupBox664_.IsEnabled = false;
                    comboBoxSort.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = sortListBoth_BF });
                    currentLMMselected = BROADCAST_ID;  // do not save BF in settings
                }
                else if (comboBox66XDevices.SelectedItem.ToString().Contains("BF"))
                    currentLMMselected = BROADCAST_ID;

                comboBoxSort.SelectedIndex = 0;
                comboBoxSort.SelectionChanged += ComboBoxSort_SelectionChanged;
                loadComboBoxRegCmds("All");
            }
        }

        private void ComboBoxRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxRegister.SelectedItem != null)
            {
                currentRegister = Convert.ToByte(((string)comboBoxRegister.SelectedValue).Substring(Math.Max(0, ((string)comboBoxRegister.SelectedValue).Length - 2)), 16);

                if (comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"))
                    Properties.Settings.Default.currentlySelectedRegister = (string)comboBoxRegister.SelectedValue;
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_0")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_1")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_4")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_5")))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_0_1_4_5 = (string)comboBoxRegister.SelectedValue;
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_2")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_6")))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_2_6 = (string)comboBoxRegister.SelectedValue;
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_3")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_7")))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_3_7 = (string)comboBoxRegister.SelectedValue;
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_8")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_9")))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_8_9 = (string)comboBoxRegister.SelectedValue;
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_A"))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_10 = (string)comboBoxRegister.SelectedValue;
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_B"))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_11 = (string)comboBoxRegister.SelectedValue;
                else if ((comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_C")) || (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_D")))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_12_13 = (string)comboBoxRegister.SelectedValue;
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_E"))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_14 = (string)comboBoxRegister.SelectedValue;
                else if (comboBoxMTPconfig.SelectedItem.ToString().Contains("MTP_Mode_F"))
                    Properties.Settings.Default.currentlySelectedRegisterMTP_15 = (string)comboBoxRegister.SelectedValue;
            }
        }

        private void CheckBoxPhase_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxPhase.IsChecked == true)
            {
                textBoxPhase.IsEnabled = false;
                if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device < 0x96)
                    BuckPhaseShift = 85;
                else
                    BuckPhaseShift = 64;
            }
            else
                textBoxPhase.IsEnabled = true;

            Properties.Settings.Default.checkBoxPhase = (bool)checkBoxPhase.IsChecked;
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            string readStatus = "";
            if ((comboBoxCommand.SelectedValue.ToString().Contains("READ")) && (comboBox66XDevices.SelectedItem.ToString().Contains("BF")))
            {
                MessageBox.Show("Selected device of \"BF\" not supported during read operations.", "Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (comboBoxCommand.SelectedValue.ToString().Contains("READ"))  // read
            {
                try
                {
       //             if (runningWD)
         //               Globals.mcuCommand.stopUARTtimer(currentLMMselected);

                    byte tempReg = currentRegister;
                    if (textboxRegOverride.Text != "")
                        tempReg = byte.Parse(textboxRegOverride.Text.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    int result = Globals.mcuCommand.LmmRdReg((MCUcommands.FrameInit)currentCommand, currentLMMselected, tempReg, ref readStatus, !comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"), textBoxReadData);
                    if ((readStatus != "") || (readStatus != null))
                        updateStatus(readStatus + "\n");

                    if (readStatus.Contains("failed"))
                    {
                        for (int i = 1; i < 6; i++)  // 5 retries
                        {
                            result = Globals.mcuCommand.LmmRdReg((MCUcommands.FrameInit)currentCommand, currentLMMselected, tempReg, ref readStatus, !comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"), textBoxReadData);
                            if ((readStatus != "") || (readStatus != null))
                                updateStatus(readStatus + " Retry: " + i + "\n");

                            if (!readStatus.Contains("failed"))
                                break;
                        }
                    }

           //         if (runningWD)
             //           Globals.mcuCommand.startUARTtimer(currentLMMselected);
                }
                catch (Exception ex)
                {
#if FAIL_ANAL
                    textboxRegOverride.Text = "";
                    MessageBox.Show("FA error on Read: " + ex.Message, "Send read Error", MessageBoxButton.OK, MessageBoxImage.Error);
#else
                    MessageBox.Show("Error executing Read: " + ex.Message, "Read Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }
            }
            else  // write
            {
                try
                {
                    byte[] dataWrite = new byte[32];
                    if (textBoxWriteData.Text != "")
                    {
                        string[] s = textBoxWriteData.Text.Split(',', ' ');
                        for (int i = 0; i < s.Count(); i++)
                            dataWrite[i] = byte.Parse(s[i].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    byte tempReg = currentRegister;
                    if (textboxRegOverride.Text != "")
                        tempReg = byte.Parse(textboxRegOverride.Text.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                    Globals.mcuCommand.LmmWrReg((MCUcommands.FrameInit)currentCommand, currentLMMselected, tempReg, dataWrite, ref readStatus, !comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"), (bool)checkBoxAckEnable.IsChecked, (bool)checkBoxbadCRC.IsChecked);
                }
                catch (Exception ex)
                {
                    textBoxWriteData.Text = "";
                    textboxRegOverride.Text = "";
#if FAIL_ANAL
                    MessageBox.Show("FA error on Write: " + ex.Message, "Send write Error", MessageBoxButton.OK, MessageBoxImage.Error);
#else
                    MessageBox.Show("Error processing hex byte values. Values to be seperated by \",\" or space with or without \"0x\" prefix. i.e. \"0x01,0xFF...\" or \"01,FF...\" \"1 ff...\" etc.", "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }

                if ((readStatus != "") || (readStatus != null))
                {
                    updateStatus(readStatus + "\n");
                }
            }         
        }

        private void ComboBoxCommand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxCommand.SelectedItem != null)
            {
                currentCommand = Convert.ToByte(((string)comboBoxCommand.SelectedValue).Substring(Math.Max(0, ((string)comboBoxCommand.SelectedValue).Length - 2)), 16);
                Properties.Settings.Default.currentCommand = comboBoxCommand.SelectedIndex;
            }
        }

        private void CheckBoxAllChannel_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.checkBoxAllChannel = (bool)checkBoxAllChannel.IsChecked;

            if (checkBoxAllChannel.IsChecked == true)
            {
                BuckPhaseShift = lmmPhaseShift;
                LMMdutyCycle = lmmDutyCycle;
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxOutput.Clear();
        }

        private void TextBoxPhase_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBoxPhase.LostFocus -= TextBoxPhase_LostFocus;  // remove event
                setPhase();
                textBoxPhase.LostFocus += TextBoxPhase_LostFocus;  // add it back
            }
        }

        private void TextBoxPhase_LostFocus(object sender, RoutedEventArgs e)
        {
            setPhase();
        }

        private void ButtonLMMcommsReset_Click(object sender, RoutedEventArgs e)
        {
            Globals.mcuCommand.lmmCommsReset();  // bus must be reset if we ping a non existant part. LMM bus will tri state
        }

        bool regMap664Open = false;
        bool regMap665Open = false;
        bool regMap667Open = false;
        private void ButtonRegMap_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            regMap = new RegMap(mainWindow, this, lmmDeviceList[0].device, lmmDeviceList[0].address, lmmDeviceList);  // 662A debug
            regMap.Show();
            regMap.Closed += RegMap_Closed;
            buttonRegMap.IsEnabled = false;
#else
            try
            {
                if ((regMap664Open == false) && (regMap664 == null) && (lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x96))
                {
                    regMap664 = new RegMap(mainWindow, this, lmmDeviceList[comboBox66XDevices.SelectedIndex].device, lmmDeviceList[comboBox66XDevices.SelectedIndex].address, lmmDeviceList);
                    regMap664.Show();
                    regMap664.Closed += RegMap664_Closed;
                    regMap664Open = true;
                    //              buttonRegMap.IsEnabled = false;
                }
                else if ((regMap665Open == false) && (regMap665 == null) && (lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x97))
                {
                    regMap665 = new RegMap(mainWindow, this, lmmDeviceList[comboBox66XDevices.SelectedIndex].device, lmmDeviceList[comboBox66XDevices.SelectedIndex].address, lmmDeviceList);
                    regMap665.Show();
                    regMap665.Closed += RegMap665_Closed;
                    regMap665Open = true;
                    //              buttonRegMap.IsEnabled = false;
                }
                else if ((regMap667Open == false) && (regMap667 == null) && (lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x98))
                {
                    regMap667 = new RegMap(mainWindow, this, lmmDeviceList[comboBox66XDevices.SelectedIndex].device, lmmDeviceList[comboBox66XDevices.SelectedIndex].address, lmmDeviceList);
                    regMap667.Show();
                    regMap667.Closed += RegMap667_Closed;
                    regMap667Open = true;
                    //              buttonRegMap.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening the Register Map tool: " + ex.Message, "Reg Map Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#endif
        }

        private void RegMap664_Closed(object sender, EventArgs e)
        {
            regMap664 = null;
            regMap664Open = false;
            //           buttonRegMap.IsEnabled = true;
        }

        private void RegMap665_Closed(object sender, EventArgs e)
        {
            regMap665 = null;
            regMap665Open = false;
            //           buttonRegMap.IsEnabled = true;
        }

        private void RegMap667_Closed(object sender, EventArgs e)
        {
            regMap667 = null;
            regMap667Open = false;
            //           buttonRegMap.IsEnabled = true;
        }

        private void ButtonPixel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pix == null)
                {
                    pix = new Pixel(mainWindow, this);
                    pix.Show();
                    pix.Closed += PixMap_Closed;
                    buttonPixel.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening the Pixel Map tool: " + ex.Message, "Pixel Map Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PixMap_Closed(object sender, EventArgs e)
        {
            pix = null;
            buttonPixel.IsEnabled = true;
        }

        private void ComboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        //    Properties.Settings.Default.comboBoxSort = (byte)comboBoxSort.SelectedIndex;
            if(groupBoxRegister.IsEnabled == true)
                loadComboBoxRegCmds((string)comboBoxSort.SelectedItem);
     //       string test = comboBoxSort.SelectedItem.ToString();
     //     System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(comboBoxSort.SelectedItem.ToString());
        }

        byte deviceRunningWatchdog;
        uint timerVal = 0;
        private void ButtonWatchDog_Click(object sender, RoutedEventArgs e)
        {
            if (buttonWatchDog.Content.ToString() == "Watchdog")
            {
                string returnString = "";
                byte[] valueDec = new byte[1];
                //Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTPCFG, ref returnString, false, null);
                //updateStatus(returnString + "\n");
                //if (returnString.Contains("Data:"))
                //{
                //    int indexOfData = returnString.IndexOf("Data:");
                //    string sub = returnString.Substring(indexOfData + 8, 2);
                //    valueDec[0] = (byte)(Convert.ToInt32(sub, 16) | 0x20);  // set the FS pin
                //    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTPCFG, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
                //}

                // set CMWTAP LMM clock = 16 / (1 / baud); @ baud 1000000 one clock LMM = .0000000625; setting tap to 7 = .0000000625 * 2^23 = .524288 secs
     //           valueDec = new byte[1] { (byte)comboBoxCMWTAP.SelectedIndex };
       //         Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.CMWTAP, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
         //       updateStatus(returnString + "\n");

                //valueDec = new byte[1] { (byte)0x09 };
                //Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.SYSCFG, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
                //updateStatus(returnString + "\n");

                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.SYSCFG, ref returnString, false, null);
                updateStatus(returnString + "\n");
                if (returnString.Contains("Data:"))
                {
                    int indexOfData = returnString.IndexOf("Data:");
                    string sub = returnString.Substring(indexOfData + 8, 2);
                    if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device > 0x94)
                        valueDec = new byte[1] { (byte)(Convert.ToInt32(sub, 16) | 0x0A) };  // parse to decimal number
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.SYSCFG, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);  // enable watchdog
                    updateStatus(returnString + "\n");
                }

                try
                {
                    if (uint.TryParse(textBoxWriteDelay.Text, out timerVal))
                    {
                        if (timerVal < 1)
                        {
                            timerVal = 1;
                            textBoxWriteDelay.Text = "1";
                        }
                        else if (timerVal > 1000)
                        {
                            timerVal = 1000;
                            textBoxWriteDelay.Text = "1000";
                        }

                        int status = mainWindow.ac.configTimerUART(timerVal * 1000);  // timer for LMM watchdog

                        if (status < 0)
                        {
                            MessageBox.Show("Timer FW set failed...", "Reg Map Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            mainWindow.ac.disableUART();
                            mainWindow.ac.enableUART();
                            mainWindow.ac.configUART(Properties.Settings.Default.baudRate, false);
                        }
                        else
                        {
                            Globals.mcuCommand.startUARTtimer(currentLMMselected);
                            deviceRunningWatchdog = currentLMMselected;

                            buttonWatchDog.Content = "Stop WD";
                            Globals.watchdogActive = true;
                            Properties.Settings.Default.textBoxWriteDelay = textBoxWriteDelay.Text;
                            textBoxWriteDelay.IsEnabled = false;
                            labelWDtime.Foreground = Brushes.Gray;
                            labelWriteDelay.Foreground = Brushes.Gray;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid value entered for WD Timer...\nPlease enter a value form 1 to 1000 ms.", "WD Timer Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        textBoxWriteDelay.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    Globals.watchdogActive = false;
                    mainWindow.ac.disableUART();
                    mainWindow.ac.enableUART();
                    mainWindow.ac.configUART(Properties.Settings.Default.baudRate, false);
                    MessageBox.Show("Exception encountered during Write delay setup: " + ex.Message, "Write Delay Setup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                buttonWatchDog.Content = "Watchdog";
                try
                {
                    Globals.mcuCommand.stopUARTtimer(currentLMMselected);
                    int status = Globals.mcuCommand.closeUARTtimer(2);  // sends close on timer 2
                    mainWindow.ac.disableUART();
                    mainWindow.ac.enableUART();
                    mainWindow.ac.configUART(Properties.Settings.Default.baudRate, false);
                    Globals.watchdogActive = false;

                    textBoxWriteDelay.IsEnabled = true;
                    labelWDtime.Foreground = Brushes.Black;
                    labelWriteDelay.Foreground = Brushes.Black;

                    string returnString = "";
                    byte[] valueDec = new byte[1];
                    Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.SYSCFG, ref returnString, false, null);
                    updateStatus(returnString + "\n");
                    if (returnString.Contains("Data:"))
                    {
                        int indexOfData = returnString.IndexOf("Data:");
                        string sub = returnString.Substring(indexOfData + 8, 2);
                        if (lmmDeviceList[comboBox66XDevices.SelectedIndex].device > 0x94)
                            valueDec = new byte[1] { (byte)(Convert.ToInt32(sub, 16) & 0xF5) };  // parse to decimal number
                        Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.SYSCFG, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);  // enable watchdog
                        updateStatus(returnString + "\n");
                    }
                }
                catch (Exception ex)
                {
                    Globals.watchdogActive = false;
                    MessageBox.Show("Exception encountered during Watchdog stop: " + ex.Message, "Watchdog Stop Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        private void ButtonTestMode664_Click(object sender, RoutedEventArgs e)
        {
#if (CSTMR && FAIL_ANAL)
            byte[] testModeBytes = new byte[3] {0x87, 0x99, 0x1E };

            string returnString = "";
            Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE3, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.TEST_MODE1, testModeBytes, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
           updateStatus(returnString + "\n");
#endif
        }

        private void ButtonProgMTP_Click(object sender, RoutedEventArgs e)
        {
#if CSTMR
            if ((lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x96) && (comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE")))
            {
                byte[] testModeBytes = new byte[4] { 0xCA, 0x23, 0x35, 0x24 };

                string returnString = "";
                Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE4, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTP_PROG1, testModeBytes, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
                updateStatus(returnString + "\n");
            }
#endif
        }

        private void ButtonRestoreVol_Click(object sender, RoutedEventArgs e)
        {
#if CSTMR
            if ((lmmDeviceList[comboBox66XDevices.SelectedIndex].device == 0x96) && (comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE")))
            {
                byte[] testModeBytes = new byte[4] { 0x1B, 0xA5, 0xFB, 0x09 };

                string returnString = "";
                Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE4, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTP_PROG1, testModeBytes, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
                updateStatus(returnString + "\n");
            }
#endif
        }

        private void ComboBoxMTPconfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBox66XDevices.SelectedItem.ToString().Contains("BF"))  // use last loaded sort from whatever device that may be....
                {
                    MessageBox.Show("MTP mode not accessible when device \"BF\" selected.", "MTP access error", MessageBoxButton.OK, MessageBoxImage.Error);
                    comboBoxMTPconfig.SelectionChanged -= ComboBoxMTPconfig_SelectionChanged;
                    comboBoxMTPconfig.SelectedIndex = 0;
                    comboBoxMTPconfig.SelectionChanged += ComboBoxMTPconfig_SelectionChanged;
                    return;
                }

                if (comboBoxMTPconfig.SelectedItem.ToString().Contains("VOLATILE"))
                {
                    buttonProgMTP.IsEnabled = true;
                    buttonRestoreVol.IsEnabled = true;
                    ComboBox66XDevices_SelectionChanged(this, null);
                }
                else
                {
                    string returnString = "";
                    byte[] valueDec = new byte[1];
                    if (!(bool)checkBoxMTPNoRw.IsChecked)
                    {
                        Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTPCFG, ref returnString, false, null);
                        updateStatus(returnString + "\n");
                    }
                    else
                        updateStatus("No MTP read of register 0x00...\n");

                    if ((returnString.Contains("Data:")) || ((bool)checkBoxMTPNoRw.IsChecked))
                    {
                        string sub = "";
                        if (!(bool)checkBoxMTPNoRw.IsChecked)
                        {
                            int indexOfData = returnString.IndexOf("Data:");
                            sub = returnString.Substring(indexOfData + 8, 2);
                        }

                        if (!(bool)checkBoxMTPNoRw.IsChecked)
                        {
                            byte mtpModeSelected = byte.Parse(comboBoxMTPconfig.SelectedItem.ToString().Substring(9, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                            valueDec[0] = (byte)(Convert.ToInt32(sub, 16) & 0xF0);  // clear it
                            valueDec[0] |= mtpModeSelected;
                            Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTPCFG, valueDec, ref returnString, true, (bool)checkBoxAckEnable.IsChecked);
                            updateStatus(returnString + "\n");
                        }
                        else
                            updateStatus("No MTP write of register 0x00...\n");

                        buttonProgMTP.IsEnabled = false;
                        buttonRestoreVol.IsEnabled = false;
                        comboBoxSort.SelectionChanged -= ComboBoxSort_SelectionChanged;
                        BindingOperations.ClearBinding(comboBoxSort, ItemsControl.ItemsSourceProperty);
                        comboBoxSort.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = sortList664MTP_0_1_4_5 });
                        comboBoxSort.SelectedIndex = 0;
                        comboBoxSort.SelectionChanged += ComboBoxSort_SelectionChanged;
                        loadComboBoxRegCmds("All");

                        updateStatus("MTP configuraion " + comboBoxMTPconfig.SelectedItem.ToString() + " loaded successfully.\n");
                        Properties.Settings.Default.mtpConfigModes = comboBoxMTPconfig.SelectedIndex;
                    }
                    else
                    {
                        Properties.Settings.Default.mtpConfigModes = 0;
                        updateStatus("Error configuring MTP. No MTP configuration loaded.\n");
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LZ: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
     
        private void ButtonFSBF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((comboBox66XDevices.SelectedItem != null) && (comboBox66XDevices.SelectedItem.ToString().Contains("BF"))) // use last loaded sort from whatever device that may be....
                {
                    MessageBox.Show("Set FS is not supported when device \"BF\" is selected.", "Set FS Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string returnString = "";
                byte[] valueDec = new byte[1];
                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTPCFG, ref returnString, false, null);
                updateStatus(returnString + "\n");
                if (returnString.Contains("Data:"))
                {
                    int indexOfData = returnString.IndexOf("Data:");
                    string sub = returnString.Substring(indexOfData + 8, 2);

                    valueDec[0] = (byte)(Convert.ToInt32(sub, 16) | 0x60);
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTPCFG, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
                    updateStatus(returnString + "\n");        
                }
                if (returnString != null)
                    updateStatus(returnString + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during device FS init...", "FS Initialize Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RadioButton500Kbps_Checked(object sender, RoutedEventArgs e)
        {
            int numBytesInReadBuffer = mainWindow.ac.controlUART(2, 0);

            if (numBytesInReadBuffer > 0)
                mainWindow.ac.readUART((ushort)numBytesInReadBuffer, Globals.uartBuf);

            Properties.Settings.Default.baudRate = 500000;
            Properties.Settings.Default.Save();
            Globals.mcuCommand.setBaud(500000);
        }

        private void RadioButton1Mbps_Checked(object sender, RoutedEventArgs e)
        {
            int numBytesInReadBuffer = mainWindow.ac.controlUART(2, 0);

            if (numBytesInReadBuffer > 0)
                mainWindow.ac.readUART((ushort)numBytesInReadBuffer, Globals.uartBuf);

            Properties.Settings.Default.baudRate = 1000000;
            Properties.Settings.Default.Save();
            Globals.mcuCommand.setBaud(1000000);
        }

        private void CheckBoxNoBusWalk_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.checkBoxNoBusWalk = (bool)checkBoxNoBusWalk.IsChecked;
        }

        public bool ackOn = false;
        private void CheckBoxAckEnable_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxAckEnable.IsChecked == false)
                return;
            try
            {
                string returnString = "";
                byte[] valueDec = new byte[1];
                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.SYSCFG, ref returnString, false, null);
                updateStatus(returnString + "\n");
                int indexOfData = returnString.IndexOf("Data:");
                string sub = returnString.Substring(indexOfData + 8, 2);
                if (returnString.Contains("Data:"))
                {
                    if ((bool)checkBoxAckEnable.IsChecked)
                        valueDec[0] = (byte)(Convert.ToInt32(sub, 16) | 0x20);
                    else
                        valueDec[0] = (byte)(Convert.ToInt32(sub, 16) & 0xDF);

                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, BROADCAST_ID, (byte)LMMRegConfigs.RegAddr664.SYSCFG, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
                    updateStatus(returnString + "\n");
                }
                else
                {
                    Properties.Settings.Default.checkBoxAckEnable = false;
                    updateStatus("Error configuring ACK. ACK not enabled.\n");
                    return;
                }

                Properties.Settings.Default.checkBoxAckEnable = (bool)checkBoxAckEnable.IsChecked;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Error LM1: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void ButtonResetReg0_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get out of standalone.... 
                string returnString = "";
                byte[] valueDec = new byte[1] { 0x00 };
                Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.MTPCFG, valueDec, ref returnString, false, (bool)checkBoxAckEnable.IsChecked);
                updateStatus(returnString + "\n");

                ButtonFSBF_Click(this, null);  // FS this device
            }
            catch (Exception ex)
            {
            }
        }

        public void ButtonErrors_Click(object sender, RoutedEventArgs e)
        {
            string returnString = "";

            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.CLK_SYNC, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 1)
                    updateStatus("INFO: CLK_SYNC Register: 0x84(132), Value: 1, Field: SSYNC, Re-initialize the internal PWM counter (TCNT) to zero.\nThis bit will always read back as 0. A broadcast write to this register may be used to synchronize all LMMs on the same UART bus to each other.\nThen programmed phase shifts are all referenced to a common starting time (within the tolerance of the UART bus timing and state machine processing the input).\n");
                if ((Globals.uartBuf[0] & 0x02) == 0)
                    updateStatus("INFO: CLK_SYNC Register: 0x84(132), Value: 0, Field: CLK_IS_EXT, Internal clock oscillator is currently being used as the system clock.\n");
                else
                    updateStatus("INFO: CLK_SYNC Register: 0x84(132), Value: 1, Field: CLK_IS_EXT, An external clock source on CLK_H and CLK_L is currently being used as the system clock.\n");
                if ((Globals.uartBuf[0] & 0x04) == 4)
                    updateStatus("FAULT: CLK_SYNC Register: 0x84(132), Value: 1, Field: VDD_OV_FLT, VDD has crossed the VDD_OV limit.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.STATUS, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 0x01)
                    updateStatus("\nSTATUS: STATUS Register: 0x85(133), Value: 1, Field: PWR, Bit was written to 1 and has not been cleared by a power cycle.\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: PWR, Bit has not been written to 1, or bit has been cleared by writing to 0 or a power cycle.\n");

                if ((Globals.uartBuf[0] & 0x02) == 0x02)
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 1, Field: MTP_ERR, An MTP error has occurred (one or more CRCs did not match expected values). If this occurs all channel switch output states will change to that defined by the FS pin.\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: MTP_ERR, No MTP error has occurred (the CRCs all matched expected values).\n");

                if ((Globals.uartBuf[0] & 0x04) == 0x04)
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 1, Field: TW, A thermal warning has occurred.\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: TW, No thermal warning has occurred.\n");

                if ((Globals.uartBuf[0] & 0x08) == 0x08)
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 1, Field: TRIM_ERR, A trim error has occurred (the CRC did not match the expected value).\nIf this occurs all channel switch output states will change to that defined by the FS pin.\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: TRIM_ERR, No trim error has occurred (the CRC matched the expected value).\n");

                if ((Globals.uartBuf[0] & 0x10) == 0x10)
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 1, Field: LIMP_HOME, Limp Home Mode is active. The CMW timer has expired and the logic is using DEFWIDTH and the programmed PHASE.\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: LIMP_HOME, Limp Home Mode is not active.\n");

                if ((Globals.uartBuf[0] & 0x20) == 0x20)
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 1, Field: VLED_ERR, An LED Voltage error has occurred (the ADC readings were outside of LEDONTH/LEDOFFTH limits)\nPoll VLEDON and VLEDOFF registers to identify the exact problem channel.\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: VLED_ERR, No LED Voltage error has occurred (the ADC readings were within LEDONTH/LEDOFFTH limits).\n");

                if ((Globals.uartBuf[0] & 0x40) == 0x40)
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 1, Field: CHPMP_ERR, A charge pump output voltage error has occurred.\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: CHPMP_ERR, No charge pump output voltage error has occurred.\n");

                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 1, Field: PWM_ERR, A PWM error has occurred (if this bit is set, the PWM_MISCOUNT registers can be read in order to diagnose which channel(s) had duty cycle errors).\n");
                else
                    updateStatus("STATUS: STATUS Register: 0x85(133), Value: 0, Field: PWM_ERR, No PWM error has occurred.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.OUTCTRL, ref returnString, false, null);
            updateStatus("\n");
            byte drv_chk = 0;
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                {
                    updateStatus("INFO: OUTCTRL Register: 0x01(1), Value: 1, Field: DRV_CHK, LED Driver Health Check operation. Sets sw_fet_hi_z output to 1. Disconnects the LED gate drivers from their corresponding FETs.\nGates of FETs are passively pulled down. In this health check mode the “OV FAULT” represents the status of the channel gate.\n(i.e. when OV_FAULT is high, the gate of the output FET is pulled low) The re-mapped OV signal may be thought of as 'Gate pulldown is active'.\n");
                    drv_chk = 1;
                }
                else
                    updateStatus("INFO: OUTCTRL Register: 0x01(1), Value: 0, Field: DRV_CHK, Normal operation. Sets sw_fet_hi_z output to 0.\nDoes not disconnect the LED gate drivers from the corresponding FETs.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.FLT_OPEN_OR_DRVL, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if (drv_chk == 0)
                {
                    if ((Globals.uartBuf[0] & 0x01) == 0x01)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 1, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 1, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x02) == 0x02)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 2, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 2, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x04) == 0x04)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 3, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 3, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x08) == 0x08)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 4, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 4, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x10) == 0x10)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 5, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 5, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x20) == 0x20)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 6, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 6, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x40) == 0x40)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 7, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 7, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x80) == 0x80)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 8, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 8, An LED open fault has not occurred.\n");
                }
                else
                {
                    if ((Globals.uartBuf[0] & 0x01) == 0x01)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 1, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 1, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x02) == 0x02)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 2, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 2, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x04) == 0x04)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 3, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 3, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x08) == 0x08)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 4, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 4, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x10) == 0x10)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 5, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 5, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x20) == 0x20)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 6, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 6, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x40) == 0x40)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 7, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 7, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x80) == 0x80)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel 8, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 0, Field: FAULT_OPEN_OR_DRVL, Channel 8, LED driver output is low.\n");
                }
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.FLT_OPEN_OR_DRVH, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if (drv_chk == 0)
                {
                    if ((Globals.uartBuf[0] & 0x01) == 0x01)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 9, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 9, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x02) == 0x02)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 10, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 10, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x04) == 0x04)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 11, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 11, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x08) == 0x08)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 12, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 12, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x10) == 0x10)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 13, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 13, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x20) == 0x20)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 14, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVL Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 14, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x40) == 0x40)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 15, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 15, An LED open fault has not occurred.\n");

                    if ((Globals.uartBuf[0] & 0x80) == 0x80)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 16, An LED open fault has occurred.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 16, An LED open fault has not occurred.\n");
                }
                else
                {
                    if ((Globals.uartBuf[0] & 0x01) == 0x01)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 9, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 9, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x02) == 0x02)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 10, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 10, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x04) == 0x04)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 11, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 11, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x08) == 0x08)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 12, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 12, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x10) == 0x10)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 13, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 13, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x20) == 0x20)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 14, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 14, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x40) == 0x40)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 15, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 15, LED driver output is low.\n");

                    if ((Globals.uartBuf[0] & 0x80) == 0x80)
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRV, Channel 16, LED driver output is high.\n");
                    else
                        updateStatus("FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 0, Field: FAULT_OPEN_OR_DRV, Channel 16, LED driver output is low.\n");
                }
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.FAULT_SHORTL, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 0x01)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 1, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 1, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x02) == 0x02)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 2, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 2, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x04) == 0x04)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 3, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 3, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x08) == 0x08)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 4, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 4, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x10) == 0x10)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 5, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 5, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x20) == 0x20)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 6, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 6, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x40) == 0x40)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 7, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 7, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel 8, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTL Register: 0x88(136), Value: 0, Field: FAULT_SHORT, Channel 8, An LED short fault has not occurred.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.FAULT_SHORTH, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 0x01)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 9, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 9, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x02) == 0x02)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 10, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 10, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x04) == 0x04)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 11, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 11, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x08) == 0x08)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 12, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 12, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x10) == 0x10)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 13, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 13, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x20) == 0x20)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 14, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 14, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x40) == 0x40)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 15, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 15, An LED short fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel 16, An LED short fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_SHORTH Register: 0x89(137), Value: 0, Field: FAULT_SHORT, Channel 16, An LED short fault has not occurred.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.FAULT_RESFETL, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 0x01)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 1, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 1, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x02) == 0x02)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 2, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 2, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x04) == 0x04)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 3, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 3, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x08) == 0x08)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 4, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 4, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x10) == 0x10)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 5, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 5, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x20) == 0x20)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 6, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 6, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x40) == 0x40)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 7, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 7, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 1, Field: FAULT_RESFET, Channel 8, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETL Register: 0x8A(138), Value: 0, Field: FAULT_RESFET, Channel 8, A resistive FET fault has not occurred.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.FAULT_RESFETH, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 0x01)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 9, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 9, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x02) == 0x02)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 10, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 10, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x04) == 0x04)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 11, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 11, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x08) == 0x08)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 12, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 12, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x10) == 0x10)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 13, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 13, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x20) == 0x20)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 14, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 14, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x40) == 0x40)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 15, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 15, A resistive FET fault has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 1, Field: FAULT_RESFET, Channel 16, A resistive FET fault has occurred.\n");
                else
                    updateStatus("FAULT: FAULT_RESFETH Register: 0x8B(139), Value: 0, Field: FAULT_RESFET, Channel 16, A resistive FET fault has not occurred.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.CERRCNT, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if (Globals.uartBuf[0] > 0)
                    updateStatus("FAULT: CERRCNT Register: 0x8C(140), Value: 1, Field: CERRCNT, 0x" + Globals.uartBuf[0].ToString("X2") + "(" + Globals.uartBuf[0] + ")" + " CRC errors have occurred.\n");
                else
                    updateStatus("FAULT: CERRCNT Register: 0x8C(140), Value: 0, Field: CERRCNT, no CRC errors have occurred.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.PWM_MISCOUNTL, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 0x01)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 1, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 1, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x02) == 0x02)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 2, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 2, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x04) == 0x04)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 3, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 3, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x08) == 0x08)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 4, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 4, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x10) == 0x10)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 5, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 5, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x20) == 0x20)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 6, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 6, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x40) == 0x40)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 7, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 7, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 1, Field: PWM_MISCOUNT, Channel 8, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTL Register: 0xB1(177), Value: 0, Field: PWM_MISCOUNT, Channel 8, A PWM miscount has not occurred.\n");
            }

            updateStatus("\n");
            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, currentLMMselected, (byte)LMMRegConfigs.RegAddr664.PWM_MISCOUNTH, ref returnString, false, null);
            updateStatus(returnString + "\n");
            if (returnString.Contains("Data:"))
            {
                if ((Globals.uartBuf[0] & 0x01) == 0x01)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 9, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 9, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x02) == 0x02)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 10, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 10, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x04) == 0x04)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 11, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 11, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x08) == 0x08)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 12, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 12, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x10) == 0x10)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 13, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 13, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x20) == 0x20)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 14, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 14, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x40) == 0x40)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 15, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 15, A PWM miscount has not occurred.\n");

                if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 1, Field: PWM_MISCOUNT, Channel 16, a PWM miscount has occurred.\n");
                else
                    updateStatus("FAULT: PWM_MISCOUNTH Register: 0xB2(178), Value: 0, Field: PWM_MISCOUNT, Channel 16, A PWM miscount has not occurred.\n");
            }

            textBoxOutput.ScrollToEnd();
        }

        private void ButtonClearReg_Click(object sender, RoutedEventArgs e)
        {
            textboxRegOverride.Clear();
        }
    }
}

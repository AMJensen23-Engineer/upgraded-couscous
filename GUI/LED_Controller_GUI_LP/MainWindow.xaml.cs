using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.IO.Ports;
using System.Configuration;
using System.Data;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Net.Http;
using System.Globalization;

namespace TPS9266xEvaluationModule
{
    internal static class UsbNotification
    {
        public const int DbtDevicearrival = 0x8000; // system detected a new device        
        public const int DbtDeviceremovecomplete = 0x8004; // device is gone      
        public const int WmDevicechange = 0x0219; // device change event      
        private const int DbtDevtypDeviceinterface = 5;
        private static readonly Guid GuidDevinterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices
        private static IntPtr notificationHandle;

        /// <summary>
        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            DevBroadcastDeviceinterface dbi = new DevBroadcastDeviceinterface
            {
                DeviceType = DbtDevtypDeviceinterface,
                Reserved = 0,
                ClassGuid = GuidDevinterfaceUSBDevice,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);

            notificationHandle = RegisterDeviceNotification(windowHandle, buffer, 0);
        }

        /// <summary>
        /// Unregisters the window for USB device notifications
        /// </summary>
        public static void UnregisterUsbDeviceNotification()
        {
            UnregisterDeviceNotification(notificationHandle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceinterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }
    }
    public partial class MainWindow : Window
    {
        public AcCtrl ac;
        public SPIcommand spiC;
        public bool currentlyConnected;       
        public bool usbRemoved = false;
        public MCUControl mcuc;

        private TPS92682Control tps682_0;
        private TPS92520Control tps520_1;
        private TPS92520Control tps520_2;
        private TPS92662Control tps66x;
        private bool closinApp = false;
        private IntPtr windowHandle;
        private DataTable dt;

        public MainWindow()
        {
            // CHECK here for language issues....
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");    // this fixes the double.Parse exception in some languages; Maybe will fix other language issues
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");  // another potential fix is double.Parse(string, new CultureInfo("en-US")) or double.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);;
                                                                                   // Not sure if float.Parse(string, new CultureInfo("en-US")) is affected (just floating point issue?)
            InitializeComponent();

            string path = AppDomain.CurrentDomain.BaseDirectory + @"Updater";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            ac = new AcCtrl(this);

            Globals.mcuCommand = new MCUcommands(this);  // MCU object;

      //      IndividualTabSettings savedTabControlSettings = TabControlSettings.SelectedTab("Tab1");

            mcuc = new MCUControl(this);  // this will start the PWMs
            gridMCUcontrol.Children.Add(mcuc);
            groupBoxMCUcontrol.Content = gridMCUcontrol;

            spiC = new SPIcommand(this);
            gridSPIcommand.Children.Add(spiC);
            groupBoxSPIcommand.Content = gridSPIcommand;

            currentlyConnected = getConnected();
            if (currentlyConnected)
            {

                tps682_0 = new TPS92682Control(this, 0, null, true);

                tps520_1 = new TPS92520Control(2, null, true, 1, this);
                grid520_1.Children.Add(tps520_1);
                groupBox520_1.Content = grid520_1;
         //       tps520_1.groupBoxChannel1.Header = "Channel 2";
           //     tps520_1.groupBoxChannel2.Header = "Channel 1";

                tps520_2 = new TPS92520Control(1, null, true, 1, this);
                grid520_2.Children.Add(tps520_2);
                groupBox520_2.Content = grid520_2;
          //      tps520_2.groupBoxChannel1.Header = "Channel 2";
            //    tps520_2.groupBoxChannel2.Header = "Channel 1";

                tps66x = new TPS92662Control(this);
                gridDevices.Children.Add(tps66x);
                groupBoxDevices.Content = gridDevices;
            
   //             if (Properties.Settings.Default.setMeUp)
     //               Globals.userNumDevices = 0;
       //         else
         //           Globals.userNumDevices = Properties.Settings.Default.comboBoxNumDevices;

   //             dt = new DataTable();  // set up data table
     //           dt.Columns.Add("Device", typeof(string));
       //         dt.Columns.Add("Address", typeof(byte));

                guiAgoGo(true);
   //             initcomboBoxWatchDogTimer();

  //              bool lastPWMSelection = Properties.Settings.Default.radioButtonPWM1;
    //            if (lastPWMSelection ? false : true)
      //              mcuc.radioButtonPWM1.IsChecked = true;
        //        else
          //          mcuc.radioButtonPWM2.IsChecked = true;
            //    if (lastPWMSelection)
              //      mcuc.radioButtonPWM1.IsChecked = true;
                //else
                  //  mcuc.radioButtonPWM2.IsChecked = true;

                buttonStopWD.Visibility = Visibility.Hidden;
            }
            else
            {
#if DEBUG
                MessageBox.Show("Error AS1", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                //        MessageBox.Show(this, "EVM initialization error.\nApplication shutting down.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
            }
        }

        private void setHardwareConfig()
        {
            SetupOptionsWindow0 setupStep0 = new SetupOptionsWindow0(dt);
            setupStep0.ShowDialog();
            setupStep0.Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Adds the windows message processing hook and registers USB device add/removal notification.
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source != null)
            {
                windowHandle = source.Handle;
                source.AddHook(HwndHandler);
                UsbNotification.RegisterUsbDeviceNotification(windowHandle);
            }
        }

        /// <summary>
        /// Method that receives window messages.
        /// </summary>
        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if ((msg == UsbNotification.WmDevicechange) && (!Globals.updateMode))
            {
                switch ((int)wparam)
                {
                    //       case 4:  // into sleep or lock; not firing?
                    //         ;
                    //       break;
                    //      case 7:  // back from sleep or lock....
#if DEBUG
                             MessageBox.Show("Error AS2", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    //       Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                    //        break;
                    case UsbNotification.DbtDeviceremovecomplete:
                        try
                        {
                            if (!usbRemoved && !ac.findController(false))
                            {
#if DEBUG
                                MessageBox.Show("Error AS3", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                                Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                            }
                        }
                        catch (Exception ex)
                        {
                            ;
#if DEBUG
                            MessageBox.Show(this, "Error L1: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                        }
                        break;
                    case UsbNotification.DbtDevicearrival:
                        break;
                }
            }

            handled = false;
            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Globals.userNumDevices > 0)
            {
#if !DEBUG
                if (!Globals.updateMode && !usbRemoved && !ac.findController(false))
                {
                    ac.stopTimerLPP(0);
                    ac.stopTimerLPP(1);
                    ac.setGPIOpin(Globals.enablePin, false);
                }
#endif
                closinApp = true;
                Properties.Settings.Default.comboBoxNumDevices = Globals.userNumDevices;
                saveAllCustomSettings();
                Properties.Settings.Default.setMeUpHolder = Properties.Settings.Default.setMeUp;
                Properties.Settings.Default.savedTab = tabControlDevices.SelectedIndex;
                Properties.Settings.Default.Save();
            }
        }

        private void saveAllCustomSettings()
        {
            int savedIndex = tabControlDevices.SelectedIndex;
            for (int i = 0; i < tabControlDevices.Items.Count; i++)
            {
                tabControlDevices.SelectedIndex = i;
                tabControlDevices.UpdateLayout();

                if ((tabControlDevices.SelectedItem as TabItem).Header.ToString().Contains("518") == true)
                {
                    var ctrl = tabControlDevices.SelectedContent as TPS92518Control;
                    ctrl.saveCustomSettings((byte)i);
                }
                else if ((tabControlDevices.SelectedItem as TabItem).Header.ToString().Contains("520") == true)
                {
                    var ctrl = tabControlDevices.SelectedContent as TPS92520Control;
                    if (ctrl.limpModeActive)  // good time to shut down limp mode window if open
                        ctrl.limpModeWindow(false);

                    ctrl.saveCustomSettings((byte)i);
                }
                else if ((tabControlDevices.SelectedItem as TabItem).Header.ToString().Contains("682") == true)
                {
                    var ctrl = tabControlDevices.SelectedContent as TPS92682Control;
                    ctrl.saveCustomSettings((byte)i);
                }
            }
            tabControlDevices.SelectedIndex = savedIndex;
        }

        private bool getConnected()
        {
#if DEBUG
            return true;
#else
            try
            {
                if (!ac.getDLLrev())
                    return false;

                if (!ac.findController(true))
                    return false;

                int port = ac.findComPort();
                if (port < 1)
                    return false;
                if (ac.findHandle(port) < 1)
                    return false;

                byte fwTest = ac.getFWrev();
                if (fwTest != Globals.PASS)
                {
                    if (fwTest == Globals.FETCH_FW_ERROR)
                    {
                        MessageBox.Show(this, "Error fetching firmware version, Application shutting down...", "Firmware Fetch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    else if (fwTest == Globals.UPGRADE_AVAILABLE)
                    {
                        string stringFormatting = "0";
                        var totalBS = Globals.MIN_FW_VERSION;
                        if (totalBS > 9.9)
                            stringFormatting = "00";

                        var x = Globals.MIN_FW_VERSION - Math.Truncate(Globals.MIN_FW_VERSION);
                        if (x > 9)
                            stringFormatting += ".00";
                        else if (x > 99)
                            stringFormatting += ".000";
                        else
                            stringFormatting += ".0";

                        var Result = MessageBox.Show(this, "Firmware version " + Globals.fwVersion + " is below the minimum acceptable revision of " + Globals.MIN_FW_VERSION.ToString(stringFormatting, CultureInfo.InvariantCulture) + ".\n\nWould you like to check for all available updates?", "Firmware Revision Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (Result == MessageBoxResult.Yes)
                        {
                            MenuItem_Update_Click(this, null);
                            return false;
                        }
                        else
                        {
                            MessageBox.Show(this, "Application update(s) declined. Application shutting down...", "Processor Fetch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                    else if (fwTest == Globals.FETCH_PRO_ERROR)
                    {
                        MessageBox.Show(this, "Error fetching processor type, Application shutting down...", "Processor Fetch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                int status = ac.initGPIO("PP2", true, 1);  // SS 2; 520
                if (status < 0)
                    return false;

                status = ac.initGPIO("PN2", true, 1);  // SS 0; 682
                if (status < 0)
                    return false;

                status = ac.initGPIO("PN3", true, 1);  // SS 1; 520
                if (status < 0)
                    return false;

                status = ac.initGPIO("PP4", true, 2);  // Pin to determine old and new long board, this line pulled high = new long board
                if (status < 0)
                    return false;
                status = ac.initGPIO("PP5", true, 2);  // fault pin U4 IC address 2
                if (status < 0)
                    return false;
                status = ac.initGPIO("PH2", true, 2);  // fault pin U5 IC address 0
                if (status < 0)
                    return false;

                bool? pinStatus = ac.currentGPIOpinState("PP4");  // if PP4 is high; new 520 marketing board
                if (pinStatus == null)
                    return false;
                if ((bool)pinStatus)  // if PP4 is high; new 520 marketing board
                {
                    Globals.enablePin = Globals.enablePinNew;
                    status = ac.initGPIO("PH0", true, 1);  // Boost 1 682
                    if (status < 0)
                        return false;
                    status = ac.initGPIO("PH1", true, 1);  // Boost 2 682
                    if (status < 0)
                        return false;
                }
                else                  // old 520 marketing board
                {
                    Globals.enablePin = Globals.enablePinOld;
                    status = ac.initGPIO("PK4", true, 1);  // Boost 1 682
                    if (status < 0)
                        return false;
                    status = ac.initGPIO("PK5", true, 1);  // Boost 2 682
                    if (status < 0)
                        return false;
                }
                status = ac.initGPIO(Globals.enablePin, false, 1);
                if (status < 0)
                    return false;

                selectSlave(0);

                status = ac.enableSPI(ac.handle);
                if (status < 0)
                    return false;
                status = ac.configSPI(ac.handle);
                if (status < 0)
                    return false;

                status = ac.enableLPP();  // LPP interface
                if (status < 0)
                    return false;
                status = ac.configLPP(1);  // 1 = (520 - 682)
                if (status < 0)
                    return false;
                //                status = ac.configTimerLPP(1000000);
                //              if (status < 0)
                //                return false;

                status = ac.enableUART();
                if (status < 0)
                    return false;

                status = ac.configUART(Properties.Settings.Default.baudRate, true);
                if (status < 0)
                    return false;

       //         status = ac.configTimerUART(400000);  // timer for LMM watchdog
         //       if (status < 0)
           //         return false;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error in getConnected(): " + ex.Message, "LaunchPad Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
#endif
        }
        public void updateStatus(string statusText)
        {
            try
            {
                textBoxSPIcommandStatus.AppendText(statusText);
                //       textBoxSPIcommandStatus.Focus();
                textBoxSPIcommandStatus.CaretIndex = textBoxSPIcommandStatus.Text.Length;
                textBoxSPIcommandStatus.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occurred during status update: " + ex.Message, "Update Status Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void guiAgoGo(bool splashIt)
        {
            if (splashIt)  // if user disconnects USB after we're running then reconnects
            {
                //if (Properties.Settings.Default.setMeUp)  // hardware set up
                //    setHardwareConfig();
                //else
                //    Globals.userSelectedEVM = Properties.Settings.Default.userSelectedEVM;

                //this.Title = Globals.EVMS[Globals.userSelectedEVM];

                SplashScreenAll splash = new SplashScreenAll();
                splash.ShowDialog();  // show will allow the rest of the app to load during the splash...
                splashIt = false;

                //if (!addControls())
                //    Application.Current.Shutdown();
                //else
                //{
                //    Properties.Settings.Default.setMeUp = Properties.Settings.Default.setMeUpHolder;  // if user checked check box not to run setup at start
                //    setMeUpVals(Properties.Settings.Default.setMeUp);
                //}
            }
        }

        public void selectSlave(int selectedSlave)
        {
            ac.setStateGPIO("PP2", true);
            ac.setStateGPIO("PN2", true);
            ac.setStateGPIO("PN3", true);
            if (selectedSlave == 0)
                ac.chipSelect = 1;
            else if (selectedSlave == 1)
                ac.chipSelect = 2;
            else if (selectedSlave == 2)
                ac.chipSelect = 4;
        }

        public void all520NoWatchdog()  // launchpad
        {
            int savedIndex = tabControlDevices.SelectedIndex;

  //          ac.stopTimerLPP(0);
            ac.setGPIOpin(Globals.enablePin, false);
            ac.setGPIOpin(Globals.enablePin, true);

            selectSlave(1);
            tps520_1.readRegStatusReg3();
            tps520_1.readRegStatusReg3();
            tps520_1.disable520Watchdog();
            tps520_1.initChannelsDevice1();
            tps520_1.initChannelsDevice2();

            selectSlave(2);
            tps520_2.readRegStatusReg3();
            tps520_2.readRegStatusReg3();
            tps520_2.disable520Watchdog();
            tps520_2.initChannelsDevice1();
            tps520_2.initChannelsDevice2();

            readAll520();
            selectSlave(0);
            tps682_0.buttonInit520_Click(this, null);
        }

        public void watchdog520NoPowerCycle(bool powerCycle)
        {
            int savedIndex = tabControlDevices.SelectedIndex;

            if (powerCycle)
            {
                ac.stopTimerLPP(0);
                ac.setGPIOpin(Globals.enablePin, false);
                ac.setGPIOpin(Globals.enablePin, true);
            }

            for (int i = 0; i < tabControlDevices.Items.Count; i++)  // do a read all to update devices
            {
                tabControlDevices.SelectedIndex = i;
                tabControlDevices.UpdateLayout();

                if ((tabControlDevices.SelectedItem as TabItem).Header.ToString().Contains("520") == true)
                {
                    var ctrl = tabControlDevices.SelectedContent as TPS92520Control;
                    ctrl.enable520Watchdog();
                }
            }
            readAll520();
            ac.startTimerLPP(0);
            tabControlDevices.SelectedIndex = savedIndex;
        }

        private void readAll520()
        {
#if CSTMR
    //        selectSlave(0);
      //      tps682_0.buttonReadAll_Click(this, null);
            selectSlave(1);
            tps520_1.buttonReadAll_Click(this, null);
            selectSlave(2);
            tps520_2.buttonReadAll_Click(this, null);
#else
            int savedIndex = tabControlDevices.SelectedIndex;
            for (int i = 0; i < tabControlDevices.Items.Count; i++)  // do a read all to update devices
            {
                tabControlDevices.SelectedIndex = i;
                tabControlDevices.UpdateLayout();

                if ((tabControlDevices.SelectedItem as TabItem).Header.ToString().Contains("520") == true)
                {
                    var ctrl = tabControlDevices.SelectedContent as TPS92520Control;
                    ctrl.buttonReadAll_Click(this, null);
                }
                else if ((tabControlDevices.SelectedItem as TabItem).Header.ToString().Contains("682") == true)
                {
                    var ctrl = tabControlDevices.SelectedContent as TPS92682Control;
                    ctrl.buttonReadAll_Click(this, null);
                }
            }
            tabControlDevices.SelectedIndex = savedIndex;
            if (watchdog)
                comboBoxWatchDogTimer_SelectionChanged(this, null);
#endif
        }

        private bool find520()
        {
            string device;
            var TabControlSettings = ConfigurationManager.GetSection("TabControlSettings") as TPS9266xEvaluationModule.TabControlSettings;

            for (int i = 0; i < Globals.userNumDevices; i++)
            {
                IndividualTabSettings savedTabControlSettings = TabControlSettings.SelectedTab("Tab" + i);

                if (dt.Rows.Count > 0)  // user set config at startup
                    device = dt.Rows[i][0].ToString();
                else
                    device = savedTabControlSettings.Device;

                if (device == "520")
                    return true;
            }

            return false;
        }

        private bool addControls()
        {
            var TabControlSettings = ConfigurationManager.GetSection("TabControlSettings") as TPS9266xEvaluationModule.TabControlSettings;

            if (TabControlSettings == null)
            {
                MessageBox.Show(this, "Missing or invalid Tab Control settings\nRestart application to run hardware setup...", "Initialize Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Properties.Settings.Default.setMeUp = true;  // force a initial hardware setup.... user may have moved the app to a new folder
                return false;
            }
            else
            {
                string device;
                int address;
                bool restoreTab = false;

                bool found520 = find520();
                if (!found520)
                {
                    ac.setStateGPIO(Globals.enablePin, true);
                    button520EnableNoWatchdogDevice.Visibility = Visibility.Hidden;
                    button520EnableWatchdogDevice.Visibility = Visibility.Hidden;
                }

                for (int i = 0; i < Globals.userNumDevices; i++)
                {
                    IndividualTabSettings savedTabControlSettings = TabControlSettings.SelectedTab("Tab" + i);

                    if (dt.Rows.Count > 0)  // user set config at startup
                    {
                        savedTabControlSettings.currentlySelectedTab += i.ToString(CultureInfo.InvariantCulture);
                        savedTabControlSettings.Device = device = dt.Rows[i][0].ToString();
                        savedTabControlSettings.Address = address = int.Parse(dt.Rows[i][1].ToString(), CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        device = savedTabControlSettings.Device;
                        address = savedTabControlSettings.Address;
                        restoreTab = true;
                    }

                    if (device == "518")
                    {
                        TabItem newTabItem = new TabItem { Header = device + " Addr " + address.ToString(), Name = "_518" };
                        newTabItem.Content = new TPS92518Control((byte)address, savedTabControlSettings, Properties.Settings.Default.setMeUp);
                        tabControlDevices.Items.Add(newTabItem);
                    }
                    else if (device == "520")
                    {
                        TabItem newTabItem = new TabItem { Header = device + " Addr " + address.ToString(), Name = "_520" };
                        newTabItem.Content = new TPS92520Control((byte)address, savedTabControlSettings, Properties.Settings.Default.setMeUp, i, this);
                        tabControlDevices.Items.Add(newTabItem);
                    }
                    else if (device == "682")
                    {
                        TabItem newTabItem = new TabItem { Header = device + " Addr " + address.ToString(), Name = "_682" };
                        newTabItem.Content = new TPS92682Control(this, (byte)address, savedTabControlSettings, Properties.Settings.Default.setMeUp);
                        tabControlDevices.Items.Add(newTabItem);
                    }
                }

                if (Globals.EVMS[Globals.userSelectedEVM].Contains("TPS9266X"))  // 520, 682, 662 EVM selected
                {
                    TabItem newTabItem = new TabItem { Header = "66(X)", Name = "_662" };
                    newTabItem.Content = new TPS92662Control(this);
                    tabControlDevices.Items.Add(newTabItem);
                }

                if (restoreTab)
                    tabControlDevices.SelectedIndex = Properties.Settings.Default.savedTab;

                return true;
            }
        }

        private void setMeUpVals(bool val)
        {
            if (val == false)
            {
                Properties.Settings.Default.setMeUp = val;
                menuItemHardwareSetup.Header = "_Run Hardware Setup at start";
            }
            else
            {
                Properties.Settings.Default.setMeUp = val;
                menuItemHardwareSetup.Header = "_Do not run Hardware Setup at start";
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!currentlyConnected)
            {
                MessageBox.Show(this, "No comms connection. Check power and connections.", "MCU Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (sender.ToString().Contains("Run"))
                setMeUpVals(true);
            else if (sender.ToString().Contains("Do"))
                setMeUpVals(false);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            AboutAll abt = new AboutAll();
            abt.ShowDialog();
        }

        private void tabControlDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!closinApp)
            {
                Properties.Settings.Default.savedTab = tabControlDevices.SelectedIndex;

                var TabControlSettings = ConfigurationManager.GetSection("TabControlSettings") as TPS9266xEvaluationModule.TabControlSettings;
                IndividualTabSettings currentlySelectedTab = TabControlSettings.SelectedTab("Tab" + tabControlDevices.SelectedIndex);
                if (currentlySelectedTab != null)
                {
                    selectSlave(currentlySelectedTab.Address);
                    spiC.comboBoxAddress.SelectedIndex = currentlySelectedTab.Address;
                    if (currentlySelectedTab.Device.Contains("518"))
                        spiC.radioButton518.IsChecked = true;
                    else if (currentlySelectedTab.Device.Contains("520"))
                        spiC.radioButton520.IsChecked = true;
                    else if (currentlySelectedTab.Device.Contains("682"))
                        spiC.radioButton682.IsChecked = true;
                }
            }
        }

        private void button520EnableNoWatchdogDevice_Click(object sender, RoutedEventArgs e)
        {
            Cursor _previousCursor;
            _previousCursor = Mouse.OverrideCursor;
            BrushConverter bc = new BrushConverter();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (!noWatchdog)
                {
                    if (watchdog)
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Please press the \"En 520 W WD\" button to stop the Watchdog timer before enabling 520(s) with no watchdog.", "Enable Infornmtion", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    all520NoWatchdog();
                    button520EnableNoWatchdogDevice.Background = Brushes.ForestGreen;
                    button520EnableNoWatchdogDevice.Foreground = Brushes.White;
                    button520EnableNoWatchdogDevice.Content = "Power Down";
                    button520EnableWatchdogDevice.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
                    noWatchdog = true;
                    watchdog = false;
                }
                else
                {
                    ac.setGPIOpin(Globals.enablePin, false);
                    button520EnableNoWatchdogDevice.Content = "Power Up";
                    button520EnableNoWatchdogDevice.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
                    button520EnableNoWatchdogDevice.Foreground = Brushes.Black;
                    noWatchdog = false;
                }

                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error L2: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                button520EnableNoWatchdogDevice.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
                button520EnableNoWatchdogDevice.Foreground = Brushes.Black;
                Mouse.OverrideCursor = _previousCursor;
            }
        }

        bool noWatchdog = false;
        bool watchdog = false;

        private void button520EnableWatchdogDevice_Click(object sender, RoutedEventArgs e)
        {
            BrushConverter bc = new BrushConverter();
            if (!watchdog)
            {
                if (noWatchdog)
                    watchdog520NoPowerCycle(false);
                else
                    watchdog520NoPowerCycle(true);
                buttonStopWD.Content = "Stop WD";
                buttonStopWD.Visibility = Visibility.Visible;
                button520EnableWatchdogDevice.Background = Brushes.Green;
                button520EnableNoWatchdogDevice.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
                watchdog = true;
                noWatchdog = false;
            }
            else
            {
                buttonStopWD.Visibility = Visibility.Hidden;
                ac.setGPIOpin(Globals.enablePin, false);
                ac.stopTimerLPP(0);
                button520EnableWatchdogDevice.Background = (Brush)bc.ConvertFrom("#FFDDDDDD");
                watchdog = false;
            }
        }

        private void initcomboBoxWatchDogTimer()
        {
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
        }

        private void comboBoxWatchDogTimer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int savedIndex = tabControlDevices.SelectedIndex;

            for (int i = 0; i < tabControlDevices.Items.Count; i++)
            {
                tabControlDevices.SelectedIndex = i;
                tabControlDevices.UpdateLayout();

                if ((tabControlDevices.SelectedItem as TabItem).Header.ToString().Contains("520") == true)
                {
                    var ctrl = tabControlDevices.SelectedContent as TPS92520Control;
                    ctrl.comboBoxWatchDogTimer_SelectionChanged((byte)comboBoxWatchDogTimer.SelectedIndex);
                }
            }
            tabControlDevices.SelectedIndex = savedIndex;           
        }

        private void buttonStopWD_Click(object sender, RoutedEventArgs e)
        {
            if (buttonStopWD.Content.ToString().Contains("Stop"))
            {
                ac.stopTimerLPP(0);
                buttonStopWD.Content = "Start WD";
                groupBoxMCUcontrol.IsEnabled = true;
                groupBoxSPIcommand.IsEnabled = true;
                groupBoxDevices.IsEnabled = true;
            }
            else
            {
                ac.startTimerLPP(0);
                buttonStopWD.Content = "Stop WD";
                groupBoxMCUcontrol.IsEnabled = false;
                groupBoxSPIcommand.IsEnabled = false;
                groupBoxDevices.IsEnabled = false;
            }
        }

        private void MenuItem_Update_Click(object sender, RoutedEventArgs e)
        {

     //       return;
//#if !UPDATER_TEST
  //          if (Globals.swVersion.Contains("Beta"))
    //        {
      //          MessageBox.Show(this, "Beta version GUI: " + Globals.swVersion + ". Updates to Beta version GUIs are not supported...", "Update Information", MessageBoxButton.OK, MessageBoxImage.Information);
        //        return;
          //  }
//#endif

            if (MessageBox.Show(this, "WARNING!!!\nYou are about to access the internet!\n\nBy pressing \"Yes\" you acknowledge and knowingly accept the potentially serious risks of accessing the Internet!\n\nWould you like to continue to check for updates for this application by accesssing the Internet?", "Update Information", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            Cursor _previousCursor;
            _previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                int responseTimeout = 15000;

                WebClient client = new WebClient();
#if !UPDATER_TEST
          //      Stream stream = client.OpenRead("https://software-dl.ti.com/evm-gui/LPP/LP/LP.txt");
                Stream stream = client.OpenRead("https://software-dl.ti.com/evm-gui/LPP/RTM66x/RTM66x.txt");
#else
                Stream stream = client.OpenRead("https://software-dl.ti.com/evm-gui/LPP/Test_Only/LP/LP.txt");  // Used only for test of new
#endif
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///search for this, if there is a language issue: CHECK here for language issues
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                StreamReader reader = new StreamReader(stream);
                float gui = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
                float fw = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
                float dll = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

   //             Stream stream2 = client.OpenRead("https://software-dl.ti.com/evm-gui/LPP/Test_Only/LP/Test.txt");  // Used only for test of new
     //           StreamReader reader2 = new StreamReader(stream2);
       //         string test = reader2.ReadLine();

                string thisGuiRev = Globals.swVersion;
                if (Globals.swVersion.Contains("Beta"))
                    thisGuiRev = Globals.swVersion.Split()[0];

                if ((gui > float.Parse(thisGuiRev, CultureInfo.InvariantCulture)) || (fw > float.Parse(Globals.fwVersion, CultureInfo.InvariantCulture)) || (dll > float.Parse(Globals.dllVersion, CultureInfo.InvariantCulture)))
                {
                    if (Directory.Exists(@".\update"))
                        Directory.Delete(@".\update", true);
                    Directory.CreateDirectory("update");
                }
                else
                {
                    Mouse.OverrideCursor = _previousCursor;
                    MessageBox.Show(this, "Congratulations!!! No update(s) needed.\nAll application files are up to date.", "Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

#if !UPDATER_TEST
                string source = "https://software-dl.ti.com/evm-gui/LPP/LP/Updater.zip";
#else
                string source = "https://software-dl.ti.com/evm-gui/LPP/Test_Only/LP/Updater.zip";
#endif
                string targ = @".\update\Updater.zip";

                var wreq = WebRequest.Create(source);
                wreq.Timeout = responseTimeout;
                var wresp = (HttpWebResponse)wreq.GetResponse();  // see if all is good web wise

                using (Stream file = File.OpenWrite(targ))
                    wresp.GetResponseStream().CopyTo(file);

                wresp.Close();
                if (File.Exists(targ))
                    System.IO.Compression.ZipFile.ExtractToDirectory(targ, Directory.GetCurrentDirectory());
                else
                    MessageBox.Show(this, "Error extracting Updater app, please try again.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (gui <= float.Parse(thisGuiRev, CultureInfo.InvariantCulture))
                    gui = 0;
                else
                {
#if !UPDATER_TEST
                    source = "https://software-dl.ti.com/evm-gui/LPP/RTM66x/app.zip";
#else
                    source = "https://software-dl.ti.com/evm-gui/LPP/Test_Only/LP/app.zip";
#endif
                    targ = @".\update\app.zip";

                    wreq = WebRequest.Create(source);
                    wreq.Timeout = responseTimeout;
                    wresp = (HttpWebResponse)wreq.GetResponse();  // see if all is good web wise

                    using (Stream file = File.OpenWrite(targ))
                        wresp.GetResponseStream().CopyTo(file);

                    wresp.Close();
                    if (File.Exists(targ))
                        ZipFile.ExtractToDirectory(targ, @".\update");
                    else
                    {
                        MessageBox.Show(this, "Error extracting application file, please try again.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        gui = 0;
                    }
                }

                if (fw <= float.Parse(Globals.fwVersion, CultureInfo.InvariantCulture))
                    fw = 0;
                else
                {
#if !UPDATER_TEST
                    source = "https://software-dl.ti.com/evm-gui/LPP/LP/Bootloader_Firmware.zip";
#else
                    source = "https://software-dl.ti.com/evm-gui/LPP/Test_Only/LP/Bootloader_Firmware.zip";
#endif
                    targ = @".\update\Bootloader_Firmware.zip";

                    wreq = WebRequest.Create(source);
                    wreq.Timeout = responseTimeout;
                    wresp = (HttpWebResponse)wreq.GetResponse();  // see if all is good web wise

                    using (Stream file = File.OpenWrite(targ))
                        wresp.GetResponseStream().CopyTo(file);

                    wresp.Close();
                    if (File.Exists(targ))
                        ZipFile.ExtractToDirectory(targ, @".\update");
                    else
                    {
                        MessageBox.Show(this, "Error extracting firmware file, please try again.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        gui = 0;
                    }
                }

                if (dll <= float.Parse(Globals.dllVersion, CultureInfo.InvariantCulture))
                    dll = 0;
                else
                {
#if !UPDATER_TEST
                    source = "https://software-dl.ti.com/evm-gui/LPP/LP/acctrl.zip";
#else
                    source = "https://software-dl.ti.com/evm-gui/LPP/Test_Only/LP/acctrl.zip";
#endif
                    targ = @".\update\acctrl.zip";

                    wreq = WebRequest.Create(source);
                    wreq.Timeout = responseTimeout;
                    wresp = (HttpWebResponse)wreq.GetResponse();  // see if all is good web wise

                    using (Stream file = File.OpenWrite(targ))
                        wresp.GetResponseStream().CopyTo(file);

                    wresp.Close();
                    if (File.Exists(targ))
                        ZipFile.ExtractToDirectory(targ, @".\update");
                    else
                    {
                        MessageBox.Show(this, "Error extracting dll file, please try again.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        gui = 0;
                    }
                }

                Globals.updateMode = true;  // stop bail out when user puts device in bootloader mode
                var Result = MessageBoxResult.No;
                if (fw > float.Parse(Globals.fwVersion, CultureInfo.InvariantCulture))
                    Result = MessageBox.Show(this, "Updates found!\n\nIn order to proceed the device must be in BootLoader mode.\nTo enter Bootloader mode place a shorting jumper on J15 then press \"RESET_SW1\" (after button has been pushed wait 3 seconds in order for device driver to load).\n\nPress the \"Yes\" button once the device is in BootLoader mode or \"No\" to exit update.", "Update Information", MessageBoxButton.YesNo, MessageBoxImage.Question);
                else
                    Result = MessageBox.Show(this, "Updates found!\nPress the \"Yes\" button to continue or \"No\" to exit update.", "Update Information", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (Result == MessageBoxResult.Yes)
                {
                    var p = new System.Diagnostics.Process();
                    string curPath = Directory.GetCurrentDirectory() + @"\Updater";
                    string updatePath = '"' + Directory.GetCurrentDirectory() + '"';
                    p.StartInfo.FileName = curPath + @"\Updater.exe";

                    // to test, just copy and paste this into debug command line arguments in  
                    // the project properties for the "Updater" applcation, remove outside quotes 
                    p.StartInfo.Arguments = @"/p " + updatePath + " /s TPS9266xEvaluationModule" + " /g " + gui.ToString() + " /f " + fw.ToString() + " /d " + dll.ToString() + " /m " + Globals.processor;

                    p.Start();
                    Mouse.OverrideCursor = _previousCursor;
#if DEBUG
                    MessageBox.Show("Error AS4", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                }
                else
                {
                    if (File.Exists(@"updater.exe"))
                        File.Delete(@"updater.exe");

                    if (Directory.Exists(@".\update"))
                        Directory.Delete(@".\update", true);
                }
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = _previousCursor;
                MessageBox.Show(this, "Update Error: " + ex.Message, "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static async Task<string> DownloadFileAsync(string url, string output, TimeSpan timeout)
        {
            using (var wcl = new WebClient())
            {
                wcl.Credentials = CredentialCache.DefaultNetworkCredentials;
                DateTime? lastReceived = null;
                wcl.DownloadProgressChanged += (o, e) =>
                {
                    lastReceived = DateTime.Now;
                };
                var download = wcl.DownloadFileTaskAsync(new Uri(url), output);
                // await two tasks - download and delay, whichever completes first
                // do that until download fails, completes, or timeout expires
                while (lastReceived == null || DateTime.Now - lastReceived < timeout)
                {
                    await Task.WhenAny(Task.Delay(10000), download); // wait 10 seconds
                    if (download.IsCompleted)
                        break;
                }
                var exception = download.Exception; // need to observe exception, if any
                bool cancelled = !download.IsCompleted && exception == null;

                // download is not completed yet, nor it is failed - cancel
                if (cancelled)
                    wcl.CancelAsync();

                if (cancelled || exception != null)
                {
                    // delete partially downloaded file if any (note - need to do with retry, might not work with a first try, because CancelAsync is not immediate)
                    int fails = 0;
                    while (true)
                    {
                        try
                        {
                            File.Delete(output);
                            break;
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            MessageBox.Show("Error L3: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                            fails++;
                            if (fails >= 10)
                                break;

                            await Task.Delay(1000);
                            return "Failed to download update file, connection to server was lost.";
                        }
                    }
                }
                if (exception != null)  // throw new Exception("Failed to download file", exception);
                    return "Failed to download update file. " + exception.InnerException.Message;

                if (cancelled)
                    return "Failed to download update file (timeout reached: {timeout} seconds).";

                return "";
            }
        }

        private void ButtonClearStatus_Click(object sender, RoutedEventArgs e)
        {
            textBoxSPIcommandStatus.Text = String.Empty;  // clear status
        }
    }
}

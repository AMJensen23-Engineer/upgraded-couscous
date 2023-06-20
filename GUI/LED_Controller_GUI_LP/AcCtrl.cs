using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace TPS9266xEvaluationModule
{
    public class AcCtrl
    {
        [DllImport("acctrl.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int LPP_dllRevision(byte[] revVal);
        [DllImport("acctrl.dll")]
        private static extern int Sys_FindACControllers();
        [DllImport("acctrl.dll")]
        private static extern int Sys_GetComPortNumber(byte ACControllerIndex);
        [DllImport("acctrl.dll")]
        private static extern int Sys_Open(int port);
        [DllImport("acctrl.dll")]
        private static extern int Sys_Close(int port);
        [DllImport("acctrl.dll")]
        public static extern int Sys_GetStatusText(int statusCode, StringBuilder textBuf);
        [DllImport("acctrl.dll")]
        public static extern int SPI_Enable(int handle, byte unit, bool enable);
        [DllImport("acctrl.dll")]
        public static extern int SPI_Config(int handle, byte unit, ref SPI_CFG p_spi_cfg);
        [DllImport("acctrl.dll")]
        public static extern int SPI_WriteAndRead(int handle, byte unit, UInt32 cs_GPIO, UInt16 bytes_to_write, byte[] p_write_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int GPIO_Enable(int handle, UInt32 pin_mask, bool enable);
        [DllImport("acctrl.dll")]
        public static extern int GPIO_Config(int handle, UInt32 pin_mask, byte mode);
        [DllImport("acctrl.dll")]
        public static extern int GPIO_Write(int handle, UInt32 pin_mask, UInt32 values);
        [DllImport("acctrl.dll")]
        public static extern int GPIO_Read(int handle, UInt32 pin_mask, UInt32[] pvalues);
        [DllImport("acctrl.dll")]
        public static extern int PWM_Enable(int handle, byte unit, bool enable);
        [DllImport("acctrl.dll")]
        public static extern int PWM_Config(int handle, byte unit, ref PWM_CFG p_pwm_cfg);
        [DllImport("acctrl.dll")]
        public static extern int PWM_Start(int handle, byte unit);
        [DllImport("acctrl.dll")]
        public static extern int PWM_Stop(int handle, byte unit);
        [DllImport("acctrl.dll")]
        public static extern int LPP_Enable(int handle, byte unit, bool enable);
        [DllImport("acctrl.dll")]
        public static extern int LPP_Config(int handle, byte unit, ref LPP_CFG p_lpp_cfg);
        [DllImport("acctrl.dll")]
        public static extern int LPP_WriteAndRead(int handle, byte unit, ref LPP_CFG p_lpp_cfg);
        [DllImport("acctrl.dll")]
        public static extern int LPP_ConfigTimer(int handle, byte unit, UInt32 usecs);
        [DllImport("acctrl.dll")]
        public static extern int LPP_StartTimer(int handle, byte unit);
        [DllImport("acctrl.dll")]
        public static extern int LPP_StopTimer(int handle, byte unit);
        [DllImport("acctrl.dll")]
        public static extern int UART_ConfigTimer(int handle, byte unit, UInt32 usecs);
        [DllImport("acctrl.dll")]
        public static extern int UART_StartTimer(int handle, byte unit, byte device);
        [DllImport("acctrl.dll")]
        public static extern int UART_StopTimer(int handle, byte unit, byte device);
        [DllImport("acctrl.dll")]
        public static extern int UART_CloseTimer(int handle, byte unit);
        [DllImport("acctrl.dll")]
        public static extern void LPP_FirePWMs(int handle, byte unit, UInt32 freq, byte dutyCycle1, byte dutyCycle2, bool syncMode, bool phase180); 
        [DllImport("acctrl.dll")]
        public static extern int LPP_fwRevision(int handle, byte unit, byte[] p_read_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int LPP_Processor(int handle, byte unit, byte[] p_read_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int LPP_SpareOne(int handle, byte unit, UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int LPP_SpareTwo(int handle, byte unit, UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int UART_Enable(int handle, byte unit, bool enable);
        [DllImport("acctrl.dll")]
        public static extern int UART_Config(int handle, byte unit, UInt32 BaudRate, byte Parity, byte CharacterLength, byte StopBits);
        [DllImport("acctrl.dll")]
        public static extern int UART_Write(int handle, byte unit, UInt16 bytes_to_write, byte[] p_write_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int UART_Read(int handle, byte unit, UInt16 bytes_to_read, byte[] p_read_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int UART_DisableReceiver(int handle, byte unit);
        [DllImport("acctrl.dll")]
        public static extern int UART_Control(int handle, byte unit, UInt32 command, UInt32 arg);
         [DllImport("acctrl.dll")]
        public static extern int UART_SpareOne(int handle, byte unit, UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer);
        [DllImport("acctrl.dll")]
        public static extern int UART_SpareTwo(int handle, byte unit, UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer);

        [StructLayout(LayoutKind.Sequential)]
        public struct SPI_CFG
        {
            public UInt32 bitrate;  // spi bitrate: unit 1k 
            public byte protocol;     // polarity, phase, spi type  
            public byte datawidth;    // data width from 4 to 16 bits
            public byte cs_mode;      // active high: 1 or active low
            public byte cs_change;    // between spi word. cs change 1: no: 0
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PWM_CFG
        {
            public byte periodUnits;    // Units in which the period is specified; 1 = Hertz
            public UInt32 periodValue;  // PWM initial period                    
            public byte dutyUnits;      // Units in which the duty is specified; 1 = fraction
            public UInt32 dutyValue;    // PWM initial duty
            public byte idleLevel;      // Pin output when PWM is stopped; 0 = low, 1 = high
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LPP_CFG
        {
            public byte part;
        }

        public int handle;
        public uint chipSelect = 0;
        public const int MAX_ERROR_STRING = 80;

        private static MainWindow mainWindow = null;  // get the MainWindow object
        private byte unitUART = 1;

        // This is the GPIO table in MSP_EXP432E401Y.c AlgCSM_DRV firmware, this list MUST match that table
        // PG0 was PK1; enable LPP chip GPIOMSP432E4_PN3
        private List<string> gpioList = new List<string>(new string[] { "PP2", "PG0", "PN2", "PN3", "PK4", "PK5", "PH0", "PH1", "PL4", "PC5", "PB4", "PB5", "PP0", "PH2", "PP5", "PP4" });

        private System.Timers.Timer commsErrorTimer;
        private byte commErrors = 0;

        public AcCtrl(MainWindow master)
        {
            mainWindow = master;

            commsErrorTimer = new System.Timers.Timer();
            commsErrorTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            commsErrorTimer.Interval = 2000;  // auto update timer; can't make this faster, takes too long to update all the controls
        }

        const byte MAX_COMM_ERRS = 2;
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (commErrors > MAX_COMM_ERRS)
            {
                Mouse.OverrideCursor = null;

                commsErrorTimer.Stop();
                if (MessageBox.Show("Multiple communication errors encountered, GUI Application to device connection is likely compromised.\nWould you like to abort the Application?", "Communications Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                else
                    commErrors = 0;
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        public bool getDLLrev()
        {
            mainWindow.updateStatus("Fetch DLL Revision...\n");

            try
            {
                string errorMessage = "";
                if (LoadLibrary("acctrl.dll") == IntPtr.Zero)
                {
                    errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    MessageBox.Show("Error loading file \"acctrl.dll\": " + errorMessage, "DLL Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception loading file \"acctrl.dll\": " + ex.Message + "\nApplication shutting down.", "DLL Load Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Globals.dllVersion = "Error";
            try
            {
                byte[] pinVal = new byte[2];
                LPP_dllRevision(pinVal);
                Globals.dllVersion = pinVal[0] + "." + pinVal[1];

                // throw new InvalidOperationException("Test Exception initiated by Bad Dog");  // test exception

                if (double.Parse(Globals.dllVersion, CultureInfo.InvariantCulture) < Globals.MIN_DLL_VERSION)
                {
                    MessageBox.Show(mainWindow, "Invalid DLL Revision: " + Globals.dllVersion + "\nMinimum DLL Revision Required: " + Globals.MIN_DLL_VERSION + "\nApplication shutting down.", "Fetch DLL Revision Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    mainWindow.updateStatus("Fetch DLL returned an invalid Revision: " + Globals.dllVersion + "\n");
                    return false;
                }

                mainWindow.updateStatus("Fetch DLL Revision completed successfully.\n");
                return true;
            }
            catch (Exception ex)
            {
                mainWindow.updateStatus("Fetch DLL Revision failed.\n");
                MessageBox.Show("Exception fetching revision (" + Globals.dllVersion + ") for file \"acctrl.dll\": " + ex.Message + "\nApplication shutting down.", "DLL Revision Fetch Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public byte getFWrev()
        {
            mainWindow.updateStatus("Fetch Firmware Revision...\n");

            Globals.fwVersion = "Error";
            try
            {
                byte[] pinVal = new byte[2];
                int fwrevStatus = LPP_fwRevision(handle, 0, pinVal);
                if (fwrevStatus < 0)
                    return Globals.FETCH_FW_ERROR;
                else
                {
                    try
                    {
                        Globals.fwVersion = pinVal[0] + "." + pinVal[1];

                        //throw new InvalidOperationException("Test Exception initiated by Bad Dog");  // test exception

                        if (double.Parse(Globals.fwVersion, CultureInfo.InvariantCulture) < Globals.MIN_FW_VERSION)
                            return Globals.UPGRADE_AVAILABLE;

                        Globals.processor = 0xF1;  // default to MSP432
                        if ((double.Parse(Globals.fwVersion, CultureInfo.InvariantCulture) > 2.7))
                        {
                            if (!getProcessor())  // 0xF1 = MSP432; 0xF2 = TM4C129
                                return Globals.FETCH_PRO_ERROR;
                        }

                        mainWindow.updateStatus("Fetch Firmware Revision completed successfully.\n");
                        return Globals.PASS;
                    }
                    catch (Exception ex)
                    {
                        return Globals.FETCH_FW_ERROR;
                    }
                }
            }
            catch (Exception ex)
            {
                return Globals.FETCH_FW_ERROR;
            }
        }

        public bool getProcessor()
        {
            mainWindow.updateStatus("Fetch processor...\n");

            try
            {
                byte[] data = new byte[1];
                int status = LPP_Processor(handle, 0, data);
                if (status < 0)
                {
                    Globals.processor = 0;
                    StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                    Sys_GetStatusText(status, errorString);
                    MessageBox.Show(mainWindow, "Fetch processor type failed...\nERROR = " + errorString + "\nApplication shutting down.", "Fetch Processor Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    mainWindow.updateStatus("Fetch processor type Failed.\n");
                    return false;
                }
                else
                {
                    // throw new InvalidOperationException("Test Exception initiated by Bad Dog");  // test exception
                    Globals.processor = data[0];
                    mainWindow.updateStatus("Fetch processor type completed successfully.\n");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Globals.processor = 0;
                MessageBox.Show("Exception while determining processor type: " + ex.Message + "\nApplication shutting down.", "Processor Fetch Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void setPWMfreq(byte pwm, UInt32 freq, byte dutyCycle1, byte dutyCycle2, bool syncMode, bool phase180)
        {
            LPP_FirePWMs(handle, pwm, freq, dutyCycle1, dutyCycle2, syncMode, phase180);
        }

        public int initGPIO(string pin, bool highLow, byte mode)
        {
            // mode = 
            // 1: GPIO_Output, Sets pin as an output.
            // 2: GPIO_Input_No_Resistor, Sets pin as a floating input with no resistor.
            // 3: GPIO_Input_Pull_Up, Sets pin as an input with a pull-up resistor.
            // 4: GPIO_Input_Pull_Down, Sets pin as an input with a pull-down resistor.
            // 5: GPIO_Output_Open_Drain, Sets pin as an output with open drain(float).

            int status = enableGPIOpin(pin, true);

            if (status < 0)
                return status;
            status = configGPIOpin(pin, mode);
            if ((status < 0) || (mode > 1))
                return status;
            status = setGPIOpin(pin, highLow);
            if (status < 0)
                return status;
            return status;
        }

        public int setStateGPIO(string pin, bool highLow)
        {
            int status = setGPIOpin(pin, highLow);
            if (status < 0)
                return status;
            return status;
        }

        public bool findController(bool showStatus)
        {
            try
            {
                if (showStatus)
                    mainWindow.updateStatus("Get Controller...\n");
                else
                    mainWindow.usbRemoved = true;

                int contllr = Sys_FindACControllers();
                if (contllr < 1)
                {
                    MessageBox.Show(mainWindow, "Controller processor not responding...\nCheck power and connections.\nApplication shutting down.", "Communications Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else if (showStatus)
                    mainWindow.updateStatus("Controller found and connected.\n");

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public int findComPort()
        {
            mainWindow.updateStatus("Get Com Port...\n");
            int port = Sys_GetComPortNumber(0);
            if (port < 1)
            {
                MessageBox.Show(mainWindow, "Comm Port failed to open...\nCheck power and connections.\nApplication shutting down.", "Comm Port Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Get Com Port Failed.\n");
                mainWindow.textBlockComPort.Text = "No Connection...";
                return port;
            }
            else
            {
                mainWindow.updateStatus("Com Port Found at port " + port + ".\n");
                mainWindow.textBlockComPort.Text = "Connected: Com Port " + port;
            }

            return port;
        }

        public int findHandle(int port)
        {
            mainWindow.updateStatus("Get Handle...\n");
            handle = Sys_Open(port);  // get handle
            if (handle < 1)
            {
                MessageBox.Show(mainWindow, "Could not obtain handle to ACController...\nApplication shutting down.", "Handle Acquisition Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Get Handle Failed.\n");
                return handle;
            }
            else
                mainWindow.updateStatus("Get Handle completed successfully.\n");

            return handle;
        }

        public int enableSPI(int handle)
        {
            mainWindow.updateStatus("Enable SPI...\n");
            int spiEnable = SPI_Enable(handle, 0, true);
            if (spiEnable < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(spiEnable, errorString);
                MessageBox.Show(mainWindow, "Enable SPI failed...\nERROR = " + errorString + "\nApplication shutting down.", "SPI Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Enable SPI Failed.\n");
                return spiEnable;
            }
            else
                mainWindow.updateStatus("Enable SPI completed successfully.\n");

            return spiEnable;
        }

        public int configSPI(int handle)
        {
            SPI_CFG spiCfg = new SPI_CFG();

            spiCfg.bitrate = 1000;        // spi bitrate: unit 1k     
            spiCfg.protocol = 0;          // polarity, phase, spi type
            spiCfg.datawidth = 16;        // data width from 4 to 16 bits
            spiCfg.cs_mode = 0;           // active high: 1 or low: 0
            spiCfg.cs_change = 1;         // between spi word. cs change 1: no: 0

            mainWindow.updateStatus("Config SPI...\n");
            int spiConfig = SPI_Config(handle, 0, ref spiCfg);
            if (spiConfig < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(spiConfig, errorString);
                MessageBox.Show(mainWindow, "Config SPI failed...\nERROR = " + errorString + "\nApplication shutting down.", "SPI Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Config SPI Failed.\n");
                return spiConfig;
            }
            else
                mainWindow.updateStatus("Config SPI completed successfully.\n");

            return spiConfig;
        }

        private int enablePWM(int handle, byte pwmUnitPin)
        {
            mainWindow.updateStatus("Enable PWM...\n");
            int pwmEnable = PWM_Enable(handle, pwmUnitPin, true);
            if (pwmEnable < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(pwmEnable, errorString);
                MessageBox.Show(mainWindow, "Enable PWM failed...\nERROR = " + errorString + "\nApplication shutting down.", "PWM Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Enable PWM Failed.\n");
                return pwmEnable;
            }
            else
                mainWindow.updateStatus("Enable PWM completed successfully.\n");

            return pwmEnable;
        }

        private int configPWM(int handle, byte pwmUnitPin)
        {
            PWM_CFG pwmCfg = new PWM_CFG();

            pwmCfg.periodUnits = 1;        // Units in which the period is specified; 1 = Hertz
            pwmCfg.periodValue = 1000000;  // PWM initial period                    
            pwmCfg.dutyUnits = 1;          // Units in which the duty is specified; 1 = fraction
            pwmCfg.dutyValue = 0x7FFFFFFF; // PWM initial duty; Duty as a fractional part of PWM_DUTY_FRACTION_MAX = 0xFFFFFFFF */
            pwmCfg.idleLevel = 0;          // Pin output when PWM is stopped; 0 = low, 1 = high

            mainWindow.updateStatus("Config PWM...\n");
            int pwmConfig = PWM_Config(handle, pwmUnitPin, ref pwmCfg);
            if (pwmConfig < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(pwmConfig, errorString);
                MessageBox.Show(mainWindow, "Config PWM failed...\nERROR = " + errorString + "\nApplication shutting down.", "PWM Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Config PWM Failed.\n");
                return pwmConfig;
            }
            else
                mainWindow.updateStatus("Config PWM completed successfully.\n");

            return pwmConfig;
        }

        private int enableGPIOpin(string gpioPin, bool enable)
        {
            int index = gpioList.IndexOf(gpioPin);

            return enableGPIO(handle, (uint)(1 << index), enable);
        }

        private int configGPIOpin(string gpioPin, byte mode)
        {
            // mode = 
            // 1: GPIO_Output, Sets pin as an output.
            // 2: GPIO_Input_No_Resistor, Sets pin as a floating input with no resistor.
            // 3: GPIO_Input_Pull_Up, Sets pin as an input with a pull-up resistor.
            // 4: GPIO_Input_Pull_Down, Sets pin as an input with a pull-down resistor.
            // 5: GPIO_Output_Open_Drain, Sets pin as an output with open drain(float).

            int index = gpioList.IndexOf(gpioPin);

            return configGPIO(handle, (uint)(1 << index), mode);
        }

        public int setGPIOpin(string gpioPin, bool enable)
        {
#if DEBUG
            return 0;
#else
            int index = gpioList.IndexOf(gpioPin);
            uint pin_Value;
            string desiredState;
            if (enable)
            {
                pin_Value = (uint)(1 << index);
                desiredState = "enable";
            }
            else
            {
                pin_Value = 0;
                desiredState = "disable";
            }

            int gpioWrite = setGPIO((uint)(1 << index), pin_Value);
            if (gpioWrite < 0)
            {
                if (commErrors == 0)
                {
                    commsErrorTimer.Start();
                    commErrors++;
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(gpioWrite, errorString);
                MessageBox.Show(mainWindow, "GPIO " + gpioPin + " " + desiredState + " failed...\nERROR = " + errorString + "\n", "GPIO Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("GPIO Error.\n");
                mainWindow.updateStatus("GPIO command Failed.\n");
                }
                else
                    commErrors++;
            }
            else
                mainWindow.updateStatus("Set GPIO " + gpioPin + " " + desiredState + " completed successfully.\n");

            return gpioWrite;
#endif
        }

        private int setGPIO(uint pin_mask, uint pin_Value)
        {
            int gpioWrite = GPIO_Write(handle, pin_mask, pin_Value);
            return gpioWrite;
        }

        public bool? currentGPIOpinState(string gpioPin)
        {
            UInt32[] pinVal = new UInt32[] { 0 };
            int status = getGPIOpin(gpioPin, pinVal);
            if (status < 0)
                return null;
            else if (pinVal[0] == (1 << gpioList.IndexOf(gpioPin)))
                return true;
            else
                return false;
        }

        private int getGPIOpin(string gpioPin, UInt32[] pin_Value)
        {
            int index = gpioList.IndexOf(gpioPin);

            int gpioRead = getGPIO((uint)(1 << index), pin_Value);
            if (gpioRead < 0)
            {
                if (commErrors == 0)
                {
                    commsErrorTimer.Start();
                    commErrors++;
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(gpioRead, errorString);
                MessageBox.Show(mainWindow, "GPIO " + gpioPin + " read failed...\nERROR = " + errorString + "\n", "GPIO Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("GPIO Error.\n");
                mainWindow.updateStatus("GPIO command Failed.\n");
                }
                else
                    commErrors++;
            }
            else
                mainWindow.updateStatus("Get GPIO " + gpioPin + " completed successfully.\n");

            return gpioRead;
        }

        private int getGPIO(uint pin_mask, UInt32[] pin_Value)
        {
            int gpioRead = GPIO_Read(handle, pin_mask, pin_Value);
            return gpioRead;
        }

        private int enableGPIO(int handle, uint pin_mask, bool enable)
        {
            mainWindow.updateStatus("Enable GPIO...\n");
            //     int gpioEnable = GPIO_Enable(handle, 2, true);
            int gpioEnable = GPIO_Enable(handle, pin_mask, enable);
            if (gpioEnable < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(gpioEnable, errorString);
                MessageBox.Show(mainWindow, "Enable GPIO failed...\nERROR = " + errorString + "\nApplication shutting down.", "GPIO Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Enable GPIO Failed.\n");
                return gpioEnable;
            }
            else
                mainWindow.updateStatus("Enable GPIO completed successfully.\n");

            return gpioEnable;
        }

        private int configGPIO(int handle, uint pin_mask, byte mode)
        {
            mainWindow.updateStatus("Config GPIO...\n");
            //    int gpioConfig = GPIO_Config(handle, 2, 1);
            int gpioConfig = GPIO_Config(handle, pin_mask, mode);
            if (gpioConfig < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(gpioConfig, errorString);
                MessageBox.Show(mainWindow, "Config GPIO failed...\nERROR = " + errorString + "\nApplication shutting down.", "GPIO Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Config GPIO Failed.\n");
                return gpioConfig;
            }
            else
                mainWindow.updateStatus("Config GPIO completed successfully.\n");

            return gpioConfig;
        }

        public byte[] writeReadSPI(UInt16 bytes_to_write, byte[] p_write_data_buffer)
        {
            int resultSPI = SPI_WriteAndRead(handle, 0, chipSelect, bytes_to_write, p_write_data_buffer);  // CS 0 = PN2 on LaunchPad
            if (resultSPI < 0)
            {
                if (commErrors == 0)
                {
                    commsErrorTimer.Start();
                    commErrors++;
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(resultSPI, errorString);
                MessageBox.Show(mainWindow, "SPI command failed...\nERROR = " + errorString + "\n", "SPI Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("SPI command Failed.\n");
                }
                else
                    commErrors++;
            }
            else
                mainWindow.updateStatus("SPI command completed successfully.\n");

            return p_write_data_buffer;
        }

        public int enableLPP()
        {
            mainWindow.updateStatus("Enable LPP...\n");
            int lppEnable = LPP_Enable(handle, 0, true);
            if (lppEnable < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(lppEnable, errorString);
                MessageBox.Show(mainWindow, "LPP enable failed...\nERROR = " + errorString + "\nApplication shutting down.", "LPP Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("LPP enable Failed.\n");
            }
            else
                mainWindow.updateStatus("LPP enable completed successfully.\n");

            return lppEnable;
        }

        public int configLPP(byte selectedDevice)
        {
            mainWindow.updateStatus("Config LPP...\n");
            LPP_CFG lppCfg = new LPP_CFG();
            lppCfg.part = selectedDevice;  // 1 = (520 - 682)
            int lppConfig = LPP_Config(handle, 0, ref lppCfg);
            if (lppConfig < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(lppConfig, errorString);
                MessageBox.Show(mainWindow, "LPP config failed...\nERROR = " + errorString + "\nApplication shutting down.", "LPP Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("LPP config Failed.\n");
                Environment.Exit(-1);
            }
            else
                mainWindow.updateStatus("LPP config completed successfully.\n");

            return lppConfig;
        }

        public int configTimerLPP(UInt32 usecsTimer)
        {
            int timerConfig = LPP_ConfigTimer(handle, 0, usecsTimer);
            if (timerConfig < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(timerConfig, errorString);
                MessageBox.Show(mainWindow, "Config Timer failed...\nERROR = " + errorString + "\nApplication shutting down.", "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Config Timer Failed.\n");
                return timerConfig;
            }
            else
                mainWindow.updateStatus("Config Timer completed successfully.\n");

            return timerConfig;
        }

        public void startTimerLPP(byte timer)
        {
            LPP_StartTimer(handle, timer);
        }

        public void stopTimerLPP(byte timer)
        {
            LPP_StopTimer(handle, timer);
        }

        public int SpareOneLPP(UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer)
        {
            int lppSpareOne = LPP_SpareOne(handle, unitUART, arg1, arg2, p_read_data_buffer);
            if (lppSpareOne < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(lppSpareOne, errorString);
                MessageBox.Show(mainWindow, "Spare One LPP failed...\nERROR = " + errorString + ".", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Spare One LPP Failed.\n");
                return lppSpareOne;
            }
            else
                mainWindow.updateStatus("Spare One LPP completed successfully.\n");

            return lppSpareOne;
        }

        public int SpareTwoLPP(UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer)
        {
            int lppSpareTwo = LPP_SpareTwo(handle, unitUART, arg1, arg2, p_read_data_buffer);
            if (lppSpareTwo < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(lppSpareTwo, errorString);
                MessageBox.Show(mainWindow, "Spare Two LPP failed...\nERROR = " + errorString + ".", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Spare Two LPP Failed.\n");
                return lppSpareTwo;
            }
            else
                mainWindow.updateStatus("Spare Two UART completed successfully.\n");

            return lppSpareTwo;
        }

        public int configTimerUART(UInt32 usecsTimer)
        {
            int timerConfig = UART_ConfigTimer(handle, 2, usecsTimer);
            if (timerConfig < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(timerConfig, errorString);
                MessageBox.Show(mainWindow, "Config UART Timer failed...\nERROR = " + errorString + "\nApplication shutting down.", "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Config UART Timer Failed.\n");
                return timerConfig;
            }
            else
                mainWindow.updateStatus("Config UART Timer completed successfully.\n");

            return timerConfig;
        }

        public void startTimerUART(byte device)
        {
            UART_StartTimer(handle, 2, device);
        }

        public void stopTimerUART(byte device)
        {
            UART_StopTimer(handle, 2, device);
        }

        public int closeTimerUART(byte unit)
        {
            return UART_CloseTimer(handle, unit);
        }

        public int enableUART()
        {
            mainWindow.updateStatus("Enable UART...\n");
            int uartEnable = UART_Enable(handle, unitUART, true);
            if (uartEnable < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(uartEnable, errorString);
                MessageBox.Show(mainWindow, "UART enable failed...\nERROR = " + errorString + "\nApplication shutting down.", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("UART enable Failed.\n");
            }
            else
                mainWindow.updateStatus("UART enable completed successfully.\n");

            return uartEnable;
        }

        public int disableUART()
        {
            mainWindow.updateStatus("Disable UART...\n");
            int uartEnable = UART_Enable(handle, unitUART, false);
            if (uartEnable < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(uartEnable, errorString);
                MessageBox.Show(mainWindow, "UART Disable failed...\nERROR = " + errorString + "\nApplication shutting down.", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("UART Disable Failed.\n");
            }
            else
                mainWindow.updateStatus("UART Disable completed successfully.\n");

            return uartEnable;
        }

        public int configUART(UInt32 baud, bool firstInit)
        {
            mainWindow.updateStatus("Config UART " + baud + "...\n");
            int uartConfig = UART_Config(handle, unitUART, baud, 0x00, 0x03, 0x00);  // baud = 500000 / 1000000, parity = none, data length = 8, stop bits = 1
            if (uartConfig < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(uartConfig, errorString);
                if(firstInit)
                    MessageBox.Show(mainWindow, "Config UART failed...\nERROR = " + errorString + "\nApplication shutting down.", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(mainWindow, "Config UART failed...\nERROR = " + errorString, "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);

                mainWindow.updateStatus("Config UART Failed.\n");
                return uartConfig;
            }
            else
                mainWindow.updateStatus("Config UART completed successfully.\n");

            return uartConfig;
        }

        public int writeUART( UInt16 bytes_to_write, byte[] p_write_data_buffer)
        {
            int resultUART = UART_Write(handle, unitUART, bytes_to_write, p_write_data_buffer);
            if (resultUART < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(resultUART, errorString);

                if (commErrors == 0)
                {
                    commsErrorTimer.Start();
                    commErrors++;
                MessageBox.Show(mainWindow, "UART write command failed...\nERROR = " + errorString + "\n", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("UART write command Failed.\n");
                }
                else
                    commErrors++;
            }
            else
                mainWindow.updateStatus("UART write command completed successfully.\n");           

            return resultUART;
        }

        public int readUART(UInt16 bytes_to_read, byte[] p_read_data_buffer)
        {
            int resultUART = UART_Read(handle, unitUART, bytes_to_read, p_read_data_buffer);
            if (resultUART < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(resultUART, errorString);

                if (commErrors == 0)
                {
                    commsErrorTimer.Start();
                    commErrors++;
                MessageBox.Show(mainWindow, "UART read command failed...\nERROR = " + errorString + "\n", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("UART read command Failed.\n");
                }
                else
                    commErrors++;
            }
            else
                mainWindow.updateStatus("UART read command completed successfully.\n");

            return resultUART;
        }

        public int controlUART(UInt32 cmd, UInt32 arg)
        {
   //         Dispatcher.BeginInvoke(new Action(() => mainWindow.updateStatus("Disable UART...\n")));
            //mainWindow.updateStatus("Disable UART...\n");
            int uartControl = UART_Control(handle, unitUART, cmd, arg);
            if (uartControl < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(uartControl, errorString);
                MessageBox.Show(mainWindow, "Control UART failed...\nERROR = " + errorString + "\nApplication shutting down.", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Control UART Failed.\n");
                return uartControl;
            }
            else
                mainWindow.updateStatus("Control UART completed successfully.\n");

            return uartControl;
        }

        public int SpareOneUART(UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer)
        {
            int uartControl = UART_SpareOne(handle, unitUART, arg1, arg2, p_read_data_buffer);
            if (uartControl < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(uartControl, errorString);
                MessageBox.Show(mainWindow, "Spare One UART failed...\nERROR = " + errorString + ".", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Spare One UART Failed.\n");
                return uartControl;
            }
            else
                mainWindow.updateStatus("Spare One UART completed successfully.\n");

            return uartControl;
        }

        public int SpareTwoUART(UInt32 arg1, UInt32 arg2, byte[] p_read_data_buffer)
        {
            int uartControl = UART_SpareTwo(handle, unitUART, arg1, arg2, p_read_data_buffer);
            if (uartControl < 0)
            {
                StringBuilder errorString = new StringBuilder(MAX_ERROR_STRING);
                Sys_GetStatusText(uartControl, errorString);
                MessageBox.Show(mainWindow, "Spare Two UART failed...\nERROR = " + errorString + ".", "UART Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.updateStatus("Spare Two UART Failed.\n");
                return uartControl;
            }
            else
                mainWindow.updateStatus("Spare Two UART completed successfully.\n");

            return uartControl;
        }
    }
}

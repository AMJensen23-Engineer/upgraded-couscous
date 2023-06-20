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
    /// Interaction logic for Window1.xaml
    /// </summary> 

    public partial class Pixel : Window
    {
        private MainWindow mainWindow;
        private TPS92662Control tps66x;
        private static TextBox[] textBoxArrU1;
        private static TextBox[] textBoxArrU2;
        private static TextBox[] textBoxArrU3;
        private static TextBox[] textBoxArrU4;
        private static List<TextBox[]> allTextBoxArr;

        private const byte NUM_LEDS = 16;
               
        public Pixel(MainWindow master, TPS92662Control tps66xObject)
        {
            mainWindow = master;
            tps66x = tps66xObject;

            InitializeComponent();

            textBoxArrU1 = new TextBox[] { textBoxU1_Addr0_LED0, textBoxU1_Addr0_LED1, textBoxU1_Addr0_LED2, textBoxU1_Addr0_LED3, textBoxU1_Addr0_LED4, textBoxU1_Addr0_LED5, textBoxU1_Addr0_LED6, textBoxU1_Addr0_LED7,
                 textBoxU1_Addr0_LED8, textBoxU1_Addr0_LED9, textBoxU1_Addr0_LEDA, textBoxU1_Addr0_LEDB, textBoxU1_Addr0_LEDC, textBoxU1_Addr0_LEDD, textBoxU1_Addr0_LEDE, textBoxU1_Addr0_LEDF };
            textBoxArrU2 = new TextBox[] { textBoxU2_Addr4_LED0, textBoxU2_Addr4_LED1, textBoxU2_Addr4_LED2, textBoxU2_Addr4_LED3, textBoxU2_Addr4_LED4, textBoxU2_Addr4_LED5, textBoxU2_Addr4_LED6, textBoxU2_Addr4_LED7,
                 textBoxU2_Addr4_LED8, textBoxU2_Addr4_LED9, textBoxU2_Addr4_LEDA, textBoxU2_Addr4_LEDB, textBoxU2_Addr4_LEDC, textBoxU2_Addr4_LEDD, textBoxU2_Addr4_LEDE, textBoxU2_Addr4_LEDF };
            textBoxArrU3 = new TextBox[] { textBoxU3_Addr8_LED0, textBoxU3_Addr8_LED1, textBoxU3_Addr8_LED2, textBoxU3_Addr8_LED3, textBoxU3_Addr8_LED4, textBoxU3_Addr8_LED5, textBoxU3_Addr8_LED6, textBoxU3_Addr8_LED7,
                 textBoxU3_Addr8_LED8, textBoxU3_Addr8_LED9, textBoxU3_Addr8_LEDA, textBoxU3_Addr8_LEDB, textBoxU3_Addr8_LEDC, textBoxU3_Addr8_LEDD, textBoxU3_Addr8_LEDE, textBoxU3_Addr8_LEDF };
            textBoxArrU4 = new TextBox[] { textBoxU4_AddrC_LED0, textBoxU4_AddrC_LED1, textBoxU4_AddrC_LED2, textBoxU4_AddrC_LED3, textBoxU4_AddrC_LED4, textBoxU4_AddrC_LED5, textBoxU4_AddrC_LED6, textBoxU4_AddrC_LED7,
                 textBoxU4_AddrC_LED8, textBoxU4_AddrC_LED9, textBoxU4_AddrC_LEDA, textBoxU4_AddrC_LEDB, textBoxU4_AddrC_LEDC, textBoxU4_AddrC_LEDD, textBoxU4_AddrC_LEDE, textBoxU4_AddrC_LEDF };

            allTextBoxArr = new List<TextBox[]>();
            allTextBoxArr.Add(textBoxArrU1);
            allTextBoxArr.Add(textBoxArrU2);
            allTextBoxArr.Add(textBoxArrU3);
            allTextBoxArr.Add(textBoxArrU4);

            ButtonReadFaults_Click(this, null);
            ButtonReadPwm_Click(this, null);
        }

        private void readResetFaults(byte lmm, byte numLeds, bool readFlts, TextBox[] currentLmmTextBoxArr)
        {
            try
            {
                // Read / reset the fault registers
                string returnString = "";

                if (!readFlts)  // clear faults
                {
                    //           Globals.mcuCommand.xaction(true, lmm, (byte)MCUcommands.Regs.FAULTL, (byte)2, false);  // clear faults
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, lmm, (byte)LMMRegConfigs.RegAddr664.FLT_OPEN_OR_DRVL, BitConverter.GetBytes(0x00), ref returnString, false, (bool)tps66x.checkBoxAckEnable.IsChecked);
                    tps66x.updateStatus(returnString + "\n");
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, lmm, (byte)LMMRegConfigs.RegAddr664.FLT_OPEN_OR_DRVH, BitConverter.GetBytes(0x00), ref returnString, false, (bool)tps66x.checkBoxAckEnable.IsChecked);
                    tps66x.updateStatus(returnString + "\n");
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, lmm, (byte)LMMRegConfigs.RegAddr664.FAULT_SHORTL, BitConverter.GetBytes(0x00), ref returnString, false, (bool)tps66x.checkBoxAckEnable.IsChecked);
                    tps66x.updateStatus(returnString + "\n");
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, lmm, (byte)LMMRegConfigs.RegAddr664.FAULT_SHORTH, BitConverter.GetBytes(0x00), ref returnString, false, (bool)tps66x.checkBoxAckEnable.IsChecked);
                    tps66x.updateStatus(returnString + "\n");

                    mainWindow.updateStatus("Faults reset...\n");
                    return;
                }

                // make sure we are a go for fault checks
                byte[] valueDec = new byte[1];
                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, lmm, (byte)LMMRegConfigs.RegAddr664.OUTCTRL, ref returnString, false, null);

                tps66x.updateStatus(returnString + "\n");
                if (returnString.Contains("Data:"))
                {
                    if ((Globals.uartBuf[0] & 0x80) == 0x80)
                    {
                        tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " INFO: OUTCTRL Register: 0x01(1), Value: 1, Field: DRV_CHK, LED Driver Health Check operation. Sets sw_fet_hi_z output to 1. Disconnects the LED gate drivers from their corresponding FETs.\nGates of FETs are passively pulled down. In this health check mode the “OV FAULT” represents the status of the channel gate.\n(i.e. when OV_FAULT is high, the gate of the output FET is pulled low) The re-mapped OV signal may be thought of as 'Gate pulldown is active'.\n");
                        valueDec[0] = (byte)(Globals.uartBuf[0] & ~((byte)0x80));
                        Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, lmm, (byte)LMMRegConfigs.RegAddr664.OUTCTRL, valueDec, ref returnString, false, (bool)tps66x.checkBoxAckEnable.IsChecked);
                    }
                    else
                        tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " INFO: OUTCTRL Register: 0x01(1), Value: 0, Field: DRV_CHK, Normal operation. Sets sw_fet_hi_z output to 0.\nDoes not disconnect the LED gate drivers from the corresponding FETs.\n");
                }
                else
                {
                    return;
                }

                // check for open
                mainWindow.updateStatus("Read open faults.\n");
                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, lmm, (byte)LMMRegConfigs.RegAddr664.FLT_OPEN_OR_DRVL, ref returnString, false, null);
                tps66x.updateStatus(returnString + "\n");
                int[] addr4Led = new int[8] {1, 2, 4, 8, 16, 32, 64, 128 };
                if (returnString.Contains("Data:"))
                {
                    if (lmm == 4)
                    {
                        for (int i = 0; i < numLeds / 2; i++)
                        {
                            int ledIndexAddr = i;
                            if ((Globals.uartBuf[0] & addr4Led[i]) == addr4Led[i])
                            {
                                tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel " + i + ", An LED open fault has occurred.\n");
                                currentLmmTextBoxArr[ledIndexAddr].Background = Brushes.Red;
                            }
                            else
                                currentLmmTextBoxArr[ledIndexAddr].Background = Brushes.ForestGreen;
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= numLeds / 2; i++)
                        {
                            int ledIndexAddr = i - 1;
                            if ((Globals.uartBuf[0] & i) == i)
                            {
                                tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " FAULT: FAULT_OPEN_OR_DRVL Register: 0x86(134), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel " + i + ", An LED open fault has occurred.\n");
                                currentLmmTextBoxArr[ledIndexAddr].Background = Brushes.Red;
                            }
                            else
                                currentLmmTextBoxArr[ledIndexAddr].Background = Brushes.ForestGreen;
                        }
                    }
                }
                else
                {
                    return;
                }

                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, lmm, (byte)LMMRegConfigs.RegAddr664.FLT_OPEN_OR_DRVH, ref returnString, false, null);
                tps66x.updateStatus(returnString + "\n");
                if (returnString.Contains("Data:"))
                {
                    int currentPixel = 0x01;
                    for (int i = 9; i <= numLeds; i++)
                    {
                        if ((Globals.uartBuf[0] & currentPixel) == currentPixel)
                        {
                            tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " FAULT: FAULT_OPEN_OR_DRVH Register: 0x87(135), Value: 1, Field: FAULT_OPEN_OR_DRVL, Channel " + i + ", An LED open fault has occurred.\n");
                            currentLmmTextBoxArr[i - 1].Background = Brushes.Red;
                        }
                        else
                            currentLmmTextBoxArr[i - 1].Background = Brushes.ForestGreen;

                        currentPixel *= 2;
                    }
                }
                else
                {
                    return;
                }

                // check for short
                mainWindow.updateStatus("Read short faults.\n");
                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, lmm, (byte)LMMRegConfigs.RegAddr664.FAULT_SHORTL, ref returnString, false, null);
                tps66x.updateStatus(returnString + "\n");
                if (returnString.Contains("Data:"))
                {
                    if (lmm == 4)
                    {
                        for (int i = 0; i < numLeds / 2; i++)
                        {
                            int ledIndexAddr = i;
                            if ((Globals.uartBuf[0] & addr4Led[i]) == addr4Led[i])
                            {
                                tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel " + i + ", An LED short fault has occurred.\n");
                                currentLmmTextBoxArr[ledIndexAddr].Background = Brushes.Red;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= numLeds / 2; i++)
                        {
                            int ledIndexAddr = i - 1;
                            if ((Globals.uartBuf[0] & i) == i)
                            {
                                tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " FAULT: FAULT_SHORTL Register: 0x88(136), Value: 1, Field: FAULT_SHORT, Channel " + i + ", An LED short fault has occurred.\n");
                                currentLmmTextBoxArr[ledIndexAddr].Background = Brushes.Orange;
                            }
                        }
                    }
                }
                else
                {
                    return;
                }

                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, lmm, (byte)LMMRegConfigs.RegAddr664.FAULT_SHORTH, ref returnString, false, null);
                tps66x.updateStatus(returnString + "\n");
                if (returnString.Contains("Data:"))
                {
                    int currentPixel = 0x01;
                    for (int i = 9; i <= numLeds; i++)
                    {
                        if ((Globals.uartBuf[0] & currentPixel) == currentPixel)
                        {
                            tps66x.updateStatus("Device: " + lmm + ", 0x" + lmm.ToString("X2") + " FAULT: FAULT_SHORTH Register: 0x89(137), Value: 1, Field: FAULT_SHORT, Channel " + i + ", An LED short fault has occurred.\n");
                            currentLmmTextBoxArr[i - 1].Background = Brushes.Orange;
                        }
                    }

                    currentPixel *= 2;
                }
                else
                {
                    return;
                }

                // restore register 1 value if we changed it
                if ((valueDec[0] & 0x80) == 0x80)
                {
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, lmm, (byte)LMMRegConfigs.RegAddr664.OUTCTRL, valueDec, ref returnString, false, (bool)tps66x.checkBoxAckEnable.IsChecked);
                    tps66x.updateStatus(returnString + "\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "An exception occurred during \"Read Faults\" Error: " + ex.Message + "\nRead Faults aborted...", "Read Faults Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void readDutyCycle(byte lmm, byte numLeds, TextBox[] currentLmmTextBoxArr)
        {
            try
            {
                mainWindow.updateStatus("Read all PWM...\n");
                string returnString = "";
                Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ20, lmm, (byte)LMMRegConfigs.RegAddr664.WIDTH01H, ref returnString, false, null);

                if (returnString.Contains("Data:"))
                {
                    byte[] lowBits = new byte[NUM_LEDS];
                    int k = 0;
                    for (int i = 0; i < NUM_LEDS; i += 4)
                    {
                        for (int j = 0; j < 4; j++)
                            lowBits[i + j] = (byte)((Globals.uartBuf[k + NUM_LEDS] & (0x03 << j * 2)) >> j * 2);
                        k++;
                    }

                    for (int i = 0; i < NUM_LEDS; i++)
                        currentLmmTextBoxArr[i].Text = (((short)Globals.uartBuf[i] << 2) | lowBits[i]).ToString();
                }
                else  // error
                {
                    mainWindow.updateStatus(returnString + "\n");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "An exception occurred during \"Read all PWM\" Error: " + ex.Message + "\nRead all PWM aborted...", "PWM Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void readWriteDutyCycle(bool isWrite)
        {
            try
            {
                short[] pwmVals = new short[NUM_LEDS];
                byte[] lowBits = new byte[NUM_LEDS];
                byte[] bytesToWrite = new byte[NUM_LEDS + 4];
                byte currentDevice = 0;

                mainWindow.updateStatus("Write all PWM...\n");
                for (int i = 0; i < allTextBoxArr.Count; i++) 
                {
                    currentDevice = byte.Parse((allTextBoxArr[i][0] as TextBox).Name.Substring((allTextBoxArr[i][0] as TextBox).Name.IndexOf('r') + 1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                    for (int j = 0; j < allTextBoxArr[i].Length; j++)  // get the values from the individual TextBox arrays
                    {
                        short textBoxValue = 0;
                        bool successfullyParsed = short.TryParse(allTextBoxArr[i][j].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out textBoxValue);
                        if (successfullyParsed)
                        {
                   //         throw new InvalidOperationException("Test Exception initiated by Bad Dog");  // test exception

                            if (textBoxValue < 0)
                            {
                                textBoxValue = 0;
                                allTextBoxArr[i][j].Text = "0";
                            }
                            else if (textBoxValue > 1023)
                            {
                                textBoxValue = 1023;
                                allTextBoxArr[i][j].Text = "1023";
                            }
                            pwmVals[j] = textBoxValue;
                        }
                        else
                        {
                            mainWindow.updateStatus("Invalid value found during Write all PWM.\nWrite all PWM aborted...");
                            MessageBox.Show(this, "Invalid value found during \"Write all PWM\".\nWrite all PWM aborted...", "PWM Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    int lowBytesRead = 0;
                    for (int k = 0; k < NUM_LEDS + 4; k++)  // get all the bit register values
                    {
                        if (k < NUM_LEDS)
                        {
                            bytesToWrite[k] = (byte)(pwmVals[k] >> 2);
                            lowBits[k] = (byte)(pwmVals[k] & 0x03);
                        }
                        else
                        {
                            bytesToWrite[k] = (byte)(lowBits[lowBytesRead] | (lowBits[lowBytesRead + 1] << 2) + (lowBits[lowBytesRead + 2] << 4) + (lowBits[lowBytesRead + 3] << 6));
                            lowBytesRead += 4;
                        }
                    }

                    string returnString = "";
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE20, currentDevice, (byte)LMMRegConfigs.RegAddr664.WIDTH01H, bytesToWrite, ref returnString, false, (bool)tps66x.checkBoxAckEnable.IsChecked);
                    tps66x.updateStatus(returnString + "\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "An exception occurred during \"Write all PWM\" Error: " + ex.Message + "\nWrite all PWM aborted...", "PWM Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonWrtieAllPwm_Click(object sender, RoutedEventArgs e)
        {
            Cursor _previousCursor;
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                readWriteDutyCycle(true);
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error L4: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                Mouse.OverrideCursor = _previousCursor;
            }
        }

        private void ButtonReadPwm_Click(object sender, RoutedEventArgs e)
        {
            Cursor _previousCursor;
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                readDutyCycle(0, NUM_LEDS, allTextBoxArr[0]);
                readDutyCycle(4, NUM_LEDS, allTextBoxArr[1]);
                readDutyCycle(8, NUM_LEDS, allTextBoxArr[2]);
                readDutyCycle(12, NUM_LEDS, allTextBoxArr[3]);
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error L5: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                Mouse.OverrideCursor = _previousCursor;
            }
        }

        private void ButtonReadFaults_Click(object sender, RoutedEventArgs e)
        {
            Cursor _previousCursor;
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                readResetFaults(0, NUM_LEDS, true, allTextBoxArr[0]);
                readResetFaults(4, NUM_LEDS, true, allTextBoxArr[1]);
                readResetFaults(8, NUM_LEDS, true, allTextBoxArr[2]);
                readResetFaults(12, NUM_LEDS, true, allTextBoxArr[3]);
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error L6: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                Mouse.OverrideCursor = _previousCursor;
            }
        }

        private void ButtonResetFaults_Click(object sender, RoutedEventArgs e)
        {
            Cursor _previousCursor;
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                readResetFaults(0, NUM_LEDS, false, allTextBoxArr[0]);
                readResetFaults(4, NUM_LEDS, false, allTextBoxArr[1]);
                readResetFaults(8, NUM_LEDS, false, allTextBoxArr[2]);
                readResetFaults(12, NUM_LEDS, false, allTextBoxArr[3]);
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error L7: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                Mouse.OverrideCursor = _previousCursor;
            }
        }
    }
}

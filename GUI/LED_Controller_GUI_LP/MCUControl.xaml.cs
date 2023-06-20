using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TPS9266xEvaluationModule
{
    /// <summary>
    /// Interaction logic for MCUControl.xaml
    /// </summary>
    public partial class MCUControl : UserControl
    {
        private MainWindow mainWindow;  // get the MainWindow object
        private byte newPWM1DC_1 = 0;  // PF2
        private byte newPWM2DC_1 = 0;  // PF3
        private UInt16 newPWM1Freq_1 = 0;
        private bool syncPulses_1;
        private bool phase180_1;

        private byte newPWM1DC_2 = 0;  // PG0
        private byte newPWM2DC_2 = 0;  // PG1
        private UInt16 newPWM1Freq_2 = 0;
        private bool syncPulses_2;
        private bool phase180_2;

        public MCUControl(MainWindow master)
        {
            InitializeComponent();
            mainWindow = master;

            radioButtonPWM1.Checked -= RadioButtonPWM1_Checked;
            radioButtonPWM1.IsChecked = Properties.Settings.Default.radioButtonPWM1;
            radioButtonPWM1.Checked += RadioButtonPWM1_Checked;

            newPWM1Freq_1 = Properties.Settings.Default.pwm1Freq_1;  // PWM 1
            newPWM1DC_1 = (byte)Properties.Settings.Default.pwm1DC_1;
            newPWM2DC_1 = (byte)Properties.Settings.Default.pwm2DC_1;
            syncPulses_1 = Properties.Settings.Default.checkBoxSyncPulses_1;
            phase180_1 = Properties.Settings.Default.checkBoxPhase180_1;

            newPWM1Freq_2 = Properties.Settings.Default.pwm1Freq_2;  // PWM2
            newPWM1DC_2 = (byte)Properties.Settings.Default.pwm1DC_2;
            newPWM2DC_2 = (byte)Properties.Settings.Default.pwm2DC_2;
            syncPulses_2 = Properties.Settings.Default.checkBoxSyncPulses_2;
            phase180_2 = Properties.Settings.Default.checkBoxPhase180_2;
        }
        private void textBoxPWM1Freq1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBoxPWM1Freq.LostFocus -= textBoxPWM1Freq_LostFocus;  // remove event
                changePWM1Freq();
                textBoxPWM1Freq.LostFocus += textBoxPWM1Freq_LostFocus;  // add it back
            }
        }

        private void textBoxPWM1Freq_LostFocus(object sender, RoutedEventArgs e)
        {
            changePWM1Freq();
        }

        public void changePWM1Freq()
        {
            UInt16 newPWM1Freq;
            byte newPWM1DC;
            byte newPWM2DC;
            byte pwm;

            if ((bool)radioButtonPWM1.IsChecked)
            {
                newPWM1DC = newPWM1DC_1;
                newPWM2DC = newPWM2DC_1;
                pwm = 0;
            }
            else
            {
                newPWM1DC = newPWM1DC_2;
                newPWM2DC = newPWM2DC_2;
                pwm = 1;
            }

            if (!UInt16.TryParse(textBoxPWM1Freq.Text, out newPWM1Freq))
            {
                //    MessageBox.Show(mainWindow, "Please enter frequency in Hz from 1000 - 64000", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show(mainWindow, "Please enter frequency in Hz from 30 - 2000", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textBoxPWM1Freq.Text = Properties.Settings.Default.pwm1Freq_1.ToString();  //TODO: Get from MCU?
            }
            else
            {
                //if ((newPWM1Freq < 1000) || (newPWM1Freq > 64000))  // need 2.3 FW for this.....
                if ((newPWM1Freq < 30) || (newPWM1Freq > 2000))  // need 2.3 FW for this.....
                {
                 //   MessageBox.Show("Please enter frequency in Hz from 1000 - 64000", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show("Please enter frequency in Hz from 30 - 2000", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBoxPWM1Freq.Text = Properties.Settings.Default.pwm1Freq_1.ToString();  //TODO: Get from MCU?
                }
                else
                {
                    if ((bool)radioButtonPWM1.IsChecked)
                        Properties.Settings.Default.pwm1Freq_1 = newPWM1Freq_1 = newPWM1Freq;  // Good value entered, save it
                    else
                        Properties.Settings.Default.pwm1Freq_2 = newPWM1Freq_2 = newPWM1Freq;

                    if (checkBoxPhase180.IsChecked == true)
                    {
                        byte phaseVal = (byte)(100 - newPWM2DC);
                        phaseShiftedOutValue.Content = phaseVal.ToString();
                    }

                    newPWM1Freq *= 32;  // dividing by 32 in FW
                    mainWindow.ac.setPWMfreq(pwm, newPWM1Freq, newPWM1DC, newPWM2DC, true, (bool)checkBoxPhase180.IsChecked);  // change PWMs here for launchpad
                }
            }
        }

        private void textBoxPWM1DC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBoxPWM1DC.LostFocus -= textBoxPWM1DC_LostFocus;  // remove event
                changePWM1DC();
                textBoxPWM1DC.LostFocus += textBoxPWM1DC_LostFocus;  // add it back
            }
        }

        private void textBoxPWM1DC_LostFocus(object sender, RoutedEventArgs e)
        {
            changePWM1DC();
        }

        public void changePWM1DC()
        {
            byte newPWM1DC;

            if (!byte.TryParse(textBoxPWM1DC.Text, out newPWM1DC))
            {
                MessageBox.Show("Please enter duty cycle between 0 and 100", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textBoxPWM1DC.Text = String.Format("{0:0}", Properties.Settings.Default.pwm1DC_1.ToString());
            }
            else
            {
                if ((newPWM1DC < 0) || (newPWM1DC > 100))
                {
                    MessageBox.Show("Please enter duty cycle between 0 and 100", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBoxPWM1DC.Text = String.Format("{0:0}", Properties.Settings.Default.pwm1DC_1.ToString());
                }
                else
                {

                    if ((bool)radioButtonPWM1.IsChecked)
                        Properties.Settings.Default.pwm1DC_1 = newPWM1DC_1 = newPWM1DC;  // Good value entered, save it
                    else
                        Properties.Settings.Default.pwm1DC_2 = newPWM1DC_2 = newPWM1DC;
            
                    changePWM1Freq();
                }
            }
        }

        private void textBoxPWM2DC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBoxPWM2DC.LostFocus -= textBoxPWM2DC_LostFocus;  // remove event
                changePWM2DC();
                textBoxPWM2DC.LostFocus += textBoxPWM2DC_LostFocus;  // add it back
            }
        }

        private void textBoxPWM2DC_LostFocus(object sender, RoutedEventArgs e)
        {
            changePWM2DC();
        }

        public void changePWM2DC()
        {
            byte newPWM2DC;

            if (!byte.TryParse(textBoxPWM2DC.Text, out newPWM2DC))
            {
                MessageBox.Show("Please enter duty cycle between 0 and 100", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textBoxPWM2DC.Text = String.Format("{0:0}", Properties.Settings.Default.pwm2DC_1.ToString());
            }
            else
            {
                if ((newPWM2DC < 0) || (newPWM2DC > 100))
                {
                    MessageBox.Show("Please enter duty cycle between 0 and 100", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBoxPWM2DC.Text = String.Format("{0:0}", Properties.Settings.Default.pwm2DC_1.ToString());
                }
                else
                {
                    if ((bool)radioButtonPWM1.IsChecked)
                        Properties.Settings.Default.pwm2DC_1 = newPWM2DC_1 = newPWM2DC;  // Good value entered, save it
                    else
                        Properties.Settings.Default.pwm2DC_2 = newPWM2DC_2 = newPWM2DC;

                    changePWM1Freq();
                }
            }
        }

        private void checkBoxPhase180_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButtonPWM1.IsChecked)
                phase180_1 = Properties.Settings.Default.checkBoxPhase180_1 = (bool)checkBoxPhase180.IsChecked;
            else
                phase180_2 = Properties.Settings.Default.checkBoxPhase180_2 = (bool)checkBoxPhase180.IsChecked;

            phaseShiftState();

            changePWM1Freq();
        }

        private void phaseShiftState()
        {
            if ((checkBoxPhase180.IsChecked == true) && (checkBoxPhase180.Content.ToString().Contains("*") == false))
            {
                checkBoxPhase180.Content += "*";
                labelPhaseShiftValue.Content = "*Phase shifted output: ";
            }
            else if (checkBoxPhase180.IsChecked == false)
            {
                checkBoxPhase180.Content = checkBoxPhase180.Content.ToString().TrimEnd('*');
                labelPhaseShiftValue.Content = "";
                phaseShiftedOutValue.Content = "";
            }
        }

        public void RadioButtonPWM1_Checked(object sender, RoutedEventArgs e)
        {
            textBoxPWM1Freq.Text = newPWM1Freq_1.ToString();
            textBoxPWM1DC.Text = newPWM1DC_1.ToString();
            textBoxPWM2DC.Text = newPWM2DC_1.ToString();
            labelPWM1DC.Content = "DutyCycle 1(PF2):";
            labelPWM2DC.Content = "DutyCycle 2(PF3):";
            checkBoxPhase180.Content = "180 Phase Shift (PF3)";
            checkBoxPhase180.IsChecked = phase180_1;
            phaseShiftState();
            Properties.Settings.Default.radioButtonPWM1 = true;
            
            changePWM1Freq();
        }

        public void RadioButtonPWM2_Checked(object sender, RoutedEventArgs e)
        {
            textBoxPWM1Freq.Text = newPWM1Freq_2.ToString();
            textBoxPWM1DC.Text = newPWM1DC_2.ToString();
            textBoxPWM2DC.Text = newPWM2DC_2.ToString();
            labelPWM1DC.Content = "DutyCycle 1(PG0):";
            labelPWM2DC.Content = "DutyCycle 2(PG1):";
            checkBoxPhase180.Content = "180 Phase Shift (PG1)";
            checkBoxPhase180.IsChecked = phase180_2;
            phaseShiftState();
            Properties.Settings.Default.radioButtonPWM1 = false;

            changePWM1Freq();
        }
    }
}

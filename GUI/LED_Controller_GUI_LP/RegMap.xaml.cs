using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TPS9266xEvaluationModule
{
    public class RegisterClass : INotifyPropertyChanged
    {
        public bool externallyChecked = true;
        public byte Dec { get; set; }
        public string Hex { get; set; }
        public string Register { get; set; }
        private bool d7;
        public bool D7
        {
            get { return d7; }
            set
            {
                if ((Type != null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d7 = value;

                if (d7)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x80).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x80)).ToString("X2");

                OnPropertyChanged("D7");
            }
        }
        private bool d6;
        public bool D6
        {
            get { return d6; }
            set
            {
                if ((Type !=null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d6 = value;

                if (d6)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x40).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x40)).ToString("X2");

                OnPropertyChanged("D6");
            }
        }
        private bool d5;
        public bool D5
        {
            get { return d5; }
            set
            {
                if ((Type != null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d5 = value;

                if (d5)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x20).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x20)).ToString("X2");

                OnPropertyChanged("D5");
            }
        }
        private bool d4;
        public bool D4
        {
            get { return d4; }
            set
            {
                if ((Type != null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d4 = value;

                if(d4)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x10).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x10)).ToString("X2");
                
                OnPropertyChanged("D4");
            }
        }
        private bool d3;
        public bool D3
        {
            get { return d3; }
            set
            {
                if ((Type != null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d3 = value;

                if (d3)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x08).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x08)).ToString("X2");

                OnPropertyChanged("D3");
            }
        }
        private bool d2;
        public bool D2
        {
            get { return d2; }
            set
            {
                if ((Type != null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d2 = value;

                if (d2)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x04).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x04)).ToString("X2");

                OnPropertyChanged("D2");
            }
        }
        private bool d1;
        public bool D1
        {
            get { return d1; }
            set
            {
                if ((Type != null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d1 = value;

                if (d1)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x02).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x02)).ToString("X2");

                OnPropertyChanged("D1");
            }
        }
        private bool d0;
        public bool D0
        {
            get { return d0; }
            set
            {
                if ((Type != null) && (!Type.Contains("w")) && externallyChecked)  // if user checked a bit on a read only register don't allow the check
                    return;

                d0 = value;

                if (d0)
                    Value = "0x" + (Convert.ToByte(Value, 16) | 0x01).ToString("X2");
                else
                    Value = "0x" + ((byte)(Convert.ToByte(Value, 16) & ~0x01)).ToString("X2");

                OnPropertyChanged("D0");
            }
        }
        private string valueVal;
        public string Value
        {
            get { return valueVal; }
            set
            {
                valueVal = value;
                OnPropertyChanged("Value");
            }
        }
        public string Default { get; set; }
        public string Type { get; set; }
        private bool read;
        public bool Read
        {
            get { return read; }
            set
            {
                read = value;
                OnPropertyChanged("Read");
            }
        }
        private bool write;
        public bool Write
        {
            get { return write; }
            set
            {
                write = value;
                OnPropertyChanged("Write");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public List<RegisterDetails> Details{ get; set; }
        //public string DetailsString
        //{
        //    get { return Register = Dec.ToString(); }
        //}
    }

    public class RegisterDetails
    {
        public int RegisterInt { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
    }

    /// <summary>
    /// Interaction logic for RegMap.xaml
    /// </summary>
    public partial class RegMap : Window
    {
        private List<string> listRegister = new List<string>();
        private List<string> listBitFields = new List<string>();
        private List<string> listQuotes = new List<string>();
        private ObservableCollection<RegisterClass> listParseRegister;
        private List<List<string>> listToolTipsData = new List<List<string>>();
        private RegisterClass currentSelectedItem;

        private string projDirString;
        private string fileNameOnly;
        private string regMapRev;
        private string toolTipsFile;
        private string currentDevice;

        private bool allCheckedRead = false;
        private bool allCheckedWrite = false;
        private bool loadComplete = false;
        private bool busyReadingWriting = false;
        private bool quotesFound = false;
        private bool regMapWindowClosing = false;

        private MainWindow mainWindow = null;  // get the MainWindow object
        private UserControl lmmWindow = null;  // get the MainWindow object
        private byte deviceFromParent = 0;
        private string deviceAddressParent;
        private LinearGradientBrush myBrushOn;
        private LinearGradientBrush myBrushOff;
        private LinearGradientBrush myBrushDefault;

        private List<lmmDevice> lmmDevices = new List<lmmDevice>();

        private const short MIN_NUM_REGS_664 = 177;
        private const short MIN_NUM_REGS_665 = 173;
        private const short MIN_NUM_REGS_667 = 113;
        private const short MIN_NUM_REGS_662 = 141;

        private RegisterClass regClass;

        public RegMap(MainWindow master, UserControl userControlLMM, byte device, string deviceAddress, List<lmmDevice> lmmDeviceList)
        {
            mainWindow = master;
            deviceFromParent = device;
            deviceAddressParent = deviceAddress;
            lmmWindow = userControlLMM;
            lmmDevices = lmmDeviceList;

            myBrushOn = new LinearGradientBrush();
            myBrushOn.StartPoint = new Point(0.5, 0);
            myBrushOn.EndPoint = new Point(0.5, 1);
            Color colorWhite = ((SolidColorBrush)Brushes.White).Color;
            Color colorGreen = ((SolidColorBrush)Brushes.Green).Color;
            myBrushOn.GradientStops.Add(new GradientStop(colorWhite, 0.0));
            myBrushOn.GradientStops.Add(new GradientStop(colorGreen, 1.0));

            myBrushOff = new LinearGradientBrush();
            myBrushOff.StartPoint = new Point(0.5, 0);
            myBrushOff.EndPoint = new Point(0.5, 1);
            Color colorRed = ((SolidColorBrush)Brushes.Red).Color;
            myBrushOff.GradientStops.Add(new GradientStop(colorWhite, 0.0));
            myBrushOff.GradientStops.Add(new GradientStop(colorRed, 1.0));

            myBrushDefault = new LinearGradientBrush();
            myBrushDefault.StartPoint = new Point(0.5, 0);
            myBrushDefault.EndPoint = new Point(0.5, 1);
            Color colorLightGray = ((SolidColorBrush)Brushes.LightGray).Color;
            myBrushDefault.GradientStops.Add(new GradientStop(colorWhite, 0.0));
            myBrushDefault.GradientStops.Add(new GradientStop(colorLightGray, 1.0));

            InitializeComponent();

            if(Properties.Settings.Default.screensNum != System.Windows.Forms.Screen.AllScreens.Count())
            {
                this.Top = 75;  // if there were changes with the monitor move the regmap app
                this.Left = 75;
            }

            dataGridRegisterMap.CanUserAddRows = false;

            dataGridRegisterMap.AutoGeneratingColumn += dataGridRegisterMap_AutoGeneratingColumn;
            dataGridRegisterMap.LoadingRowDetails += dataGridRegisterMap_LoadingRowDetails;

            dataGridRegisterMap.EnableColumnVirtualization = false;
            dataGridRegisterMap.AlternationCount = 2;

            // placed this in main
    //        ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));  // toolTip will stay on until mouse moves

    //        checkBoxAutoLoadFile.IsChecked = Properties.Settings.Default.checkBoxAutoLoadFile;
            menuItemAutoOpen.IsChecked = Properties.Settings.Default.checkBoxAutoLoadFile;
            menuItemAutoReadAll.IsChecked = Properties.Settings.Default.checkBoxAutoReadAll;

            readAllCmd.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(readAllCmd, readAll));

            readSelectedCmd.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(readSelectedCmd, readSelected));

            writeAllCmd.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(writeAllCmd, writeAll));

            writeSelectedCmd.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(writeSelectedCmd, writeSelected));

            toolBarTrayRegMap.IsEnabled = false;
            menuItemRegister.IsEnabled = false;
            menuItemActions.IsEnabled = false;

            if(deviceFromParent == 0x96)
                Properties.Settings.Default.autoLoadFileName = Properties.Settings.Default.autoLoadFileName664;
            else if (deviceFromParent == 0x97)
                Properties.Settings.Default.autoLoadFileName = Properties.Settings.Default.autoLoadFileName665;
            else if (deviceFromParent == 0x98)
                Properties.Settings.Default.autoLoadFileName = Properties.Settings.Default.autoLoadFileName667;

            regClass = new RegisterClass();

            startMeUp();
        }

        public RoutedCommand readAllCmd = new RoutedCommand();
        private void readAll(object sender, ExecutedRoutedEventArgs e)
        {
            readAll();
        }

        public RoutedCommand readSelectedCmd = new RoutedCommand();
        private void readSelected(object sender, ExecutedRoutedEventArgs e)
        {
            ButtonReadSelected_Click(this, null);
        }

        public RoutedCommand writeAllCmd = new RoutedCommand();
        private void writeAll(object sender, ExecutedRoutedEventArgs e)
        {
            writeAll();
        }

        public RoutedCommand writeSelectedCmd = new RoutedCommand();
        private void writeSelected(object sender, ExecutedRoutedEventArgs e)
        {
            ButtonWriteSelected_Click(this, null);
        }

        private void startMeUp()
        {
            if (Properties.Settings.Default.autoLoadFileName == "")
            {
                //          checkBoxAutoLoadFile.IsChecked = false;
                //        checkBoxAutoLoadFile.IsEnabled = false;
                menuItemAutoOpen.IsChecked = false;
            }
            else if ((Properties.Settings.Default.checkBoxAutoLoadFile) && (Properties.Settings.Default.autoLoadFileName != ""))
                OpenFile_Click(this, null);
        }

        private void myTest(object sender, ExecutedRoutedEventArgs e)
        {
            ;
        }

        private void dataGridRegisterMap_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            DataGrid innerGrid = e.DetailsElement as DataGrid;
            if (innerGrid != null)
            {
                ;
            }

            var dataGrid = (e.DetailsElement as FrameworkElement);

            TextBox tbTest = e.DetailsElement.FindName("D7") as TextBox;
            if (tbTest != null)
            {
                tbTest.Text = "Juhuu";
            }
        }

        void dataGridRegisterMap_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if ((e.PropertyName == "Default") || (e.PropertyName == "Details"))
            {
                e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToHeader);
                e.Column.IsReadOnly = true;
            }
            else
                e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

            if ((e.PropertyName == "Dec") || (e.PropertyName == "Hex") || (e.PropertyName == "Register") || (e.PropertyName == "Type"))
                e.Column.IsReadOnly = true;
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

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (listRegister.Count != 0)
                CloseFile_Click(this, null);

            Cursor _previousCursor = null;
            _previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = false;
                if (!menuItemAutoOpen.IsChecked)
                {
                    dlg.DefaultExt = ".txt";
                    dlg.Filter = "Map Files (.txt)|*.txt";
                    result = dlg.ShowDialog();
                }

                if ((result == true) || (menuItemAutoOpen.IsChecked))
                {
                    string filenameWithPath = "";

                    if (!menuItemAutoOpen.IsChecked)
                    {
                        filenameWithPath = dlg.FileName;
                        fileNameOnly = dlg.SafeFileName;
                        projDirString = dlg.FileName.Replace(fileNameOnly, "");
                     
                        if (deviceFromParent == 0x96)
                            Properties.Settings.Default.autoLoadFileName664 = filenameWithPath;
                        else if (deviceFromParent == 0x97)
                            Properties.Settings.Default.autoLoadFileName665 = filenameWithPath;
                        else if (deviceFromParent == 0x98)
                            Properties.Settings.Default.autoLoadFileName667 = filenameWithPath;
                        else
                            Properties.Settings.Default.autoLoadFileName = filenameWithPath;
                        //       checkBoxAutoLoadFile.IsEnabled = true;
                        //       menuItemAutoOpen.IsChecked = true;
                    }
                    else
                    {
                        filenameWithPath = Properties.Settings.Default.autoLoadFileName;
                        char[] charArr = filenameWithPath.ToCharArray();
                        fileNameOnly = filenameWithPath.Substring(filenameWithPath.LastIndexOf("\\") + 1, filenameWithPath.Length - (filenameWithPath.LastIndexOf("\\") + 1));
                        projDirString = filenameWithPath.Replace(fileNameOnly, "");
                    }

                    menuItemProject.IsEnabled = false;
                    menuItemSettings.IsEnabled = false;

                    char[] charsToTrim = { ' ', '\t' };
                    using (StreamReader sr = new StreamReader(filenameWithPath))
                    {
                        string line;
                        regMapRev = sr.ReadLine();
                        if (!regMapRev.Contains("Revision"))
                        {
                            Mouse.OverrideCursor = _previousCursor;
                            menuItemAutoOpen.IsChecked = false;
                            updateStatus("Invalid map file selected, \"" + fileNameOnly + "\", revision number not found. Map load aborted.\n");                          
                            CloseFile_Click(this, null);                          
                            return;
                        }
                        updateStatus("Map file \"" + fileNameOnly + "\" revision " + regMapRev.Substring(9, 3) + " has been successfully opened in directory \"" + projDirString + "\".\n");
                        toolTipsFile = sr.ReadLine();
                        updateStatus("Register field Tool Tips file specified as =\"" + toolTipsFile + "\".\n");
                        currentDevice = sr.ReadLine();

                        if ((currentDevice.Contains("664") && (deviceFromParent == 0x96)) || (currentDevice.Contains("665") && (deviceFromParent == 0x97)) || (currentDevice.Contains("667") && (deviceFromParent == 0x98)))
                        {
                            updateStatus("This map file is for the \"Texas Instruments " + currentDevice + "\" device.\n");
                        }
                        else
                        {
                            Mouse.OverrideCursor = _previousCursor;
                            menuItemAutoOpen.IsChecked = false;
                            updateStatus("File " + fileNameOnly + " specifying device " + currentDevice + " is incorrect for the selected device (channel " + deviceAddressParent + "); IDing - 0x" + deviceFromParent.ToString("X2") + ". Map load aborted.\n");
                            CloseFile_Click(this, null);
                            return;
                        }
    
                        sr.ReadLine();  // file header

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line != "")
                            listRegister.Add(line.Trim(charsToTrim));
                        }
                    }

                    dataGridRegisterMap.Visibility = Visibility.Hidden;
                    listParseRegister = new ObservableCollection<RegisterClass>();
                    dataGridRegisterMap.ItemsSource = listParseRegister;

                    //DataTemplate template = TemplateGenerator.CreateDataTemplate(() =>
                    //{
                    //    //var result2 = new TextBox();
                    //    //result2.SetBinding(TextBox.TextProperty, "BindingPathHere");
                    //    //return result;
                    //    var result2 = new TextBox();
                    //    result2.SetBinding(TextBox.TextProperty, listParseRegister.Hex);
                    //    return result;
                    //});
                    //dataGridRegisterMap.RowDetailsTemplate = template;

                    var progressReport = new Progress<int>((i) => this.progressBar1.Value = (100 * i / (listRegister.Count - 1)));
                    var progressReportText = new Progress<int>((i) => textBoxStatusBar.Text = "Loading register data " + (100 * i / (listRegister.Count - 1)).ToString() + "%");

                    if (await Task.Run(() => processDataList(progressReport, progressReportText)))
                    {
                        setRWtask();
                        updateStatus("Map file \"" + fileNameOnly + "\" has been successfully parsed.\n");
                        bool toolTipsLoad = await Task.Run(() => loadToolTips(progressReport, progressReportText));

                        if(!toolTipsLoad)
                            MessageBox.Show("Error loading Tooltips file. Tooltips will not be availabe.", "Tooltips File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        if (menuItemAutoReadAll.IsChecked)  // auto read all
                            readAll();
                    }
                    else
                    {
                        updateStatus("Map file \"" + fileNameOnly + "\" failed to load.\n");
                        dataGridRegisterMap.Visibility = Visibility.Visible;
                        CloseFile_Click(this, null);
                    }
                    for (int i = 0; i < lmmDevices.Count - 1; i++)  // - 1; do not BF option
                    {
                        if (currentDevice.Contains("662") && ((lmmDevices[i].device > 0x91) && (lmmDevices[i].device < 0x96)))
                        {
                     //       lmmDeviceList[i].address + " = " + lmmDeviceList[i].deviceName + "-0x" + lmmDeviceList[i].device.ToString("X")
                            comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " = " + lmmDevices[i].deviceName + "-0x" + lmmDevices[i].device.ToString("X"));
                         //   comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " - 0x" + lmmDevices[i].device.ToString("X2"));
                        }
                        else if (currentDevice.Contains("664") && (lmmDevices[i].device == 0x96))
                        {
                            comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " = " + lmmDevices[i].deviceName + "-0x" + lmmDevices[i].device.ToString("X"));
                //            comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " - 0x" + lmmDevices[i].device.ToString("X2"));
                        }
                        else if (currentDevice.Contains("665") && (lmmDevices[i].device == 0x97))
                        {
                            comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " = " + lmmDevices[i].deviceName + "-0x" + lmmDevices[i].device.ToString("X"));
                            //            comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " - 0x" + lmmDevices[i].device.ToString("X2"));
                        }
                        else if (currentDevice.Contains("667") && (lmmDevices[i].device == 0x98))
                        {
                            comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " = " + lmmDevices[i].deviceName + "-0x" + lmmDevices[i].device.ToString("X"));
                            //            comboBoxToolBarDevice.Items.Add(lmmDevices[i].address + " - 0x" + lmmDevices[i].device.ToString("X2"));
                        }
                        //if (currentDevice.Contains("662") && ((lmmDevices[i].device > 0x91) && (lmmDevices[i].device < 0x96)))
                        //    comboBoxDevices.Items.Add(new ComboBoxItem { Content = lmmDevices[i].address + " - 0x" + lmmDevices[i].device.ToString("X2") });
                        //else if (currentDevice.Contains("664") && (lmmDevices[i].device > 0x95))
                        //    comboBoxDevices.Items.Add(new ComboBoxItem { Content = lmmDevices[i].address + " - 0x" + lmmDevices[i].device.ToString("X2") });      
                    }

                    for (int j = 0; j < comboBoxToolBarDevice.Items.Count; j++)
                    {
                        if ((comboBoxToolBarDevice.Items[j].ToString().Split(' '))[0].Contains(deviceAddressParent))
                        {
                            comboBoxToolBarDevice.SelectedIndex = j;
                            break;
                        }
                    }
                }

                toolBarTrayRegMap.IsEnabled = true;
                menuItemRegister.IsEnabled = true;
                menuItemActions.IsEnabled = true;
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
                Properties.Settings.Default.autoLoadFileName = "";
                menuItemAutoOpen.IsChecked = false;
                menuItemProject.IsEnabled = true;
                menuItemSettings.IsEnabled = true;
                toolBarTrayRegMap.IsEnabled = false;
                menuItemRegister.IsEnabled = false;
                menuItemActions.IsEnabled = false;
                dataGridRegisterMap.Visibility = Visibility.Visible;
                Mouse.OverrideCursor = _previousCursor;
                MessageBox.Show("Error loading map file... " + ex.Message, "Map File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CloseFile_Click(this, null);
                ButtonStatus_Click(this, null);
            }
        }

        private bool loadToolTips(IProgress<int> progressReport, IProgress<int> progressReportText)
        {
            try
            {
                using (StreamReader sr = new StreamReader(projDirString + toolTipsFile))
                {
                    Dispatcher.BeginInvoke(new Action(() => updateStatus("ToolTip file \"" + toolTipsFile + "\" has been successfully opened in directory \"" + projDirString + "\".\n")));

                    var listTips = new List<string>();
                    string line;
                    int reg = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if ((line.Length > 3) && (line.Substring(0, 2) == "--"))
                        {
                            if (line.Length > 3)
                            {
                                reg = Convert.ToInt32(line.Substring(2, 2), 16);
                                if (reg > 0)
                                {
                                    listToolTipsData.Add(listTips.ToList());
                                    listTips.Clear();
                                }
                                listTips.Add(reg.ToString("00"));
                            }
                            listTips.Add(line.Substring(5, line.Length - 5));
                        }
                        else
                            listTips.Add(line);
                    }
                    listToolTipsData.Add(listTips.ToList());
                }
                Dispatcher.BeginInvoke(new Action(() => updateStatus("ToolTip file \"" + toolTipsFile + "\" has loaded successfully.\n")));
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error L8: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        private void setRWtask()
        {
            Application.Current.Dispatcher.InvokeAsync(new Action(() =>  // this gets called after gird is completely loaded
            {
                setHeaderColor();
                setRWenable();  // set read write CheckBoxes
                dataGridRegisterMap.SelectedItem = 0;
                dataGridRegisterMap.Visibility = Visibility.Visible;
                menuItemProject.IsEnabled = true;
                menuItemRegister.IsEnabled = true;
                menuItemSettings.IsEnabled = true;
                menuItemActions.IsEnabled = true;
                loadComplete = true;
                buttonReadChecked.IsEnabled = true;
                buttonReadAll.IsEnabled = true;
                buttonWriteChecked.IsEnabled = true;
                buttonWriteAll.IsEnabled = true;
                loadQuotes();
            }), DispatcherPriority.ContextIdle);

        }

        private void loadQuotes()
        {
            try
            {
                string[] fileQuotes = Properties.Resources.q.Split('\n');

                foreach(string s in fileQuotes)
                    listQuotes.Add(s);
               
                quotesFound = true;
                changeQuote();
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error L9: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void changeQuote()
        {
            if (quotesFound)
            {
                Random rnd = new Random();
                int num = rnd.Next(listQuotes.Count);
                statusBarLabel.Content = listQuotes[num];
            }
        }

        private bool processDataList(IProgress<int> progressReport, IProgress<int> progressReportText)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() => Mouse.OverrideCursor = Cursors.Wait));
                for (int i = 0; i < listRegister.Count; i++)
                 {
                    progressReport.Report(i);
                    progressReportText.Report(i);
                    Thread.Sleep(25);  // delay in order to update progress; allow mouse cursor to spin
                    if (!parseListLines(listRegister[i]))
                    {
                        listRegister.Clear();
                        Dispatcher.BeginInvoke(new Action(() => listParseRegister.Clear()));
                        return false;
                    }
                };
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error LA: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                listRegister.Clear();
                Dispatcher.BeginInvoke(new Action(() => listParseRegister.Clear()));
                return false;
            }
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listToolTipsData != null)
                    listToolTipsData.Clear();

                if (listRegister != null)
                    listRegister.Clear();

                if (listBitFields != null)
                    listBitFields.Clear();

                if (listParseRegister != null)
                    listParseRegister.Clear();

                if (dataGridRegisterMap != null)
                {
                    dataGridRegisterMap.ItemsSource = null;
                    dataGridRegisterMap.Items.Clear();
                }

                if (comboBoxToolBarDevice.Items.Count > 0)
                {
                    comboBoxToolBarDevice.SelectionChanged -= ComboBoxToolBarDevice_SelectionChanged;
                    comboBoxToolBarDevice.Items.Clear();
                    comboBoxToolBarDevice.SelectionChanged += ComboBoxToolBarDevice_SelectionChanged;
                }

                menuItemProject.IsEnabled = true;
                menuItemRegister.IsEnabled = false;
                menuItemSettings.IsEnabled = true;
                menuItemActions.IsEnabled = false;
                toolBarTrayRegMap.IsEnabled = false;
                progressBar1.Value = 0;
                textBoxStatusBar.Text = "";
                projDirString = "";
                fileNameOnly = "";
                loadComplete = false;
                quotesFound = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error LB: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        public void updateStatus(string statusText)
        {
            textBoxStatus.AppendText(statusText);
            textBoxStatus.Focus();
            textBoxStatus.CaretIndex = textBoxStatus.Text.Length;
            textBoxStatus.ScrollToEnd();
        }

        private void ButtonStatus_Click(object sender, RoutedEventArgs e)
        {
            textBoxStatus.Text = String.Empty;  // clear status
        }

        private void ButtonCommsReset_Click(object sender, RoutedEventArgs e)
        {
            Globals.mcuCommand.lmmCommsReset();
        }
        
        private bool parseListLines(string line)
        {
            try
            {
            var lines = line.Replace(" ", "").Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            byte decRegValue;
            bool result = byte.TryParse(lines[0], out decRegValue);
            if (!result)
            {
                MessageBox.Show("Error parsing map file at register... \"" + line.Replace("\t", " ") + "\"", "Map File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return result;
            }

            int indexDX = 0;
            string[] newLines = new string[13];
            string listLine = "";
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(":"))
                {
                    string bitRange = lines[i].Substring(lines[i].LastIndexOf('[') + 1);
                    var numbers = Regex.Matches(bitRange, @"\d+").OfType<Match>().Select(m => int.Parse(m.Value, CultureInfo.InvariantCulture)).ToArray();
                    if (numbers.Count() == 2)
                    {
                        int index = lines[i].IndexOf('[');
                        string prefix = lines[i].Substring(0, index);
                        for (index = numbers[0]; index >= numbers[1]; index--)
                        {
                            listLine += prefix + " B" + index.ToString() + ",";
                            newLines[indexDX++] = prefix + " B" + index.ToString();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error parsing map file at register... \"" + line.Replace("\t", " ") + "\"", "Map File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else
                {
                    listLine += lines[i] + ",";
                    newLines[indexDX++] = lines[i];
                }
            }

            //        if (decRegValue == 132)
            //          ;

            listBitFields.Add(listLine);

            newLines[11] = parseDefaultVal(newLines[11]);
            byte regValue = Convert.ToByte(newLines[11], 16);

            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                listParseRegister.Add(new RegisterClass()
                {
                    Dec = decRegValue,
                    Hex = newLines[1],
                    Register = newLines[2],
                    D7 = Convert.ToBoolean(regValue & 0x80),
                    D6 = Convert.ToBoolean(regValue & 0x40),
                    D5 = Convert.ToBoolean(regValue & 0x20),
                    D4 = Convert.ToBoolean(regValue & 0x10),
                    D3 = Convert.ToBoolean(regValue & 0x08),
                    D2 = Convert.ToBoolean(regValue & 0x04),
                    D1 = Convert.ToBoolean(regValue & 0x02),
                    D0 = Convert.ToBoolean(regValue & 0x01),
                    Value = newLines[11],
                    Default = newLines[11],
                    Type = newLines[12],
                    Read = false,
                    Write = false,
                })
            ));

            return result;
        }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error LC: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }
        }

        private string parseDefaultVal(string defaultVal)
        {
            if (defaultVal == null)
                return "";

            int resultInt = 0;
            char[] characters = defaultVal.ToCharArray();
            Array.Reverse(characters);

            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == '1')
                    resultInt += 1 << i;
            }

            return "0x" + resultInt.ToString("X2"); ;
        }

        private bool[] getAllCheckedThisRow(int row)
        {
            bool[] rowChecks = new bool[8];
            int i = 0;

            foreach (var p in listParseRegister[row].GetType().GetProperties().Where(p => p.PropertyType == typeof(bool)))
            {
                if (i < 8)
                    rowChecks[i++] = (Boolean)p.GetValue(listParseRegister[row], null);
            }

            return rowChecks;
        }

        private void DataGridRegisterMap_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            changeQuote();

            progressBar1.Value = 0;
            textBoxStatusBar.Text = "";

            if (listBitFields.Count == 0)
                return;

            string lineFromList = "";
            try
            {
                currentSelectedItem = (RegisterClass)dataGridRegisterMap.CurrentItem;
                if (currentSelectedItem == null)
                    return;

                if (currentSelectedItem.Type.Contains("r"))
                    buttonReadSelected.IsEnabled = true;
                else
                    buttonReadSelected.IsEnabled = false;

                if (currentSelectedItem.Type.Contains("w"))
                    buttonWriteSelected.IsEnabled = true;
                else
                    buttonWriteSelected.IsEnabled = false;

                int row = currentSelectedItem.Dec;

                string[] bitfields = null;
                for (int i = 0; 0 < listBitFields.Count; i++)
                {
                    bitfields = listBitFields[i].Split(',');
                    if (bitfields[0].Contains(row.ToString()))
                    {
                        lineFromList = listBitFields[i];
                        //            setHeaderColors(i);
                        break;
                    }
                }

                for (int i = 3; i < 11; i++)
                    dataGridRegisterMap.Columns[i].Header = bitfields[i];
                
                if (setFocusAfterMultiRead)
                {
                    dataGridRegisterMap.LayoutUpdated += DataGridRegisterMap_LayoutUpdated;  // turn this event back on if read or write all was ran
       //             dispatcherTimer.Start();                 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error parsing bit field list at register... \"" + lineFromList.Replace("\t", " ") + "\"", "Bit Field Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGridRegisterMap_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.AlternationIndex == 0)
            {
                e.Row.Background = new SolidColorBrush(Colors.LightGray);
            }
            else if (e.Row.AlternationIndex == 1)
            {
                e.Row.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void DataGridRegisterMap_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////
            //       return;
            // THIS COMPLETELY KILLS ALL EVENTS ASSOCIATED WITH CHECKBOXES!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //        if I need to be able to search properties for checkBox values as below comment out the code below comments

            //var item = dataGridRegisterMap.Items[row];
            //var mycheckbox = dataGridRegisterMap.Columns[i].GetCellContent(item) as CheckBox;

            //           if (mycheckbox.IsChecked == true) 
            /////////////////////////////////////////////////////////////////////////////////////////////
            if (e.Column is DataGridCheckBoxColumn && !e.Column.IsReadOnly)
            {
                var checkboxFactory = new FrameworkElementFactory(typeof(CheckBox));
                checkboxFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                checkboxFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                checkboxFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding(e.PropertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                // Binding = BindingMode.TwoWay,

                e.Column = new DataGridTemplateColumn
                {
                    Header = e.Column.Header,
                    CellTemplate = new DataTemplate { VisualTree = checkboxFactory },
                    SortMemberPath = e.Column.SortMemberPath
                };
            }

            e.Column.HeaderStyle = new Style(typeof(DataGridColumnHeader));
            e.Column.HeaderStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

            var cellStyle = new Style
            {
                TargetType = typeof(TextBlock),
                Setters =
                    {
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center),
                        new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center),
                        new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center),
                    }
            };

            var c = e.Column as DataGridTextColumn;
            if (c != null)
                c.ElementStyle = cellStyle;
        }

        private void DataGridRegisterMap_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regMapWindowClosing)
                return;

            try
            {
                Control ctrl = FocusManager.GetFocusedElement(this) as Control;
                if (ctrl.Parent != null && ctrl.Parent.GetType() != typeof(DataGridCell))
                {
                    int j = 7;
                    for (int i = 3; i < 11; i++)
                    {
                        dataGridRegisterMap.Columns[i].Header = "D" + j--.ToString();
                    }
                }
                setHeaderColor();
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error LD: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                ;  // get exception when shutting down
            }
        }

        private void setHeaderColor()
        {
            Style style0 = new Style(typeof(DataGridColumnHeader));
            style0.Setters.Add(new Setter { Property = BackgroundProperty, Value = myBrushDefault });
            style0.Setters.Add(new Setter { Property = BorderBrushProperty, Value = Brushes.Gray });
            style0.Setters.Add(new Setter { Property = BorderThicknessProperty, Value = new Thickness(0, 0, 1, 1) });
            style0.Setters.Add(new Setter { Property = System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });

            //      for (int i = 3; i < 11; i++)
            for (int i = 0; i < 16; i++)
                dataGridRegisterMap.Columns[i].HeaderStyle = style0;
        }

        private Style getHeaderStyle(Style styleBitField, int columnNum, bool onOff, string regHex)
        {
            var styleListBox = new Style(typeof(ListBox));
            Style newStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader), styleBitField);

            newStyle.Setters.Add(new Setter { Property = BorderBrushProperty, Value = Brushes.LightGray });
            newStyle.Setters.Add(new Setter { Property = BorderThicknessProperty, Value = new Thickness(0, 0, 1, 1) });
            newStyle.Setters.Add(new Setter { Property = System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });
            if (onOff)
                newStyle.Setters.Add(new Setter { Property = BackgroundProperty, Value = myBrushOn });
            else
                newStyle.Setters.Add(new Setter { Property = BackgroundProperty, Value = myBrushOff });

            string[] tipsByBit = new string[8];
            bool foundReg = false;
            int totLines = 0;
            for (int i = 0; i < listToolTipsData.Count; i++)
            {
                if (foundReg)
                    break;
                foreach (var list in listToolTipsData)
                {
                    if (foundReg)
                        break;
                    List<int> fieldIndex = new List<int>();

                    foreach (string line in list as IEnumerable) //casting to IEnumerable will let you iterate
                    {
                        totLines++;
                        if ((line == regHex.Substring(2, 2)) && (!foundReg))
                        {
                            foundReg = true;
                            continue;
                        }
                        else if ((line != regHex.Substring(2, 2)) && (!foundReg))
                        {
                            continue;
                        }
                        else if (line.Contains(":") && line.Contains("[") && line.Contains("]"))
                        {
                            string bitRange = line.Substring(line.LastIndexOf('[') + 1);
                            var numbers = Regex.Matches(bitRange, @"\d+").OfType<Match>().Select(m => int.Parse(m.Value, CultureInfo.InvariantCulture)).ToArray();

                            if (numbers.Count() > 1)
                            {
                                int index = line.IndexOf(';') + 1;
                                string firstLine = line.Substring(index, line.Length - index);
                                fieldIndex.Clear();
                                for (index = numbers[0]; index >= numbers[1]; index--)
                                {
                                    tipsByBit[index] += firstLine;
                                    if (line.Substring(Math.Max(0, line.Length - 2)) == ";;")
                                        tipsByBit[index] = tipsByBit[index].TrimEnd(';');
                                    else
                                        tipsByBit[index] += "\n";
                                    fieldIndex.Add(index);
                                }
                            }
                        }
                        else if (line.Contains("["))
                        {
                            string test3 = line.Substring(line.IndexOf('[') + 1, 1);
                            fieldIndex.Clear();
                            fieldIndex.Add(int.Parse(test3, CultureInfo.InvariantCulture));
                            int index1 = line.IndexOf(';') + 1;
                            string firstLine = line.Substring(index1, line.Length - index1);
                            tipsByBit[fieldIndex[0]] += firstLine;
                            if (line.Substring(Math.Max(0, line.Length - 2)) == ";;")
                                tipsByBit[fieldIndex[0]] = tipsByBit[fieldIndex[0]].TrimEnd(';');
                            else
                                tipsByBit[fieldIndex[0]] += "\n";
                        }
                        else
                        {
                            for (int j = 0; j < fieldIndex.Count; j++)
                            {
                                tipsByBit[fieldIndex[j]] += line;
                                if (line.Substring(Math.Max(0, line.Length - 2)) == ";;")
                                    tipsByBit[fieldIndex[j]] = tipsByBit[fieldIndex[j]].TrimEnd(';');
                                else
                                    tipsByBit[fieldIndex[j]] += "\n";
                            }
                        }
                    }
                }
            }

            switch (columnNum)
            {
                case 3:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[7]));  // bit 7
                    break;
                case 4:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[6]));  // bit 6
                    break;
                case 5:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[5]));  // bit 5
                    break;
                case 6:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[4]));  // bit 4
                    break;
                case 7:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[3]));  // bit 3
                    break;
                case 8:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[2]));  // bit 2
                    break;
                case 9:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[1]));  // bit 1
                    break;
                case 10:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, tipsByBit[0]));  // bit 0
                    break;
                default:
                    newStyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, ""));
                    break;
            }
            return newStyle;
        }

        private void setHeaderColorsWorker()
        {
            return;

//            try
//            {
//                Style styleColHeader = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
//                RegisterClass getrow = (RegisterClass)dataGridRegisterMap.Items[dataGridRegisterMap.SelectedIndex];

//                bool[] getChecks = getAllCheckedThisRow(dataGridRegisterMap.SelectedIndex);
//            //    byte newValue = 0;
//                for (int i = 3; i < 11; i++)
//                {
//                    styleColHeader = null;
//                    if (getChecks[i - 3] == true)
//                    {
//                        dataGridRegisterMap.Columns[i].HeaderStyle = getHeaderStyle(styleColHeader, i, true, getrow.Hex);
//               //         newValue += (byte)(1 << (7 - (i - 3)));
//                    }
//                    else
//                        dataGridRegisterMap.Columns[i].HeaderStyle = getHeaderStyle(styleColHeader, i, false, getrow.Hex);
//                }
////                getrow.Value = "0x" + newValue.ToString("X2");

//                setRwCheckBox();  // this will keep the read or write checkbox from being re-enabled
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Exception at setHeaderColorsWorker " + dataGridRegisterMap.SelectedIndex + " " + ex.Message, "Read Selected Exception Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
        }

        private void setHeaderColors(int row)
        {
            setHeaderColorsWorker();
        }

        private void SetHeader()
        {
            if (!loadComplete)
                return;

            RegisterClass getRow = (RegisterClass)dataGridRegisterMap.CurrentItem;

            if (getRow == null)
                return;

            int row = getRow.Dec;
            string[] bitfields = null;
            for (int i = 0; 0 < listBitFields.Count; i++)
            {
                bitfields = listBitFields[i].Split(',');
                if (bitfields[0].Contains(row.ToString()))
                {
                    setHeaderColors(i);
                    break;
                }
            }
        }

        private void DataGridRegisterMap_LayoutUpdated(object sender, EventArgs e)
        {
         //   SetHeader();
        }

        private void setRWenable()
        {
            Cursor _previousCursor;
            _previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                for (int i = 0; i < listParseRegister.Count(); i++)
                {
                    var item = dataGridRegisterMap.Items[i];

                    object viewItem = dataGridRegisterMap.ItemContainerGenerator.ContainerFromItem(item);
                    if (viewItem == null)
                    {
                        dataGridRegisterMap.ScrollIntoView(dataGridRegisterMap.Items[i]);
                        dataGridRegisterMap.UpdateLayout();
                    }

                    bool mycheckbox = dataGridRegisterMap.Columns[14].GetCellContent(item).IsEnabled;
                    RegisterClass getrow = (RegisterClass)dataGridRegisterMap.Items[i];

                    if ((getrow.Type.Contains("r")) && (!getrow.Type.Contains("w")))
                        dataGridRegisterMap.Columns[15].GetCellContent(item).IsEnabled = false;
                    else if ((!getrow.Type.Contains("r")) && (getrow.Type.Contains("w")))
                    {
                        dataGridRegisterMap.Columns[14].GetCellContent(item).IsEnabled = false;
                    }
                }
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error LE: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                Mouse.OverrideCursor = _previousCursor;
            }
        }

        private void setRwCheckBox()
        {
            int row = dataGridRegisterMap.SelectedIndex;

            if (row == -1)
                return;

            var item = dataGridRegisterMap.Items[row];
            RegisterClass getrow = (RegisterClass)dataGridRegisterMap.Items[row];

            if ((getrow.Type.Contains("r")) && (!getrow.Type.Contains("w")))
                dataGridRegisterMap.Columns[15].GetCellContent(item).IsEnabled = false;
            else if ((!getrow.Type.Contains("r")) && (getrow.Type.Contains("w")))
                dataGridRegisterMap.Columns[14].GetCellContent(item).IsEnabled = false;
        }

        private void DataGridRegisterMap_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;

            if ((string)column.Header == "Read")
            {
                e.Handled = true;
                for (int i = 0; i < listParseRegister.Count(); i++)
                {
                    RegisterClass getrow = (RegisterClass)dataGridRegisterMap.Items[i];
                    if (getrow.Type.Contains("r"))
                    {
                        if (allCheckedRead == false)
                            getrow.Read = true;
                        else
                            getrow.Read = false;
                    }
                }
                if (allCheckedRead == false)
                    allCheckedRead = true;
                else
                    allCheckedRead = false;
            }
            else if ((string)column.Header == "Write")
            {
                e.Handled = true;
                for (int i = 0; i < listParseRegister.Count(); i++)
                {
                    RegisterClass getrow = (RegisterClass)dataGridRegisterMap.Items[i];
                    if (getrow.Type.Contains("w"))
                    {
                        if (allCheckedWrite == false)
                            getrow.Write = true;
                        else
                            getrow.Write = false;
                    }
                }
                if (allCheckedWrite == false)
                    allCheckedWrite = true;
                else
                    allCheckedWrite = false;
            }
        }

        private void DataGridRegisterMap_CurrentCellChanged(object sender, EventArgs e)
        {
            if (!busyReadingWriting)
                dataGridRegisterMap.LayoutUpdated += DataGridRegisterMap_LayoutUpdated;  // turn this event back on if read or write all was ran

            SetHeader();
        }

        private void MainWindowRegMap_Deactivated(object sender, EventArgs e)
        {
            if (loadComplete)
            {
                dataGridRegisterMap.LayoutUpdated -= DataGridRegisterMap_LayoutUpdated;
                setHeaderColor();
       //         buttonReadSelected.IsEnabled = false;
         //       buttonWriteSelected.IsEnabled = false;
           //     buttonReadChecked.IsEnabled = false;
             //   buttonReadAll.IsEnabled = false;
               // buttonWriteChecked.IsEnabled = false;
               // buttonWriteAll.IsEnabled = false;
            }
        }

        private void MainWindowRegMap_Activated(object sender, EventArgs e)
        {
            try
            {
                dataGridRegisterMap.LayoutUpdated += DataGridRegisterMap_LayoutUpdated;
                DataGridRegisterMap_LayoutUpdated(this, null);

                if (loadComplete)
                {
                    buttonReadChecked.IsEnabled = true;
                    buttonReadAll.IsEnabled = true;
                    buttonWriteChecked.IsEnabled = true;
                    buttonWriteAll.IsEnabled = true;
                }

                if (currentSelectedItem == null)
                    return;

                if (currentSelectedItem.Type.Contains("r"))
                    buttonReadSelected.IsEnabled = true;

                if (currentSelectedItem.Type.Contains("w"))
                    buttonWriteSelected.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ;
            }
        }

        //private void DataGridRegisterMapRegainFocus()
        //{
        //    for (int i = 0; i < dataGridRegisterMap.Items.Count; i++)
        //    {
        //        DataGridRow row = (DataGridRow)dataGridRegisterMap.ItemContainerGenerator.ContainerFromIndex(i);
        //        TextBlock cellContent = dataGridRegisterMap.Columns[1].GetCellContent(row) as TextBlock;
        //        if (cellContent != null && cellContent.Text.Equals(currentSelectedItem.Hex))
        //        {
        //            object item = dataGridRegisterMap.Items[i];
        //            dataGridRegisterMap.SelectedItem = item;
        //            dataGridRegisterMap.ScrollIntoView(item);
        //            //         row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //            dataGridRegisterMap.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //            break;
        //        }
        //    }

            

        //    currentSelectedItem = (RegisterClass)dataGridRegisterMap.CurrentItem;
        //    if (currentSelectedItem == null)
        //        return;

            

        //    int rowSelected = currentSelectedItem.Dec;
        //    string lineFromList = "";
        //    string[] bitfields = null;
        //    for (int i = 0; 0 < listBitFields.Count; i++)
        //    {
        //        bitfields = listBitFields[i].Split(',');
        //        if (bitfields[0].Contains(rowSelected.ToString()))
        //        {
        //            lineFromList = listBitFields[i];
        //            //            setHeaderColors(i);
        //            break;
        //        }
        //    }

        //    for (int i = 3; i < 11; i++)
        //        dataGridRegisterMap.Columns[i].Header = bitfields[i];

        //    //     DataGridRegisterMap_SelectedCellsChanged(this, null);
        //}

        private void DataGridRegisterMapRegainFocus(RegisterClass currentRow, DataGridCellInfo currCell)
        {
            //for (int i = 0; i < dataGridRegisterMap.Items.Count; i++)
            //{
            //    DataGridRow row = (DataGridRow)dataGridRegisterMap.ItemContainerGenerator.ContainerFromIndex(i);
            //    TextBlock cellContent = dataGridRegisterMap.Columns[1].GetCellContent(row) as TextBlock;
            //    if (cellContent != null && cellContent.Text.Equals(currentSelectedItem.Hex))
            //    {
            //        object item = dataGridRegisterMap.Items[i];
            //        dataGridRegisterMap.SelectedItem = item;
            //        dataGridRegisterMap.ScrollIntoView(item);
            //        //         row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            //      //  dataGridRegisterMap.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            //        break;
            //    }
            //}

            dataGridRegisterMap.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            currentSelectedItem = currentRow;
            if (currentSelectedItem == null)
                return;

            int rowSelected = currentSelectedItem.Dec;
            string lineFromList = "";
            string[] bitfields = null;
            for (int i = 0; 0 < listBitFields.Count; i++)
            {
                bitfields = listBitFields[i].Split(',');
                if (bitfields[0].Contains(rowSelected.ToString()))
                {
                    lineFromList = listBitFields[i];
                    //            setHeaderColors(i);
                    break;
                }
            }

            for (int i = 3; i < 11; i++)
                dataGridRegisterMap.Columns[i].Header = bitfields[i];

            dataGridRegisterMap.CurrentCell = currCell;

            //     DataGridRegisterMap_SelectedCellsChanged(this, null);
        }

        private void setChecksFromRead(string readVal)
        {
            try
            {
                changeQuote();
                byte regValue = Convert.ToByte(readVal, 16);

                currentSelectedItem.D7 = Convert.ToBoolean(regValue & 0x80);
                currentSelectedItem.D6 = Convert.ToBoolean(regValue & 0x40);
                currentSelectedItem.D5 = Convert.ToBoolean(regValue & 0x20);
                currentSelectedItem.D4 = Convert.ToBoolean(regValue & 0x10);
                currentSelectedItem.D3 = Convert.ToBoolean(regValue & 0x08);
                currentSelectedItem.D2 = Convert.ToBoolean(regValue & 0x04);
                currentSelectedItem.D1 = Convert.ToBoolean(regValue & 0x02);
                currentSelectedItem.D0 = Convert.ToBoolean(regValue & 0x01);
                currentSelectedItem.Value = readVal;
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(this, "Error LF: " + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private void ButtonReadSelected_Click(object sender, RoutedEventArgs e)
        {
            RegisterClass lastSelected = currentSelectedItem;
            DataGridCellInfo currCell = dataGridRegisterMap.CurrentCell;

            Cursor _previousCursor = null;

            try
            {
                _previousCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
                SPIcommadReturnData scrd = new SPIcommadReturnData();

                lockDuringReadWrite();
                mainWindow.textBoxSPIcommandStatus.IsEnabled = false;
                busyReadingWriting = true;

                changeQuote();
                string returnString = "";

                if (currentSelectedItem == null)
                {
                    Mouse.OverrideCursor = _previousCursor;
                    unlockDuringReadWrite();
                    DataGridRegisterMap_SelectedCellsChanged(this, null);  // see if there is a selection made, the grid may have lost focus which nulled the selected item
                    if (currentSelectedItem == null)  // re-test for null
                        return;
                }

                regClass.externallyChecked = false;  // allow read only registers to update the current value if they were modified by some other "mechanism"
                if (currentSelectedItem.Type.Contains("r"))
                {
                    if (!fakeRead)
                        Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString, false, null);

                    if (returnString != "")
                    {
                        setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2, 4));
                    updateStatus(returnString + "\n");
                }
                }


                regClass.externallyChecked = true;
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);

                //          throw new InvalidOperationException("Test Exception initiated by Bad Dog");  // test exception
            }
            catch (Exception ex)
            {
                updateStatus("Exception occured processing line " + dataGridRegisterMap.SelectedIndex + " in file " + fileNameOnly + ". Read Selected Aborted.\n");
                MessageBox.Show("Exception occured processing line " + dataGridRegisterMap.SelectedIndex + " in file " + fileNameOnly + ". Read Selected Aborted.", "Read Selected Exception Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
            }
        }

        private void ButtonWriteSelected_Click(object sender, RoutedEventArgs e)
        {
            RegisterClass lastSelected = currentSelectedItem;
            DataGridCellInfo currCell = dataGridRegisterMap.CurrentCell;

            Cursor _previousCursor = null;

            try
            {
                _previousCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
                lockDuringReadWrite();
                busyReadingWriting = true;

                mainWindow.textBoxSPIcommandStatus.IsEnabled = false;

                changeQuote();
                string returnString = "";

                if (currentSelectedItem == null)
                {
                    mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                    unlockDuringReadWrite();
                    Mouse.OverrideCursor = _previousCursor;
                    busyReadingWriting = false;
                    return;
                }

                if (currentSelectedItem.Type.Contains("w"))
                {
                    byte[] regValue = new byte[1];
                    regValue[0] = Convert.ToByte(currentSelectedItem.Value, 16);
                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), currentSelectedItem.Dec, regValue, ref returnString, false, (bool)((TPS92662Control)lmmWindow).checkBoxAckEnable.IsChecked);
                    updateStatus(returnString + "\n");
                }

                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
            }
            catch (Exception ex)
            {
                updateStatus("Exception occured processing line " + dataGridRegisterMap.SelectedIndex + " in file " + fileNameOnly + ". Write Selected Aborted.\n");
                MessageBox.Show("Exception occured processing line " + dataGridRegisterMap.SelectedIndex + " in file " + fileNameOnly + ". Write Selected Aborted.", "Write Selected Exception Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
            }
        }

        bool fakeRead = false;
        bool setFocusAfterMultiRead = false;
        DispatcherTimer dispatcherTimer;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // super hack to get the grid to update properly after a read or write all; fixes a weird bug where the scroll slider or scroll arrow keys would lock up
            // the scoll function; had to have a row selected, then read / write all then select another row, then scroll and scroll slider or arrows would lock had
            // to click on status box to then back to grid to get everything right again; tried to call focus routines using dispatcher invoke and Task async but that didn't work fully
            // this forces an update to the grid on a seperate thread
            dispatcherTimer.Stop();
            setFocusAfterMultiRead = false;
            fakeRead = true;
            ButtonReadSelected_Click(this, null);
            fakeRead = false;
        }

        private void lockDuringReadWrite()
        {
            toolBarTrayRegMap.IsEnabled = false;
            dockPanelRegMap.IsEnabled = false;
            groupBoxRegister.IsEnabled = false;
        }

        private void unlockDuringReadWrite()
        {
            toolBarTrayRegMap.IsEnabled = true;
            dockPanelRegMap.IsEnabled = true;
            groupBoxRegister.IsEnabled = true;
        }

        private void ButtonReadChecked_Click(object sender, RoutedEventArgs e)
            {
                readAllChecked();
        }

        private void readAllChecked()
        {
            DataGridCellInfo currCell = dataGridRegisterMap.CurrentCell;
            RegisterClass lastSelected = currentSelectedItem;

            Cursor _previousCursor = null;
            _previousCursor = Mouse.OverrideCursor;
            int i = 0;

            try
            {
                lockDuringReadWrite();
                mainWindow.textBoxSPIcommandStatus.IsEnabled = false;
                changeQuote();
                busyReadingWriting = true;
                Mouse.OverrideCursor = Cursors.Wait;

                if (currentSelectedItem != null)
                {
                    dataGridRegisterMap.LayoutUpdated -= DataGridRegisterMap_LayoutUpdated;
                    if (dispatcherTimer == null)
                    {
                        dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                        dispatcherTimer.Tick += dispatcherTimer_Tick;
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                    }
                }

                regClass.externallyChecked = false;  // allow read only registers to update the current value if they were modified by some other "mechanism"
                string returnString = "";

                for (i = 0; i < listParseRegister.Count; i++)
                {
                    if ((listParseRegister[i].Read == true) && (listParseRegister[i].Type.Contains("r")))
                    {
                    //    setHeaderColorsWorker();  // cludge to get currentSelectedItem.Value to update properly; sometimes it will not 
                    //    SetHeader();
                        currentSelectedItem = listParseRegister[i];                      
                        Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString, false, null);
                        Console.WriteLine("readAllChecked: i = " + i + " " + listParseRegister[i].Dec + " = " + returnString);
                        if (returnString.Contains("Error"))
                        {
                            mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                            updateStatus(returnString + "\n");
                            Mouse.OverrideCursor = _previousCursor;
                            unlockDuringReadWrite();
                            busyReadingWriting = false;

                            currentSelectedItem = lastSelected;
                            if (currentSelectedItem != null)
                            {
                                DataGridRegisterMapRegainFocus(lastSelected, currCell);
                                setHeaderColorsWorker();
                                setFocusAfterMultiRead = true;
                            }
                            else
                                dataGridRegisterMap.Focus();
                            return;
                        }
                        setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2, 4));
                        updateStatus(returnString + "\n");
                    }
                }
                regClass.externallyChecked = true;

                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;

                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                currentSelectedItem = lastSelected;
                if (currentSelectedItem != null)
                {
                    DataGridRegisterMapRegainFocus(lastSelected, currCell);
                    setHeaderColorsWorker();
                    setFocusAfterMultiRead = true;
                }
                else
                    dataGridRegisterMap.Focus();
            }
            catch (Exception ex)
            {
                updateStatus("Exception occured processing line " + i + " in file " + fileNameOnly + ". Read all Checked Aborted.\n");
                MessageBox.Show("Exception occured processing line " + i + " in file " + fileNameOnly + ". Read all Checked Aborted.", "Read All Checked Exception Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
            }
        }

        private void ButtonReadAll_Click(object sender, RoutedEventArgs e)
        {
            readAll();
        }

        private void readAll()
        {
            DataGridCellInfo currCell = dataGridRegisterMap.CurrentCell;
            RegisterClass lastSelected = currentSelectedItem;

            Cursor _previousCursor = null;
            _previousCursor = Mouse.OverrideCursor;
            int i = 0;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                lockDuringReadWrite();
                mainWindow.textBoxSPIcommandStatus.IsEnabled = false;
                changeQuote();
                busyReadingWriting = true;

                if (currentSelectedItem != null)
                {
                    dataGridRegisterMap.LayoutUpdated -= DataGridRegisterMap_LayoutUpdated;
                    if (dispatcherTimer == null)
                    {
                        dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                        dispatcherTimer.Tick += dispatcherTimer_Tick;
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                    }
                }

                regClass.externallyChecked = false;  // allow read only registers to update the current value if they were modified by some other "mechanism"
                string returnString = "";

                //int totalRegs = listParseRegister.Count;
                //int numRegs32ThisDevice = listParseRegister.Count / 32;
                //totalRegs -= numRegs32ThisDevice * 32;
                //int numRegs16ThisDevice = totalRegs / 16;
                //totalRegs -= numRegs16ThisDevice * 16;
                //int numRegs12ThisDevice = totalRegs / 12;
                //totalRegs -= numRegs12ThisDevice * 12;
                //int numRegs4ThisDevice = totalRegs / 4;
                //totalRegs -= numRegs4ThisDevice * 4;
                //int numRegs3ThisDevice = totalRegs / 3;
                //totalRegs -= numRegs3ThisDevice * 3;
                //int numRegs2ThisDevice = totalRegs / 2;
                ////      totalRegs -= numRegs2ThisDevice * 2;

                int numRegs32ThisDevice = 0;
                int numRegs16ThisDevice = 0;
                if (currentDevice.Contains("664") && (listParseRegister.Count > MIN_NUM_REGS_664))  // check for number of regs so if user makes a reister map with only a portion of the full register range
                {
                    numRegs32ThisDevice = 3;
                }
                else if (currentDevice.Contains("665") && (listParseRegister.Count > MIN_NUM_REGS_665))  // check for number of regs so if user makes a reister map with only a portion of the full register range
                {
                    numRegs32ThisDevice = 3;
                }
                else if (currentDevice.Contains("667") && (listParseRegister.Count > MIN_NUM_REGS_667))  // check for number of regs so if user makes a reister map with only a portion of the full register range
                {
                    numRegs32ThisDevice = 3;
                }
                else if ((currentDevice.Contains("662") && (listParseRegister.Count > MIN_NUM_REGS_662)))
                {
                    numRegs32ThisDevice = 3;
                    numRegs16ThisDevice = 1;
                }

                for (i = 0; i < listParseRegister.Count; i++)
                {
                    //         if (listParseRegister[i].Read == true)
                    //       {
                    currentSelectedItem = listParseRegister[i];
                    if (numRegs32ThisDevice > 0)
                    {
                        Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ32, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString, false, null);
                        if (returnString.Contains("Error"))
                        {
                            Mouse.OverrideCursor = _previousCursor;
                            mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                            updateStatus(returnString + "\n");
                            unlockDuringReadWrite();
                            busyReadingWriting = false;

                            if (currentSelectedItem != null)
                            {
                                DataGridRegisterMapRegainFocus(lastSelected, currCell);
                                setHeaderColorsWorker();
                                setFocusAfterMultiRead = true;
                            }
                            else
                                dataGridRegisterMap.Focus();
                            return;
                        }
                        for (int j = 0; j < 32; j++)
                        {
                            currentSelectedItem = listParseRegister[i + j];
                            setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2 + (j * 6), 4));
                        }
                        i += 31;
                        numRegs32ThisDevice--;
                    }
                    else if (numRegs16ThisDevice > 0)
                    {
                        Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ16, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString, false, null);
                        if (returnString.Contains("Error"))
                        {
                            updateStatus(returnString + "\n");
                            Mouse.OverrideCursor = _previousCursor;
                            return;
                        }
                        for (int j = 0; j < 16; j++)
                        {
                            currentSelectedItem = listParseRegister[i + j];
                            setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2 + (j * 6), 4));
                        }
                        i += 15;
                        numRegs16ThisDevice--;
                    }
                    //else if (numRegs12ThisDevice > 0)
                    //{
                    //    Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ12, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString);
                    //    for (int j = 0; j < 12; j++)
                    //    {
                    //        currentSelectedItem = listParseRegister[i + j];
                    //        setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2 + (j * 6), 4));
                    //    }
                    //    i += 11;
                    //    numRegs12ThisDevice--;
                    //}
                    //else if (numRegs4ThisDevice > 0)
                    //{
                    //    Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ4, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString);
                    //    for (int j = 0; j < 4; j++)
                    //    {
                    //        currentSelectedItem = listParseRegister[i + j];
                    //        setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2 + (j * 6), 4));
                    //    }
                    //    i += 3;
                    //    numRegs4ThisDevice--;
                    //}
                    //else if (numRegs3ThisDevice > 0)
                    //{
                    //    Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ3, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString);
                    //    for (int j = 0; j < 3; j++)
                    //    {
                    //        currentSelectedItem = listParseRegister[i + j];
                    //        setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2 + (j * 6), 4));
                    //    }
                    //    i += 2;
                    //    numRegs3ThisDevice--;
                    //}
                    //else if (numRegs2ThisDevice > 0)
                    //{
                    //    Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ2, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString);
                    //    for (int j = 0; j < 2; j++)
                    //    {
                    //        currentSelectedItem = listParseRegister[i + j];
                    //        setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2 + (j * 6), 4));
                    //    }
                    //    i += 1;
                    //    numRegs2ThisDevice--;
                    //}
                    else
                    {
                        if (listParseRegister[i].Type.Contains("r"))
                        {
                            Globals.mcuCommand.LmmRdReg(MCUcommands.FrameInit.READ1, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), (byte)currentSelectedItem.Dec, ref returnString, false, null);
                            if (returnString.Contains("Error"))
                            {
                                Mouse.OverrideCursor = _previousCursor;
                                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                                updateStatus(returnString + "\n");
                                unlockDuringReadWrite();
                                busyReadingWriting = false;

                                currentSelectedItem = lastSelected;
                                if (currentSelectedItem != null)
                                {
                                    DataGridRegisterMapRegainFocus(lastSelected, currCell);
                                    setHeaderColorsWorker();
                                    setFocusAfterMultiRead = true;
                                }
                                else
                                    dataGridRegisterMap.Focus();
                                return;
                            }
                            setChecksFromRead(returnString.Substring(returnString.IndexOf(':') + 2, 4));
                        }
                        if (currentDevice.Contains("664"))
                        {
                            if (listParseRegister[i].Dec == 100)
                                numRegs32ThisDevice = 1;
                        }
                        else if (currentDevice.Contains("665"))
                        {
                            if (listParseRegister[i].Dec == 100)
                                numRegs16ThisDevice = 1;
                            else if (listParseRegister[i].Dec == 143)
                                numRegs32ThisDevice = 1;
                        }
                    }
                    updateStatus(returnString + "\n");
                    //  }
                }
                regClass.externallyChecked = true;

                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;

                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                currentSelectedItem = lastSelected;
                if (currentSelectedItem != null)
                {
                    DataGridRegisterMapRegainFocus(lastSelected, currCell);
                    setHeaderColorsWorker();
                    setFocusAfterMultiRead = true;
                }
                else
                    dataGridRegisterMap.Focus();
            }
            catch (Exception ex)
            {
                updateStatus("Exception occured processing line " + i + " in file " + fileNameOnly + ". Read all Aborted.\n");
                MessageBox.Show("Exception occured processing line " + i + " in file " + fileNameOnly + ". Read all Aborted.", "Read All Exception Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
            }
        }

        private void ButtonWriteChecked_Click(object sender, RoutedEventArgs e)
            {
                writeAllChecked();
        }

        async Task PutTaskDelay(int miliSecs)
        {
            await Task.Delay(miliSecs);
        }

        private void writeAllChecked()
        {
            DataGridCellInfo currCell = dataGridRegisterMap.CurrentCell;
            RegisterClass lastSelected = currentSelectedItem;

            Cursor _previousCursor = null;
            _previousCursor = Mouse.OverrideCursor;
            int i = 0;

            try
            {
                lockDuringReadWrite();
                mainWindow.textBoxSPIcommandStatus.IsEnabled = false;
                changeQuote();
                busyReadingWriting = true;
                Mouse.OverrideCursor = Cursors.Wait;

                if (currentSelectedItem != null)
                {
                    dataGridRegisterMap.LayoutUpdated -= DataGridRegisterMap_LayoutUpdated;
                    if (dispatcherTimer == null)
                    {
                        dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                        dispatcherTimer.Tick += dispatcherTimer_Tick;
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                    }
                }

                byte[] regValue = new byte[1];
                string returnString = "";
        //        byte newValue = 0;
                for (i = 0; i < listParseRegister.Count; i++)
                {
                    if (listParseRegister[i].Write == true)
                    {
                        currentSelectedItem = listParseRegister[i];
                        //bool[] getChecks = getAllCheckedThisRow(dataGridRegisterMap.SelectedIndex);
                        //newValue = 0;
                        //for (int j = 3; j < 11; j++)
                        //{
                        //    if (getChecks[j - 3] == true)
                        //        newValue += (byte)(1 << (7 - (j - 3)));
                        //}
                        //regValue[0] = newValue;
                  //      Console.WriteLine("writeAllChecked 1: i = " + i + " regValue " + regValue[0].ToString("X2") + " currentSelectedItem " + currentSelectedItem.Value);
                        regValue[0] = Convert.ToByte(currentSelectedItem.Value, 16);
                  //      Console.WriteLine("writeAllChecked 2: i = " + i + " regValue " + regValue[0].ToString("X2") + " currentSelectedItem " + currentSelectedItem.Value);
                        Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), currentSelectedItem.Dec, regValue, ref returnString, false, (bool)((TPS92662Control)lmmWindow).checkBoxAckEnable.IsChecked);
                        if (returnString.Contains("failed"))
                        {
                            mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                            unlockDuringReadWrite();
                            Mouse.OverrideCursor = _previousCursor;
                            busyReadingWriting = false;

                            currentSelectedItem = lastSelected;
                            if (currentSelectedItem != null)
                            {
                                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
                                setHeaderColorsWorker();
                                setFocusAfterMultiRead = true;
                            }
                            else
                                dataGridRegisterMap.Focus();
                            return;
                        }
                        updateStatus(returnString + "\n");
                    }
                }
               
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();

                currentSelectedItem = lastSelected;
                if (currentSelectedItem != null)
                {
                    DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
                    setHeaderColorsWorker();
                    setFocusAfterMultiRead = true;
                }
                else
                    dataGridRegisterMap.Focus();
            }
            catch (Exception ex)
            {
                updateStatus("Exception occured processing line " + i + " in file " + fileNameOnly + ". Write all Checked Aborted.\n");
                MessageBox.Show("Exception occured processing line " + i + " in file " + fileNameOnly + ". Write all Checked.", "Write all Checked Exception Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
            }
        }

        private void ButtonWriteAll_Click(object sender, RoutedEventArgs e)
            {
                writeAll();
        }

        private void writeAll()
        {
            DataGridCellInfo currCell = dataGridRegisterMap.CurrentCell;
            RegisterClass lastSelected = currentSelectedItem;
            Cursor _previousCursor = null;
            _previousCursor = Mouse.OverrideCursor;
            int i = 0;

            try
            {
                lockDuringReadWrite();
                mainWindow.textBoxSPIcommandStatus.IsEnabled = false;
                changeQuote();
                busyReadingWriting = true;
                Mouse.OverrideCursor = Cursors.Wait;

                if (currentSelectedItem != null)
                {
                    dataGridRegisterMap.LayoutUpdated -= DataGridRegisterMap_LayoutUpdated;
                    if (dispatcherTimer == null)
                    {
                        dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                        dispatcherTimer.Tick += dispatcherTimer_Tick;
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                    }
                }

                string returnString = "";
                int numRegs32ThisDevice = 0;
                int numRegs16ThisDevice = 0;
                if ((currentDevice.Contains("664") && (listParseRegister.Count > MIN_NUM_REGS_664)))
                    numRegs32ThisDevice = 5;
                else if ((currentDevice.Contains("662") && (listParseRegister.Count > MIN_NUM_REGS_662)))
                {
                    numRegs32ThisDevice = 3;
                    numRegs16ThisDevice = 1;
                }

                for (i = 0; i < listParseRegister.Count; i++)
                {
                    if (numRegs32ThisDevice > 0)
                    {
                        byte[] regValue = new byte[32];
                        for (int j = i; j < 32 + i; j++)
                        {
                            //              currentSelectedItem = listParseRegister[j];
                            regValue[j - i] = Convert.ToByte(listParseRegister[j].Value, 16);
                        }
                        currentSelectedItem = listParseRegister[i];
                        Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE32, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), currentSelectedItem.Dec, regValue, ref returnString, false, (bool)((TPS92662Control)lmmWindow).checkBoxAckEnable.IsChecked);
                        if (returnString.Contains("failed"))
                        {
                            updateStatus(returnString + "\n");
                            mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                            unlockDuringReadWrite();
                            Mouse.OverrideCursor = _previousCursor;
                            busyReadingWriting = false;

                            currentSelectedItem = lastSelected;
                            if (currentSelectedItem != null)
                            {
                                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
                                setHeaderColorsWorker();
                                setFocusAfterMultiRead = true;
                            }
                            else
                                dataGridRegisterMap.Focus();
                            return;
                        }
                        i += 31;
                        numRegs32ThisDevice--;
                        ;
                    }
                    else if (numRegs16ThisDevice > 0)
                    {
                        byte[] regValue = new byte[16];
                        for (int j = i; j < 16 + i; j++)
                            regValue[j - i] = Convert.ToByte(listParseRegister[j].Value, 16);
                        currentSelectedItem = listParseRegister[i];
                        Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE16, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), currentSelectedItem.Dec, regValue, ref returnString, false, (bool)((TPS92662Control)lmmWindow).checkBoxAckEnable.IsChecked);
                        if (returnString.Contains("failed"))
                        {
                            updateStatus(returnString + "\n");
                            mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                            unlockDuringReadWrite();
                            Mouse.OverrideCursor = _previousCursor;
                            busyReadingWriting = false;

                            currentSelectedItem = lastSelected;
                            if (currentSelectedItem != null)
                            {
                                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
                                setHeaderColorsWorker();
                                setFocusAfterMultiRead = true;
                            }
                            else
                                dataGridRegisterMap.Focus();
                            return;
                        }
                        i += 15;
                        numRegs16ThisDevice--;
                        ;
                    }
                    else
                    {
                        if (listParseRegister[i].Type.Contains("w"))
                        {
                            byte[] regValue = new byte[1];
                            currentSelectedItem = listParseRegister[i];
                            regValue[0] = Convert.ToByte(currentSelectedItem.Value, 16);
                            Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), currentSelectedItem.Dec, regValue, ref returnString, false, (bool)((TPS92662Control)lmmWindow).checkBoxAckEnable.IsChecked);
                            if (returnString.Contains("failed"))
                            {
                                updateStatus(returnString + "\n");
                                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                                unlockDuringReadWrite();
                                Mouse.OverrideCursor = _previousCursor;
                                busyReadingWriting = false;

                                currentSelectedItem = lastSelected;
                                if (currentSelectedItem != null)
                                {
                                    DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
                                    setHeaderColorsWorker();
                                    setFocusAfterMultiRead = true;
                                }
                                else
                                    dataGridRegisterMap.Focus();
                                return;
                            }
                        }
                    }
                    updateStatus(returnString + "\n");
                }
                
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();

                currentSelectedItem = lastSelected;
                if (currentSelectedItem != null)
                {
                    DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
                    setHeaderColorsWorker();
                    setFocusAfterMultiRead = true;
                }
                else
                    dataGridRegisterMap.Focus();
            }
            catch (Exception ex)
            {
                updateStatus("Exception occured processing line " + i + " in file " + fileNameOnly + ". Write all Aborted.\n");
                MessageBox.Show("Exception occured processing line " + i + " in file " + fileNameOnly + ". Write all.", "Write all Exception Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainWindow.textBoxSPIcommandStatus.IsEnabled = true;
                unlockDuringReadWrite();
                Mouse.OverrideCursor = _previousCursor;
                busyReadingWriting = false;
                currentSelectedItem = lastSelected;
                DataGridRegisterMapRegainFocus(currentSelectedItem, currCell);
            }
        }

        private void ComboBoxToolBarDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            deviceAddressParent = (comboBoxToolBarDevice.SelectedItem.ToString().Split(' '))[0];
        }

        private void WindowRegMap_Closing(object sender, CancelEventArgs e)
        {
            regMapWindowClosing = true;
  //          Properties.Settings.Default.screenName = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle).DeviceName;
            Properties.Settings.Default.screensNum = System.Windows.Forms.Screen.AllScreens.Count();
            Properties.Settings.Default.Save();
        }

        private void WindowRegMap_Closed(object sender, EventArgs e)
        {
  //          Globals.regMap = null;
        }

        //private void CheckBoxAutoLoadFile_Checked(object sender, RoutedEventArgs e)
        //{
        //    Properties.Settings.Default.checkBoxAutoLoadFile = (bool)checkBoxAutoLoadFile.IsChecked;
        //    menuItemAutoOpen.Checked -= MenuItemAutoOpen_Checked;
        //    menuItemAutoOpen.IsChecked = (bool)checkBoxAutoLoadFile.IsChecked;
        //    menuItemAutoOpen.Checked += MenuItemAutoOpen_Checked;
        //}

        private void MenuItemAutoOpen_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.checkBoxAutoLoadFile = (bool)menuItemAutoOpen.IsChecked;
   //         checkBoxAutoLoadFile.Checked -= CheckBoxAutoLoadFile_Checked;
     //       checkBoxAutoLoadFile.IsChecked = menuItemAutoOpen.IsChecked;
       //     checkBoxAutoLoadFile.Checked += CheckBoxAutoLoadFile_Checked;
        }

        private void MenuItemAutoReadAll_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.checkBoxAutoReadAll = (bool)menuItemAutoReadAll.IsChecked;
        }

        private void registerDumpItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                changeQuote();
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "registerDump"; // Default file name
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "Text Files (.txt)|*.txt"; // Filter files by extension

                Nullable<bool> result = dlg.ShowDialog();  // Show save file dialog box

                if (result == true)  // Process save file dialog box results
                {
                    string filenameWithPath = dlg.FileName;
                    //string fileNameOnly = dlg.SafeFileName;
                    //File.WriteAllText(filenameWithPath, string.Empty);
                    //string projDirString = dlg.FileName.Replace(fileNameOnly, "");

                    using (TextWriter tw = new StreamWriter(filenameWithPath))
                    {
                        string regData = "";
                        tw.WriteLine(currentDevice);
                        tw.WriteLine("Device Address = " + deviceAddressParent);
                        tw.WriteLine(listParseRegister.Count);
                        tw.WriteLine("Address;Data");
                        for (int i = 0; i < listParseRegister.Count; i++)
                        {
                            regData = listParseRegister[i].Hex + ";" + listParseRegister[i].Value;
                            tw.WriteLine(regData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving Register Data file... " + ex.Message, "Register Data File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void registerLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            Cursor _previousCursor = null;
            _previousCursor = Mouse.OverrideCursor;

            try
            {
                changeQuote();
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = false;
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Register Files (.txt)|*.txt";
                result = dlg.ShowDialog();

                if (result == true)
                {
                    string filenameWithPath = "";

                    filenameWithPath = dlg.FileName;
                    fileNameOnly = dlg.SafeFileName;
                    projDirString = dlg.FileName.Replace(fileNameOnly, "");

                    Mouse.OverrideCursor = Cursors.Wait;
                    using (StreamReader sr = new StreamReader(filenameWithPath))
                    {
                        updateStatus("Register Map file \"" + fileNameOnly + "\" has been successfully opened in directory \"" + projDirString + "\".\n");
                        String deviceFromFile = sr.ReadLine();
                        String addressDevice = sr.ReadLine();
                        int numRegsToWrite = int.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);

                        if ((deviceFromFile != currentDevice) || ((currentDevice.Contains("664") && (deviceFromParent < 0x96)) || (!currentDevice.Contains("664") && (deviceFromParent > 0x95))))
                        {
                            Mouse.OverrideCursor = _previousCursor;
                            updateStatus("File " + fileNameOnly + " specifying device " + deviceFromFile + " is incorrect for the selected device (channel " + deviceAddressParent + "); IDing - 0x" + deviceFromParent.ToString("X2") + ". Register Map load aborted.\n");
                            return;
                        }
                        else
                            updateStatus("This register map file is for the \"Texas Instruments " + deviceFromFile + "\" device.\n");

                        sr.ReadLine();  // file header

                        string line = "";
                        updateStatus("File " + fileNameOnly + " specifying device " + deviceFromFile + " loading" + numRegsToWrite + " registers.\n");
                        string[] regAndData;
                        byte registerFromFile;
                        byte[] dataFromFile = new byte[1];
                        string returnString = "";
                        int num32ByteWrites = 0;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line == "")
                                continue;

                            if ((numRegsToWrite > MIN_NUM_REGS_664) && (num32ByteWrites < 5))
                            {
                                regAndData = line.Split(';');
                                registerFromFile = byte.Parse(regAndData[0].Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                                byte[] data4Write = new byte[32];
                                data4Write[0] = byte.Parse(regAndData[1].Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                for (int i = 1; i < 32; i++)
                                {
                                    line = sr.ReadLine();
                                    regAndData = line.Split(';');
                                    data4Write[i] = byte.Parse(regAndData[1].Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                }
                                Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE32, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), registerFromFile, data4Write, ref returnString, false, (bool)((TPS92662Control)lmmWindow).checkBoxAckEnable.IsChecked);
                                updateStatus(returnString + "\n");

                                num32ByteWrites++;
                            }
                            else
                            {
                                regAndData = line.Split(';');
                                registerFromFile = byte.Parse(regAndData[0].Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                dataFromFile[0] = byte.Parse(regAndData[1].Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                                for (int i = 0; i < listParseRegister.Count; i++)
                                {
                                    if (listParseRegister[i].Dec == registerFromFile)
                                    {
                                        currentSelectedItem = listParseRegister[i];
                                        break;
                                    }
                                }
                                if (currentSelectedItem.Type.Contains("w"))
                                {
                                    Globals.mcuCommand.LmmWrReg(MCUcommands.FrameInit.WRITE1, byte.Parse(deviceAddressParent, CultureInfo.InvariantCulture), currentSelectedItem.Dec, dataFromFile, ref returnString, false, (bool)((TPS92662Control)lmmWindow).checkBoxAckEnable.IsChecked);
                                    updateStatus(returnString + "\n");
                                }
                            }
                        }
                        updateStatus("File " + fileNameOnly + " specifying device " + deviceFromFile + " loaded"  + numRegsToWrite + " registers successfully.\n");
                        updateStatus("Please wait, currently updating all register values.\n");
                        readAll();
                    }
                }
                Mouse.OverrideCursor = _previousCursor;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = _previousCursor;
                MessageBox.Show("Error loading register map file... " + ex.Message, "Register Map File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCheckRow(object sender, RoutedEventArgs e)
        {
            if (currentSelectedItem != null)
            {
                int row = currentSelectedItem.Dec;
                setAllCheckedThisRow(row, true);
            }
        }

        private void ButtonUncheckRow(object sender, RoutedEventArgs e)
        {
            if (currentSelectedItem != null)
            {
                int row = currentSelectedItem.Dec;
                setAllCheckedThisRow(row, false);
            }
        }

        private void setAllCheckedThisRow(int row, bool checkState)
        {
            foreach (var p in listParseRegister[row].GetType().GetProperties().Where(p => p.PropertyType == typeof(bool)))
            {
                if (!p.Name.Contains("Read") && !p.Name.Contains("Write"))
                    p.SetValue(listParseRegister[row], checkState);
            }

    //        SetHeader();
            setRwCheckBox();
        }
    }
}

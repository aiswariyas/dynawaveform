using System;

using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using MTSCRANET;
using MTLIB;

namespace MTNETDemo
{
    public partial class MainWindow : Window
    {
        private MTSCRA m_SCRA;

        private MTConnectionType m_connectionType;

        private MTConnectionState m_connectionState;

        private bool m_startTransactionActionPending;

        private bool m_turnOffLEDPending = false;

        private int m_emvMessageFormat = 0;

        private bool m_emvMessageFormatRequestPending = false;

        private int m_emvARCType;

        private byte m_emvTimeout = 60;

        private bool m_enableSwipe = false;
        private bool m_enableChip = true;
        private bool m_enableContactless = false;

        private bool m_quickChip = false;

        private byte[] ApprovedARC = new byte[] { (byte)0x8A, 0x02, 0x30, 0x30 };
        private byte[] DeclinedARC = { (byte)0x8A, 0x02, 0x30, 0x35 };

        private static AutoResetEvent m_syncEvent = new AutoResetEvent(false);
        private String m_syncData = "";

        delegate void deviceListDispatcher(List<MTDeviceInformation> deviceList);
        delegate void updateDisplayDispatcher(string text);
        delegate void clearDispatcher();
        delegate void updateStateDispatcher(MTConnectionState state);
        delegate void userSelectionsDisptacher(string title, List<string> selectionList, long timeout);

        public MainWindow()
        {
            InitializeComponent();

            m_SCRA = new MTSCRA();

            m_SCRA.OnDeviceList += OnDeviceList;
            m_SCRA.OnDeviceConnectionStateChanged += OnDeviceConnectionStateChanged;
            m_SCRA.OnCardDataState += OnCardDataStateChanged;
            m_SCRA.OnDataReceived += OnDataReceived;

            m_SCRA.OnDeviceResponse += OnDeviceResponse;
            m_SCRA.OnDeviceExtendedResponse += OnDeviceExtendedResponse;

            m_SCRA.OnTransactionStatus += OnTransactionStatus;
            m_SCRA.OnDisplayMessageRequest += OnDisplayMessageRequest;
            m_SCRA.OnUserSelectionRequest += OnUserSelectionRequest;
            m_SCRA.OnARQCReceived += OnARQCReceived;
            m_SCRA.OnTransactionResult += OnTransactionResult;
            m_SCRA.OnEMVCommandResult += OnEMVCommandResult;

            if (DeviceTypeCB.Items.Count > 0)
            {
                DeviceTypeCB.SelectedIndex = 0;
            }

            if (ARQCResponseCB.Items.Count > 0)
            {
                m_emvARCType = 0;
                ARQCResponseCB.SelectedIndex = 0;
            }

            m_emvTimeout = 60;
            EMVTimeoutCB.SelectedIndex = 2; // 60

            SwipeCheckBox.IsChecked = m_enableSwipe;
            ChipCheckBox.IsChecked = m_enableChip;
            ContactlessCheckBox.IsChecked = m_enableContactless;

            updateState(MTConnectionState.Disconnected);

            initDeviceWatchers();
        }

        private void initDeviceWatchers()
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBControllerDevice'");
            insertQuery.GroupWithinInterval = new TimeSpan(0, 0, 1);
            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBControllerDevice'");
            removeQuery.GroupWithinInterval = new TimeSpan(0, 0, 1);
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            sendToDisplay("[Device Inserted]");
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            sendToDisplay("[Device Removed]");
        }

        private void clearDisplay()
        {
            try
            {
                if (OutputTextBox.Dispatcher.CheckAccess())
                {
                    OutputTextBox.Clear();
                }
                else
                {
                    OutputTextBox.Dispatcher.BeginInvoke(new clearDispatcher(clearDisplay),
                    System.Windows.Threading.DispatcherPriority.Normal);
                }
            }
            catch (Exception)
            {
            }
        }

        private void clearMessage()
        {
            displayMessage("");
        }

        private void clearMessage2()
        {
            displayMessage2("");
        }

        private void displayMessage(string message)
        {
            try
            {
                if (MessageTextBox.Dispatcher.CheckAccess())
                {
                    MessageTextBox.Text = message;
                }
                else
                {
                    MessageTextBox.Dispatcher.BeginInvoke(new updateDisplayDispatcher(displayMessage),
                    System.Windows.Threading.DispatcherPriority.Normal,
                                                            new object[] { message });
                }
            }
            catch (Exception)
            {
            }
        }

        private void displayMessage2(string message)
        {
            try
            {
                if (MessageTextBox2.Dispatcher.CheckAccess())
                {
                    MessageTextBox2.Text = message;
                }
                else
                {
                    MessageTextBox2.Dispatcher.BeginInvoke(new updateDisplayDispatcher(displayMessage2),
                                                            System.Windows.Threading.DispatcherPriority.Normal,
                                                            new object[] { message });
                }
            }
            catch (Exception)
            {
            }
        }

        private void sendToDisplay(string data)
        {
            try
            {
                if (OutputTextBox.Dispatcher.CheckAccess())
                {
                    OutputTextBox.AppendText(data + "\n");
                    OutputTextBox.ScrollToEnd();
                }
                else
                {
                    OutputTextBox.Dispatcher.BeginInvoke(new updateDisplayDispatcher(sendToDisplay),
                                                            System.Windows.Threading.DispatcherPriority.Normal,
                                                            new object[] { data });
                }
            }
            catch (Exception)
            {
            }
        }

        private MTLIB.MTConnectionType getConnectionType()
        {
            int connectionTypeIndex = DeviceTypeCB.SelectedIndex;

            MTLIB.MTConnectionType connectionType = MTLIB.MTConnectionType.USB;

            switch (connectionTypeIndex)
            {
                case 0:
                    connectionType = MTLIB.MTConnectionType.USB;
                    break;
                case 1:
                    connectionType = MTLIB.MTConnectionType.BLE;
                    break;
                case 2:
                    connectionType = MTLIB.MTConnectionType.BLEEMV;
                    break;
                case 3:
                    connectionType = MTLIB.MTConnectionType.BLEEMVT;
                    break;
                case 4:
                    connectionType = MTLIB.MTConnectionType.Serial;
                    break;
                case 5:
                    connectionType = MTLIB.MTConnectionType.Audio;
                    break;
            }

            return connectionType;
        }

        private string getAddress()
        {
            string address = "";

            MTDeviceInformation devInfo = (MTDeviceInformation)DeviceAddressCB.SelectedValue;

            if (devInfo != null)
            {
                address = devInfo.Address;
            }

            return address;
        }

        private void scanDevices(MTLIB.MTConnectionType connectionType)
        {
            m_SCRA.requestDeviceList(connectionType);
        }

        private void connect()
        {
            if (m_connectionState == MTConnectionState.Connected)
            {
                return;
            }

            m_connectionType = getConnectionType();

            if (m_connectionType == MTConnectionType.Audio)
            {
                MessageBoxResult result = MessageBox.Show("Please make sure the audio reader is fully plugged into the headset jack and the volume is set to the highest level.",
                                                            "Connecting Audio Reader", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            m_SCRA.setConnectionType(m_connectionType);

            string address = getAddress();

            if (m_connectionType == MTConnectionType.Serial)
            {
                address = "PORT=" + address + ",BAUDRATE=115200";
            }

            m_SCRA.setAddress(address);

            m_SCRA.openDevice();
        }


        private void displayDeviceInformation()
        {
            MTDeviceInformation devInfo = (MTDeviceInformation)DeviceAddressCB.SelectedValue;

            if (devInfo != null)
            {
                string deviceInfo = "[Device Information]\n";

                deviceInfo += string.Format("Name={0}\n", devInfo.Name);
                deviceInfo += string.Format("Address={0}\n", devInfo.Address);

                if (devInfo.ProductId > 0)
                {
                    deviceInfo += string.Format("ProductID=0x{0,2:X2}\n", devInfo.ProductId);
                }

                deviceInfo += string.Format("Model={0}\n", devInfo.Model);
                deviceInfo += string.Format("Serial={0}\n", devInfo.Serial);

                sendToDisplay(deviceInfo);
            }
        }
        private void displayDeviceFeatures()
        {
            if (m_SCRA != null)
            {
                MTDeviceFeatures features = m_SCRA.getDeviceFeatures();

                if (features != null)
                {
                    string info = "[Device Features]\n";

                    info += string.Format("Supported Types: ", features.MSR);

                    if (features.MSR)
                        info += "(MSR) ";

                    if (features.Contact)
                        info += "(Contact) ";

                    if (features.Contactless)
                        info += "(Contactless) ";

                    info += "\n";
                    info += string.Format("MSR Power Saver: {0}\n", (features.MSRPowerSaver ? "Yes" : "No"));
                    info += string.Format("Battery Backed Clock: {0}", (features.BatteryBackedClock ? "Yes" : "No"));

                    sendToDisplay(info);
                }
            }
        }

        private void disconnect()
        {
            if (m_connectionState != MTConnectionState.Disconnected)
            {
                m_SCRA.closeDevice();
            }
        }

        private void sendSetDateTimeCommand()
        {
            DateTime now = DateTime.Now;

            int month = now.Month;
            int day = now.Day;
            int hour = now.Hour;
            int minute = now.Minute;
            int second = now.Second;
            int year = now.Year - 2008;

            string dateTimeString = String.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}00{5:X2}", month, day, hour, minute, second, year);

            string command = "491E0000030C00180000000000000000000000000000000000" + dateTimeString;

            sendCommand(command);
        }

        private void setMSRPower(bool state)
        {
            string command = "5801" + (state ? "01" : "00");

            sendCommand(command);
        }

        private void getDeviceInfo()
        {
            String response = sendCommandSync("000100");
            sendToDisplay("[Firmware ID]");
            sendToDisplay(response);

            String response2 = sendCommandSync("000103");
            sendToDisplay("[Device SN]");
            sendToDisplay(response2);

            String response3 = sendCommandSync("1500");
            sendToDisplay("[Security Level]");
            sendToDisplay(response3);
        }

        private void getBatteryLevel()
        {
            long level = m_SCRA.getBatteryLevel();
            sendToDisplay("Battery Level=" + level);
        }

        private String sendCommandSync(string command)
        {
            String response = "";

            sendCommand(command);

            if (m_syncEvent.WaitOne(3000))
            {
                response = m_syncData;
            }
            else
            {
                // response timed out 
            }

            return response;
        }

        private int sendCommand(string command)
        {
            //string moreInfo = getMoreInformation();
            //sendToDisplay(moreInfo);

            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA.isDeviceConnected())
            {
                sendToDisplay("[Sending Command]");
                sendToDisplay(command);

                result = m_SCRA.sendCommandToDevice(command);
            }

            return result;
        }

        private void requestEMVMessageFormat()
        {
            Task task = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(1000);

                m_emvMessageFormatRequestPending = true;

                int status = sendCommand("000168");

                if (status != MTSCRA.SEND_COMMAND_SUCCESS)
                {
                    m_emvMessageFormatRequestPending = false;
                }
            });
        }

        private void enableConnectedInterface(bool enabled)
        {
            ConnectButton.IsEnabled = !enabled;
            DisconnectButton.IsEnabled = enabled;
            DeviceTypeCB.IsEnabled = !enabled;
            DeviceAddressCB.IsEnabled = !enabled;
            ScanButton.IsEnabled = !enabled;
            SendCommandButton.IsEnabled = enabled;
            GetDeviceInfoButton.IsEnabled = enabled;

            //SetMSROnButton.IsEnabled = enabled;
            //SetMSROffButton.IsEnabled = enabled;
            bool msrPowerSaver = m_SCRA.getDeviceFeatures().MSRPowerSaver;
            SetMSROnButton.IsEnabled = msrPowerSaver ? enabled : false;
            SetMSROffButton.IsEnabled = msrPowerSaver ? enabled : false;

            //SetTimeButton.IsEnabled = enabled;
            bool batteryBackedClock = m_SCRA.getDeviceFeatures().BatteryBackedClock;
            //SetTimeButton.IsEnabled = batteryBackedClock ? false:enabled;
            SetTimeButton.IsEnabled = true;

            SwipeCheckBox.IsEnabled = m_SCRA.getDeviceFeatures().MSR ? enabled : false;
            ChipCheckBox.IsEnabled = m_SCRA.getDeviceFeatures().Contact ? enabled : false;
            ContactlessCheckBox.IsEnabled = m_SCRA.getDeviceFeatures().Contactless ? enabled : false;

            SwipeCheckBox.IsChecked = m_SCRA.getDeviceFeatures().MSR ? enabled : false;
            ChipCheckBox.IsChecked = m_SCRA.getDeviceFeatures().Contact ? enabled : false;
            ContactlessCheckBox.IsChecked = m_SCRA.getDeviceFeatures().Contactless ? enabled : false;
        }

        private void enableEMVInterface(bool enabled)
        {
            EMVStartButton.IsEnabled = enabled;
            EMVCancelButton.IsEnabled = enabled;
            GetEMVConfigButton.IsEnabled = enabled;
            SetEMVConfigButton.IsEnabled = enabled;
            CommitConfigButton.IsEnabled = enabled;
        }

        private void updateState(MTConnectionState state)
        {
            m_connectionState = state;

            try
            {
                if (OutputTextBox.Dispatcher.CheckAccess())
                {
                    switch (state)
                    {
                        case MTConnectionState.Connecting:
                            enableConnectedInterface(false);
                            enableEMVInterface(false);
                            OutputTextBox.Background = Brushes.Gray;
                            sendToDisplay("[Connecting....]");
                            displayDeviceInformation();
                            break;
                        case MTConnectionState.Connected:
                            enableConnectedInterface(true);
                            displayDeviceFeatures();
                            if (m_SCRA.isDeviceOEM())
                            {
                                sendToDisplay("This device is OEM.");
                            }
                            if (m_SCRA.isDeviceEMV())
                            {
                                sendToDisplay("This device supports EMV.");
                                enableEMVInterface(true);
                                requestEMVMessageFormat();
                            }
                            sendToDisplay("Power Management Value: " + m_SCRA.getPowerManagementValue());
                            OutputTextBox.Background = Brushes.White;
                            sendToDisplay("[Connected]");
                            clearMessage();
                            clearMessage2();
                            break;
                        case MTConnectionState.Disconnecting:
                            enableConnectedInterface(false);
                            enableEMVInterface(false);
                            OutputTextBox.Background = Brushes.Gray;
                            sendToDisplay("[Disconnecting....]");
                            break;
                        case MTConnectionState.Disconnected:
                            enableConnectedInterface(false);
                            enableEMVInterface(false);
                            OutputTextBox.Background = Brushes.Gray;
                            sendToDisplay("[Disconnected]");
                            break;
                    }
                }
                else
                {
                    OutputTextBox.Dispatcher.BeginInvoke(new updateStateDispatcher(updateState),
                                                    System.Windows.Threading.DispatcherPriority.Normal,
                                                    new object[] { state });
                }
            }
            catch (Exception ex)
            {

            }

        }

        protected void clearDeviceList()
        {
            try
            {
                if (DeviceAddressCB.Dispatcher.CheckAccess())
                {
                    DeviceAddressCB.Items.Clear();
                }
                else
                {
                    DeviceAddressCB.Dispatcher.BeginInvoke(new clearDispatcher(clearDeviceList),
                                                            System.Windows.Threading.DispatcherPriority.Normal);
                }
            }
            catch (Exception)
            {
            }
        }

        protected void updateDeviceList(List<MTLIB.MTDeviceInformation> deviceList)
        {
            try
            {
                if (DeviceAddressCB.Dispatcher.CheckAccess())
                {
                    DeviceAddressCB.Items.Clear();

                    if (deviceList.Count > 0)
                    {
                        foreach (var device in deviceList)
                        {
                            DeviceAddressCB.Items.Add(device);
                        }

                        DeviceAddressCB.Visibility = Visibility.Visible;

                        DeviceAddressCB.SelectedIndex = 0;
                    }

                    DeviceAddressCB.IsEnabled = true;
                }
                else
                {
                    OutputTextBox.Dispatcher.BeginInvoke(new deviceListDispatcher(updateDeviceList),
                                                            System.Windows.Threading.DispatcherPriority.Normal,
                                                            new object[] { deviceList });
                }
            }
            catch (Exception)
            {
            }

        }

        private void ARQCResponseCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_emvARCType = ARQCResponseCB.SelectedIndex;
        }


        private void GetDeviceInfoButton_Click(object sender, RoutedEventArgs e)
        {
            getDeviceInfo();
        }

        private void SetTimeCommandButton_Click(object sender, RoutedEventArgs e)
        {
            sendSetDateTimeCommand();
        }

        private void SetMSROnButton_Click(object sender, RoutedEventArgs e)
        {
            setMSRPower(true);
        }

        private void SetMSROffButton_Click(object sender, RoutedEventArgs e)
        {
            setMSRPower(false);
        }

        private void BatteryLevelButton_Click(object sender, RoutedEventArgs e)
        {
            getBatteryLevel();
        }

        private void DeviceTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceAddressCB.Items.Clear();

            MTLIB.MTConnectionType connectionType = getConnectionType();

            scanDevices(connectionType);
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            clearDeviceList();

            MTLIB.MTConnectionType connectionType = getConnectionType();

            scanDevices(connectionType);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_connectionState == MTConnectionState.Disconnected)
            {
                connect();
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_connectionState != MTConnectionState.Disconnected)
            {
                disconnect();
            }
        }

        private void SendCommandButton_Click(object sender, RoutedEventArgs e)
        {
            string command = CommandTextBox.Text;

            sendCommand(command);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            clearDisplay();
            clearMessage();
            clearMessage2();
        }

        private byte getEMVTimeoutValue()
        {
            try
            {
                String valueString = EMVTimeoutCB.Text;

                m_emvTimeout = (byte)Convert.ToInt32(valueString);
            }
            catch (Exception)
            {
            }

            return m_emvTimeout;
        }

        private void updateCardType()
        {
            bool? isSwipe = SwipeCheckBox.IsChecked;
            bool? isChip = ChipCheckBox.IsChecked;
            bool? isContactless = ContactlessCheckBox.IsChecked;

            m_enableSwipe = (isSwipe == true);
            m_enableChip = (isChip == true);
            m_enableContactless = (isContactless == true);
        }

        private byte getCardTypeValue()
        {
            byte value = 0;
            //byte cardType = 0x02;   // Chip Only
            //byte cardType = 0x03;   // Chip and MSR
            //byte cardType = 0x04;   // Contactless
            //byte cardType = 0x07;   // Chip, MSR, Contactless

            if (m_enableSwipe)
            {
                value |= 0x01;
            }

            if (m_enableChip)
            {
                value |= 0x02;
            }

            if (m_enableContactless)
            {
                value |= 0x04;
            }

            return value;
        }

        private void QuickChipCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ARQCResponseCB.IsEnabled = false;
            m_quickChip = true;
        }

        private void QuickChipCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ARQCResponseCB.IsEnabled = true;
            m_quickChip = false;
        }

        private void EMVStartButton_Click(object sender, RoutedEventArgs e)
        {
            updateCardType();

            //startTransactionWithLED();
            startTransaction();
        }

        private void EMVCancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTransaction();
        }

        private void GetEMVConfigButton_Click(object sender, RoutedEventArgs e)
        {
            //getTerminalConfiguration();
            displayGetEMVConfig();
        }

        private void SetEMVConfigButton_Click(object sender, RoutedEventArgs e)
        {
            //setTerminalConfiguration();
            displaySetEMVConfig();
        }

        private void CommitConfigButton_Click(object sender, RoutedEventArgs e)
        {
            //commitConfiguration();
            displayCommitEMVConfig();
        }

        private bool isQuickChipEnabled()
        {
            return m_quickChip;
        }

        public void startTransactionWithLED()
        {
            m_startTransactionActionPending = true;
            setLED(true);
        }

        private void startTransaction()
        {
            if (m_SCRA != null)
            {

                //byte timeLimit = 0x3C;
                //byte timeLimit = 0x00;
                byte timeLimit = getEMVTimeoutValue();

                byte cardType = getCardTypeValue();

                byte option = 0x00;

                if (isQuickChipEnabled())
                {
                    option |= (byte)0x80; // Quick Chip
                }

                byte[] amount = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x15, 0x00 };
                byte transactionType = 0x00; // Purchase
                //byte transactionType = 0x04; // Goods
                //byte transactionType = 0x50; // Test
                byte[] cashBack = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                byte[] currencyCode = new byte[] { 0x08, 0x40 };
                byte reportingOption = 0x02;  // All Status Changes

                clearMessage();
                clearMessage2();

                long result = m_SCRA.startTransaction(timeLimit, cardType, option, amount, transactionType, cashBack, currencyCode, reportingOption);

                sendToDisplay("[Start Transaction] (Result=" + result + ")");
            }
        }

        private void cancelTransaction()
        {
            if (m_SCRA != null)
            {
                m_turnOffLEDPending = true;

                long result = m_SCRA.cancelTransaction();

                sendToDisplay("[Cancel Transaction] (Result=" + result + ")");
            }
        }

        public void setLED(bool on)
        {
            if (m_SCRA != null)
            {
                if (on)
                {
                    m_SCRA.sendCommandToDevice(MTDeviceConstants.SCRA_DEVICE_COMMAND_STRING_SET_LED_ON);
                }
                else
                {
                    m_SCRA.sendCommandToDevice(MTDeviceConstants.SCRA_DEVICE_COMMAND_STRING_SET_LED_OFF);
                }
            }
        }

        public void getEMVConfiguration(string extendedCommand)
        {
            if (string.IsNullOrEmpty(extendedCommand))
            {
                sendToDisplay("[Get EMV Configuration]\nCommand is empty");
                return;
            }

            if (m_SCRA != null)
            {
                sendToDisplay("[Get EMV Configuration]");
                sendToDisplay("(" + extendedCommand + ")");

                m_SCRA.sendExtendedCommand(extendedCommand);
            }
        }

        public void setEMVConfiguration(string extendedCommand)
        {
            if (string.IsNullOrEmpty(extendedCommand))
            {
                sendToDisplay("[Set EMV Configuration]\nCommand is empty");
                return;
            }

            if (m_SCRA != null)
            {
                sendToDisplay("[Set EMV Configuration]");
                sendToDisplay("(" + extendedCommand + ")");

                m_SCRA.sendExtendedCommand(extendedCommand);
            }
        }

        public void commitEMVConfiguration(string extendedCommand)
        {
            if (string.IsNullOrEmpty(extendedCommand))
            {
                sendToDisplay("[Commit EMV Configuration]\nCommand is empty");
                return;
            }

            if (m_SCRA != null)
            {
                sendToDisplay("[Commit EMV Configuration]");
                sendToDisplay("(" + extendedCommand + ")");

                m_SCRA.sendExtendedCommand(extendedCommand);
            }
        }

        public void getTerminalConfiguration()
        {
            if (m_SCRA != null)
            {
                string commandString = "0306";
                string sizeString = "0003";
                string dataString = "010F00";

                sendToDisplay("[Get Terminal Configuration]");

                m_SCRA.sendExtendedCommand(commandString + sizeString + dataString);
            }
        }

        public void setTerminalConfiguration()
        {
            if (m_SCRA != null)
            {
                string commandString = "0305";
                string sizeString = "001D";
                string dataString = "0001010042333039343633303932323135414100FA00000000B75CD164";

                sendToDisplay("[Set Terminal Configuration]");

                m_SCRA.sendExtendedCommand(commandString + sizeString + dataString);
            }
        }

        public void commitConfiguration()
        {
            if (m_SCRA != null)
            {
                string commandString = "030E";
                string sizeString = "0001";
                string dataString = "00";

                sendToDisplay("[Commit Configuration]");

                m_SCRA.sendExtendedCommand(commandString + sizeString + dataString);
            }
        }

        public void setUserSelectionResult(byte status, byte selection)
        {
            if (m_SCRA != null)
            {
                sendToDisplay("[Sending Selection Result] Status=" + status + " Selection=" + selection);

                m_SCRA.setUserSelectionResult(status, selection);
            }
        }

        public void setAcquirerResponse(byte[] response)
        {
            if ((m_SCRA != null) && (response != null))
            {
                sendToDisplay("[Sending Acquirer Response]\n" + MTParser.getHexString(response));

                m_SCRA.setAcquirerResponse(response);
            }
        }

        private string formatStringIfNotEmpty(string format, string data)
        {
            string result = "";

            if (!string.IsNullOrEmpty(data))
            {
                result = string.Format(format, data);
            }

            return result;
        }

        public string getMoreInformation()
        {
            string moreInfo = "";

            moreInfo += string.Format("SDK.Version={0}\n", m_SCRA.getSDKVersion());

            moreInfo += formatStringIfNotEmpty("Device.Serial={0}\n", m_SCRA.getDeviceSerial());

            moreInfo += formatStringIfNotEmpty("Device.Firmware={0}\n", m_SCRA.getFirmware());

            moreInfo += formatStringIfNotEmpty("Cap.Tracks={0}\n", m_SCRA.getCapTracks());

            moreInfo += formatStringIfNotEmpty("Cap.MagneSafe2.0={0}\n", m_SCRA.getCapMagneSafe20Encryption());

            moreInfo += formatStringIfNotEmpty("Cap.MagneStripeEncryption={0}\n", m_SCRA.getCapMagStripeEncryption());

            moreInfo += formatStringIfNotEmpty("Battery.Level={0}\n", m_SCRA.getBatteryLevel().ToString());

            return moreInfo;
        }

        public string getCardInfo()
        {
            string cardData = "";

            cardData += string.Format("SDK.Version={0}\n", m_SCRA.getSDKVersion());

            cardData += formatStringIfNotEmpty("TLV.Version={0}\n", m_SCRA.getTLVVersion());

            cardData += formatStringIfNotEmpty("Response.Type={0}\n", m_SCRA.getResponseType());

            cardData += string.Format("Tracks.Masked={0}\n", m_SCRA.getMaskedTracks());
            cardData += string.Format("Track1.Encrypted={0}\n", m_SCRA.getTrack1());
            cardData += string.Format("Track2.Encrypted={0}\n", m_SCRA.getTrack2());
            cardData += string.Format("Track3.Encrypted={0}\n", m_SCRA.getTrack3());
            cardData += string.Format("Track1.Masked={0}\n", m_SCRA.getTrack1Masked());
            cardData += string.Format("Track2.Masked={0}\n", m_SCRA.getTrack2Masked());
            cardData += string.Format("Track3.Masked={0}\n", m_SCRA.getTrack3Masked());
            string mpData = m_SCRA.getMagnePrint();
            cardData += string.Format("MagnePrint.Encrypted={0}\n", mpData);
            cardData += string.Format("MagnePrint.Length={0} bytes\n", (mpData.Length / 2));
            cardData += string.Format("MagnePrint.Status={0}\n", m_SCRA.getMagnePrintStatus());
            cardData += string.Format("Device.Serial={0}\n", m_SCRA.getDeviceSerial());
            cardData += string.Format("Session.ID={0}\n", m_SCRA.getSessionID());
            cardData += string.Format("KSN={0}\n", m_SCRA.getKSN());
            sendToDisplay("KSN");


            if (m_SCRA.getSwipeCount() >= 0)
            {
                cardData += string.Format("Swipe.Count={0}\n", m_SCRA.getSwipeCount());
            }

            cardData += formatStringIfNotEmpty("Cap.MagnePrint={0}\n", m_SCRA.getCapMagnePrint());
            cardData += formatStringIfNotEmpty("Cap.MagnePrintEncryption={0}\n", m_SCRA.getCapMagnePrintEncryption());

            if (m_connectionType == MTConnectionType.Audio)
            {
                cardData += formatStringIfNotEmpty("Cap.MagneSafe20Encryption={0}\n", m_SCRA.getCapMagneSafe20Encryption());
            }

            cardData += formatStringIfNotEmpty("Cap.MagStripeEncryption={0}\n", m_SCRA.getCapMagStripeEncryption());
            cardData += formatStringIfNotEmpty("Cap.MSR={0}\n", m_SCRA.getCapMSR());

            if (m_connectionType == MTConnectionType.Audio)
            {
                cardData += formatStringIfNotEmpty("Cap.Tracks={0}\n", m_SCRA.getCapTracks());
            }

            cardData += string.Format("Card.Data.CRC={0}\n", m_SCRA.getCardDataCRC());
            cardData += string.Format("Card.Exp.Date={0}\n", m_SCRA.getCardExpDate());
            cardData += string.Format("Card.IIN={0}\n", m_SCRA.getCardIIN());
            cardData += string.Format("Card.Last4={0}\n", m_SCRA.getCardLast4());
            cardData += string.Format("Card.Name={0}\n", m_SCRA.getCardName());
            cardData += string.Format("Card.PAN={0}\n", m_SCRA.getCardPAN());
            cardData += string.Format("Card.PAN.Length={0}\n", m_SCRA.getCardPANLength());
            cardData += string.Format("Card.Service.Code={0}\n", m_SCRA.getCardServiceCode());

            cardData += formatStringIfNotEmpty("Card.Status={0}\n", m_SCRA.getCardStatus());
            cardData += formatStringIfNotEmpty("Card.EncodeType={0}\n", m_SCRA.getCardEncodeType());

            cardData += formatStringIfNotEmpty("HashCode={0}\n", m_SCRA.getHashCode());

            if (m_SCRA.getDataFieldCount() != 0)
            {
                cardData += string.Format("Data.Field.Count={0}\n", m_SCRA.getDataFieldCount());
            }

            cardData += string.Format("Encryption.Status={0}\n", m_SCRA.getEncryptionStatus());

            cardData += formatStringIfNotEmpty("MagTek.Device.Serial={0}\n", m_SCRA.getMagTekDeviceSerial());

            cardData += string.Format("Track.Decode.Status={0}\n", m_SCRA.getTrackDecodeStatus());
            string tkStatus = m_SCRA.getTrackDecodeStatus();

            string tk1Status = "01";
            string tk2Status = "01";
            string tk3Status = "01";

            if (tkStatus.Length >= 6)
            {
                tk1Status = tkStatus.Substring(0, 2);
                tk2Status = tkStatus.Substring(2, 2);
                tk3Status = tkStatus.Substring(4, 2);

                cardData += string.Format("Track1.Status={0}\n", tk1Status);
                cardData += string.Format("Track2.Status={0}\n", tk2Status);
                cardData += string.Format("Track3.Status={0}\n", tk3Status);
            }

            cardData += string.Format("Battery.Level={0}\n", m_SCRA.getBatteryLevel());

            return cardData;
        }

        protected void OnDeviceList(object sender, MTLIB.MTConnectionType connectionType, List<MTLIB.MTDeviceInformation> deviceList)
        {
            updateDeviceList(deviceList);
        }

        protected void OnDeviceConnectionStateChanged(object sender, MTLIB.MTConnectionState state)
        {
            updateState(state);
        }

        protected void OnCardDataStateChanged(object sender, MTLIB.MTCardDataState state)
        {
            switch (state)
            {
                case MTCardDataState.DataError:
                    sendToDisplay("[Data Error]");
                    break;
                case MTCardDataState.DataNotReady:
                    sendToDisplay("[Data Not Ready]");
                    break;
                case MTCardDataState.DataReady:
                    sendToDisplay("[Data Ready]");
                    break;
            }
        }

        protected void OnDataReceived(object sender, IMTCardData cardData)
        {
            clearDisplay();

            sendToDisplay("[Raw Data]");
            sendToDisplay(m_SCRA.getResponseData());

            sendToDisplay("[Card Data]");
            sendToDisplay(getCardInfo());


            sendToDisplay("[TLV Payload]");
            sendToDisplay(cardData.getTLVPayload());
        }

        protected void OnDeviceResponse(object sender, string data)
        {
            sendToDisplay("[Device Response]");
            sendToDisplay(data);

            m_syncData = data;
            m_syncEvent.Set();

            if (m_emvMessageFormatRequestPending)
            {
                m_emvMessageFormatRequestPending = false;

                byte[] emvMessageFormatResponseByteArray = MTParser.getByteArrayFromHexString(data);

                if (emvMessageFormatResponseByteArray.Length == 3)
                {
                    if ((emvMessageFormatResponseByteArray[0] == 0) && (emvMessageFormatResponseByteArray[1] == 1))
                    {
                        m_emvMessageFormat = emvMessageFormatResponseByteArray[2];
                    }
                }
            }
            else if (m_startTransactionActionPending)
            {
                m_startTransactionActionPending = false;
                startTransaction();
            }
        }

        protected void OnDeviceExtendedResponse(object sender, string data)
        {
            sendToDisplay("[Device Extended Response]");
            sendToDisplay(data);

            processDeviceExtendedResponse(data);
        }

        protected void processDeviceExtendedResponse(string data)
        {

        }

        protected void OnTransactionStatus(object sender, byte[] data)
        {
            sendToDisplay("[Transaction Status]");
            sendToDisplay(MTParser.getHexString(data));
        }

        protected void OnDisplayMessageRequest(object sender, byte[] data)
        {
            sendToDisplay("[Display Message Request]");

            String message = "";

            if ((data != null) && (data.Length > 0))
            {
                message = System.Text.Encoding.UTF8.GetString(data);
            }

            sendToDisplay(message);
            displayMessage(message);
        }

        protected List<string> getSelectionList(byte[] data, int offset)
        {
            List<string> selectionList = new List<string>();

            if (data != null)
            {
                int dataLen = data.Length;

                if (dataLen >= offset)
                {
                    int start = offset;

                    for (int i = offset; i < dataLen; i++)
                    {
                        if (data[i] == 0x00)
                        {
                            int len = i - start;

                            if (len >= 0)
                            {
                                byte[] selectionBytes = new byte[len];

                                Array.Copy(data, start, selectionBytes, 0, len);

                                string selectionString = System.Text.Encoding.UTF8.GetString(selectionBytes);

                                selectionList.Add(selectionString);
                            }

                            start = i + 1;
                        }
                    }
                }
            }

            return selectionList;
        }

        protected void OnUserSelectionRequest(object sender, byte[] data)
        {
            sendToDisplay("[User Selection Request]");

            sendToDisplay(MTParser.getHexString(data));

            processSelectionRequest(data);
        }

        protected void displayUserSelections(string title, List<string> selectionList, long timeout)
        {
            try
            {
                if (Dispatcher.CheckAccess())
                {
                    SelectionsWindow dialog = new SelectionsWindow();

                    dialog.Owner = this;
                    dialog.init(title, selectionList, timeout);

                    bool? result = dialog.ShowDialog();

                    int selectionIndex = (byte)dialog.getSelectedIndex();

                    dialog.Owner = null;
                    dialog = null;

                    if (result == false)
                    {
                        if (selectionIndex < 0)
                        {
                            setUserSelectionResult(MTEMVDeviceConstants.SELECTION_STATUS_TIMED_OUT, (byte)0);
                        }
                        else
                        {
                            setUserSelectionResult(MTEMVDeviceConstants.SELECTION_STATUS_CANCELLED, (byte)0);
                        }
                    }
                    else
                    {
                        setUserSelectionResult(MTEMVDeviceConstants.SELECTION_STATUS_COMPLETED, (byte)(selectionIndex));
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(new userSelectionsDisptacher(displayUserSelections),
                                                            System.Windows.Threading.DispatcherPriority.Normal,
                                                            new object[] { title, selectionList, timeout });
                }
            }
            catch (Exception)
            {
            }

        }

        protected void processSelectionRequest(byte[] data)
        {
            if (data != null)
            {
                int dataLen = data.Length;

                if (dataLen > 2)
                {
                    byte selectionType = data[0];
                    //data[1] = 10;  // Test
                    long timeout = ((long)(data[1] & 0xFF) * 1000);

                    List<string> selectionList = getSelectionList(data, 2);

                    string title = selectionList[0];

                    selectionList.RemoveAt(0);

                    int nSelections = selectionList.Count();

                    if (nSelections > 0)
                    {
                        if (selectionType == MTEMVDeviceConstants.SELECTION_TYPE_LANGUAGE)
                        {
                            for (int i = 0; i < nSelections; i++)
                            {
                                byte[] code = System.Text.Encoding.UTF8.GetBytes(selectionList[i].ToArray());
                                EMVLanguage language = EMVLanguage.GetLanguage(code);

                                if (language != null)
                                {
                                    selectionList[i] = language.Name;
                                }
                            }
                        }

                        timeout = 10000;

                        displayUserSelections(title, selectionList, timeout);
                    }
                }
            }
        }

        protected void displayGetEMVConfig()
        {
            try
            {
                if (Dispatcher.CheckAccess())
                {
                    GetEMVConfigWindow dialog = new GetEMVConfigWindow();

                    dialog.Owner = this;

                    bool? result = dialog.ShowDialog();

                    string extendedCommand = dialog.getExtendedCommand();

                    dialog.Owner = null;
                    dialog = null;

                    if (result == true)
                    {
                        getEMVConfiguration(extendedCommand);
                    }
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }

        }

        protected void displaySetEMVConfig()
        {
            try
            {
                if (Dispatcher.CheckAccess())
                {
                    SetEMVConfigWindow dialog = new SetEMVConfigWindow();

                    dialog.Owner = this;

                    bool? result = dialog.ShowDialog();

                    string extendedCommand = dialog.getExtendedCommand();

                    dialog.Owner = null;
                    dialog = null;

                    if (result == true)
                    {
                        setEMVConfiguration(extendedCommand);
                    }
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }

        }

        protected void displayCommitEMVConfig()
        {
            try
            {
                if (Dispatcher.CheckAccess())
                {
                    CommitEMVConfigWindow dialog = new CommitEMVConfigWindow();

                    dialog.Owner = this;

                    bool? result = dialog.ShowDialog();

                    string extendedCommand = dialog.getExtendedCommand();

                    dialog.Owner = null;
                    dialog = null;

                    if (result == true)
                    {
                        commitEMVConfiguration(extendedCommand);
                    }
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }

        }

        protected void OnARQCReceived(object sender, byte[] data)
        {
            sendToDisplay("[ARQC Received]");

            sendToDisplay(MTParser.getHexString(data));

            if (isQuickChipEnabled())
            {
                sendToDisplay("** Not sending ARQC response for Quick Chip");

                return;
            }

            if (m_emvARCType == 2)
            {
                sendToDisplay("** Not sending ARQC response for No Response");

                return;
            }

            List<Dictionary<String, String>> parsedTLVList = MTParser.parseEMVData(data, true, "");

            if (parsedTLVList != null)
            {
                String deviceSNString = MTParser.getTagValue(parsedTLVList, "DFDF25");
                byte[] deviceSN = MTParser.getByteArrayFromHexString(deviceSNString);

                sendToDisplay("SN Bytes=" + deviceSNString);

                byte[] response = null;

                byte[] arc = null;

                if (m_emvARCType == 0)
                {
                    arc = ApprovedARC;
                }
                else if (m_emvARCType == 1)
                {
                    arc = DeclinedARC;
                }

                if (m_emvMessageFormat == 0)
                {
                    response = buildAcquirerResponseFormat0(deviceSN, arc);
                }
                else if (m_emvMessageFormat == 1)
                {
                    String macKSNString = MTParser.getTagValue(parsedTLVList, "DFDF54");
                    byte[] macKSN = MTParser.getByteArrayFromHexString(macKSNString);

                    String macEncryptionTypeString = MTParser.getTagValue(parsedTLVList, "DFDF55");
                    byte[] macEncryptionType = MTParser.getByteArrayFromHexString(macEncryptionTypeString);

                    response = buildAcquirerResponseFormat1(macKSN, macEncryptionType, deviceSN, arc);
                    //              response = buildAcquirerResponseFormat1Test(macKSN, macEncryptionType, deviceSN, arc);
                    sendToDisplay("macKSNString =" + macKSNString);
                }

                setAcquirerResponse(response);
            }

        }

        protected byte[] buildAcquirerResponseFormat0(byte[] deviceSN, byte[] arc)
        {
            byte[] response = null;

            int lenSN = 0;

            if (deviceSN != null)
            {
                lenSN = deviceSN.Length;
            }

            if (lenSN > 0)
            {
                byte[] snTag = new byte[] { (byte)0xDF, (byte)0xDF, 0x25, (byte)lenSN };
                byte[] container = new byte[] { (byte)0xFA, 0x06, 0x70, 0x04 };

                int len = 4 + snTag.Length + lenSN + container.Length + arc.Length;

                response = new byte[len];

                int i = 0;
                len -= 2;
                response[i++] = (byte)((len >> 8) & 0xFF);
                response[i++] = (byte)(len & 0xFF);
                len -= 2;
                response[i++] = (byte)0xF9;
                response[i++] = (byte)len;
                Array.Copy(snTag, 0, response, i, snTag.Length);
                i += snTag.Length;
                Array.Copy(deviceSN, 0, response, i, deviceSN.Length);
                i += deviceSN.Length;
                Array.Copy(container, 0, response, i, container.Length);
                i += container.Length;

                if (arc != null)
                {
                    Array.Copy(arc, 0, response, i, arc.Length);
                }
            }

            return response;
        }

        protected byte[] buildAcquirerResponseFormat1(byte[] macKSN, byte[] macEncryptionType, byte[] deviceSN, byte[] arc)
        {
            byte[] response = null;

            int lenMACKSN = 0;
            int lenMACEncryptionType = 0;
            int lenSN = 0;

            if (macKSN != null)
            {
                lenMACKSN = macKSN.Length;
                Console.WriteLine(macKSN);
            }


            if (macEncryptionType != null)
            {
                lenMACEncryptionType = macEncryptionType.Length;
            }

            if (deviceSN != null)
            {
                lenSN = deviceSN.Length;
            }

            byte[] macKSNTag = new byte[] { (byte)0xDF, (byte)0xDF, 0x54, (byte)lenMACKSN };
            byte[] macEncryptionTypeTag = new byte[] { (byte)0xDF, (byte)0xDF, 0x55, (byte)lenMACEncryptionType };
            byte[] snTag = new byte[] { (byte)0xDF, (byte)0xDF, 0x25, (byte)lenSN };
            byte[] container = new byte[] { (byte)0xFA, 0x06, 0x70, 0x04 };


            int lenTLV = 4 + macKSNTag.Length + lenMACKSN + macEncryptionTypeTag.Length + lenMACEncryptionType + snTag.Length + lenSN + container.Length + arc.Length;

            int lenPadding = 0;

            if ((lenTLV % 8) > 0)
            {
                lenPadding = (8 - lenTLV % 8);
            }

            int lenData = lenTLV + lenPadding + 4;

            response = new byte[lenData];

            int i = 0;
            response[i++] = (byte)(((lenData - 2) >> 8) & 0xFF);
            response[i++] = (byte)((lenData - 2) & 0xFF);
            response[i++] = (byte)0xF9;
            response[i++] = (byte)(lenTLV - 4);

            Array.Copy(macKSNTag, 0, response, i, macKSNTag.Length);
            i += macKSNTag.Length;

            if (macKSN != null)
            {
                Array.Copy(macKSN, 0, response, i, macKSN.Length);
                i += macKSN.Length;
            }

            Array.Copy(macEncryptionTypeTag, 0, response, i, macEncryptionTypeTag.Length);
            i += macEncryptionTypeTag.Length;

            if (macEncryptionType != null)
            {
                Array.Copy(macEncryptionType, 0, response, i, macEncryptionType.Length);
                i += macEncryptionType.Length;
            }

            Array.Copy(snTag, 0, response, i, snTag.Length);
            i += snTag.Length;

            if (deviceSN != null)
            {
                Array.Copy(deviceSN, 0, response, i, deviceSN.Length);
                i += deviceSN.Length;

            }

            Array.Copy(container, 0, response, i, container.Length);
            i += container.Length;

            if (arc != null)
            {
                Array.Copy(arc, 0, response, i, arc.Length);
            }

            return response;

        }

        protected void OnTransactionResult(object sender, byte[] data)
        {
            sendToDisplay("[Transaction Result]");

            sendToDisplay(MTParser.getHexString(data));

            if (data != null)
            {
                if (data.Length > 0)
                {
                    bool signatureRequired = (data[0] != 0);

                    int lenBatchData = data.Length - 3;
                    if (lenBatchData > 0)
                    {
                        byte[] batchData = new byte[lenBatchData];

                        Array.Copy(data, 3, batchData, 0, lenBatchData);

                        sendToDisplay("(Parsed Batch Data)");

                        List<Dictionary<String, String>> parsedTLVList = MTParser.parseEMVData(batchData, false, "");

                        displayParsedTLV(parsedTLVList);

                        bool approved = false;

                        if (m_emvMessageFormat == 0)
                        {
                            String cidString = MTParser.getTagValue(parsedTLVList, "9f27");
                            byte[] cidValue = MTParser.getByteArrayFromHexString(cidString);


                            if (cidValue != null)
                            {
                                if (cidValue.Length > 0)
                                {
                                    if ((cidValue[0] & (byte)0x40) != 0)
                                    {
                                        approved = true;
                                    }
                                }
                            }
                        }
                        else if (m_emvMessageFormat == 1)
                        {
                            String statusString = MTParser.getTagValue(parsedTLVList, "dfdf1a");
                            byte[] statusValue = MTParser.getByteArrayFromHexString(statusString);


                            if (statusValue != null)
                            {
                                if (statusValue.Length > 0)
                                {
                                    if (statusValue[0] == 0)
                                    {
                                        approved = true;
                                    }
                                }
                            }
                        }

                        if (approved)
                        {
                            if (signatureRequired)
                            {
                                displayMessage2("( Signature Required )");
                            }
                            else
                            {
                                displayMessage2("( No Signature Required )");
                            }
                        }
                    }
                }
            }

            setLED(false);
        }
      

        protected void OnEMVCommandResult(object sender, byte[] data)
        {
            sendToDisplay("[EMV Command Result]");

            sendToDisplay(MTParser.getHexString(data));

            if (m_turnOffLEDPending)
            {
                m_turnOffLEDPending = false;

                setLED(false);
            }
        }

        private void displayParsedTLV(List<Dictionary<string, string>> parsedTLVList)
        {
            if (parsedTLVList != null)
            {
                foreach (Dictionary<String, String> map in parsedTLVList)
                {
                    string tagString;
                    string valueString;

                    if (map.TryGetValue("tag", out tagString))
                    {
                        if (map.TryGetValue("value", out valueString))
                        {
                            sendToDisplay("  " + tagString + "=" + valueString);

                        }
                    }
                }
            }

        }
    }
}


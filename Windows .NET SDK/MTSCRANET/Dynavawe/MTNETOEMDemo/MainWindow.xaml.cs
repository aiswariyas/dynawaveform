using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using MTSCRANET;
using MTLIB;

namespace MTNETOEMDemo
{
    public partial class MainWindow : Window
    {
        private enum STATE
        {
            STARTUP,
            GET_EMV_MESSSAGE_FORMAT,
            SETUP_TIME,
            READY
        }

        private STATE mState;

        private MTSCRA m_SCRA;

        private MTConnectionType m_connectionType;

        private MTConnectionState m_connectionState;

        private int m_cardTypeSelection = 0;

        private bool m_startTransactionActionPending;

        private bool m_turnOffLEDPending;

        private int m_emvMessageFormat = 0;

        private int m_emvARCType;

        private byte m_emvTimeout = 60;

        private byte[] ApprovedARC = new byte[] { (byte)0x8A, 0x02, 0x30, 0x30 };
        private byte[] DeclinedARC = new byte[] { (byte)0x8A, 0x02, 0x30, 0x35 };

        private int m_spiType = 0;
        private int m_uartType = 0;

        private MTOEMSpiMsr m_oemSpiMsr;
        private MTOEMUartMsr m_oemUartMsr;
        private MTOEMUartNfc m_oemUartNfc;

        delegate void deviceListDispatcher(List<MTDeviceInformation> deviceList);
        delegate void updateDisplayDispatcher(string text);
        delegate void clearDispatcher();
        delegate void updateStateDispatcher(MTConnectionState state);
        delegate void userSelectionsDisptacher(string title, List<string> selectionList, long timeout);

 
        public MainWindow ()
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

            updateState(MTConnectionState.Disconnected);
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            sendToDisplay("[PowerMode=" + e.Mode + "]");

            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    if ((m_SCRA != null) && m_SCRA.isDeviceConnected())
                    {
                        disconnect();
                    }

                    break;
                case PowerModes.Resume:
                    sendToDisplay("Power Resume");
                    break;
            }
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

        private void clearMessage(int id)
        {
            if (id == 1)
            {
                displayMessage("");
            }
            else if (id == 2)
            {
                displayMessage2("");
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

        private void displayMessage(int id, string message)
        {
            if (id == 1)
            {
                displayMessage(message);
            }
            else if (id == 2)
            {
                displayMessage2(message);
            }
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

        private void sendToDisplay(int id, string data)
        {
            if (id == 1)
            {
                sendToDisplay("[Board] " + data);
            }
            else if (id == 2)
            {
                sendToDisplay("[UART] " + data);
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

            m_SCRA.setConnectionType(m_connectionType);

            string address = getAddress();

            m_SCRA.setAddress(address);

            resetStates();

            m_spiType = SPIPortCB.SelectedIndex;
            m_uartType = UARTPortCB.SelectedIndex;

            m_SCRA.openDevice();
        }

        private void resetStates()
        {
            mState = STATE.STARTUP;

            m_startTransactionActionPending = false;
            m_turnOffLEDPending = false;
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

                sendToDisplay(deviceInfo);
            }
        }

        private void disconnect()
        {
            if (m_connectionState != MTConnectionState.Disconnected)
            {
                m_SCRA.closeDevice();
            }
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

        private String getSetDateTimeCommand()
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

            return command;
        }

        private void sendSetDateTimeCommandToBoard()
        {
            String command = getSetDateTimeCommand();

            sendCommand(command);

        }

        private void sendSetDateTimeCommandToUART()
        {
            String command = getSetDateTimeCommand();

            sendUARTCommand(command);
        }

        private int sendCommand(string command)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA.isDeviceConnected())
            {
                sendToDisplay("[Sending Command]");
                sendToDisplay(command);

                result = m_SCRA.sendCommandToDevice(command);
            }

            return result;
        }

        private int sendSPICommand(string command)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA.isDeviceConnected())
            {
                if (m_oemSpiMsr != null)
                {
                    sendToDisplay("[Sending Command to SPI MSR]");
                    sendToDisplay(command);

                    result = m_oemSpiMsr.sendData(command);
                }
            }

            return result;
        }

        private int sendUARTCommand(string command)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA.isDeviceConnected())
            {
                if (m_oemUartNfc != null)
                {
                    sendToDisplay("[Sending Command to UART NFC]");
                    sendToDisplay(command);

                    result = m_oemUartNfc.sendData(command);
                }
                else if (m_oemUartMsr != null)
                {
                    sendToDisplay("[Sending Command to UART MSR]");
                    sendToDisplay(command);

                    result = m_oemUartMsr.sendData(command);
                }
                else
                {
                    sendToDisplay("[No UART device is configured]");
                }
            }

            return result;
        }

        public int sendUARTExtendedCommand(byte[] command, byte[] data)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA.isDeviceConnected())
            {
                if (m_oemUartNfc != null)
                {
                    sendToDisplay("[Sending Exented Command to UART NFC]");

                    result = m_oemUartNfc.sendExtendedCommandBytes(command, data);
                }
                else
                {
                    sendToDisplay("[UART NFC is not configured]");
                }
            }

            return result;
        }

        private void requestEMVMessageFormat()
        {
            Task task = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(1000);

                mState = STATE.GET_EMV_MESSSAGE_FORMAT;

                int status = sendCommand("000168");

                if (status != MTSCRA.SEND_COMMAND_SUCCESS)
                {
                    mState = STATE.SETUP_TIME;
                    sendSetDateTimeCommandToBoard();
                }
            });
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
                            SetTimeButton.IsEnabled = false;
                            ScanButton.IsEnabled = false;
                            SPIPortCB.IsEnabled = false;
                            UARTPortCB.IsEnabled = false;
                            ConnectButton.IsEnabled = false;
                            DisconnectButton.IsEnabled = true;
                            DeviceTypeCB.IsEnabled = false;
                            DeviceAddressCB.IsEnabled = false;
                            SendCommandButton.IsEnabled = false;
                            EMVStartButton.IsEnabled = false;
                            EMVCancelButton.IsEnabled = false;
                            OutputTextBox.Background = Brushes.Gray;
                            sendToDisplay("[Connecting....]");
                            displayDeviceInformation();
                            break;
                        case MTConnectionState.Connected:
                            SetTimeButton.IsEnabled = true;
                            ScanButton.IsEnabled = false;
                            SPIPortCB.IsEnabled = false;
                            UARTPortCB.IsEnabled = false;
                            ConnectButton.IsEnabled = false;
                            DisconnectButton.IsEnabled = true;
                            DeviceTypeCB.IsEnabled = false;
                            DeviceAddressCB.IsEnabled = false;
                            SendCommandButton.IsEnabled = true;
                            EMVStartButton.IsEnabled = true;
                            EMVCancelButton.IsEnabled = true;
                            OutputTextBox.Background = Brushes.White;
                            clearMessage();
                            clearMessage2();
                            sendToDisplay("[Connected]");
                            requestEMVMessageFormat();
                            break;
                        case MTConnectionState.Disconnecting:
                            SetTimeButton.IsEnabled = false;
                            ScanButton.IsEnabled = false;
                            SPIPortCB.IsEnabled = false;
                            UARTPortCB.IsEnabled = false;
                            ConnectButton.IsEnabled = false;
                            DisconnectButton.IsEnabled = true;
                            DeviceTypeCB.IsEnabled = false;
                            DeviceAddressCB.IsEnabled = false;
                            SendCommandButton.IsEnabled = false;
                            EMVStartButton.IsEnabled = false;
                            EMVCancelButton.IsEnabled = false;
                            OutputTextBox.Background = Brushes.Gray;
                            sendToDisplay("[Disconnecting....]");
                            break;
                        case MTConnectionState.Disconnected:
                            SetTimeButton.IsEnabled = false;
                            ScanButton.IsEnabled = true;
                            SPIPortCB.IsEnabled = true;
                            UARTPortCB.IsEnabled = true;
                            ConnectButton.IsEnabled = true;
                            DisconnectButton.IsEnabled = false;
                            DeviceTypeCB.IsEnabled = true;
                            DeviceAddressCB.IsEnabled = true;
                            SendCommandButton.IsEnabled = false;
                            EMVStartButton.IsEnabled = false;
                            EMVCancelButton.IsEnabled = false;
                            OutputTextBox.Background = Brushes.Gray;
                            sendToDisplay("[Disconnected]");
                            m_oemSpiMsr = null;
                            m_oemUartMsr = null;
                            m_oemUartNfc = null;
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
                string address = getAddress();

                if (!string.IsNullOrEmpty(address))
                {
                    connect();
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Please make sure a device is plugged in and selected before connecting.",
                                            "No Device Selected", MessageBoxButton.OK);
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                }
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

            int index = SendToCB.SelectedIndex;

            switch (index)
            {
                case 0:
                    sendCommand(command);
                    break;
                case 1:
                    sendUARTCommand(command);
                    break;
                case 2:
                    sendSPICommand(command);
                    break;
            }
        }

        private void SetTimeCommandButton_Click(object sender, RoutedEventArgs e)
        {
            int index = SendToCB.SelectedIndex;

            switch (index)
            {
                case 0:
                    sendSetDateTimeCommandToBoard();
                    break;
                case 1:
                    sendSetDateTimeCommandToUART();
                    break;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            clearDisplay();
            clearMessage();
            clearMessage2();
        }

        private void QuickChipCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ARQCResponseCB.IsEnabled = false;
        }

        private void QuickChipCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ARQCResponseCB.IsEnabled = true;
        }

        private void EMVStartButton_Click(object sender, RoutedEventArgs e)
        {
            m_cardTypeSelection = CardTypeCB.SelectedIndex;

            switch (m_cardTypeSelection)
            {
                case 0:
                    startChipTransaction();
                    break;
                case 1:
                    if (m_uartType == 2)
                    {
                        startContactlessTransaction();
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("No NFC/Contactless device has been selected.",
                                                "No NFC Device", MessageBoxButton.OK);
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                    }
                    break;
                case 2:
                    if (m_uartType == 2)
                    {
                        startChipTransaction();
                        startContactlessTransaction();
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("No NFC/Contactless device has been selected.",
                                                "No NFC Device", MessageBoxButton.OK);
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                    }
                    break;
            }
        }

        private void EMVCancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTransaction();
        }

        private void GetTerminalConfigButton_Click(object sender, RoutedEventArgs e)
        {
            getTerminalConfiguration();
        }

        private void SetTerminalConfigButton_Click(object sender, RoutedEventArgs e)
        {
            setTerminalConfiguration();
        }

        private void CommitConfigButton_Click(object sender, RoutedEventArgs e)
        {
            commitConfiguration();
        }

        private bool isQuickChipEnabled()
        {
            return (bool)QuickChipCheckBox.IsChecked;
        }


        public void startChipTransaction()
        {
            m_startTransactionActionPending = true;

            setLED(true);
        }

        private void startTransaction()
        {
            if (m_SCRA != null)
            {
                //byte timeLimit = 0x3C;
                byte timeLimit = getEMVTimeoutValue();

                byte cardType = 0x02;   // Chip

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

        public void startContactlessTransaction()
        {
            if (m_SCRA != null)
            {
                clearMessage();
                clearMessage2();

                //m_uartContactlessInProgress = true;

                //byte timeLimit = 0x3C;
                //byte timeLimit = 0x00; // Unattended Mode
                byte timeLimit = getEMVTimeoutValue();

                byte cardType = 0x04;   // Contactless

                byte option = 0x00;
                byte[] amount = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x15, 0x00 };
                byte transactionType = 0x00; // Purchase
                //byte transactionType = 0x04; // Goods
                //byte transactionType = 0x50; // Test
                byte[] cashBack = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                byte[] currencyCode = new byte[] { 0x08, 0x40 };
                byte reportingOption = 0x02;  // All Status Changes

                byte[] data = new byte[19];

                data[0] = timeLimit;
                data[1] = cardType;
                data[2] = option;

                int lenAmount = amount.Length;

                if (lenAmount > 6)
                    lenAmount = 6;

                int i;

                for (i = 0; i < lenAmount; i++)
                {
                    data[3 + i] = amount[i];
                }

                data[9] = transactionType;

                int lenCashBack = cashBack.Length;

                if (lenCashBack > 6)
                    lenCashBack = 6;

                for (i = 0; i < lenCashBack; i++)
                {
                    data[10 + i] = cashBack[i];
                }

                data[16] = currencyCode[0];
                data[17] = currencyCode[1];

                data[18] = reportingOption;

                byte[] START_EMV_COMMAND = new byte[] { 0x03, 0x00 };

                sendToDisplay("[Start Contactless Transaction] data=" + MTParser.getHexString(data));

                sendUARTExtendedCommand(START_EMV_COMMAND, data);
            }
        }

        private void cancelTransaction()
        {
            if (m_SCRA != null)
            {
                if (m_cardTypeSelection == 0)
                {
                    cancelTransaction(0);
                }
                else if (m_cardTypeSelection == 1)
                {
                    cancelTransaction(1);
                }
                else if (m_cardTypeSelection == 2)
                {
                    cancelTransaction(0);
                    cancelTransaction(1);
                }
            }
        }

        private void cancelTransaction(int cardTypeSelection)
        {
            if (m_SCRA != null)
            {
                if (cardTypeSelection == 0)
                {
                    m_turnOffLEDPending = true;

                    long result = m_SCRA.cancelTransaction();

                    sendToDisplay("[Cancel Transaction] (Result=" + result + ")");
                }
                else if (cardTypeSelection == 1)
                {
                    sendToDisplay("[Cancel Contactless Transaction]\n");

                    byte[] CANCEL_COMMAND = new byte[] { 0x03, 0x04 };

                    sendUARTExtendedCommand(CANCEL_COMMAND, null);
                }
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

        public void setAcquirerResponse(int id, byte[] response)
        {
            if ((m_SCRA != null) && (response != null))
            {

                //if (m_cardTypeSelection == 0)
                if (id == 1)
                {
                    sendToDisplay(id, "[Sending Acquirer Response]\n" + MTParser.getHexString(response));

                    m_SCRA.setAcquirerResponse(response);
                }
                //else if (m_cardTypeSelection == 1)
                else if (id == 2)
                {
                    sendToDisplay(id, "[Sending UART Acquirer Response]\n" + MTParser.getHexString(response));

                    byte [] ARPC_COMMAND = new byte[] { 0x03, 0x03 };

                    sendUARTExtendedCommand(ARPC_COMMAND, response);
                }
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
            cardData += string.Format("MagnePrint.Length={0} bytes\n", (mpData.Length / 2) );
            cardData += string.Format("MagnePrint.Status={0}\n", m_SCRA.getMagnePrintStatus());
            cardData += string.Format("Device.Serial={0}\n", m_SCRA.getDeviceSerial());
            cardData += string.Format("Session.ID={0}\n", m_SCRA.getSessionID());
            cardData += string.Format("KSN={0}\n", m_SCRA.getKSN());

            if (m_SCRA.getSwipeCount() >= 0)
            {
                cardData += string.Format("Swipe.Count={0}\n", m_SCRA.getSwipeCount());
            }

            cardData += formatStringIfNotEmpty("Cap.MagnePrint={0}\n", m_SCRA.getCapMagnePrint());
            cardData += formatStringIfNotEmpty("Cap.MagnePrintEncryption={0}\n", m_SCRA.getCapMagnePrintEncryption());

            cardData += formatStringIfNotEmpty("Cap.MagStripeEncryption={0}\n", m_SCRA.getCapMagStripeEncryption());
            cardData += formatStringIfNotEmpty("Cap.MSR={0}\n", m_SCRA.getCapMSR());

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

            return cardData;
        }

        public string getCardDataInfo(IMTCardData cardDataObject)
        {
            MTSCRAHIDCardData cardDataCopy = new MTSCRAHIDCardData();
            cardDataCopy.setData(cardDataObject.getData());


            string cardData = "";

            cardData += string.Format("SDK.Version={0}\n", m_SCRA.getSDKVersion());

            cardData += formatStringIfNotEmpty("TLV.Version={0}\n", cardDataObject.getTLVVersion());

            cardData += formatStringIfNotEmpty("Response.Type={0}\n", cardDataObject.getResponseType());

            cardData += string.Format("Tracks.Masked={0}\n", cardDataObject.getMaskedTracks());
            cardData += string.Format("Track1.Encrypted={0}\n", cardDataObject.getTrack1());
            cardData += string.Format("Track2.Encrypted={0}\n", cardDataObject.getTrack2());
            cardData += string.Format("Track3.Encrypted={0}\n", cardDataObject.getTrack3());
            cardData += string.Format("Track1.Masked={0}\n", cardDataObject.getTrack1Masked());
            cardData += string.Format("Track2.Masked={0}\n", cardDataObject.getTrack2Masked());
            cardData += string.Format("Track3.Masked={0}\n", cardDataObject.getTrack3Masked());
            string mpData = cardDataObject.getMagnePrint();
            cardData += string.Format("MagnePrint.Encrypted={0}\n", mpData);
            cardData += string.Format("MagnePrint.Length={0} bytes\n", (mpData.Length / 2));
            cardData += string.Format("MagnePrint.Status={0}\n", cardDataObject.getMagnePrintStatus());
            cardData += string.Format("Device.Serial={0}\n", cardDataObject.getDeviceSerial());
            cardData += string.Format("Session.ID={0}\n", cardDataObject.getSessionID());
            cardData += string.Format("KSN={0}\n", cardDataObject.getKSN());

            if (m_SCRA.getSwipeCount() >= 0)
            {
                cardData += string.Format("Swipe.Count={0}\n", cardDataObject.getSwipeCount());
            }

            cardData += formatStringIfNotEmpty("Cap.MagnePrint={0}\n", cardDataObject.getCapMagnePrint());
            cardData += formatStringIfNotEmpty("Cap.MagnePrintEncryption={0}\n", cardDataObject.getCapMagnePrintEncryption());

            cardData += formatStringIfNotEmpty("Cap.MagStripeEncryption={0}\n", m_SCRA.getCapMagStripeEncryption());
            cardData += formatStringIfNotEmpty("Cap.MSR={0}\n", cardDataObject.getCapMSR());

            cardData += string.Format("Card.Data.CRC={0}\n", cardDataObject.getCardDataCRC());
            cardData += string.Format("Card.Exp.Date={0}\n", cardDataObject.getCardExpDate());
            cardData += string.Format("Card.IIN={0}\n", cardDataObject.getCardIIN());
            cardData += string.Format("Card.Last4={0}\n", cardDataObject.getCardLast4());
            cardData += string.Format("Card.Name={0}\n", cardDataObject.getCardName());
            cardData += string.Format("Card.PAN={0}\n", cardDataObject.getCardPAN());
            cardData += string.Format("Card.PAN.Length={0}\n", cardDataObject.getCardPANLength());
            cardData += string.Format("Card.Service.Code={0}\n", cardDataObject.getCardServiceCode());

            cardData += formatStringIfNotEmpty("Card.Status={0}\n", m_SCRA.getCardStatus());
            cardData += formatStringIfNotEmpty("Card.EncodeType={0}\n", m_SCRA.getCardEncodeType());

            cardData += formatStringIfNotEmpty("HashCode={0}\n", cardDataObject.getHashCode());

            if (cardDataObject.getDataFieldCount() != 0)
            {
                cardData += string.Format("Data.Field.Count={0}\n", cardDataObject.getDataFieldCount());
            }

            cardData += string.Format("Encryption.Status={0}\n", cardDataObject.getEncryptionStatus());

            cardData += formatStringIfNotEmpty("MagTek.Device.Serial={0}\n", cardDataObject.getMagTekDeviceSerial());

            cardData += string.Format("Track.Decode.Status={0}\n", cardDataObject.getTrackDecodeStatus());
            string tkStatus = cardDataObject.getTrackDecodeStatus();

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

            if (mState == STATE.GET_EMV_MESSSAGE_FORMAT)
            {
                byte[] emvMessageFormatResponseByteArray = MTParser.getByteArrayFromHexString(data);

                if (emvMessageFormatResponseByteArray.Length == 3)
                {
                    if ((emvMessageFormatResponseByteArray[0] == 0) && (emvMessageFormatResponseByteArray[1] == 1))
                    { 
                        m_emvMessageFormat = emvMessageFormatResponseByteArray[2];
                    }
                }

                mState = STATE.SETUP_TIME;
                sendSetDateTimeCommandToBoard();
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

            if (mState == STATE.SETUP_TIME)
            {
                mState = STATE.READY;

                if (m_spiType  == 1) // SPI MSR Head
                {
                    m_oemSpiMsr = new MTOEMSpiMsr(m_SCRA);

                    m_oemSpiMsr.OnDataReceived += OnSpiMsrDataReceived;
                    m_oemSpiMsr.OnResponseReceived += OnSpiMsrResponseReceived;
                    m_oemSpiMsr.OnDebugInfo += OnSpiMsrDebugInfo;
                }

                if (m_uartType == 1) // UART MSR Head
                {
                    m_oemUartMsr = new MTOEMUartMsr(m_SCRA);

                    m_oemUartMsr.OnDataReceived += OnUartMsrDataReceived;
                    m_oemUartMsr.OnDebugInfo += OnUartMsrDebugInfo;
                }
                else if (m_uartType == 2) // UART NFC Module
                {
                    m_oemUartNfc = new MTOEMUartNfc(m_SCRA);

                    m_oemUartNfc.OnDebugInfo += OnUartNfcDebugInfo;
                    m_oemUartNfc.OnDeviceResponse += OnUartNfcDeviceResponse;
                    m_oemUartNfc.OnTransactionStatus += OnUartTransactionStatus;
                    m_oemUartNfc.OnDisplayMessageRequest += OnUartDisplayMessageRequest;
                    m_oemUartNfc.OnUserSelectionRequest += OnUartUserSelectionRequest;
                    m_oemUartNfc.OnARQCReceived += OnUartARQCReceived;
                    m_oemUartNfc.OnTransactionResult += OnUartTransactionResult;
                }
            }
            else
            {
                processDeviceExtendedResponse(data);
            }
        }

        protected void OnSpiMsrDebugInfo(object sender, string data)
        {
            sendToDisplay("SPI MSR - DebugInfo: " + data);
        }

        protected void OnSpiMsrDataReceived(object sender, IMTCardData cardData)
        {
            sendToDisplay("[SPI MSR - DataReceived]");
            sendToDisplay(getCardDataInfo(cardData));
        }

        protected void OnSpiMsrResponseReceived(object sender, string response)
        {
            sendToDisplay("[SPI MSR - ResponseReceived]");
            sendToDisplay(response);
        }

        protected void OnUartMsrDebugInfo(object sender, string data)
        {
            sendToDisplay("UART MSR - DebugInfo: " + data);
        }

        protected void OnUartMsrDataReceived(object sender, string cardData)
        {
            sendToDisplay("[UART MSR - Data Received]");
            sendToDisplay(cardData);
        }

        protected void OnUartNfcDebugInfo(object sender, string data)
        {
            sendToDisplay(2, "UART NFC - Debug Info: " + data);
        }

        protected void OnUartNfcDeviceResponse(object sender, string data)
        {
            sendToDisplay(2, "[UART NFC - Device Response]");
            sendToDisplay(2, data);
        }

        protected void OnUartTransactionStatus(object sender, byte[] data)
        {
            processTransactionStatus(2, data);
        }

        protected void OnUartDisplayMessageRequest(object sender, byte[] data)
        {
            processDisplayMessageRequest(2, data);
        }

        protected void OnUartUserSelectionRequest(object sender, byte[] data)
        {
            sendToDisplay("[User Selection Request]");

            sendToDisplay(MTParser.getHexString(data));

            processSelectionRequest(2, data);
        }

        protected void OnUartARQCReceived(object sender, byte[] data)
        {
            processARQCReceived(2, data);
        }

        protected void OnUartTransactionResult(object sender, byte[] data)
        {
            processTransactionResult(2, data);
        }

        protected void processDeviceExtendedResponse(string data)
        {
            if (data.Length > 0)
            {
                byte[] dataBytes = MTParser.getByteArrayFromHexString(data);


                if (m_spiType == 1) // SPI MSR Head
                {
                    if (m_oemSpiMsr != null)
                    {
                        m_oemSpiMsr.processDeviceExtendedResponse(dataBytes);
                    }
                }

                if (m_uartType == 1) // UART MSR Head
                {
                    if (m_oemUartMsr != null)
                    {
                        m_oemUartMsr.processDeviceExtendedResponse(dataBytes);
                    }
                }
                else if (m_uartType == 2) // UART NFC Module
                {
                    if (m_oemUartNfc != null)
                    {
                        m_oemUartNfc.processDeviceExtendedResponse(dataBytes);
                    }
                }
            }
        }

        protected void OnTransactionStatus(object sender, byte[] data)
        {
            processTransactionStatus(1, data);
        }

        protected void OnDisplayMessageRequest(object sender, byte[] data)
        {
            processDisplayMessageRequest(1, data);
        }

        protected void OnUserSelectionRequest(object sender, byte [] data)
        {
            sendToDisplay(1, "[User Selection Request]");

            sendToDisplay(1, MTParser.getHexString(data));

            processSelectionRequest(1, data);
        }

        protected void OnARQCReceived(object sender, byte [] data)
        {
            processARQCReceived(1, data);
        }

        protected void OnTransactionResult(object sender, byte [] data)
        {
            processTransactionResult(1, data);
        }

        protected void processTransactionStatus(int id, byte [] data)
        {
            sendToDisplay(id, "[Transaction Status]");
            sendToDisplay(id, MTParser.getHexString(data));
        }

        protected void processDisplayMessageRequest(int id, byte [] data)

        {
            sendToDisplay(id, "[Display Message Request]");

            String message = "";

            if (( data != null ) && ( data.Length > 0 ))
            {
                message = System.Text.Encoding.UTF8.GetString(data);
            }

            sendToDisplay(id, message);
            displayMessage(id, message);
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

        protected void processSelectionRequest(int id, byte[] data)
        {
    	    if (data != null)
    	    {
    		    int dataLen = data.Length;
    		
    		    if (dataLen > 2)
    		    {
    			    byte selectionType = data[0];
    			    //data[1] = 10;  // Test
    			    long timeout = ((long) (data[1] & 0xFF) * 1000);
    			
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

        protected void processARQCReceived(int id, byte [] data)
        {
            sendToDisplay(id, "[ARQC Received]");

            sendToDisplay(id, MTParser.getHexString(data));

            if (isQuickChipEnabled())
            {
                sendToDisplay(id, "** Not sending ARQC response for Quick Chip");

                return;
            }

            if (m_emvARCType == 2)
            {
                sendToDisplay(id, "** Not sending ARQC response for No Response");

                return;
            }

            List<Dictionary<String, String>> parsedTLVList = MTParser.parseEMVData(data, true, "");

            if (parsedTLVList != null)
            {

                displayParsedTLV(id, parsedTLVList);

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
                }

                setAcquirerResponse(id, response);
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
                response[i++] = (byte) 0xF9;
                response[i++] = (byte) len;
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

        protected void processTransactionResult(int id, byte[] data)
        {
            sendToDisplay(id, "[Transaction Result]");

            sendToDisplay(id, MTParser.getHexString(data));

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

                        displayParsedTLV(id, parsedTLVList);

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
/*
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
*/
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

        private void displayParsedTLV(int id, List<Dictionary<string, string>> parsedTLVList)
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
                            sendToDisplay(id, "  " + tagString + "=" + valueString);
                        }
                    }
                }
            }
        }

    }
}

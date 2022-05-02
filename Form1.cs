using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;


using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using MTSCRANET;
using MTLIB;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Threading;

namespace Dynawave
{
  
    public partial class Form1 : Form
    {

        //string[] devicelists_array = new string[0];

        private MTSCRA m_SCRA;

        private MTConnectionType m_connectionType;

        private MTConnectionState m_connectionState;

        public object devicelists_array { get; private set; }

        string address = "";


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
        public override System.Drawing.Color ForeColor { get; set; }
      


        public Form1()
        {
            InitializeComponent();

            /* updateState(MTConnectionState.Disconnected);*/
            m_SCRA = new MTSCRA();

            m_SCRA.OnDeviceList += OnDeviceList;
            m_SCRA.OnDeviceConnectionStateChanged += OnDeviceConnectionStateChanged;


             m_emvTimeout = 60;
            // 60


            updateState(MTConnectionState.Disconnected);

        }


        private void Form1_Load(object sender, EventArgs e)
        {



        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void maximizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void Startbtn_Click(object sender, EventArgs e)
        {

            startTransaction();

            //startTransactionWithLED();



        }


        private void enableEMVInterface(bool enabled)
        {
            Startbtn.Enabled = enabled;
            Cancelbtn.Enabled = enabled;
            
        }



        private bool isQuickChipEnabled()
        {
            return m_quickChip;
        }

        private byte getEMVTimeoutValue()
        {
            /*try
            {
                String valueString = EMVTimeoutCB.Text;

                m_emvTimeout = (byte)Convert.ToInt32(valueString);
            }
            catch (Exception)
            {
            }*/

            return m_emvTimeout;
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Cancelbtn_Click(object sender, EventArgs e)
        {
            cancelTransaction();
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



        private void Clearbtn_Click(object sender, EventArgs e)
        {
            OutputTextbox.Text = String.Empty;
       
            OutputTextbox.Text = " ";
            clearDisplay();
            clearMessage();
            clearMessage2();

            
            
        }

        private void clearDisplay()
        {
            try
            {
                if (OutputTextbox != null)
                {
                    OutputTextbox.Clear();
                }
                else
                {
                    OutputTextbox.BeginInvoke(new clearDispatcher(clearDisplay),
                    System.Windows.Threading.DispatcherPriority.Normal);
                }
            }
            catch (Exception)
            {
            }
        }

        private void MessageTextBox_TextChanged(object sender, EventArgs e)
        {

        }
        private void MessageTextBox2_TextChanged(object sender, EventArgs e)
        {

        }


        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (m_connectionState == MTConnectionState.Disconnected)
            {
                connect();
            }

         
        }
        private void connect()
        {
            if (m_connectionState == MTConnectionState.Connected)
            {
                return;
            }

            m_connectionType = MTLIB.MTConnectionType.USB;


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
        private int sendCommand(string command)
        {
            //string moreInfo = getMoreInformation();
            //sendToDisplay(moreInfo);

            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA.isDeviceConnected())
            {
               OutputTextbox.AppendText("[Sending Command]");
                OutputTextbox.AppendText(command);

                result = m_SCRA.sendCommandToDevice(command);
            }

            return result;
        }


        

        private string getAddress()
        {
            string address = "";

            MTDeviceInformation devInfo = (MTDeviceInformation)devicelists_array;

            if (devInfo != null)
            {
                address = devInfo.Address;
            }

            return address;
        }

        protected void OnDeviceList(object sender, MTLIB.MTConnectionType connectionType, List<MTLIB.MTDeviceInformation> deviceList)
        {
            updateDeviceList(deviceList);
        }


        protected void updateDeviceList(List<MTLIB.MTDeviceInformation> deviceList)
        {
            try
            {
                if (devicelists_array == null)
                {
                    if (deviceList.Count > 0)
                    {
                        foreach (var device in deviceList)
                        {
                            for (var i = 0; i < deviceList.Count; i++)
                            {
                                devicelists_array.Equals(device);


                            }


                        }
                    }

                }
            }
            catch (Exception)
            {
            }
        }

            
        


        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            //  OutputTextBox.Text = "Disconnected";
            if (m_connectionState != MTConnectionState.Disconnected)
            {
                disconnect();
            }

        }

        private void disconnect()
        {
            if (m_connectionState != MTConnectionState.Disconnected)
            {
                m_SCRA.closeDevice();
            }
        }

        protected void OnDeviceConnectionStateChanged(object sender, MTLIB.MTConnectionState state)
        {
            updateState(state);
        }

        private void DeviceInsertedEvent(object sender, EventArgs e)
        {
            OutputTextbox.AppendText("[Device Inserted]");
        }

        private void DeviceRemovedEvent(object sender, EventArgs e)
        {
            OutputTextbox.AppendText("[Device Removed]");
        }




        private void OutputTextBox_TextChanged(object sender, EventArgs e)
        {
         

            // OutputTextBox.Text = "Disconnected";
            if (m_connectionState == MTConnectionState.Disconnected)
            {
                OutputTextbox.Text = "Connected";
                OutputTextbox.BackColor = System.Drawing.Color.Gray;
            }
            else
            {
                OutputTextbox.Text = "Disconnected";
            }
        }



        private void updateState(MTConnectionState state)
        {
            m_connectionState = state;

            try
            {
                if (OutputTextbox != null)
                {
                    switch (state)
                    {
                        case MTConnectionState.Connecting:
                            enableConnectedInterface(false);
                            OutputTextbox.BackColor = System.Drawing.Color.Gray;
                            enableEMVInterface(false);
                            sendToDisplay("[Connecting....]");
                            displayDeviceInformation();
                            break;
                        case MTConnectionState.Connected:
                            enableConnectedInterface(true);
                            displayDeviceFeatures();
                            enableEMVInterface(true);
                            if (m_SCRA.isDeviceOEM())
                            {
                                sendToDisplay("This device is OEM.");
                            }
                            if (m_SCRA.isDeviceEMV())
                            {
                                sendToDisplay("This device supports EMV.");
                             
                                requestEMVMessageFormat();
                            }
                            sendToDisplay("Power Management Value: " + m_SCRA.getPowerManagementValue());
                            OutputTextbox.BackColor = System.Drawing.Color.White;
                            sendToDisplay("[Connected]");
                            clearMessage();
                            clearMessage2();
                            break;
                        case MTConnectionState.Disconnecting:
                            enableConnectedInterface(false);
                            enableEMVInterface(false);
                            OutputTextbox.BackColor = System.Drawing.Color.Gray;
                            sendToDisplay("[Disconnecting....]");
                            break;
                        case MTConnectionState.Disconnected:
                            enableConnectedInterface(false);
                            enableEMVInterface(false);
                            OutputTextbox.BackColor = System.Drawing.Color.Gray;
                            sendToDisplay("[Disconnected]");
                            break;
                    }
                }
                else
                {
                    OutputTextbox.BeginInvoke(new updateStateDispatcher(updateState),
                                                    System.Windows.Threading.DispatcherPriority.Normal,
                                                    new object[] { state });
                }
            }
            catch (Exception ex)
            {

            }

        }


        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

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
                if (OutputTextbox != null)
                {
                    OutputTextbox.Text = message;
                }
                else
                {
                    OutputTextbox.BeginInvoke(new updateDisplayDispatcher(displayMessage),
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
                if (MessageTextBox2 != null)
                {
                    MessageTextBox2.Text = message;
                }
                else
                {
                  
                    MessageTextBox2.BeginInvoke(new updateDisplayDispatcher(displayMessage2),
                                                            System.Windows.Threading.DispatcherPriority.Normal,
                                                            new object[] { message });
                }
            }
            catch (Exception)
            {
            }
        }
       





        private void enableConnectedInterface(bool enabled)
        {
            ConnectButton.Enabled = !enabled;
            DisconnectButton.Enabled = enabled;
        
        }

        private void displayDeviceInformation()
        {
            MTDeviceInformation devInfo = (MTDeviceInformation)devicelists_array;

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
                OutputTextbox.Text = (deviceInfo);
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
                    OutputTextbox.Text = (info);
                }
            }
        }

       

        

        private void sendToDisplay(string data)
        {
            try
            {
                if (OutputTextbox != null)
                {
                    OutputTextbox.AppendText(data + "\n");
                 
                }
                else
                {
                    OutputTextbox.BeginInvoke(new updateDisplayDispatcher(sendToDisplay),
                                                            System.Windows.Threading.DispatcherPriority.Normal,
                                                            new object[] { data });
                }
            }
            catch (Exception)
            {
            }
            OutputTextbox.AppendText(data + "\n");
            OutputTextbox.Text = data;

        }

        private void OutputTextbox_TextChanged_2(object sender, EventArgs e)
        {
            if (m_connectionState == MTConnectionState.Disconnected)
            {
                OutputTextbox.Text = "DisConnected";
                OutputTextbox.BackColor = System.Drawing.Color.Gray;
            }
            else if (m_connectionState == MTConnectionState.Connected)
            {
                OutputTextbox.Text = "Connected";

            }
        }
    }
}
        










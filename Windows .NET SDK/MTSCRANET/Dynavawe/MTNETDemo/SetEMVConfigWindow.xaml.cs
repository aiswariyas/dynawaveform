using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MTNETDemo
{
    /// <summary>
    /// Interaction logic for SetEMVConfigWindow.xaml
    /// </summary>
    public partial class SetEMVConfigWindow : Window
    {
        private string mExtendedCommand;

        public SetEMVConfigWindow()
        {
            InitializeComponent();
        }

        public string getExtendedCommand()
        {
            return mExtendedCommand;
        }

        private string getCommandString()
        {
            if (TerminalRB.IsChecked == true)
            {
                return "0305";
            }
            else if (ApplicationRB.IsChecked == true)
            {
                return "0307";
            }
            else if (CapkRB.IsChecked == true)
            {
                return "0309";
            }

            return null;
        }

        private string getSlotString()
        {
            string slotString = "01";

            string slotText = SlotTextBox.Text;

            if (slotText != null)
            {
                int slotInt = 0;

                try
                {
                    Int32.TryParse(slotText, out slotInt);

                    StringBuilder hexString = new StringBuilder(2);
                    hexString.AppendFormat("{0:X2}", (byte)slotInt);
                    slotString = hexString.ToString();
                }
                catch (Exception)
                {
                }
            }

            return slotString;
        }

        private string getDatabaseString()
        {
            if (Db0RB.IsChecked == true)
            {
                return "00";
            }
            else if (Db1RB.IsChecked == true)
            {
                return "01";
            }
            else if (Db2RB.IsChecked == true)
            {
                return "02";
            }
            else if (Db3RB.IsChecked == true)
            {
                return "03";
            }
            else if (Db4RB.IsChecked == true)
            {
                return "04";
            }

            return "00";
        }

        private string getDeviceSerialString()
        {
            string serialString = SerialTextBox.Text;

            return serialString;
        }

        private string getObjectString()
        {
            string objectString = DataTextBox.Text;

            return objectString;
        }

        private string getMACString()
        {
            string macString = MACTextBox.Text;

            return macString;
        }

        private void updateCommand()
        {
            string commandString = getCommandString();

            if (commandString != null)
            {
                string macTypeString = "00";
                string opString = "01"; // write operation
                string serialString = getDeviceSerialString();
                string objectString = getObjectString();
                string macString = getMACString();

                if (!string.IsNullOrEmpty(serialString) && !string.IsNullOrEmpty(objectString) && (macString != null) && (macString.Length == 8))
                {
                    string dataString = macTypeString + getSlotString() + opString + getDatabaseString() + serialString + objectString + macString;
                    string sizeString = MTParser.getTwoByteLengthString(dataString.Length / 2);

                    mExtendedCommand = commandString + sizeString + dataString;
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                updateCommand();

                this.DialogResult = true;

                this.Close();
            }
            catch (Exception)
            {
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = false;

                this.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}

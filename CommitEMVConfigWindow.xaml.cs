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
    /// Interaction logic for CommitEMVConfigWindow.xaml
    /// </summary>
    public partial class CommitEMVConfigWindow : Window
    {
        private string mExtendedCommand;

        public CommitEMVConfigWindow()
        {
            InitializeComponent();
        }

        public string getExtendedCommand()
        {
            return mExtendedCommand;
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

        private void updateCommand()
        {
            string commandString ="030E";
            string sizeString = "0001";
            string databaseString = getDatabaseString();

            mExtendedCommand = commandString + sizeString + databaseString;
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

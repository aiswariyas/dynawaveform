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
            //InitializeComponent();
        }

        public string getExtendedCommand()
        {
            return mExtendedCommand;
        }

        

       

        

        

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               // updateCommand();

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

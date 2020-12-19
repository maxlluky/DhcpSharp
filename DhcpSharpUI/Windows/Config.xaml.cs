using PcapDotNet.Core;
using System;
using System.Windows;

namespace DhcpSharpUI
{
    /// <summary>
    /// Interaktionslogik für Config.xaml
    /// </summary>
    public partial class Config : Window
    {
        Localhost localhost = new Localhost();

        public Config()
        {
            InitializeComponent();
        }


        private void Window_Initialized(object sender, EventArgs e)
        {
            txbInterface.Text = DhcpSharpUI.Properties.Settings.Default.s_InterfaceIndex.ToString();
            txbGateway.Text = DhcpSharpUI.Properties.Settings.Default.s_Gateway;
            txbStart.Text = DhcpSharpUI.Properties.Settings.Default.s_ScopeStart;
            txbEnd.Text = DhcpSharpUI.Properties.Settings.Default.s_ScopeEnd;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //--Get all Interfaces
            if (localhost.getUseableInterfaces().Count == 0)
            {
                rtxbInfo.AppendText("No interfaces found! Make sure WinPcap is installed.\r");
                return;
            }

            // Print the list of all Interfaces with Information
            for (int i = 0; i != localhost.getUseableInterfaces().Count; ++i)
            {
                LivePacketDevice device = localhost.getUseableInterfaces()[i];
                if (device.Description != null)
                {
                    rtxbInfo.AppendText(i + ": " + device.Description + "\r");
                }
                else
                {
                    rtxbInfo.AppendText(i + ": " + " (No description available)\r");
                }

                // Print IP-Information for each Interface
                printInterfaceInfo(localhost.getUseableInterfaces()[i]);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //--Starts DhcpSharp with the configured values.
            if (txbInterface.Text != "" & txbGateway.Text != "" & txbStart.Text != "" & txbEnd.Text != "")
            {
                MainWindow mainWindow = new MainWindow(Convert.ToInt32(txbInterface.Text), txbGateway.Text, txbStart.Text, txbEnd.Text);
                mainWindow.Show();

                this.Close();

                DhcpSharpUI.Properties.Settings.Default.s_InterfaceIndex = Convert.ToInt32(txbInterface.Text);
                DhcpSharpUI.Properties.Settings.Default.s_Gateway = txbGateway.Text;
                DhcpSharpUI.Properties.Settings.Default.s_ScopeStart = txbStart.Text;
                DhcpSharpUI.Properties.Settings.Default.s_ScopeEnd = txbEnd.Text;

                DhcpSharpUI.Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("Please fill the Textboxes with valid Data!");
            }
        }

        private void printInterfaceInfo(IPacketDevice pDevice)
        {
            foreach (DeviceAddress address in pDevice.Addresses)
            {
                if (address.Address != null & address.Address.Family.Equals(SocketAddressFamily.Internet))
                {
                    rtxbInfo.AppendText("\tAddress: " + address.Address + "\r");
                }
            }
        }
    }
}

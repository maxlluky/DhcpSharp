using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace DhcpSharpUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //--Classes
        private static Localhost localhost = new Localhost();
        private static AddressPool addresspool = new AddressPool();
        private static Interface inter = new Interface(localhost);
        private Service service;

        //--Constructor Parameters
        private int interfaceIndex;
        private string gatewayIp;
        private string scopeStartIp;
        private string scopeEndIp;

        public MainWindow(int pInterfaceIndex, string pGateway, string pStart, string pEnd)
        {
            IntPtr pHandle = GetCurrentProcess();
            SetProcessWorkingSetSize(pHandle, -1, -1);

            interfaceIndex = pInterfaceIndex;
            gatewayIp = pGateway;
            scopeStartIp = pStart;
            scopeEndIp = pEnd;

            InitializeComponent();

            //--Create a new Service, Builder-Class can do GUI changes.
            service = new Service(localhost, addresspool, inter, this);
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //--Configures Service
            initializeLocalhost();

            //--Starts Service in a seperat Thread
            Thread dhcpServiceThread = new Thread(service.startListen);
            dhcpServiceThread.Start();
        }

        private void initializeLocalhost()
        {
            //--Set the interface by Interface-Index
            inter.setInterfaceIndex(interfaceIndex);
            localhost.setActiveInterface(interfaceIndex);

            //--Define the Gateway
            addresspool.setGatewayIpAddress(IPAddress.Parse(gatewayIp));

            //--Define Domain Name
            addresspool.setDomainName("caly.soft");

            //--Define the Addresspool
            addresspool.setAddressScope(IPAddress.Parse(scopeStartIp), IPAddress.Parse(scopeEndIp));
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GC.Collect();
        }

        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();
    }
}

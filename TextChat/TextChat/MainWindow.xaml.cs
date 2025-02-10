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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace TextChat
{
    public partial class MainWindow : Window
    {
        Grid currentWindow;

        TcpListener listener;
        TcpClient client;
        NetworkStream stream;
        string username = "";

        public MainWindow()
        {
            InitializeComponent();
            currentWindow = mainMenuGrid;
            UsernameContinue.Click += SetUserName;
        }

        private void HideCurrentWindow()
        {
            if(currentWindow != null)
            {
                currentWindow.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowCurrentWindow()
        {
            currentWindow.Visibility = Visibility.Visible;
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OpenWindow(object sender, RoutedEventArgs e)
        {
            HideCurrentWindow();
            currentWindow = this.FindName((string)((Button)sender).Tag) as Grid;
            ShowCurrentWindow();
        }

        private void SetUserName(object sender, RoutedEventArgs e)
        {
            username = UsernameField.Text;
        }

        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsernameContinue.IsEnabled = ValidUsername(UsernameField.Text);
        }

        private void Servername_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServernameContinue.IsEnabled = ValidUsername(ServernameField.Text);
        }

        private bool ValidUsername(string name)
        {
            if (name.Length < 3)
            {
                return false;
            }
            for(int i = 0; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private void ipAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            IpContinue.IsEnabled = validIpAdress(ipAddressField.Text);
        }

        private bool validIpAdress(string ip)
        {
            Error.Text = "";
            if(ip.Length < 8 || ip.Length > 16) //Too short or too long
            {
                Error.Text = "Ip adress is wrong size";
                return false;
            }
            // -- Checks that symbols are correct -- //
            char[] validSymbols = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
            for(int i = 0; i < ip.Length; i++)
            {
                if (!validSymbols.Contains(ip[i]))
                {
                    Error.Text = "Contains invalid symbol: " + ip[i];
                    return false;
                }
            }

            string[] chunks = ip.Split('.');
            if(chunks.Length != 5) //Must be 5 blocks, leaves an empty block afterwards
            {
                Error.Text = "Wrong amount of chunks";
                return false;
            }

            for(int i = 0; i < chunks.Length; i++)
            {
                if(i == 4)
                {
                    if (chunks[i] != "")
                    {
                        return false;
                    }
                    continue;
                }
                bool valid = int.TryParse(chunks[i], out int result);
                if (!valid)
                {
                    Error.Text = "Invalid chunk";
                    return false;
                }
                if (result < 0 || result > 255)
                {
                    Error.Text = "Invalid size for a chunk";
                    return false;
                }
            }

            return true;
        }

        bool SendMessage()
        {
            string messageToSend = chatInputField.Text;

            //Disconnecting
            if (messageToSend.ToLower() == "" || messageToSend.ToLower() == null || messageToSend.ToLower() == " ")
            {
                return false;
            }

            messageToSend = username + ": " + messageToSend;
            int byteCount = Encoding.UTF8.GetByteCount(messageToSend + 1);
            byte[] sendData = Encoding.UTF8.GetBytes(messageToSend);
            stream.Write(sendData, 0, sendData.Length);
            return true;
        }

        bool Connect(string ip = "127.0.0.1", int port = 1302)
        {
            try
            {
                client = new TcpClient(ip, port);
                stream = client.GetStream();
                ConnectionText.Text = "Connected to the server " + ip;
                return true;
            }
            catch (Exception e)
            {
                ConnectionText.Text = "Connection to " + ip + " failed";
                return false;
            }

        }

        void CloseStream()
        {
            stream.Close();
            client.Close();
        }
    }
}
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using NetScope.Services;

namespace NetScope.Views
{
    public partial class WakeOnLanWindow : Window
    {
        private readonly string _ip;
        private readonly string _mac;

        public WakeOnLanWindow(string ip, string mac, string hostname)
        {
            InitializeComponent();
            _ip = ip;
            _mac = mac;
            
            lblHost.Text = hostname;
            lblIp.Text = ip;
            lblMac.Text = mac;
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendWakeOnLan(_mac);
                MessageBox.Show("Magic packet sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending Wake-on-LAN: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggerService.LogError("WOL failed", ex);
            }
        }

        private void SendWakeOnLan(string macAddress)
        {
            byte[] macBytes = ParseMac(macAddress);
            byte[] packet = new byte[17 * 6];

            for (int i = 0; i < 6; i++) packet[i] = 0xFF;
            for (int i = 1; i <= 16; i++)
            {
                for (int j = 0; j < 6; j++) packet[i * 6 + j] = macBytes[j];
            }

            using (UdpClient client = new UdpClient())
            {
                client.Connect(IPAddress.Broadcast, 9);
                client.Send(packet, packet.Length);
            }
        }

        private byte[] ParseMac(string mac)
        {
            string cleanMac = System.Text.RegularExpressions.Regex.Replace(mac, "[^A-Fa-f0-9]", "");
            byte[] bytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                bytes[i] = Convert.ToByte(cleanMac.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}

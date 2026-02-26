using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;

namespace NetScope
{
    public partial class WakeOnLanWindow : Window
    {
        private string ip;
        private string mac;
        private string hostname;

        public WakeOnLanWindow(string ip, string mac, string hostname)
        {
            InitializeComponent();
            this.ip = ip;
            this.mac = mac;
            this.hostname = hostname;

            lblHostname.Text = string.IsNullOrEmpty(hostname) ? "Desconhecido" : hostname;
            lblIpAddress.Text = ip;
            lblMacAddress.Text = mac;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnWake_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendWakeOnLan(mac);
                MessageBox.Show($"Comando Wake-on-LAN enviado com sucesso para {mac}!", "Sucesso", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao enviar pacote WOL: {ex.Message}", "Erro", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendWakeOnLan(string macAddress)
        {
            // Limpar o MAC address de qualquer separador
            string cleanMac = Regex.Replace(macAddress, "[^A-F0-9a-f]", "");
            
            if (cleanMac.Length != 12)
            {
                throw new ArgumentException("Formato de MAC address inválido.");
            }

            byte[] dgram = new byte[102];

            // 6 bytes de 0xFF
            for (int i = 0; i < 6; i++)
            {
                dgram[i] = 0xFF;
            }

            // 16 repetições do MAC address
            byte[] macBytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                macBytes[i] = byte.Parse(cleanMac.Substring(i * 2, 2), NumberStyles.HexNumber);
            }

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    dgram[6 + i * 6 + j] = macBytes[j];
                }
            }

            // Enviar via UDP broadcast porta 9
            using (UdpClient client = new UdpClient())
            {
                client.Connect(IPAddress.Broadcast, 9);
                client.Send(dgram, dgram.Length);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;


namespace ChatApp
{
    public partial class Form1 : Form
    {
        private Socket socket;
        private EndPoint epLocal, epRemote;
        private byte[] buffer;
        private string senderPassword;
        private string algorithmType;
        private string message;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // set up socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress,true);

            // get user ip
            txtLocalIp.Text = GetLocalIp();
            txtRemoteIp.Text = GetLocalIp();
            txtLocalIp.Enabled = false;

            btnEnterPassword.Hide();
            
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
           
            try
            {
                if (txtLocalPort.Text.Trim() != string.Empty && txtRemoteIp.Text.Trim() != string.Empty && txtRemotePort.Text.Trim() != string.Empty)
                {

                    txtRemotePort.Enabled = false;
                    txtLocalPort.Enabled = false;
                    txtRemoteIp.Enabled = false;
                    // binding socket
                    epLocal = new IPEndPoint(IPAddress.Parse(txtLocalIp.Text), Convert.ToInt32(txtLocalPort.Text));
                    socket.Bind(epLocal);

                    // connecting to remote ip
                    epRemote = new IPEndPoint(IPAddress.Parse(txtRemoteIp.Text), Convert.ToInt32(txtRemotePort.Text));
                    socket.Connect(epRemote);

                    // listening the specific port
                    buffer = new byte[1500];
                    var a = socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote,
                        new AsyncCallback(MessageCallBack), buffer);

                    btnConnect.Enabled = false;
                }
                else
                {
                    MessageBox.Show("Please enter all fields");
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }


        }

        private string GetLocalIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
                
            }
            return "127.0.0.1";
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Convert string message to byte[]
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                byte[] sendingMessage = new byte[1500];
                
                if (cmbAlgoritm.Text != "" && txtPassword.Text != "") 
                {

                    sendingMessage = aEncoding.GetBytes(cmbAlgoritm.Text.Trim() + "??__" + txtMessage.Text.Trim() + "??__" + sha256(txtPassword.Text.Trim()));

                    //sendingPassword = aEncoding.GetBytes(sha256(txtPassword.Text));
                    // Sending the encoded message
                    socket.Send(sendingMessage);
                    
                    //socket.Send(sendingPassword);

                    // adding to the listbox
                    listMessages.Items.Add("Me: " + txtMessage.Text);
                    txtMessage.Text = "";
                    txtPassword.Text = "";

                    ButtonCheck();

                }
                else
                {
                    MessageBox.Show("Please choose an encryption algorithm");
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                byte[] receiveData = new byte[1500];
                receiveData = (byte[])aResult.AsyncState;
                
                // converting byte[] to string
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                string receivedMessage = aEncoding.GetString(receiveData);
                string[] post = receivedMessage.Split("??__");
                // adding this message into Listbox
                
                listMessages.Items.Add("Friend: " + post[0] + "-"+ "Encrypted message");

                algorithmType = post[0];
                message = post[1];
                senderPassword = post[2];


                //MessageBox.Show(senderPassword);
                buffer = new byte[1500];
                socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote,
                    new AsyncCallback(MessageCallBack), buffer);
                ButtonCheck();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private string sha256(string text)
        {
            SHA256 sha256Encrypting = new SHA256CryptoServiceProvider();
            byte[] bytes = sha256Encrypting.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder builder = new StringBuilder();

            foreach (var item in bytes)
            {
                builder.Append(item.ToString("x2"));
            }

            return builder.ToString();
        }

        private void btnEnterPassword_Click(object sender, EventArgs e)
        {
            
            if (txtPassword.Text != "")
            {
                //MessageBox.Show("label: " + lblPassword.Text);
                string password = sha256(txtPassword.Text);
                //MessageBox.Show("password: " + password);

                if (senderPassword.Contains(password))
                {
                    MessageBox.Show("Şifre doğru");
                    listMessages.Items[^1] = "Friend: " + algorithmType + "-" + message;
                    btnEnterPassword.Hide();
                    btnSend.Show();
                    txtMessage.Enabled = true;
                    txtPassword.Text = "";
                    //ButtonCheck();
                }
                else
                {
                    MessageBox.Show("Şifre yanlış");
                }
            }
            else
            {
                MessageBox.Show("Şifre giriniz");
            }
        }

        
        private void ButtonCheck()
        {
            string message = listMessages.Items[^1].ToString();
            //MessageBox.Show(message);
            string[] messagelist = message.Split(":");
            if (messagelist[0] == "Friend")
            {
                btnEnterPassword.Show();
                btnSend.Hide();
                txtMessage.Enabled = false;
            }
            else
            {
                btnEnterPassword.Hide();
                btnSend.Show();
                txtMessage.Enabled = true;
            }

        }
    }
}

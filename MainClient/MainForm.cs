using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using Nancy.Json;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.Net.Sockets;

namespace MainClient
{
    public partial class MainForm : Form
    {
        private string user;
        private string user_id;
        private string peer;
        private string ip;
        private int port;
        private System.ComponentModel.BackgroundWorker stateUpdateWorker;
        private List<System.Windows.Forms.Button> buttons;


        public MainForm(string user, string user_id)
        {
            this.user = user;
            this.user_id = user_id;
            Console.WriteLine(user);
            Console.WriteLine(user_id);
            InitializeComponent();
            this.stateUpdateWorker = new System.ComponentModel.BackgroundWorker();
            this.stateUpdateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.UpdateState);
            this.buttons = new List<System.Windows.Forms.Button>() {
                this.button1,
                this.button2,
                this.button3,
                this.button4,
                this.button5,
                this.button6,
            };
            Start();
        }

        private void Start()
        {

            this.stateUpdateWorker.RunWorkerAsync();

        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        private void UpdateState(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("yes");
            while (true)
            {
                Console.WriteLine("hi");
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.1.13:5000/get_state");
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                Console.WriteLine("hi2");
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    Console.WriteLine("hi2");
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        user = this.user,
                        user_id = this.user_id,
                    });

                    streamWriter.Write(json);
                }
                Console.WriteLine("hi3");
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {

                    var result = streamReader.ReadToEnd();
                    Console.WriteLine();
                    Console.WriteLine(result.ToString());
                    StateResponse json = new JavaScriptSerializer().Deserialize<StateResponse>(result.ToString());
                    ParseNewState(json);


                }
                Thread.Sleep(4000);
            }
        }
        private void ParseNewState(StateResponse json)
        {
            foreach (string username in json.new_disconnections)
            {
                int index = GetButtonIndexByName(username);
                if (index != -1)
                {
                    this.buttons[index].Text = "";
                    this.buttons[index].Enabled = false;
                }
            }
            foreach (string username in json.new_connections)
            {
                int index = EmptyButtonIndex();
                if (index != -1)
                {
                    this.buttons[index].Text = username;
                    this.buttons[index].Enabled = true;
                }
            }
            if (json.requesting_user != "")
            {
                this.peer = json.requesting_user;
                this.label3.Text = this.peer + " Wants to control your screen";
                this.acceptButton.Enabled = true;
                this.declineButton.Enabled = true;
            }
            if (json.target_answer != "")
            {
                if (json.target_answer == "accept")
                {
                    this.ip = json.ip;
                    this.port = json.port;
                    IpLabel.Text = this.ip;
                    PortLabel.Text = this.port.ToString();
                    ControllerForm cf = new ControllerForm(this.ip, this.port);
                    cf.ShowDialog();

                }
            }
        }


        private int GetButtonIndexByName(string username)
        {
            int index = 0;

            foreach (Button b in this.buttons)
            {
                if (b.Text == username)
                {
                    return index;
                }
                else
                {
                    index++;
                }

            }
            return -1;
        }
        private void ParseConnectionRequest(ConnectionResponse json)
        {
            string message = json.message;
            this.label3.Text = message;

        }

        private int EmptyButtonIndex()
        {
            int index = 0;
            foreach (Button b in this.buttons)
            {
                if (b.Text == "")
                {
                    return index;

                }
                else
                {
                    index++;
                }
            }
            return -1;
        }
        private void RequestConnection(object sender, EventArgs e)
        {
            string target_user = (sender as Button).Text;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.1.13:5000/request_connection");
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            Console.WriteLine("hi2");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                Console.WriteLine("hi2");
                string json = new JavaScriptSerializer().Serialize(new
                {
                    user = this.user,
                    user_id = this.user_id,
                    target_user = target_user,
                });

                streamWriter.Write(json);
            }
            Console.WriteLine("hi3");
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {

                var result = streamReader.ReadToEnd();
                Console.WriteLine();
                Console.WriteLine(result.ToString());
                ConnectionResponse json = new JavaScriptSerializer().Deserialize<ConnectionResponse>(result.ToString());
                ParseConnectionRequest(json);


            }
        }
        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        private void ReplyHandler(object sender, EventArgs e)
        {
            string buttonText = (sender as Button).Text;
            if (buttonText == "Accept")
            {
                this.ip = GetLocalIPAddress();
                this.port = FreeTcpPort();
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.1.13:5000/answer_request");
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        user = this.user,
                        user_id = this.user_id,
                        target_answer = "accept",
                        ip = this.ip,
                        port = this.port,

                    }); ;

                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {

                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result.ToString());
                    ConnectionResponse json = new JavaScriptSerializer().Deserialize<ConnectionResponse>(result.ToString());
                    ParseConnectionRequest(json);

                }
                ControlledForm cf = new ControlledForm(this.ip, this.port);
                cf.ShowDialog();
            }
            else
            {

                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.1.13:5000/answer_request");
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        user = this.user,
                        user_id = this.user_id,
                        target_answer = "decline",
                    });

                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {

                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result.ToString());
                    ConnectionResponse json = new JavaScriptSerializer().Deserialize<ConnectionResponse>(result.ToString());
                    ParseConnectionRequest(json);

                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
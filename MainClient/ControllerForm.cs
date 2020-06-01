using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace MainClient
{
    public partial class ControllerForm : Form // server code
    {
        double MouseXRatio; double MouseYRatio;
        public static Bitmap bmpScreenshot;
        public bool start = false;
        public static Graphics gfxScreenshot;
        public string ip;
        public int port;
        int countRead = 0, countWrite = 0;
        private TcpClient client = null;
        
        public ControllerForm(string ip,int port) {
            this.ip = ip;
            this.port = port;
            InitializeComponent();
            recievingWorker.RunWorkerAsync();
            button2.Enabled = false;
             // run server
        }
        private void button1_Click(object sender, EventArgs e) { // turn on server ready to receive picture
            button1.Enabled = false; button2.Enabled = true;
            this.start = true;
            recievingWorker.RunWorkerAsync(); // run client
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            double xCoordinate = e.X;
            double yCoordinate = e.Y;
            this.MouseXRatio = xCoordinate / this.pictureBox1.Size.Width;
            this.MouseYRatio = yCoordinate / this.pictureBox1.Size.Height;
            if (this.client == null)
                return;
            Console.WriteLine("Down");
            ControlMessage message = new ControlMessage(this.MouseXRatio, this.MouseYRatio);
            if (e.Button == MouseButtons.Left)
            {
                message.clickFlag = ControlMessage.LeftDown;
            }
            else if (e.Button == MouseButtons.Right)
            {
                message.clickFlag = ControlMessage.RightDown;
            }     
            sendControlMessage(message);
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            double xCoordinate = e.X;
            double yCoordinate = e.Y;
            this.MouseXRatio = xCoordinate / this.pictureBox1.Size.Width;
            this.MouseYRatio = yCoordinate / this.pictureBox1.Size.Height;
            if (this.client == null)
                return;
            Console.WriteLine("Up");
            
            ControlMessage message = new ControlMessage(this.MouseXRatio, this.MouseYRatio);
            if (e.Button == MouseButtons.Left)
            {
                message.clickFlag = ControlMessage.LeftUp;
            }
            else if (e.Button == MouseButtons.Right)
            {
                message.clickFlag = ControlMessage.RightUp;
            }
            sendControlMessage(message);
            
        }
        private void sendControlMessage(ControlMessage con)
        {
            NetworkStream ns;
            byte[] data;
            ns = this.client.GetStream();
            if (ns.CanWrite)
            {
                BinaryFormatter bf = new BinaryFormatter();
                data = con.ToBytes();
                bf.Serialize(ns, data);
                //ns.Write(data, 0, data.Length);
                Console.WriteLine("bytearray sent to server {0}", data.Length);
            }
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            double xCoordinate = e.X;
            double yCoordinate = e.Y;
            this.MouseXRatio = xCoordinate / this.pictureBox1.Size.Width;
            this.MouseYRatio = yCoordinate / this.pictureBox1.Size.Height;

            if (this.client == null)
                return;
            Console.WriteLine("Click");
            ControlMessage message = new ControlMessage(this.MouseXRatio, this.MouseYRatio);
            if (e.Button == MouseButtons.Left)
            {
                message.clickFlag = ControlMessage.LeftClick;
            }
            else if(e.Button == MouseButtons.Right)
            {
                message.clickFlag = ControlMessage.RightClick;
            }
            sendControlMessage(message);
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            double xCoordinate = e.X;
            double yCoordinate = e.Y;
            this.MouseXRatio = xCoordinate / this.pictureBox1.Size.Width;
            this.MouseYRatio = yCoordinate / this.pictureBox1.Size.Height;

            if (this.client == null)
                return;
            ControlMessage message = new ControlMessage(this.MouseXRatio, this.MouseYRatio);
            sendControlMessage(message);

        }

        
        private void button2_Click(object sender, EventArgs e) {
            //Console.WriteLine("button2_Click - retry process"); 
            button1.Enabled = true;
            button2.Enabled = false;
            start = false;
        }
        public byte[] imageToByteArray(System.Drawing.Image imageIn) { // image to byte array - client to server
            countWrite++;
            //Console.WriteLine("save to byte array {0}",countWrite);  
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public void imageFromByteArray(byte[] arr)
        { // bytearray to image and display in picturebox
            countRead++;
            //Console.WriteLine("converting byte array to picturebox {0}", countRead);
            MemoryStream mStream = new MemoryStream();
            mStream.Write(arr, 0, Convert.ToInt32(arr.Length));
            pictureBox1.Image = new Bitmap(mStream, false);
            mStream.Dispose();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Console.WriteLine("Hello world!1");
            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.client == null)
                return;
            ControlMessage message = new ControlMessage(this.MouseXRatio, this.MouseYRatio);
            byte KeyCode = (byte)e.KeyCode;
            message.KeyCode = KeyCode;            
            message.KeyBoardFlag = ControlMessage.KeyDown;
            sendControlMessage(message);
            Console.WriteLine(KeyCode);
            e.Handled = true;
            e.SuppressKeyPress = true;

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.client == null)
                return;
            ControlMessage message = new ControlMessage(this.MouseXRatio,this.MouseYRatio);
            byte KeyCode = (byte)e.KeyCode;
            message.KeyCode = KeyCode;
            message.KeyBoardFlag = ControlMessage.KeyUp;
            sendControlMessage(message);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
 

        private void Form1_Resize(object sender, EventArgs e)
        {
            
        }

        private void clientToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e) { // this is the server - receive the picture & display
            //Console.WriteLine("start server");
            Console.WriteLine("start client");
            TcpClient TCPC = new TcpClient();
            TCPC.Connect(this.ip, this.port);
            this.client = TCPC;
            

            while (true) {
                NetworkStream netStream = client.GetStream();
                if (netStream.DataAvailable)
                {
                    //Console.WriteLine("data available");
                    //byte[] bytes = new byte[client.ReceiveBufferSize]; // Reads NetworkStream into a byte buffer. / client.ReceiveBufferSize = 8192
                    //netStream.Read(bytes, 0, (int)client.ReceiveBufferSize); // return anything from 0 to numBytesToRead.
                    //imageFromByteArray(bytes);
                    BinaryFormatter bf = new BinaryFormatter();
                    byte[] bytes = (byte[])bf.Deserialize(netStream);
                    imageFromByteArray(bytes);
                    //using (MemoryStream ms = new MemoryStream())
                    //{
                    //    int read;
                    //    do
                    //    {
                    //        read = netStream.Read(bytes, 0, bytes.Length);
                    //        //Console.WriteLine("data read {0}", read);
                    //        ms.Write(bytes, 0, read);
                    //    } while (read == client.ReceiveBufferSize);

                    //    byte[] bytesToSend = ms.ToArray();
                    //    //Console.WriteLine("calling imageFromByteArray with bytesTosend {0}", bytesToSend.Length);
                    //    imageFromByteArray(bytesToSend);
                    //}
                }
            }
        }
    }
}

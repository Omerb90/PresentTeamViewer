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
using WindowsInput;

namespace MainClient
{
    public partial class ControlledForm : Form
    {
        private TcpListener socket = null;
        private TcpClient client = null;
        public static Bitmap bmpScreenshot;
        public bool start = false;
        public static Graphics gfxScreenshot;
        public bool CanControl;
        public string ip;
        public int port;
        int countRead = 0, countWrite = 0;
        Win32.POINT p;

        public ControlledForm(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            InitializeComponent();
            this.Connect();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            this.button2.Enabled = true;
            this.Connect();

        }

        public void fillpic()
        { // screen shot taken by client and displayed on picturebox
            //Console.WriteLine("fillpic");
            bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        { // image to byte array - client to server
            countWrite++;
            //Console.WriteLine("save to byte array {0}", countWrite);
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        private void Connect()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Parse(this.ip), this.port);
            serverSocket.Start();
            Console.WriteLine("started socket");
            TcpClient client = serverSocket.AcceptTcpClient();
            Console.WriteLine("receive tcp by server");
            this.client = client;


            if (this.client!=null)
            {
                //Console.WriteLine("client connected");
                this.recievingWorker.RunWorkerAsync();
                this.sendingWorker.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            NetworkStream ns;
            byte[] data;
            while (true)
            {
                ns = this.client.GetStream();
                fillpic(); // take screenshot
                if (ns.CanWrite)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    data = imageToByteArray(bmpScreenshot);
                    bf.Serialize(ns, data);
                   // ns.Write(data, 0, data.Length);
                    //Console.WriteLine("bytearray sent to server {0}", data.Length);
                }
            }
        }


        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                NetworkStream netStream = this.client.GetStream();
                if (netStream.DataAvailable)
                {
                    Console.WriteLine("data available");
                    byte[] bytes = new byte[this.client.ReceiveBufferSize]; // Reads NetworkStream into a byte buffer. / client.ReceiveBufferSize = 8192
                    //netStream.Read(bytes, 0, (int)client.ReceiveBufferSize); // return anything from 0 to numBytesToRead.
                    //imageFromByteArray(bytes);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        //int read;
                        //do
                        //{
                        //    read = netStream.Read(bytes, 0, bytes.Length);
                        //    Console.WriteLine("data read {0}", read);
                        //    ms.Write(bytes, 0, read);
                        //} while (read == this.client.ReceiveBufferSize);
                        BinaryFormatter bf = new BinaryFormatter();
                        byte[] bytesToSend = (byte[])bf.Deserialize(netStream);
                        //byte[] bytesToSend = ms.ToArray();
                        Console.WriteLine("calling imageFromByteArray with bytesTosend {0}", bytesToSend.Length);
                        ControlMessage message = ControlMessage.FromBytes(bytesToSend);
                        Win32.POINT p = new Win32.POINT();
                        Rectangle resolution = Screen.PrimaryScreen.Bounds;
                        p.x = Convert.ToInt16(message.MouseX * resolution.Width - this.Location.X)-8;
                        p.y = Convert.ToInt16(message.MouseY * resolution.Height - this.Location.Y)-31;
                        this.p = p;
                        //this.label1.Text = "x:" + p.x + "y: " + p.y;
                        Win32.ClientToScreen(this.Handle, ref p);
                        Win32.SetCursorPos(p.x, p.y);
                        
                        //this.label2.Text = System.Windows.Forms.Control.MousePosition.X.ToString() + "  " + System.Windows.Forms.Control.MousePosition.Y.ToString();
                        switch (message.clickFlag)
                        {
                            case 0:
                                break;
                            case 1:
                                MouseControl.LeftDown();
                                break;
                            case 2:
                                MouseControl.LeftUp();
                                break;
                            case 3:
                                MouseControl.LeftClick();                               
                                break;
                            case 4:
                                MouseControl.RightDown();
                                break;
                            case 5:
                                MouseControl.RightUp();
                                break;
                            case 6:
                                MouseControl.RightClick();
                                break;
                            default:
                                break;
                        }
                        this.label1.Text = message.KeyBoardFlag.ToString();
                        this.label2.Text = message.KeyCode.ToString();
                        switch (message.KeyBoardFlag)
                        {
                            case 0:
                                break;
                            case 1:
                                KeyBoardControl.KeyUp(message.KeyCode);                               
                                break;
                            case 2:
                                KeyBoardControl.KeyDown(message.KeyCode);
                                break;
                            default:
                                break;
                        }
                            
                    }
                }
            }
        }

    

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}

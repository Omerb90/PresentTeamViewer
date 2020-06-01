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
using System.IO;
using Nancy.Json;
using System.Runtime.InteropServices;


namespace MainClient
{
    public partial class LoginForm : Form
    {
        private string user_id;
        private string user;
        public LoginForm()
        {
            InitializeComponent();
        }


        private void label3_Click(object sender, EventArgs e)
            {

            }

        public string GetUser()
        {
            return this.user;
        }
        public string GetUser_id()
        {
            return this.user_id;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.1.13:5000/login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = new JavaScriptSerializer().Serialize(new
                {
                    user = textBox1.Text,
                });

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine();
                Console.WriteLine(result.ToString());
                dynamic json = new JavaScriptSerializer().DeserializeObject(result.ToString());
                var user_id = json["user_id"].ToString();
                this.user_id = user_id.ToString();
                this.user = textBox1.Text;
                Console.WriteLine(user_id.ToString());
                DialogResult = DialogResult.OK;
                
            }
        }
    }
}
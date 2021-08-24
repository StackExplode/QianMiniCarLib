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
using MiniCarLib;
using MiniCarLib.Core;

namespace MiniCarLibTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        UDPDriver udps = new UDPDriver();
        CarUDPClient last_cl;
        private void button1_Click(object sender, EventArgs e)
        {
            udps.SetParameter(IPAddress.Any, 8989);
            udps.Init();
            udps.Start();
            udps.OnComDataReceived += (c, data) =>
            {
                
                this.Invoke((MethodInvoker)(() => {
                    last_cl = c as CarUDPClient;
                    textBox1.Text += last_cl.Client.ToString() + ":";
                    textBox1.Text += Encoding.Default.GetString(data);
                    textBox1.Text += Environment.NewLine;
                }));
            };

       
        }

 

        private void button2_Click(object sender, EventArgs e)
        {
            udps.SendData(last_cl, Encoding.Default.GetBytes("Welcome!"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            udps.Stop();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var list = Util.GetAllNetInterfaceV4();
            textBox1.Text = "";
            foreach(var x in list)
            {
                textBox1.Text += x.Item1 + ":" + x.Item2.ToString();
                textBox1.Text += Environment.NewLine;
            }
        }

        TCPDriver tcps = new TCPDriver();
        private void button7_Click(object sender, EventArgs e)
        {
            tcps.SetParameter(IPAddress.Any, 8988);
            tcps.Init();
            
            tcps.OnComClientConnected += (cl) => {
                this.Invoke((MethodInvoker)(() =>
                {
                    CarTCPClient carclient = cl as CarTCPClient;
                    textBox2.Text += (carclient.Client.Client.RemoteEndPoint as IPEndPoint).ToString() + "已连接！";
                    textBox2.Text += Environment.NewLine;
                }));
            };
            tcps.OnComClientDisconneted += (cl) =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    CarTCPClient carclient = cl as CarTCPClient;
                    textBox2.Text += (carclient.Client.Client.RemoteEndPoint as IPEndPoint).ToString() + "已断开！";
                    textBox2.Text += Environment.NewLine;
                }));
            };
            tcps.OnComDataReceived += (cl,data) =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    CarTCPClient carclient = cl as CarTCPClient;
                    textBox2.Text += (carclient.Client.Client.RemoteEndPoint as IPEndPoint).ToString() + ":";
                    textBox2.Text += Encoding.Default.GetString(data);
                    textBox2.Text += Environment.NewLine;
                }));
            };

            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tcps.Stop();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tcps.Start();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[] { 0xa5, 0x07, 0x00, 0x88, 0x00, 0x77, 0x07, 0x01, 0x05, 0x41, 0x42, 0x43, 0x44, 0x45 };

            QianComHeader header = new QianComHeader();
            header.ParseByteData(data);
            QianComData dataobj = QianComDataFactory.CreateInstance(header.FuncType);
            dataobj.ParseByteData(data, QianComHeader.HeaderLen);
        }
    }
}

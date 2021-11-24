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

using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;

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
            var list = Util.GetAvalibleNetInterfaceV4();
            textBox1.Text = "";
            foreach(var x in list)
            {
                textBox1.Text += x.InterfaceName + ":" + x.IP.ToString();
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

        private void button9_Click(object sender, EventArgs e)
        {
            var ver = Util.GetLibVersion();
            textBox1.Text = ver[0].ToString() + "." + ver[1].ToString();
  
        }

   
    

        private void button12_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Test 啊啊啊中文 にほんごテスト");
        }
        QianCar thiscar;
        private void button10_Click(object sender, EventArgs e)
        {
            QianCarAPI.Init(0x01, 8881, 8882, ".\\Map.txt");
            QianCarAPI.StartServer();

            QianCarAPI.OnCarRegistered += (car) => { 
                Console.WriteLine($"小车{car.ID}号注册成功。");
                thiscar = car;
            };
            QianCarAPI.OnCarApplyForEnter += (car,point) =>
            {
                Console.WriteLine($"小车{car.ID}号申请入场到{point.Name}");
            };
            QianCarAPI.OnCarEntered += (car, point) => { Console.WriteLine($"小车{car.ID}号已经入场到{point.Name}"); };

            QianCarAPI.OnCarStateReported += (car,ack) => {
                Console.WriteLine($"小车{car.ID}号报告信息：状态={car.State}，当前经过={car.CurrentPoint.Name}，是否应答={ack}");
            };
            QianCarAPI.OnCarApplyForLeave += (car,point) => { Console.WriteLine($"小车{car.ID}号申请离场从{point.Name}"); };
            QianCarAPI.OnCarLeft += (car) => { Console.WriteLine($"小车{car.ID}已经离场！"); };
            QianCarAPI.OnCarErrorReported += (car,code, data) => {

                Console.WriteLine($"小车{car.ID}报告错误码={code},错误描述{data}");
            };
            QianCarAPI.OnCustomData += (car,head ,data) => { 
                Console.WriteLine($"小车{car.ID}报告自定义信息！"); 
            };
        }

        private void button11_Click(object sender, EventArgs e)
        {
            QianCarAPI.StopServer();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            QianCarAPI.CallCarEnter(thiscar, true, QianCarAPI.Map["B2"]);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            QianCarAPI.QueryCarState(thiscar);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            QianCarAPI.SendRoutes(thiscar, false, new byte[] { 1, 2, 3, 4 });
        }

        private void button16_Click(object sender, EventArgs e)
        {
            QianCarAPI.CallCarLeave(thiscar, true);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            QianCarAPI.PauseCar(thiscar, 1);
            //QianCarAPI.ResumeCar(thiscar, 4);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            QianCarAPI.SetMotroSpeed(thiscar, 50);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            QianCarAPI.SendCustomData(thiscar, new byte[] { 5, 6, 7, 8 });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MiniCarLib.Core;

namespace MiniCarLib
{
    public delegate void OnCarDataRecDlg(IComClient client, QianComHeader header, QianComData data);
    public delegate void OnCarConnectedDlg(IComClient client);
    public delegate void OnCarDisConnectedDlg(IComClient client);

    public class QianCarServer
    {
        UDPDriver udpserver = new UDPDriver();
        TCPDriver tcpserver = new TCPDriver();
        public ushort ServerID { get; }
        public UDPDriver RegServer => udpserver;
        public TCPDriver DataServer => tcpserver;

        public event OnCarDataRecDlg OnCarDataReceived;
        public event OnComDataSentDlg OnCarDataSent;
        public event OnCarConnectedDlg OnCarConnected;

        public QianCarServer(ushort serverid)
        {
            ServerID = serverid;
            udpserver.OnComDataReceived += OnRegDataReceived;
            tcpserver.OnComClientConnected += OnClientConnected;
            tcpserver.OnComClientDisconneted += OnClientDisconnected;
            tcpserver.OnComDataReceived += OnDataReceived;
            udpserver.OnComDataSent += OnCarDataSent;
        }

        public void InitRegServer(IPAddress ip, ushort port)
        {
            udpserver.SetParameter(ip, port);
            udpserver.Init();
        }

        public void InitDataServer(IPAddress ip,ushort port)
        {
            tcpserver.SetParameter(ip, port);
            tcpserver.Init();
        }

        public void RegServerStart() => udpserver.Start();
        public void RegServerStop() => udpserver.Stop();
        public void DataServerStart() => tcpserver.Start();
        public void DataServerStop() => tcpserver.Stop();

        public void SendPack(IComClient client,ushort carid,QianComData data)
        {
            QianComHeader header = new QianComHeader();
            header.LocalAddress = ServerID;
            header.RemoteAddress = carid;
            header.FuncType = data.FuncType;
            header.DataLength = data.DataLen;
            SendPack(client, header, data);
        }

        public void SendPack(IComClient client, QianComHeader header,QianComData data)
        {
            byte[] buffer = new byte[header.DataLength + QianComHeader.HeaderLen];
            header.FillByteData(buffer);
            data.FillByteData(buffer, QianComHeader.HeaderLen);
            
            if(client.IsAlive)
            {
                if (header.FuncType == DataFunctionType.RegisterResponse)
                    udpserver.SendData(client, buffer);
                else
                    tcpserver.SendData(client, buffer);
            }
        }

        protected virtual void OnDataReceived(IComClient client, byte[] data)
        {
            if (data[0] != QianComHeader.DataHead)
                return;
            QianComHeader header = new QianComHeader();
            header.ParseByteData(data);
            if (header.RemoteAddress != ServerID)
                return;

            QianComData dataobj = QianComDataFactory.CreateInstance(header.FuncType);
            dataobj.ParseByteData(data, QianComHeader.HeaderLen);

            OnCarDataReceived?.Invoke(client, header, dataobj);
        }

        protected virtual void OnClientDisconnected(IComClient client)
        {
            client.IsAlive = false;
        }

        protected virtual void OnClientConnected(IComClient client)
        {
            client.IsAlive = true;
            OnCarConnected?.Invoke(client);
        }

        protected virtual void OnRegDataReceived(IComClient client, byte[] data)
        {
            if (data[0] != QianComHeader.DataHead)
                return;
            QianComHeader header = new QianComHeader();
            header.ParseByteData(data);
            if (header.RemoteAddress != ServerID)
                return;
            if (header.FuncType !=  DataFunctionType.RegisterRequest)
                return;
            QianComData dataobj = QianComDataFactory.CreateInstance(DataFunctionType.RegisterRequest);
            dataobj.ParseByteData(data, QianComHeader.HeaderLen);

            OnCarDataReceived?.Invoke(client, header, dataobj);
        }

        public void DisconnectClient(IComClient client)
        {
            client.Disconnect();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MiniCarLib.Core
{
    public interface IComClient
    {
        bool IsAlive { get; set; }
    }

    public class CarUDPClient : IComClient
    {
        public CarUDPClient(IPEndPoint cl)
        {
            Client = cl;
        }

        public bool IsAlive { get { return true; } set { } }

        public IPEndPoint Client { get; }
    }

    public class CarTCPClient : IComClient
    {
        public CarTCPClient(TcpClient cl)
        {
            Client = cl;
        }
        public TcpClient Client { get; }
        public bool IsAlive { get ; set ; }
        //public bool IsAlive => Client.Connected;
    }
}

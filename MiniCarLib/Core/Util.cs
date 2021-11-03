using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;



namespace MiniCarLib.Core
{
    public static class Util
    {
        public static NetWorkInterfaceInfo[] GetAvalibleNetInterfaceV4()
        {
            List<NetWorkInterfaceInfo> rst = new List<NetWorkInterfaceInfo>();
            rst.Add(new NetWorkInterfaceInfo()
            {
                InterfaceID = "Any",
                InterfaceName = "Any",
                IP = IPAddress.Any,
                Mask = IPAddress.Any
            }) ;
            var all = NetworkInterface.GetAllNetworkInterfaces();
            foreach(var it in all)
            {
                if (it.OperationalStatus == OperationalStatus.Up)
                {
                    var addrs = it.GetIPProperties().UnicastAddresses;
                    //IPAddress a = IPAddress.Any;
                    foreach(var addr in addrs)
                    {
                        if(addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            rst.Add(new NetWorkInterfaceInfo()
                            {
                                InterfaceID = it.Id,
                                InterfaceName = it.Name,
                                IP = addr.Address,
                                Mask = addr.IPv4Mask
                            }) ;
                        }
                    }
                    
                }    
            }
            return rst.ToArray();
        }

        public static string ErrorLogGenerator(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("[");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff"));
            sb.Append("]异常类型:");
            sb.Append(ex.GetType());
            sb.Append("\t异常信息：");
            sb.AppendLine(ex.Message);
            sb.AppendLine("详细信息：");
            sb.AppendLine(ex.ToString());
            return sb.ToString();
        }

        public static bool CompareByteArray(byte[] a,byte[] b)
        {
            for (int i = 0; i < a.Length; i++)
                if (!a[i].Equals(b[i]))
                    return false;
            return true;
        }

        public static ushort BytesToHalfword(byte hi,byte lo)
        {
            return (ushort)(hi << 8 | lo);
        }

        public static void HalfWordToByte(ushort hword,ref byte hi,ref byte lo)
        {
            hi = (byte)(hword >> 8);
            lo = (byte)(hword & 0xFF);
        }

        public static IPAddress GetLocalIpInSameSubnet(IPAddress ip)
        {
            var all = GetAvalibleNetInterfaceV4();
            foreach(var a in all)
            {
                if (ip.IsInSameSubnet(a.IP, a.Mask))
                    return a.IP;
            }
            return IPAddress.Any;
        }

        public static int[] GetLibVersion()
        {
            int[] rt = new int[2];
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            rt[0] = ver.Major;
            rt[1] = ver.Minor;
            return rt;
        }

        public static string SourceCodeURL => "https://www.google.com";
        public static string AuthorBlogURL => "https://blog.jloli.cc";
    }

    public static class IPAddressExtensions
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            if (subnetMask.GetAddressBytes()[0] != 255)
                return false;
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }

  
    }

    public class NetWorkInterfaceInfo
    {
        public string InterfaceID { get; set; }
        public string InterfaceName { get; set; }
        public IPAddress IP { get; set; }
        public IPAddress Mask { get; set; }
    }
}

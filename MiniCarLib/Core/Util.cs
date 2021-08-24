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
        public static Tuple<string,IPAddress>[] GetAllNetInterfaceV4()
        {
            List<Tuple<string, IPAddress>> rst = new List<Tuple<string, IPAddress>>();
            rst.Add(new Tuple<string,IPAddress>("Any", IPAddress.Any));
            var all = NetworkInterface.GetAllNetworkInterfaces();
            foreach(var it in all)
            {
                if (it.OperationalStatus == OperationalStatus.Up)
                {
                    var addrs = it.GetIPProperties().UnicastAddresses;
                    IPAddress a = IPAddress.Any;
                    foreach(var addr in addrs)
                    {
                        if(addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            a = addr.Address;
                        }
                    }
                    rst.Add(new Tuple<string, IPAddress>(it.Name,a));
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

        public static string SourceCodeURL => "https://www.google.com";
        public static string AuthorBlogURL => "https://blog.jloli.cc";
    }

  
}

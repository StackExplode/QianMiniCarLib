using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using MiniCarLib.Core;

namespace MiniCarLib
{
    public class QianCarMap : ICarMap<QianMapPoint,QianMapRoute>
    {
        Dictionary<string, QianMapPoint> _points = new Dictionary<string, QianMapPoint>();
        public Dictionary<string, QianMapPoint> Points => _points;

        public QianMapPoint this[string name]
        {
            get
            {
                QianMapPoint rst;
                    bool ok = _points.TryGetValue(name, out rst);
                    if (ok)
                        return rst;
                    else
                        return null;
            }
        }

        public void ParseMapFile(string fname)
        {
            if (!File.Exists(fname))
                return;
            using (FileStream fs = new FileStream(fname, FileMode.Open))
            {
                using(StreamReader sr = new StreamReader(fs))
                {
                    string s = null;
                    while((s = sr.ReadLine()) != null)
                    {
                        int i = s.IndexOf(':');
                        string name = s.Substring(0, i);
                        string data = s.Substring(i + 1, s.Length - i - 1);
                        var pt = new QianMapPoint(name);
                        _points.Add(name, pt);
                        var mts = Regex.Matches(data, "{(.*?)}");
                        foreach(Match mt in mts)
                        {
                            string[] args = mt.Groups[1].Value.Split(',');
                            int w = args.Length < 3 ? 1 : Convert.ToInt32(args[2]);
                            int dir = Convert.ToInt32(args[0]);
                            pt.AddRoute(dir, args[1], w, true);
                        }
                    }
                }
            }
        }
        
    }
}

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
    public delegate void OnMapFileSyntaxErrorDlg(int line, Exception ex);
    public class QianCarMap : ICarMap<QianMapPoint,QianMapRoute>
    {
        Dictionary<string, QianMapPoint> _points = new Dictionary<string, QianMapPoint>();
        public Dictionary<string, QianMapPoint> Points => _points;
        public event OnMapFileSyntaxErrorDlg OnMapFileSyntaxError;

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
            int line = 0;
            if (!File.Exists(fname))
                return;
            using (FileStream fs = new FileStream(fname, FileMode.Open))
            {
                using(StreamReader sr = new StreamReader(fs))
                {
                    try
                    {
                        string s = null;
                        while ((s = sr.ReadLine()) != null)
                        {
                            line++;
                            int i_fen = s.IndexOf(";");
                            s = s.Substring(0, i_fen).TrimStart(' ');
                            int i = s.IndexOf(':');
                            if (i == -1)
                            {
                                int i_k = s.IndexOf("[");
                                if (i_k == -1)
                                    continue;
                                else
                                    Parse_Shift(s);
                            }
                            string name = s.Substring(0, i);
                            string data = s.Substring(i + 1, s.Length - i - 1);
                            var pt = new QianMapPoint(name);
                            _points.Add(name, pt);
                            var mts = Regex.Matches(data, "{(.*?)}");
                            foreach (Match mt in mts)
                            {
                                string[] args = mt.Groups[1].Value.Split(',');
                                int w = args.Length < 3 ? 1 : Convert.ToInt32(args[2]);
                                int dir = Convert.ToInt32(args[0]);
                                pt.AddRoute(dir, args[1], w, true);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        OnMapFileSyntaxError?.Invoke(line, ex);
                    }                    
                }
            }
        }

        protected void Parse_Shift(string s)
        {
            throw new NotImplementedException();
        }
        
    }
}

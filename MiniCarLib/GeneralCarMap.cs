using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }
        
    }
}

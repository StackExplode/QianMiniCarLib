using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiniCarLib.Core;

namespace MiniCarLib
{
    public class QianMapPoint : IMapPoint<QianMapRoute>
    {
        public QianMapPoint(string name)
        {
            Name = name;
        }
        public string Name { get; set; }

        public Dictionary<int, QianMapRoute> Routes => _routes;

        public QianMapRoute this[int index] {
            get
            {
                QianMapRoute rst;
                bool ok = _routes.TryGetValue(index, out rst);
                if (ok)
                    return rst;
                else
                    return null;

            }
        }

        public QianMapRoute this[QianRouteDirection dir]
        {
            get 
            {
                return this[(int)dir];
            }
        }

        protected Dictionary<int, QianMapRoute> _routes = new Dictionary<int, QianMapRoute>();

        public void AddRoute(int dir, string dst, int w,bool rewrite = false)
        {
            if (_routes.ContainsKey(dir))
            {
                if (rewrite)
                    _routes.Remove(dir);
                else
                    return;

            }
            _routes.Add(dir, new QianMapRoute(Name, dst, w));
        }
    }

    public enum QianRouteDirection : int
    {
        Stop = 0,
        Up = 1,
        Right = 2,
        Down = 3,
        Left = 4
    }
    public class QianMapRoute : IMapRoute
    {
        public int Weight { get; set; }

        public string Starter { get; }

        public string Ender { get;  }
        public QianMapRoute(string start,string end,int w = 1)
        {
            Weight = w;
            Starter = start;
            Ender = end;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public interface IMapPoint<T> where T : IMapRoute
    {
        string Name { get; set; }
        
        Dictionary<int, T> Routes { get; }

        T this[int index] { get; }
        void AddRoute(int dir, string dst, int w);
    }


    public interface IMapRoute
    {
        int Weight { get; set; }
        string Starter { get; }
        string Ender { get; }
        
    }
}

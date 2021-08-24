using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public interface ICarMap<T,K> where T : IMapPoint<K> where K : IMapRoute
    {
        Dictionary<string, T> Points { get; }
        T this[string name] { get; }
    }
}

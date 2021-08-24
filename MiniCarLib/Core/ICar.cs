using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public interface ICar
    {
        CarType carType { get; }
        int ID { get; set; }
        IComClient ComClient { get; set; }
    }
}

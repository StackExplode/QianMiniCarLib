using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiniCarLib.Core;

namespace MiniCarLib
{
    public class QianCar : ICar
    {
        public CarType carType => CarType.QianCar;
        public string CarName { get; set; }
        public CarState State { get; set; }
        public int Speed { get; set; }
        public IComClient ComClient { get; set; }
        public CarErrorState ErrorState { get; set; }
        public object ErrorInfo { get; set; }

        public int ID { get; set; }

        public QianMapPoint CurrentPoint { get; set; }
        public byte Direction { get; set; }

        public ushort RouteRemain { get; set; }
    }
}

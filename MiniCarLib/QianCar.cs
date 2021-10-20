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
        public byte CarVersion { get; set; }
        public string CarName { get; set; }
        public CarState State { get; set; }
        public int CarSpeed { get; set; }
        public IComClient ComClient { get; set; }
        public CarErrorState ErrorState { get; set; }
        public object ErrorInfo { get; set; }

        public int ID { get; set; }

        public virtual QianMapPoint CurrentPoint { get; set; }
        public virtual byte Direction { get; set; }

        public ushort RouteRemain { get; set; }
        public byte Battery { get; set; }
        public byte MoterSpeed { get; set; } = 100;
    }
}

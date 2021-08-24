using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public enum CarType
    {
        Unkown = 0,
        QianCar = 1
    }
    public static class CarFactory
    {
        public static ICar CreateInstance(CarType type)
        {
            switch(type)
            {
                case CarType.QianCar:return new QianCar();
                default:throw new Exception("No such type of car!");
            }
        }
    }
}

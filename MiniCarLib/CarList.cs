using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using MiniCarLib.Core;

namespace MiniCarLib
{
    public class CarList
    {
        Dictionary<int,ICar> Cars { get; } = new Dictionary<int, ICar>();
        object locker = new object();

        public bool AddCar(ICar car)
        {
            lock(locker)
            {
                if (Cars.ContainsKey(car.ID))
                    return false;
                Cars.Add(car.ID,car);
                return true;
            }
        }

        public void AddorAlterCar(ICar car)
        {
            lock (locker)
            {
                if (Cars.ContainsKey(car.ID))
                    Cars[car.ID] = car;
                else
                    Cars.Add(car.ID, car);
            }
        }

        public bool RemoveCar(ICar car)
        {
            lock (locker)
            {
                if (!Cars.ContainsKey(car.ID))
                    return false;
                Cars.Remove(car.ID);
                return true;
            }
        }


        public int GetAvailibleID(int max = int.MaxValue, int serverid = 0x0000)
        {
            for (int i = 1; i < max; i++)
                if (!Cars.ContainsKey(i) && i != serverid)
                    return i;
            return 0;
        }

        public ICar this[int id]
        {
            get
            {
                lock(locker)
                {
                    ICar rst;
                    bool ok = Cars.TryGetValue(id, out rst);
                    if (ok)
                        return rst;
                    else
                        return null;
                }
            }
        }
    }
}

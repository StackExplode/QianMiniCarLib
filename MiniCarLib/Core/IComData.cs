using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public interface IComData
    {

        void ParseByteData(byte[] data);
        byte[] ToByteData();
    }

    public abstract class QianComData : IComData
    {
        public abstract byte DataLen { get; }
        public abstract DataFunctionType FuncType { get; }

        public virtual void ParseByteData(byte[] data)
        {
            ParseByteData(data, 0);
        }

        public abstract void ParseByteData(byte[] data, int offset);
        public abstract void FillByteData(byte[] data, int offset);

        public virtual byte[] ToByteData()
        {
            byte[] arr = new byte[DataLen];
            FillByteData(arr, 0);
            return arr;
        }
    }
 
    
}

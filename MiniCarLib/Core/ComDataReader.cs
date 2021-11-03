using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public class ComDataReader
    {
        byte[] buffer;
        int offset;
        int ptr = 0;
        public ComDataReader(byte[] arr,int offset)
        {
            buffer = arr;
            this.offset = offset;
            ptr = offset;
        }

        public int Pointer => ptr;

        public void Seek(int index)
        {
            ptr = index;
        }

        public void SeekRelevant(int index)
        {
            ptr += index;
        }

        public void SeekToStart()
        {
            ptr = offset;
        }

        public bool ReadBoolean()
        {
            return buffer[ptr++] == 0 ? false : true;
        }

        public byte ReadByte()
        {
            return buffer[ptr++];
        }

        public ushort ReadHalfWord()
        {
            return Util.BytesToHalfword(buffer[ptr++], buffer[ptr++]);
        }

        public uint ReadWord()
        {
            return (uint)(buffer[ptr++] << 24 | buffer[ptr++] << 16 | buffer[ptr++] << 8 | buffer[ptr++]);
        }

        public void ReadByteArray(byte[] buff,int offset,int len)
        {
            Array.Copy(buffer, ptr, buff, offset, len);
            ptr += len;
        }

        public string ReadString(int len,Encoding en = null)
        {
            if (en is null)
                en = Encoding.ASCII;
            string rt = en.GetString(buffer, ptr, len);
            ptr += len;
            return rt;
        }

        public T ReadEnum<T>() where T : Enum
        {
            return (T)(object)Convert.ToInt32(buffer[ptr++]);
        }
    }
}

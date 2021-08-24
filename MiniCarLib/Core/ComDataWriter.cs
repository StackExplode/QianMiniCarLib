using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public class ComDataWriter
    {
        byte[] buffer;
        int offset;
        int ptr = 0;
        public ComDataWriter(byte[] arr, int offset)
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
        public void WriteBoolean(bool b)
        {
            buffer[ptr++] = (byte)(b ? 1 : 0);
        }

        public void WriteByte(byte b)
        {
            buffer[ptr++] = b;
        }

        public void WriteHalfWord(ushort i)
        {
            buffer[ptr++] = (byte)(i >> 8);
            buffer[ptr++] = (byte)(i & 0xFF);
        }

        public void WriteWord(uint i)
        {
            buffer[ptr++] = (byte)(i >> 24);
            buffer[ptr++] = (byte)((i & 0x00FF0000) >> 16);
            buffer[ptr++] = (byte)((i & 0x0000FF00) >> 8);
            buffer[ptr++] = (byte)(i & 0x000000FF);
        }

        public void WriteByteArray(byte[] buff, int offset, int len)
        {
            Array.Copy(buff, offset, buffer, ptr, len);
            ptr += len;
        }

        public void WriteString(string s, Encoding en = null)
        {
            if (en is null)
                en = Encoding.ASCII;
            byte[] temp = en.GetBytes(s);
            Array.Copy(temp, 0, buffer, ptr, temp.Length);
            ptr += temp.Length;
        }

        public void WriteEnum<T>(T e) where T : Enum
        {
            buffer[ptr++] = (byte)(object)e;
        }
    }
}

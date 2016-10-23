using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Util
    {
        public static int GetShort(byte[] buffer, int index, out short value)
        {
            value = BitConverter.ToInt16(buffer, index);
            return index + 2;
        }

        public static int GetInteger(byte[] buffer, int index, out int value)
        {
            value = BitConverter.ToInt32(buffer, index);
            return index + 4;
        }

        public static int GetString(byte[] buffer, int index, out string text)
        {
            short length;
            GetShort(buffer, index, out length);
            text = Encoding.UTF8.GetString(buffer, index + 2, length);
            return index + text.Length + 2;
        }

        public static byte[] IntToByte(int val)
        {
            byte[] temp = new byte[4];
            temp[3] = (byte)((val & 0xff000000) >> 24);
            temp[2] = (byte)((val & 0x00ff0000) >> 16);
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        public static byte[] ShortToByte(int val)
        {
            byte[] temp = new byte[2];
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        public static int SetShort(byte[] buffer, int index, int value)
        {
            Buffer.BlockCopy(ShortToByte(value), 0, buffer, index, 2);
            return index + 2;
        }

        public static int SetInteger(byte[] buffer, int index, int value)
        {
            Buffer.BlockCopy(IntToByte(value), 0, buffer, index, 4);
            return index + 4;
        }

        public static int SetString(byte[] buffer, int index, string text)
        {
            byte[] temp = Encoding.UTF8.GetBytes(text);
            Buffer.BlockCopy(ShortToByte(temp.Length), 0, buffer, index, 2);
            Buffer.BlockCopy(temp, 0, buffer, index + 2, temp.Length);
            return index + temp.Length + 2;
        }
    }
}

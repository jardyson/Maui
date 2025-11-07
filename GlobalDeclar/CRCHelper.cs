using System.Collections;
using System.Text;

namespace GlobalDeclar
{
    public static class CRCHelper
    {
        #region CRC16

        public static byte[] CRC16(byte[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;
                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8); //高位置
                byte lo = (byte)(crc & 0x00FF); //低位置
                return new byte[] { hi, lo };
            }
            return new byte[] { 0, 0 };
        }
        /// <summary>
        /// 以XMODEM产生验证码，
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static UInt16 Cal_crc16XMODEM(byte[] data, int size)
        {

            UInt32 i = 0;
            UInt16 crc = 0;
            for (i = 0; i < size; i++)
            {
                crc = UpdateCRC16XMODEM(crc, data[i]);
            }
            crc = UpdateCRC16XMODEM(crc, 0);
            crc = UpdateCRC16XMODEM(crc, 0);
            
            return (UInt16)(crc);
        }
        public static UInt16 UpdateCRC16XMODEM(UInt16 crcIn, byte bytee)
        {
            UInt32 crc = crcIn;
            UInt32 ins = (UInt32)bytee | 0x100;

            do
            {
                crc <<= 1;
                ins <<= 1;
                if ((ins & 0x100) == 0x100)
                {
                    ++crc;
                }
                if ((crc & 0x10000) == 0x10000)
                {
                    crc ^= 0x1021;
                }
            }
            while (!((ins & 0x10000) == 0x10000));
            return (UInt16)crc;
        }
        #endregion

        #region ToCRC16

        public static string ToCRC16(string content)
        {
            return ToCRC16(content, Encoding.UTF8);
        }

        public static string ToCRC16(string content, bool isReverse)
        {
            return ToCRC16(content, Encoding.UTF8, isReverse);
        }

        public static string ToCRC16(string content, Encoding encoding)
        {
            return ByteToString(CRC16(encoding.GetBytes(content)), true);
        }

        public static string ToCRC16(string content, Encoding encoding, bool isReverse)
        {
            return ByteToString(CRC16(encoding.GetBytes(content)), isReverse);
        }

        public static string ToCRC16(byte[] data)
        {
            return ByteToString(CRC16(data), true);
        }

        public static string ToCRC16(byte[] data, bool isReverse)
        {
            return ByteToString(CRC16(data), isReverse);
        }

        #endregion

        #region ToModbusCRC16

        public static string ToModbusCRC16(string s)
        {
            return ToModbusCRC16(s, true);
        }

        public static string ToModbusCRC16(string s, bool isReverse)
        {
            return ByteToString(CRC16(StringToHexByte(s)), isReverse);
        }

        public static string ToModbusCRC16(byte[] data)
        {
            return ToModbusCRC16(data, true);
        }

        public static string ToModbusCRC16(byte[] data, bool isReverse)
        {
            return ByteToString(CRC16(data), isReverse);
        }

        #endregion

        #region ByteToString

        public static string ByteToString(byte[] arr, bool isReverse)
        {
            try
            {
                byte hi = arr[0], lo = arr[1];
                return Convert.ToString(isReverse ? hi + lo * 0x100 : hi * 0x100 + lo, 16).ToUpper().PadLeft(4, '0');
            }
            catch (Exception ex) { throw (ex); }
        }

        public static string ByteToString(byte[] arr)
        {
            try
            {
                return ByteToString(arr, true);
            }
            catch (Exception ex) { throw (ex); }
        }

        #endregion

        #region StringToHexString

        public static string StringToHexString(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                s.Append(c.ToString("X4"));
            }
            return s.ToString();
        }

        #endregion

        #region StringToHexByte

        private static string ConvertChinese(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                if (c <= 0 || c >= 127)
                {
                    s.Append(c.ToString("X4"));
                }
                else
                {
                    s.Append((char)c);
                }
            }
            return s.ToString();
        }

        private static string FilterChinese(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                if (c > 0 && c < 127)
                {
                    s.Append((char)c);
                }
            }
            return s.ToString();
        }

        /// <summary>
        /// 字符串转16进制字符数组
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToHexByte(string str)
        {
            return StringToHexByte(str, false);
        }

        /// <summary>
        /// 字符串转16进制字符数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isFilterChinese">是否过滤掉中文字符</param>
        /// <returns></returns>
        public static byte[] StringToHexByte(string str, bool isFilterChinese)
        {
            string hex = isFilterChinese ? FilterChinese(str) : ConvertChinese(str);
            //清除所有空格
            hex = hex.Replace(" ", "");
            //若字符个数为奇数，补一个0
            hex += hex.Length % 2 != 0 ? "0" : "";
            byte[] result = new byte[hex.Length / 2];
            for (int i = 0, c = result.Length; i < c; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        #endregion

        /// <summary>
        /// int 取高低位，高在前，低在后
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static byte[] IntToHighitAndLowByte(int a)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(a >> 8);//高位
            bytes[1] = (byte)(a & 0xff);//低位
            return bytes;
        }
        /// <summary>
        /// bits转Byte,length=0 ,全部转
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static int BitArrayToInt(BitArray bit)
        {
            int[] res = new int[1];
            for (int i = 0; i < bit.Count; i++)
            {
                bit.CopyTo(res, 0);
            }
            return res[0];
        }
        /// <summary>
        /// BitArray(16)ToOneInt
        /// </summary>
        /// <param name="ba">BitArray</param>
        /// <param name="start">开始位置</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static int BitArrayToOneInt(BitArray ba, int start = 0, int length = 0)
        {
            BitArray des = new BitArray(16);
            int s = start > 0 ? start : 0;
            int e = length == 0 ? s + ba.Length : s + length;
            int j = 0;
            for (int i = s; i < e; i++)
            {
                des[j] = ba[i];
                j++;
            }
            int ret = BitArrayToInt(des);
            return ret;
        }

        /// <summary>
        /// Int to BitArray
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static BitArray IntToBitArray(int val)
        {
            byte[] _bytes = BitConverter.GetBytes((ushort)val);
            BitArray bits = new BitArray(_bytes);
            return bits;
        }
        /// <summary>
        /// modbus 取负值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int strToNegativeInt(string str)
        {
            int a = int.Parse(str);
            int b = (short)a;
            return b;
        }


        /// <summary>
        /// List 打乱顺序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> RandomSort<T>(List<T> list)
        {
            var random = new Random();
            var newList = new List<T>();
            foreach (var item in list)
            {
                newList.Insert(random.Next(newList.Count), item);
            }
            return newList;
        }
        /// <summary>
        /// List 打乱顺序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        public static void ListRandom<T>(List<T> sources)
        {
            Random rd = new Random();
            int index = 0;
            T temp;
            for (int i = 0; i < sources.Count; i++)
            {
                index = rd.Next(0, sources.Count - 1);
                if (index != i)
                {
                    temp = sources[i];
                    sources[i] = sources[index];
                    sources[index] = temp;
                }
            }
        }

        /// <summary>
        /// 2个BYTE 返回一个int
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ByteToInt(byte[] bytes)
        {
            if (bytes.Length == 2)
            {
                int result = bytes[1] | (bytes[0] << 8);
                return result;
            }
            else
            {
                return 0;
            }
        }
    }
}
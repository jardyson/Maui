using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.IO;
namespace DeviceHelper.Controller
{
    public class CRC
    {
        /// <summary>
        /// 判断数据中crc是否正确
        /// </summary>
        /// <param name="datas">传入的数据后两位是crc</param>
        /// <returns></returns>
        public  bool IsCrcOK(byte[] datas)
        {
            int length = datas.Length - 2;

            byte[] bytes = new byte[length];
            Array.Copy(datas, 0, bytes, 0, length);
            byte[] getCrc = GetModbusCrc16(bytes);

            if (getCrc[0] == datas[length] && getCrc[1] == datas[length + 1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 传入数据添加两位crc
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public  byte[] GetCRCDatas(byte[] datas)
        {
            int length = datas.Length;
            byte[] crc16 = GetModbusCrc16(datas);
            byte[] crcDatas = new byte[length + 2];
            Array.Copy(datas, crcDatas, length);
            Array.Copy(crc16, 0, crcDatas, length, 2);
            return crcDatas;
        }
        /// <summary>
        /// get crcl,crch
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private  byte[] GetModbusCrc16(byte[] bytes)
        {
            byte crcRegister_H = 0xFF, crcRegister_L = 0xFF;// 预置一个值为 0xFFFF 的 16 位寄存器

            byte polynomialCode_H = 0xA0, polynomialCode_L = 0x01;// 多项式码 0xA001

            for (int i = 0; i < bytes.Length; i++)
            {
                crcRegister_L = (byte)(crcRegister_L ^ bytes[i]);

                for (int j = 0; j < 8; j++)
                {
                    byte tempCRC_H = crcRegister_H;
                    byte tempCRC_L = crcRegister_L;

                    crcRegister_H = (byte)(crcRegister_H >> 1);
                    crcRegister_L = (byte)(crcRegister_L >> 1);
                    // 高位右移前最后 1 位应该是低位右移后的第 1 位：如果高位最后一位为 1 则低位右移后前面补 1
                    if ((tempCRC_H & 0x01) == 0x01)
                    {
                        crcRegister_L = (byte)(crcRegister_L | 0x80);
                    }

                    if ((tempCRC_L & 0x01) == 0x01)
                    {
                        crcRegister_H = (byte)(crcRegister_H ^ polynomialCode_H);
                        crcRegister_L = (byte)(crcRegister_L ^ polynomialCode_L);
                    }
                }
            }

            return new byte[] { crcRegister_L, crcRegister_H };
        }
        /// <summary>
        /// 十进制对应的七段码值
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private string getDecimal(int num)
        {
            string temp = null;
            switch (num)
            {
                case 0:
                    temp = "63";
                    break;
                case 1:
                    temp = "6";
                    break;
                case 2:
                    temp = "155";
                    break;
                case 3:
                    temp = "143";
                    break;
                case 4:
                    temp = "166";
                    break;
                case 5:
                    temp = "173";
                    break;
                case 6:
                    temp = "189";
                    break;
                case 7:
                    temp = "7";
                    break;
                case 8:
                    temp = "191";
                    break;
                default:
                    temp = "175";
                    break;
            }
            return temp;
        }
        
        /// <summary>
        /// 七段码转十进制数字
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public int getCodeToDecimal(string num)
        {
            string[] sno = num.Split(',');
            if (sno.Length == 1)
                return getNum(sno[0]);
            else if (sno.Length == 2)
                return getNum(sno[0]) * 10+ getNum(sno[1]);
            else
                return getNum(sno[0]) * 100 + getNum(sno[1]) * 10 + getNum(sno[2]);
            
        }
        /// <summary>
        /// 十进制数字转七段码
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string getDecimalToCode(int num)
        {
            string sno = getDecimal((num % 1000) / 100) + "," + getDecimal((num % 100) / 10)+","+getDecimal(num % 10);
            return sno;
        }
        /// <summary>
        /// 单个七段码对应十进制数字
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        private int getNum(string fn)
        {
            int no = 0;
            switch(fn)
            {
                case "63":
                    no=0;
                break;
                case "6":
                no = 1;
                break;
                case "155":
                no = 2;
                break;
                case "143":
                no = 3;
                break;
                case "166":
                no = 4;
                break;
                case "173":
                no = 5;
                break;
                case "189":
                no = 6;
                break;
                case "7":
                no = 7;
                break;
                case "191":
                no = 8;
                break;
                case "175":
                no = 9;
                break;
            }
            return no;
        }
        /// <summary>
        /// recieve angle->real angle
        /// </summary>
        /// <param name="bfw"></param>
        /// <returns></returns>
        public double getAngleByFourWords(byte[] bfw){
            double temp=0;
            int split = bfw.Length / 8;           
            string t1 = bfw[0].ToString() + bfw[1].ToString() + bfw[2].ToString() + bfw[3].ToString();
            if (t1.Substring(0, 1) == "0")
            {
                temp = Convert.ToInt32(t1, 16) / 30000.0;
            }
            else
            {
                t1 = "F" + bfw[0].ToString().Substring(1, 3) + bfw[1].ToString().Substring(1, 3) + bfw[2].ToString().Substring(1, 3) + bfw[3].ToString().Substring(1, 3);
            }
            temp = Convert.ToInt32(t1, 16) / 30000.0;
           
            return temp;
        }
        public double getTempByTwoWords(byte[] bfw)
        {
            double temp = 0;
            string t = bfw[0].ToString() + bfw[1].ToString();
            string t0 = Convert.ToString(Convert.ToInt32(t.ToString(), 16), 2);

            if (t0.Length < 16)
            {
            }
            string t1 =  Convert.ToString(Convert.ToInt32(bfw[0].ToString(), 16), 2).Substring(1, 7) + Convert.ToString(Convert.ToInt32(bfw[1].ToString(), 16), 2).Substring(1, 7);
            
            temp = Convert.ToInt32(t1, 16) * 0.0625;
            return temp;
        }
        /// <summary>
        /// 通过NetworkInterface获取MAC地址
        /// </summary>
        /// <returns></returns>
        public string GetMacByNetworkInterface()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                }
            }
            catch (Exception)
            {
            }
            return "00-00-00-00-00-00";
        }
    }
}

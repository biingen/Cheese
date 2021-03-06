using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cheese;
using log4net;

namespace ModuleLayer
{
    public class DataTypeConversion
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);        //log4net

        private ushort CalCRC(ushort PreCRC, byte data)
        {
            log.Debug("CalCRC: " + PreCRC + ", " + data);
            int j;
            ushort to_xor;

            to_xor = (ushort)((PreCRC ^ data) & 0xff);

            for (j = 0; j < 8; j++)
            {
                if ((to_xor & 0x01) == 1)
                {
                    to_xor = (ushort)((to_xor >> 1) ^ 0xA001);
                }

                else
                    to_xor >>= 1;
            }
            PreCRC = (ushort)((PreCRC >> 8) ^ to_xor);

            return PreCRC;
        }

        public ushort CalculateCRC(int dataLen,byte[] inBuf)
        {
            log.Debug("CalculateCRC: " + dataLen + ", " + inBuf);
            int i;
            ushort CRCResult = 0xFFFF;

            for (i = 0; i <= (dataLen - 1); i++)
            {
                CRCResult = CalCRC(CRCResult, inBuf[i]);

            }
            return CRCResult;
        }
        public byte BytetoAscii(byte data)
        {
            log.Debug("BytetoAscii: " + data);
            byte tempByte = 0;
            //data = (byte)(data & 0x0F);
            if ((data >= 0x00) && (data <= 0x09))
            {
                tempByte = (byte)(data | 0x30);
            }
            else
            {
                switch (data)
                {
                    case 10:
                        tempByte = 0x41;
                        break;
                    case 11:
                        tempByte = 0x42;
                        break;
                    case 12:
                        tempByte = 0x43;
                        break;
                    case 13:
                        tempByte = 0x44;
                        break;
                    case 14:
                        tempByte = 0x45;
                        break;
                    case 15:
                        tempByte = 0x46;
                        break;
                    default:
                        tempByte = 0x00;
                        break;
                }

            }
            return tempByte;
        }
        public byte AsciiToByte(byte data)
        {
            log.Debug("AsciiToByte: " + data);
            byte i;
            if ((data >= 0x30) && (data <= 0x39))
            {
                return ((byte)(data & 0x0F));
            }
            else
            {
                switch (data)
                {
                    case 0x41:
                    case 0x61:
                        i = 0x0A;
                        break;
                    case 0x42:
                    case 0x62:
                        i = 0x0B;
                        break;
                    case 0x43:
                    case 0x63:
                        i = 0x0C;
                        break;
                    case 0x44:
                    case 0x64:
                        i = 0x0D;
                        break;
                    case 0x45:
                    case 0x65:
                        i = 0x0E;
                        break;
                    case 0x46:
                    case 0x66:
                        i = 0x0F;
                        break;
                    default:
                        i = 0;
                        break;
                }
                return i;
            }
        }

        public static byte XOR(byte bHEX1, byte bHEX2)
        {
            log.Debug("XOR: " + bHEX1 + ", " + bHEX2);
            byte bHEX_OUT = new byte();
            bHEX_OUT = (byte)(bHEX1 ^ bHEX2);
            return bHEX_OUT;
        }

        public static byte[] OR(byte[] bHEX1, byte[] bHEX2)
        {
            log.Debug("OR: " + bHEX1 + ", " + bHEX2);
            byte[] bHEX_OUT = new byte[bHEX1.Length];
            for (int i = 0; i < bHEX1.Length; i++)
            {
                bHEX_OUT[i] = (byte)(bHEX1[i] | bHEX2[i]);
            }
            return bHEX_OUT;
        }

        public static byte[] AND(byte[] bHEX1, byte[] bHEX2)
        {
            log.Debug("AND: " + bHEX1 + ", " + bHEX2);
            byte[] bHEX_OUT = new byte[bHEX1.Length];
            for (int i = 0; i < bHEX1.Length; i++)
            {
                bHEX_OUT[i] = (byte)(bHEX1[i] & bHEX2[i]);
            }
            return bHEX_OUT;
        }

        public static byte XOR_Bytes(byte[] bHEX1, int length)
        {
            log.Debug("XOR_Bytes: " + bHEX1 + ", " + length);
            byte bHEX_OUT = bHEX1[0];
            for (int i = 1; i < length; i++)
            {
                bHEX_OUT = (byte)(bHEX_OUT ^ bHEX1[i]);
            }
            return bHEX_OUT;
        }

        public static byte XOR_List(List<byte> bHEX1, int length)
        {
            log.Debug("XOR_List: " + bHEX1 + ", " + length);
            byte bHEX_OUT = bHEX1[0];
            for (int i = 1; i < length; i++)
            {
                bHEX_OUT = (byte)(bHEX_OUT ^ bHEX1[i]);
            }
            return bHEX_OUT;
        }

        public string Medical_XOR8(string original_data)
        {
            log.Debug("Medical_XOR8: " + original_data);
            string[] hexValuesSplit = original_data.Split(' ');
            byte[] bytes = new byte[hexValuesSplit.Count()];
            byte XOR_value = new byte();
            int hex_number = 0;
            try
            {
                foreach (string hex in hexValuesSplit)          //turn into Byte array
                {
                    // Convert the number expressed in base-16 to an integer.
                    byte number = Convert.ToByte(Convert.ToInt32(hex, 16));
                    // Get the character corresponding to the integral value.
                    bytes[hex_number++] = number;
                    if (hex_number > 0)
                        XOR_value = XOR(bytes[hex_number - 1], XOR_value);
                }
            }
            catch (OverflowException)
            {
                MessageBox.Show("Please check HEX command format.", "Format error");
            }

            string XOR_Hex = ((int)XOR_value).ToString("X2").PadLeft(2, '0');
            string XOR_calu = " " + XOR_Hex;   //inclusive of XOR string
            return XOR_calu;
        }

        public string BenQ_Mod256(ref string original_data, string[] strArr, byte[] cmdBytes)
        {
            log.Debug("Mod256: " + original_data);
            string chksum = "", tstStr = "";
            int hex_number = 0, sumOfData = 0;
            try
            {
                foreach (string hex in strArr)
                {
                    byte byteValue = 0x00;
                    if (hex != "XX")
                        byteValue = Convert.ToByte(Convert.ToInt32(hex, 16));
                    else if (hex == "XX")
                        byteValue = 0x00;
                    
                    cmdBytes[hex_number++] = byteValue;
                }
                //Length Byte
                int lenValue = cmdBytes[1];
                int dataMaxIndex = lenValue - 4;
                for (int i = 2; i <= (dataMaxIndex + 1); i++)
                {
                    sumOfData += cmdBytes[i];
                }

                sumOfData %= 256;
                cmdBytes[hex_number - 2] = (byte)sumOfData;
                chksum = sumOfData.ToString("X2").PadLeft(2, '0');
                int cnt = 0;
                foreach (var bt in cmdBytes)
                {
                    if (cnt < cmdBytes.Length - 1)
                        tstStr += bt.ToString("X2").PadLeft(2, '0') + " ";
                    else if (cnt == cmdBytes.Length - 1)
                        tstStr += bt.ToString("X2").PadLeft(2, '0');
                    cnt++;
                }
                original_data = tstStr;
            }
            catch (OverflowException)
            {
                MessageBox.Show("Please check HEX command format.", "Format error");
            }
            
            return chksum;
        }

        public string XOR8_BytesWithChksum(string original_data, byte[] BytesToBeWritten, int BytesLength, bool withChksum = false)
        {
            log.Debug("XOR8_BytesWithChksum: " + original_data + ", " + BytesToBeWritten + ", " + BytesLength + ", " + withChksum);
            string[] hexValuesSplit = original_data.Split(' ');
            byte XOR_value = new byte(), byteValue = 0x00;
            int hex_number = 0;
            try
            {
                foreach (string hex in hexValuesSplit)          //turn into Byte array
                {
                    // Convert the number expressed in base-16 to an integer.
                    if (hex != "XX")
                        byteValue = Convert.ToByte(Convert.ToInt32(hex, 16));
                    else if (hex == "XX")
                        byteValue = 0x00;

                    BytesToBeWritten[hex_number++] = byteValue;

                    if (hex_number > 0)
                        XOR_value = XOR(BytesToBeWritten[hex_number - 1], XOR_value);
                }

                BytesToBeWritten[BytesLength-1] = XOR_value;
            }
            catch (OverflowException)
            {
                MessageBox.Show("Please check HEX command format.", "Format error");
            }

            string XOR_Hex = ((int)XOR_value).ToString("X2").PadLeft(2, '0');
            string original_data_chksum = original_data + " " + XOR_Hex;   //inclusive of XOR string
            if (!withChksum)
                return XOR_Hex;
            else
                return original_data_chksum;
        }

        public string XOR8_FtdiDataParsing(ref string original_data, ref string content_data, byte[] cmdLineBytes, byte[] returnBytes, ref byte DeviceAddress, int arraySize)
        {
            //if (original_data == "" && DeviceAddress == 0x50)
            log.Debug("Command Line: " + original_data + " ; DeviceAddress: " + DeviceAddress);
            string content_data_ascii = "";
            try
            {
                string byteString = "", XOR_Hex = "";
                byte XOR_value = DeviceAddress;
                byte[] tmpArray = new byte[arraySize];
                Array.Copy(returnBytes, tmpArray, arraySize);
                tmpArray[arraySize - 1] = 0x00;

                for (int i = 0; i < arraySize; i++)          //turn into String for debug print
                {
                    //Come out updated cmd string with calculated chksum
                    if (i < arraySize - 1)
                    {   // i = 1 to (arraySize - 1)
                        XOR_value = XOR(tmpArray[i], XOR_value);
                        byteString = ((int)returnBytes[i]).ToString("X2").PadLeft(2, '0');
                        original_data += byteString + " ";
                    }
                    else if (i == arraySize - 1)
                    {
                        XOR_value = XOR(tmpArray[i], XOR_value);
                        tmpArray[i] = XOR_value;
                        XOR_Hex = ((int)XOR_value).ToString("X2").PadLeft(2, '0');
                        original_data += XOR_Hex;
                    }

                }

                if (tmpArray[arraySize - 1] == returnBytes[arraySize - 1])
                {
                    if (returnBytes[4] == cmdLineBytes[4] && cmdLineBytes[4] > 0xDF && returnBytes[6] == cmdLineBytes[5])
                    {   //cmdLineBytes[4]: vcpCode && cmdLineBytes[5]: sub-cmd && cmdLineBytes[6]: opCode
                        int num = 0;

                        for (int j = 7; j < arraySize - 1; j++)
                        {
                            if (j < arraySize - 2)
                                content_data += ((int)returnBytes[j]).ToString("X2").PadLeft(2, '0') + " ";
                            else if (j == arraySize - 2)
                                content_data += ((int)returnBytes[j]).ToString("X2").PadLeft(2, '0');

                            tmpArray[num++] = returnBytes[j];
                        }
                        byte[] tmpByte = new byte[num];
                        Array.Copy(tmpArray, tmpByte, num);
                        content_data_ascii = Encoding.ASCII.GetString(tmpByte);
                    }
                    else if ((returnBytes[4] == cmdLineBytes[4] && cmdLineBytes[4] <= 0xDF) ||
                             (returnBytes[4] == cmdLineBytes[4] && cmdLineBytes[4] > 0xDF && cmdLineBytes[2] == 0x82 && returnBytes[6] == cmdLineBytes[5]))
                    {   //such case is for VESA VCP or for i2c without Sub-cmd byte
                        int num = 0;

                        for (int j = 6; j < arraySize - 1; j++)
                        {
                            if (j < arraySize - 2)
                                content_data += ((int)returnBytes[j]).ToString("X2").PadLeft(2, '0') + " ";
                            else if (j == arraySize - 2)
                                content_data += ((int)returnBytes[j]).ToString("X2").PadLeft(2, '0');

                            tmpArray[num++] = returnBytes[j];
                        }
                        byte[] tmpByte = new byte[num];
                        Array.Copy(tmpArray, tmpByte, num);
                        content_data_ascii = Encoding.ASCII.GetString(tmpByte);
                    }
                    else
                    {
                        content_data = "Reply with Error!";
                        content_data_ascii = content_data;
                    }
                }
                else
                {
                    string readChk_Hex = ((int)returnBytes[arraySize - 1]).ToString("X2").PadLeft(2, '0');
                    content_data = $"Replied Chksum - {readChk_Hex} vs Calculated Chksum - {XOR_Hex}";
                    content_data_ascii = content_data;
                }
                
            }
            catch (OverflowException)
            {
                MessageBox.Show("Please check HEX command format.", "Format error");
            }

            return content_data_ascii;
        }

        public byte[] StrToByte(string hexString)
        {
            string[] orginal_array = hexString.Split(' ');
            byte[] orginal_bytes = new byte[orginal_array.Count()];
            int orginal_index = 0;
            try
            {
                foreach (string hex in orginal_array)
                {
                    // Convert the number expressed in base-16 to an integer.
                    byte number = Convert.ToByte(Convert.ToInt32(hex, 16));
                    // Get the character corresponding to the integral value.
                    orginal_bytes[orginal_index++] = number;
                }
            }
            catch (Exception ex)
            {
                if (ex is OverflowException || ex is FormatException)
                {
                    MessageBox.Show("Please check your schedule is in hexadecimal format.", "Format error");
                }
            }
            return orginal_bytes;
        }
    }
 
}
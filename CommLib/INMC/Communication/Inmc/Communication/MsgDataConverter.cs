using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace INMC.Communication.Inmc.Communication
{
    /// <summary>
    /// This class is made by JUJ. 2012.10.4
    /// This class is used in serialization and deserialization of InmcMessage and
    /// very useful in converting data from and to byte array
    /// </summary>
    static class MsgDataConverter
    {
        #region public properties
        ///<summary>
        ///
        ///</summary>
        public static int DateFieldLen = 8;
        #endregion
        


        ///<summary>
        ///
        ///</summary>
        public static byte[] String2ByteArray(string str, ref bool bAscii)
        {
            if (str == null)
                return null;

            char[] charArr = str.ToCharArray();
            int i, byteLen = charArr.Length;
            bool bAsciiString = true;

            //determines wheter input string is ascii or unicode string
            for (i = 0; i < charArr.Length; i++)
                if (charArr[i] > 0xFF)
                {
                    bAsciiString = false;
                    byteLen *= 2;
                    break;
                }

            byte[] bytes = new byte[byteLen];
            
            for (i = 0; i < charArr.Length; i++)
            {
                if (bAsciiString)
                    bytes[i] = (byte)charArr[i];
                else
                {
                    bytes[i * 2] = (byte)((charArr[i] >> 8) & 0xFF);
                    bytes[i * 2 + 1] = (byte)((charArr[i]) & 0xFF);
                }
            }

            if (bAscii) bAscii = bAsciiString;
            return bytes;
        }

        ///<summary>
        ///
        ///</summary>
        public static string ByteArray2String(byte[] bytes, bool bAscii)
        {
            string str;
            int strLen;
            if (bAscii) 
                strLen = bytes.Length;
            else
                strLen = bytes.Length / 2;
            char[] charArray = new char[strLen];
            uint charVal;
            for (int i = 0; i < strLen; i++)
            {
                if (bAscii)
                {
                    charVal = bytes[i];
                }
                else
                {
                    charVal = (uint)bytes[i * 2] << 8;
                    charVal += bytes[i * 2 + 1];
                }
                charArray[i] = Convert.ToChar(charVal);
            }
            str = new string(charArray);
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        public static byte[] ReadLenFieldBytes(Int32 fieldLen)
        {
            byte[] bytes;
            if (fieldLen > 0x7FFF)
            {
                bytes = new byte[4];
                bytes[0] = 0x10;
                bytes[1] = (byte)(uint)((fieldLen >> 16) & 0xFF);
                bytes[2] = (byte)(uint)((fieldLen >> 8) & 0xFF);
                bytes[3] = (byte)(uint)(fieldLen & 0xFF);
            }
            else
            {
                bytes = new byte[2];
                bytes[0] = (byte)(uint)((fieldLen >> 8) & 0xFF);
                bytes[1] = (byte)(uint)(fieldLen & 0xFF);
            }
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void WriteStringField(MemoryStream memStream, string str)
        {
            bool bAscii = true;
            byte[] dataField = String2ByteArray(str, ref bAscii);
            int fieldLen;

            if (dataField == null)
                fieldLen = 0;
            else
                fieldLen = dataField.Length;

            WriteLenField(memStream, fieldLen);              

            if (dataField != null)
            {
                if (bAscii)
                    memStream.WriteByte(0);
                else
                    memStream.WriteByte(1);
                memStream.Write(dataField, 0, dataField.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void WriteLenField(MemoryStream memStream, int len)
        {
            byte[] lenField = ReadLenFieldBytes(len);
            memStream.Write(lenField, 0, lenField.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        public static string ReadStringField(MemoryStream memStream)
        {
            int nFieldLen = ReadLenField(memStream);
            if (nFieldLen == 0)
                return null;
            byte[] byteArray = new byte[nFieldLen];
            bool bAscii = (memStream.ReadByte() == 0);

            memStream.Read(byteArray, 0, nFieldLen);
            return ByteArray2String(byteArray, bAscii);
        }

        /// <summary>
        /// 
        /// </summary>
        public static Int32 ReadLenField(MemoryStream memStream)
        {
            int primaryByte = memStream.ReadByte();
            int secondaryByte, retVal = 0;
            
            //2byte length field
            if ((primaryByte & 0x080) == 0)
            {
                secondaryByte = memStream.ReadByte();
                retVal = (primaryByte << 8) + secondaryByte;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    secondaryByte = memStream.ReadByte();
                    retVal <<= 8;
                    retVal += secondaryByte;
                }
            }
            return retVal;
        }

        public static byte[] Date2ByteArray(DateTime time)
        {
            Int64 nBinaryTime = time.ToBinary();
            byte[] bytes = new byte[8];
            bytes[0] = (byte)nBinaryTime; nBinaryTime /= 0x100;
            bytes[1] = (byte)nBinaryTime; nBinaryTime /= 0x100;
            bytes[2] = (byte)nBinaryTime; nBinaryTime /= 0x100;
            bytes[3] = (byte)nBinaryTime; nBinaryTime /= 0x100;
            bytes[4] = (byte)nBinaryTime; nBinaryTime /= 0x100;
            bytes[5] = (byte)nBinaryTime; nBinaryTime /= 0x100;
            bytes[6] = (byte)nBinaryTime; nBinaryTime /= 0x100;
            bytes[7] = (byte)nBinaryTime;

            return bytes;
        }
        public static DateTime ByteArray2Date(byte[] bytes)
        {
            Int64 nBinaryTime = bytes[7];
            nBinaryTime *= 0x100; nBinaryTime += bytes[6];
            nBinaryTime *= 0x100; nBinaryTime += bytes[5];
            nBinaryTime *= 0x100; nBinaryTime += bytes[4];
            nBinaryTime *= 0x100; nBinaryTime += bytes[3];
            nBinaryTime *= 0x100; nBinaryTime += bytes[2];
            nBinaryTime *= 0x100; nBinaryTime += bytes[1];
            nBinaryTime *= 0x100; nBinaryTime += bytes[0];
            DateTime time = DateTime.FromBinary(nBinaryTime);
            return time;
        }

        public static byte[] Image2ByteArray(Image image)
        {
            return null;
        }
        public static Image ByteArray2Image(byte[] bytes)
        {
            return null;
        }

        public static DateTime ReadDateField(MemoryStream memStream)
        {
            byte[] bytes = new byte[DateFieldLen];
            memStream.Read(bytes, 0, DateFieldLen);
            return ByteArray2Date(bytes);
        }

        public static void WriteDateField(MemoryStream memStream, DateTime date)
        {
            memStream.Write(MsgDataConverter.Date2ByteArray(date), 0, MsgDataConverter.DateFieldLen);
        }

        public static bool ReadBoolDataField(MemoryStream memStream)
        {
            int boolVal = memStream.ReadByte();
            if (boolVal == 0)
                return false;
            else
                return true;
        }

        public static void WriteBoolDataField(MemoryStream memStream, bool bVal)
        {
            if (bVal)
                memStream.WriteByte(1);
            else
                memStream.WriteByte(0);
        }

        /// <summary>
        /// Writes a int value to a byte array from a starting index.
        /// </summary>
        public static void WriteInt32(MemoryStream memStream, int number)
        {
            memStream.WriteByte((byte)((number >> 24) & 0xFF));
            memStream.WriteByte((byte)((number >> 16) & 0xFF));
            memStream.WriteByte((byte)((number >> 8) & 0xFF));
            memStream.WriteByte((byte)((number) & 0xFF));
        }

        /// <summary>
        /// Deserializes and returns a serialized integer.
        /// </summary>
        /// <returns>Deserialized integer</returns>
        public static int ReadInt32(Stream stream)
        {
            var buffer = ReadByteArray(stream, 4);
            return ((buffer[0] << 24) |
                    (buffer[1] << 16) |
                    (buffer[2] << 8) |
                    (buffer[3])
                   );
        }

        /// <summary>
        /// Writes a int value to a byte array from a starting index.
        /// </summary>
        public static void WriteInt64(MemoryStream memStream, long number)
        {
            memStream.WriteByte((byte)((number >> 56) & 0xFF));
            memStream.WriteByte((byte)((number >> 48) & 0xFF));
            memStream.WriteByte((byte)((number >> 40) & 0xFF));
            memStream.WriteByte((byte)((number >> 32) & 0xFF));
            memStream.WriteByte((byte)((number >> 24) & 0xFF));
            memStream.WriteByte((byte)((number >> 16) & 0xFF));
            memStream.WriteByte((byte)((number >> 8) & 0xFF));
            memStream.WriteByte((byte)((number) & 0xFF));
        }

        /// <summary>
        /// Deserializes and returns a serialized integer.
        /// </summary>
        /// <returns>Deserialized integer</returns>
        public static int ReadInt64(Stream stream)
        {
            var buffer = ReadByteArray(stream, 8);
            return ((buffer[0] << 56) |
                    (buffer[0] << 48) |
                    (buffer[0] << 40) |
                    (buffer[0] << 32) |
                    (buffer[0] << 24) |
                    (buffer[1] << 16) |
                    (buffer[2] << 8) |
                    (buffer[3])
                   );
        }

        /// <summary>
        /// 
        /// </summary>
        public static double ReadDoubleField(MemoryStream memStream)
        {
            string doubleString = ReadStringField(memStream);
            return Convert.ToDouble(doubleString);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void WriteDoubleField(MemoryStream memStream, double val)
        {
            string doubleString = Convert.ToString(val);
            WriteStringField(memStream, doubleString);
        }

        /// <summary>
        /// Reads a byte array with specified length.
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Length of the byte array to read</param>
        /// <returns>Read byte array</returns>
        /// <exception cref="EndOfStreamException">Throws EndOfStreamException if can not read from stream.</exception>
        private static byte[] ReadByteArray(Stream stream, int length)
        {
            var buffer = new byte[length];
            var totalRead = 0;
            while (totalRead < length)
            {
                var read = stream.Read(buffer, totalRead, length - totalRead);
                if (read <= 0)
                {
                    throw new EndOfStreamException("Can not read from stream! Input stream is closed.");
                }

                totalRead += read;
            }

            return buffer;
        }

    }
}

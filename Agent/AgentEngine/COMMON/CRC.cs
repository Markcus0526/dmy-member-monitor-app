using System;
using System.Collections.Generic;
using System.Text;

namespace AgentEngine
{
    class CRC       // Check Class For Right Value
    {
        public CRC(ref Byte[] srcBuffer, uint Len)
        {
            CRCTableValues = new uint[0x100];

            Init();
	        MakeCRCTable();

            CRCResult = GetBlockCRC(ref srcBuffer, Len, CRCResult);
        }

        public uint GetCRC()
        {
            return CRCResult ^ MaskValue;
        }

        private void Init()
        {
            MaskValue = PMX_CRC32_MASK;
            Polynomial = PMX_CRC32B_POLYNOMIAL;

            CRCResult = MaskValue;
        }

        // Make CRC Table
        private void MakeCRCTable()
        {
            uint i, j;
            uint dwValue;
            uint dwResult;

            for (i = 0; i <= 0xFF; i++)
            {
                for (dwValue = i, j = 8; j > 0; j--)
                {
                    dwResult = dwValue & 0x01;
                    if (dwResult != 0)
                    {
                        dwValue = (dwValue >>= 1) ^ Polynomial;
                    }
                    else
                    {
                        dwValue = dwValue >>= 1;
                    }
                }

                CRCTableValues[i] = dwValue;
            }
        }

        // Calculate CRC with the input Byte Array
        public uint GetBlockCRC(ref Byte[] BufArr, uint dwBlockSize, uint dwPrevCRC)
        {
            int index = 0;
            for (index = 0; dwBlockSize > 0; dwBlockSize--, index++)
            {
                dwPrevCRC = (dwPrevCRC >> 8) ^ CRCTableValues[(dwPrevCRC ^ BufArr[index]) & 0xFF];
            }

	        return dwPrevCRC;
        }

        private const uint MAX_READ_BUF_SIZE = 0x400;
        private const uint PMX_CRC4_MASK = 0xF;
        private const uint PMX_CRC7_MASK = 0x7F;
        private const uint PMX_CRC8_MASK = 0xFF;
        private const uint PMX_CRC12_MASK = 0xFFF;
        private const uint PMX_CRC16_ANSI_MASK = 0x0000;
        private const uint PMX_CRC16_CCITT_MASK = 0xFFFF;
        private const uint PMX_CRC24_MASK = 0xFFFFFF;
        private const uint PMX_CRC32_MASK = 0xFFFFFFFF;

        private const uint PMX_CRC4_POLYNOMIAL = 0xF;
        private const uint PMX_CRC7_POLYNOMIAL = 0x45;
        private const uint PMX_CRC8_POLYNOMIAL = 0xAB;
        private const uint PMX_CRC12_POLYNOMIAL = 0xF01;
        private const uint PMX_CRC16_ANSI_POLYNOMIAL = 0xA001;
        private const uint PMX_CRC16_CCITT_POLYNOMIAL = 0x8408;
        private const uint PMX_CRC16_SDLC_POLYNOMIAL = 0xE905;
        private const uint PMX_CRC24_POLYNOMIAL = 0x808A01;
        private const uint PMX_CRC32A_POLYNOMIAL = 0x47190202;
        private const uint PMX_CRC32B_POLYNOMIAL = 0xEDB88320;

        uint MaskValue, Polynomial;
        uint CRCResult;
        uint[] CRCTableValues;
    }
}

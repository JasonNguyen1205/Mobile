using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile.Function
{
    public class Checksum
    {
        const byte CRC8_INIT = 0x4F;
        const byte CRC8_POLY = 0x8C;
        public static byte Crc8(byte[] data, int length)
        {
            byte crc = CRC8_INIT;
            int i;
            int j;
            for (i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (j = 0; j < 8; j++)
                {
                    if ((crc & 0x01) != 0)
                    {
                        crc = (byte)((crc >> 1) ^ CRC8_POLY);
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }
    }
}

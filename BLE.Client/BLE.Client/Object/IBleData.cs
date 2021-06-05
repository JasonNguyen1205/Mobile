using System;
using System.Collections.Generic;
using System.Text;

namespace BLE.Client.Object
{
    public interface IBleData
    {
        void Parse(byte[] data);
        byte[] GetBytes();
    }
}

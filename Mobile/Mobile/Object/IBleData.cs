using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile.Object
{
    public interface IBleData
    {
        void Parse(byte[] data);
        byte[] GetBytes();
    }
}

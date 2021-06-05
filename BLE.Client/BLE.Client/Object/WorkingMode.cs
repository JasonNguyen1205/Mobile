using System;
using System.Collections.Generic;
using System.Text;

namespace BLE.Client.Object
{
    public enum WorkingMode
    {
        STANDALONE,
        CORPORATE
    }

    public enum StateMachine
    {
        IDLE,
        OVEN,
        SAFETY,
        FAILURE
    }
}

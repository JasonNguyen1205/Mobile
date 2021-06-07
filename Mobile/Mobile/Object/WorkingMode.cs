using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile.Object
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

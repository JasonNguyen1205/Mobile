using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile.Object
{
    public enum COM_Command : byte
    {
        COM_CMD_NONE,
        COM_CMD_STATUS,
        COM_CMD_STOVE_LOCK,
        COM_CMD_INFO,
        COM_CMD_DEVICE_RESET,
        COM_CMD_SYNCWORD,
        COM_CMD_SMOKE
    }

    public class COM_CommandHelp
    {
        public static string GetString(COM_Command cmd)
        {
            string str = "";
            switch (cmd)
            {
                case COM_Command.COM_CMD_NONE:
                    str = "COM_CMD_NONE";
                    break;
                case COM_Command.COM_CMD_STATUS:
                    str = "COM_CMD_STATUS";
                    break;
                case COM_Command.COM_CMD_STOVE_LOCK:
                    str = "COM_CMD_STOVE_LOCK";
                    break;
                case COM_Command.COM_CMD_INFO:
                    str = "COM_CMD_INFO";
                    break;
                case COM_Command.COM_CMD_DEVICE_RESET:
                    str = "COM_CMD_DEVICE_RESET";
                    break;
                case COM_Command.COM_CMD_SYNCWORD:
                    str = "COM_CMD_SYNCWORD";
                    break;
                case COM_Command.COM_CMD_SMOKE:
                    str = "COM_CMD_SMOKE";
                    break;
                default:
                    break;
            }

            return str;
        }
    }
}

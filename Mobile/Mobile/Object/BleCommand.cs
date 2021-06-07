using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile.Object
{
    public enum BleCommand : byte
    {
        BLE_CMD_NONE = 0x00,

        BLE_CMD_COM_SYNC = 10,
        BLE_CMD_TEMP_SCAN,
        BLE_CMD_MOVE_SCAN,
        BLE_CMD_MODE_STATE,
        BLE_CMD_STOVE_TYPE_SET,

        BLE_CMD_APP_STATE_SET = 30,
        BLE_CMD_MAIN_INFO,
        BLE_CMD_SENSOR_INFO,
        BLE_CMD_STOVELOCK_SET,
        BLE_CMD_STOVELOCK_GET,
        BLE_CMD_OVEN_PROFILE_SET,

        BLE_CMD_TEMP_00_07,
        BLE_CMD_TEMP_08_15,
        BLE_CMD_TEMP_16_23,
        BLE_CMD_TEMP_24_31,
        BLE_CMD_TEMP_32_39,
        BLE_CMD_TEMP_40_47,
        BLE_CMD_TEMP_48_55,
        BLE_CMD_TEMP_56_63,
        BLE_CMD_MOVEMENT_STATE,

        BLE_CMD_INVALID = 100,
        BLE_CMD_FAILURE,            /** Command failure */
        BLE_CMD_SUCCESS,            /** Command success */

        BLE_CMD_LOG_SET = 200,
        BLE_CMD_LOG_GET,
        BLE_CMD_LOG_WORKING_STATE,
        BLE_CMD_LOG_BUTTON_CONFIRM,
        BLE_CMD_LOG_COM_SYNC,
        BLE_CMD_LOG_COM_RECV,
        BLE_CMD_LOG_SNAPSHOT,
        BLE_CMD_LOG_DISABLE_MOVE,

        BLE_CMD_APP_CFG_RESET,
        BLE_CMD_LOG_TEMP_AREA,
    }

    public class BleCommandHelp
    {
        public static string GetString(BleCommand cmd)
        {
            string str = "";
            switch (cmd)
            {
                case BleCommand.BLE_CMD_NONE:
                    str = "BLE_CMD_NONE";
                    break;
                case BleCommand.BLE_CMD_COM_SYNC:
                    str = "BLE_CMD_COM_SYNC";
                    break;
                case BleCommand.BLE_CMD_TEMP_SCAN:
                    str = "BLE_CMD_TEMP_SCAN";
                    break;
                case BleCommand.BLE_CMD_MOVE_SCAN:
                    str = "BLE_CMD_MOVEMENT_SCAN";
                    break;
                case BleCommand.BLE_CMD_MODE_STATE:
                    str = "BLE_CMD_WORKING_MODE";
                    break;
                case BleCommand.BLE_CMD_STOVE_TYPE_SET:
                    str = "BLE_CMD_STOVE_TYPE_SET";
                    break;
                case BleCommand.BLE_CMD_APP_STATE_SET:
                    str = "BLE_CMD_APP_STATE_SET";
                    break;
                case BleCommand.BLE_CMD_MAIN_INFO:
                    str = "BLE_CMD_MAIN_INFO";
                    break;
                case BleCommand.BLE_CMD_SENSOR_INFO:
                    str = "BLE_CMD_SENSOR_INFO";
                    break;
                case BleCommand.BLE_CMD_STOVELOCK_SET:
                    str = "BLE_CMD_STOVELOCK_SET";
                    break;
                case BleCommand.BLE_CMD_STOVELOCK_GET:
                    str = "BLE_CMD_STOVELOCK_GET";
                    break;
                case BleCommand.BLE_CMD_OVEN_PROFILE_SET:
                    str = "BLE_CMD_OVEN_PROFILE_SET";
                    break;
                case BleCommand.BLE_CMD_TEMP_00_07:
                    str = "BLE_CMD_TEMP_00_07";
                    break;
                case BleCommand.BLE_CMD_TEMP_08_15:
                    str = "BLE_CMD_TEMP_08_15";
                    break;
                case BleCommand.BLE_CMD_TEMP_16_23:
                    str = "BLE_CMD_TEMP_16_23";
                    break;
                case BleCommand.BLE_CMD_TEMP_24_31:
                    str = "BLE_CMD_TEMP_24_31";
                    break;
                case BleCommand.BLE_CMD_TEMP_32_39:
                    str = "BLE_CMD_TEMP_32_39";
                    break;
                case BleCommand.BLE_CMD_TEMP_40_47:
                    str = "BLE_CMD_TEMP_40_47";
                    break;
                case BleCommand.BLE_CMD_TEMP_48_55:
                    str = "BLE_CMD_TEMP_48_55";
                    break;
                case BleCommand.BLE_CMD_TEMP_56_63:
                    str = "BLE_CMD_TEMP_56_63";
                    break;
                case BleCommand.BLE_CMD_MOVEMENT_STATE:
                    str = "BLE_CMD_MOVEMENT";
                    break;
                case BleCommand.BLE_CMD_FAILURE:
                    str = "BLE_CMD_FAILURE";
                    break;
                case BleCommand.BLE_CMD_SUCCESS:
                    str = "BLE_CMD_SUCCESS";
                    break;
                case BleCommand.BLE_CMD_LOG_SET:
                    str = "BLE_CMD_LOG_SET";
                    break;
                case BleCommand.BLE_CMD_LOG_GET:
                    str = "BLE_CMD_LOG_GET";
                    break;
                case BleCommand.BLE_CMD_LOG_WORKING_STATE:
                    str = "BLE_CMD_LOG_WORKING_STATE";
                    break;
                case BleCommand.BLE_CMD_LOG_BUTTON_CONFIRM:
                    str = "BLE_CMD_LOG_BUTTON_CONFIRM";
                    break;
                case BleCommand.BLE_CMD_LOG_COM_SYNC:
                    str = "BLE_CMD_LOG_COM_SYNC";
                    break;
                case BleCommand.BLE_CMD_LOG_COM_RECV:
                    str = "BLE_CMD_LOG_COM_RECV";
                    break;
                case BleCommand.BLE_CMD_LOG_SNAPSHOT:
                    str = "BLE_CMD_LOG_SNAPSHOT";
                    break;
                case BleCommand.BLE_CMD_APP_CFG_RESET:
                    str = "BLE_CMD_APP_CFG_RESET";
                    break;
                case BleCommand.BLE_CMD_LOG_TEMP_AREA:
                    str = "BLE_CMD_LOG_TEMP_AREA";
                    break;
                case BleCommand.BLE_CMD_INVALID:
                    str = "BLE_CMD_INVALID";
                    break;
                case BleCommand.BLE_CMD_LOG_DISABLE_MOVE:
                    str = "BLE_CMD_LOG_DISABLE_MOVE";
                    break;
                default:
                    break;
            }

            return str;
        }
    }
}

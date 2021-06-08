using Mobile.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Mobile.Function
{
    public class Ble
    {
        public const int DATA_BUFFER_SIZE = 16;
        public const int DATA_PACKET_SIZE = 18;

        //public BleSyncWord SyncWord;
        //public BleTempScan TempScan;
        //public BleTempData TempData;
        //public BleMoveScan MoveScan;
        //public BleMoveData MoveData;
        //public BleSetup Setup;
        //public BleSensorInfo SensorInfo;
        //public BleStoveLock StoveLock;
        //public BleSafetyProfile SafetyProfile;
        //public BleSuccss Success;
        //public BleFailure Failure;
        //public BleWorkingMode WorkingMode;
        //public BleLog Log;
        //public BleComSync ComSync;
        //public BleButtonConfirm ButtonConfirm;
        //public BleComData ComData;

        public delegate void SendEventHandle(byte[] datas);
        public delegate void RecvEventHandle(BleCommand cmd, byte[] datas);

        public event SendEventHandle SendCallback;
        public event RecvEventHandle RecvCallbcak;

        public void Recv(byte[] data)
        {
            if (data == null || data.Length != DATA_PACKET_SIZE)
            {
                Debug.WriteLine("[Ble] Data packet size not fit");
                return;
            }

            if (Checksum.Crc8(data, DATA_PACKET_SIZE - 1) != data[DATA_PACKET_SIZE - 1])
            {
                Debug.WriteLine("[Ble] Checksum failure");
                return;
            }

            BleCommand cmd = (BleCommand)data[0];
            byte[] datas = new byte[DATA_BUFFER_SIZE];
            for (int i = 0; i < DATA_BUFFER_SIZE; i++)
            {
                datas[i] = data[i + 1];
            }

            RecvCallbcak?.Invoke(cmd, datas);
        }

        public byte[] Send(BleCommand cmd, byte[] data)
        {
            if (data == null || data.Length != DATA_BUFFER_SIZE)
            {
                throw new Exception();
            }

            byte[] buf = new byte[DATA_PACKET_SIZE];
            buf[0] = (byte)cmd;
            for (int i = 0; i < DATA_BUFFER_SIZE; i++)
            {
                buf[i + 1] = data[i];
            }

            Debug.WriteLine("[Ble] Send CMD: {0}", (int)cmd);
            buf[DATA_BUFFER_SIZE - 1] = Checksum.Crc8(buf, DATA_BUFFER_SIZE - 1);
            return buf;
        }
    }

    public class BleSyncWord : IBleData
    {
        public byte[] MainSyncWords;
        public byte[] SmokeSyncWords;
        public SyncType SType;

        public enum SyncType
        {
            MAIN,
            MAIN_SMOKE
        };
        public BleSyncWord()
        {
            MainSyncWords = new byte[4] { 0, 0, 0, 0 };
            SmokeSyncWords = new byte[4] { 0, 0, 0, 0 };
            SType = SyncType.MAIN;
        }

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = MainSyncWords[0];
            b[1] = MainSyncWords[1];
            b[2] = MainSyncWords[2];
            b[3] = MainSyncWords[3];
            b[4] = SmokeSyncWords[0];
            b[5] = SmokeSyncWords[1];
            b[6] = SmokeSyncWords[2];
            b[7] = SmokeSyncWords[3];
            b[8] = (byte)SType;
            return b;
        }

        public void Parse(byte[] data)
        {
        }
    }

    public class BleLogDisMove : IBleData
    {
        public bool Disable;
        public BleLogDisMove()
        {
            Disable = false;
        }

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Disable ? 0x01 : 0x00);
            return b;
        }

        public void Parse(byte[] data)
        {
            Disable = data[0] != 0 ? true : false;
        }
    }

    public class BleComHeader : IBleData
    {
        public bool Check;
        public BleComHeader()
        {
        }
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Check ? 0x01 : 0x00);
            return b;
        }

        public void Parse(byte[] data)
        {
            return;
        }
    }

    public class BleTempScan : IBleData
    {
        public bool Enable;
        public BleTempScan()
        {
            Enable = false;
        }

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Enable ? 0x01 : 0x00);
            return b;
        }

        public void Parse(byte[] data)
        {
            Enable = (data[0] != 0) ? true : false;
        }
    }

    public class BleLogArea : IBleData
    {
        public int Index;
        public int MaxIndex;
        public int StartRow;
        public int StartCol;
        public int EndRow;
        public int EndCol;

        public BleLogArea()
        {

        }

        public byte[] GetBytes()
        {
            return null;
        }

        public void Parse(byte[] data)
        {
            Index = data[0];
            MaxIndex = data[1];
            StartRow = data[2];
            StartCol = data[3];
            EndRow = data[4];
            EndCol = data[5];
        }
    }

    public class BleTempData : IBleData
    {
        public double[] Values;

        public BleTempData()
        {
            Values = new double[8];
        }
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            return b;
        }

        public void Parse(byte[] data)
        {
            Values[0] = BitConverter.ToUInt16(data, 0);
            Values[1] = BitConverter.ToUInt16(data, 2);
            Values[2] = BitConverter.ToUInt16(data, 4);
            Values[3] = BitConverter.ToUInt16(data, 6);
            Values[4] = BitConverter.ToUInt16(data, 8);
            Values[5] = BitConverter.ToUInt16(data, 10);
            Values[6] = BitConverter.ToUInt16(data, 12);
            Values[7] = BitConverter.ToUInt16(data, 14);
        }
    }

    public class BleMoveData : IBleData
    {
        public bool Movement;
        public BleMoveData()
        {
            Movement = false;
        }

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Movement ? 1 : 0);
            return b;
        }

        public void Parse(byte[] data)
        {
            Movement = data[0] != 0 ? true : false;
        }
    }

    public class BleMoveScan : IBleData
    {
        public bool Enable;
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Enable ? 1 : 0);
            return b;
        }

        public void Parse(byte[] data)
        {
            Enable = data[0] != 0 ? true : false;
        }
    }

    public class BleSetup : IBleData
    {
        public bool Enable;

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Enable ? 1 : 0);
            return b;
        }

        public void Parse(byte[] data)
        {
            Enable = data[0] != 0 ? true : false;
        }
    }

    public class BleSensorInfo : IBleData
    {
        public int BatteryCapacity;
        public int SafetyProfile;
        public int WorkingMode;
        public float BatteryVoltage;

        public uint FirmwareVersionMajor;
        public uint FirmwareVersionMinor;
        public uint FirmwareVersionPatch;

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            return b;
        }

        public void Parse(byte[] data)
        {
            BatteryCapacity = data[0];
            SafetyProfile = data[1];
            WorkingMode = data[2];
            BatteryVoltage = (float)((BitConverter.ToUInt16(data, 4)) / 1000.0f);
            ushort version = BitConverter.ToUInt16(data, 6);
            FirmwareVersionMajor = (ushort)((version >> 0) & 0x000f);
            FirmwareVersionMinor = (ushort)((version >> 4) & 0x000f);
            FirmwareVersionPatch = (ushort)((version >> 8) & 0x000f);
        }
    }

    public class BleStoveLock : IBleData
    {
        public bool Lock;
        public int Pswd1;
        public int Pswd2;
        public int Pswd3;
        public int Pswd4;

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Lock ? 1 : 0);
            b[1] = (byte)Pswd1;
            b[2] = (byte)Pswd2;
            b[3] = (byte)Pswd3;
            b[4] = (byte)Pswd4;
            return b;
        }

        public void Parse(byte[] data)
        {
            Lock = data[0] != 0 ? true : false;
            Pswd1 = data[1];
            Pswd2 = data[2];
            Pswd3 = data[3];
            Pswd4 = data[4];
        }
    }

    public class BleSafetyProfile : IBleData
    {
        public SafetyProfile Value;
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)Value;
            return b;
        }

        public void Parse(byte[] data)
        {
            Value = (SafetyProfile)data[0];
        }
    }

    public class BleSuccss : IBleData
    {
        public BleCommand Cmd;
        public byte[] GetBytes()
        {
            return null;
        }

        public void Parse(byte[] data)
        {
            Cmd = (BleCommand)data[0];
        }
    }

    public class BleMainInfo : IBleData
    {
        public bool StoveLock;
        public byte StoveLockPswd1;
        public byte StoveLockPswd2;
        public byte StoveLockPswd3;
        public byte StoveLockPswd4;
        public int FwVersionMijor;
        public int FwVersionMinor;
        public int FwVersionPatch;
        public int SmokeLastSync;

        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            return b;
        }

        public void Parse(byte[] data)
        {
            StoveLock = data[0] != 0 ? true : false;
            ushort tmp = BitConverter.ToUInt16(data, 3);
            StoveLockPswd1 = (byte)((tmp >> 0) & 0x000f);
            StoveLockPswd2 = (byte)((tmp >> 4) & 0x000f);
            StoveLockPswd3 = (byte)((tmp >> 8) & 0x000f);
            StoveLockPswd4 = (byte)((tmp >> 12) & 0x000f);

            tmp = BitConverter.ToUInt16(data, 4);
            FwVersionMijor = (byte)((tmp >> 0) & 0x000f);
            FwVersionMinor = (byte)((tmp >> 4) & 0x000f);
            FwVersionPatch = (byte)((tmp >> 8) & 0x000f);

            SmokeLastSync = BitConverter.ToUInt16(data, 4);
        }
    }

    public class BleCOM_Check : IBleData
    {
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            return b;
        }

        public void Parse(byte[] data)
        {

        }
    }

    public class BleFailure : IBleData
    {
        public BleCommand Cmd;
        public byte[] GetBytes()
        {
            return null;
        }

        public void Parse(byte[] data)
        {
            Cmd = (BleCommand)data[0];
        }
    }

    public class BleWorkingMode : IBleData
    {
        public WorkingMode Value;
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)Value;
            return b;
        }

        public void Parse(byte[] data)
        {
            Value = (WorkingMode)data[0];
        }
    }

    public class BleLog : IBleData
    {
        public bool Enable;
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Enable ? 1 : 0);
            return b;
        }

        public void Parse(byte[] data)
        {

        }
    }

    public class BleComSync : IBleData
    {
        public bool Value;
        public byte[] GetBytes()
        {
            byte[] b = new byte[Ble.DATA_BUFFER_SIZE] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            b[0] = (byte)(Value ? 1 : 0);
            return b;
        }

        public void Parse(byte[] data)
        {

        }
    }

    public class BleButtonConfirm : IBleData
    {
        public int Value;

        public byte[] GetBytes()
        {
            return null;
        }

        public void Parse(byte[] data)
        {
            Value = data[0];
        }
    }

    public class BleComData : IBleData
    {
        public COM_Command Cmd;
        public bool Result;
        public Com Data;

        public BleComData()
        {
            Data = new Com();
        }

        public byte[] GetBytes()
        {
            return null;
        }

        public void Parse(byte[] data)
        {
            Cmd = (COM_Command)data[0];
            Result = data[1] != 0;
            byte[] b = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                b[i] = data[i + 2];
            }

            if (Cmd == COM_Command.COM_CMD_STATUS)
            {
                Data.Main.Parse(b);
            }
            else if (Cmd == COM_Command.COM_CMD_INFO)
            {
                Data.MainInfo.Parse(b);
            }
        }
    }
}

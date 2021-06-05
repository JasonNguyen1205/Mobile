using System;
using System.Collections.Generic;
using System.Text;

namespace BLE.Client.Object
{
    public class Com
    {
        public ComMainInfo MainInfo;
        public ComMain Main;

        public Com()
        {
            Main = new ComMain();
            MainInfo = new ComMainInfo();
        }
    }

    public class ComMain : IBleData
    {
        public bool CurrentActive;
        public bool Safety;
        public bool Failure;
        public bool SafetyRecovery;
        public bool StoveLock;

        public byte[] GetBytes()
        {
            return null;
        }

        public void Parse(byte[] data)
        {
            CurrentActive = (data[0] >> 0 & 0x1) != 0;
            Safety = (data[0] >> 1 & 0x1) != 0;
            Failure = (data[0] >> 2 & 0x1) != 0;
            SafetyRecovery = (data[0] >> 3 & 0x1) != 0;
            StoveLock = (data[0] >> 4 & 0x1) != 0;
        }

        public override string ToString()
        {
            return string.Format(
                "Current active :{0}\r\n" +
                "Safety         :{1}\r\n" +
                "Failure        :{2}\r\n" +
                "Safety recovery:{3}\r\n" +
                "Stove lock     :{4}", CurrentActive, Safety, Failure, SafetyRecovery, StoveLock);
        }
    }

    public class ComMainInfo : IBleData
    {
        public bool StoveLock;
        public int Pswd1;
        public int Pswd2;
        public int Pswd3;
        public int Pswd4;

        public byte[] GetBytes()
        {
            return null;
        }

        public void Parse(byte[] data)
        {
            StoveLock = data[0] != 0;
            ushort pswd = BitConverter.ToUInt16(data, 2);
            Pswd1 = pswd >> 0 & 0x000f;
            Pswd1 = pswd >> 4 & 0x000f;
            Pswd1 = pswd >> 8 & 0x000f;
            Pswd1 = pswd >> 12 & 0x000f;
        }

        public override string ToString()
        {
            return string.Format(
                "Lock: {0}\r\n" +
                "Password: {1} {2} {3} {4}", StoveLock, Pswd1, Pswd2, Pswd3, Pswd4);
        }
    }
}

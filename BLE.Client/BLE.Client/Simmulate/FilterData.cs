using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class FilterData
    {
        const int FILTER_DEEP = 5;
        const int DIF_RISING = 2;
        const int DIF_FALLING = 3;

        bool countEnabe = false;
        double[] Datas;
        Trend Direction;
        int SecCount;

        public FilterData(double value)
        {
            Datas = new double[FILTER_DEEP];
            Direction = Trend.FLATTING;
            for (int i = 0; i < FILTER_DEEP; i++)
            {
                Datas[i] = value;
            }
        }

        public void ResetCount()
        {
            SecCount = 0;
        }

        public void Update(double value)
        {
            for (int i = 0; i < FILTER_DEEP - 1; i++)
            {
                Datas[i] = Datas[i + 1];
            }
            Datas[FILTER_DEEP - 1] = value;

            var dif = value - Datas[0];
            if(dif >= DIF_RISING)
            {
                Direction = Trend.RISING;
                countEnabe = true;
            }
            else if(dif <= DIF_FALLING)
            {
                Direction = Trend.FALLING;
                countEnabe = false;
            }
            else
            {
                Direction = Trend.FLATTING;
            }
        }

        public double GetTemperature()
        {
            return Datas[FILTER_DEEP - 1];
        }

        public int GetCount()
        {
            return SecCount;
        }

        public Trend GetTrend()
        {
            return Direction;
        }

        public void CountUpdate()
        {
            if(countEnabe)
            {
                SecCount++;
            }
        }
    }


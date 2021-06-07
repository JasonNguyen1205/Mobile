using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


    public class Filter
    {
        const int START_TEMPERATURE_VALUE = 60; // degree C
        public FilterData[,] Matrix;
        public Thread thCount;
        int[] xs = Enumerable.Range(0, 200).ToArray();
        int[] ys = Enumerable.Range(0, 200).ToArray();
        public double[,] intensities;
        public  Filter()
        {
            intensities = new double[2000, 2000];
            Matrix = new FilterData[8, 8];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Matrix[row, col] = new FilterData(30);
                }
            }
        }

        public void Update(int row, int col, double value)
        {
            Matrix[row, col].Update(value);
            if(value < START_TEMPERATURE_VALUE)
            {
                Matrix[row, col].ResetCount();
                
            }
        }

        public Trend[,] GetTrends()
        {
            Trend[,] trends = new Trend[8, 8];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    trends[row, col] = Matrix[row, col].GetTrend();
                }
            }
            return trends;
        }

        bool threadCountStop = false;
        public void Start()
        {
            thCount = new Thread(() =>
            {
                while(threadCountStop == false)
                {
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            Matrix[row, col].CountUpdate();
                        }
                    }
                    //Thread.Sleep(1000);
                }
            });
            thCount.IsBackground = true;
            thCount.Start();
        }

        public void Stop()
        {
            threadCountStop = true;
        }

        public double GetTemperature(int row, int col)
        {
            return Matrix[row, col].GetTemperature();
        }

        public Trend GetTrend(int row, int col)
        {
            return Matrix[row, col].GetTrend();
        }
        
        public void ReturnIntensities()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    intensities[row, col] = Matrix[row, col].GetTemperature();
                }
            }
        }
    }


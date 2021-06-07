using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile.Object
{
    public class ChartData
    {
        public double[] XDatas;
        public double[] YDatas;
        public ChartData()
        {
            XDatas = new double[100_000];
            YDatas = new double[100_000];
        }
    }
}

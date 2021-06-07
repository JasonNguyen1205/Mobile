using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile.Object
{
    class MatrixSelect
    {
        int numOfRow;
        int numOfCol;
        int width;
        int height;
        int recDistanceWidth;
        int recDistanceHeight;

        public delegate void SelectedChangedEventHandle(int newRow, int newCol);
        public event SelectedChangedEventHandle SelectedChange;

        int lastRow = 0;    // Default select
        int lastCol = 0;

        public MatrixSelect(int numOfRow, int numOfCol, int width, int height)
        {
            this.numOfRow = numOfRow;
            this.numOfCol = numOfCol;
            this.width = width;
            this.height = height;

            recDistanceWidth = this.width / this.numOfCol;
            recDistanceHeight = this.height / this.numOfRow;
        }

        public void MouseClick(int x, int y)
        {
            int colIndex = x / recDistanceWidth;
            int rowIndex = y / recDistanceHeight;

            if(rowIndex != lastRow || colIndex != lastCol)
            {
                lastRow = rowIndex;
                lastCol = colIndex;
                SelectedChange?.Invoke(rowIndex, colIndex);
            }
        }

        public int GetRow()
        {
            return lastRow;
        }
        public int GetCol()
        {
            return lastCol;
        }
    }

}

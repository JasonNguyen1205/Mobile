//using System;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;

//namespace ELPRO.Tools.Utils
//{
//    public class Heatmap
//    {
//        const ushort ROOM_TEMPERATURE = 30;
//        const int widthMin = 64;
//        const int heightMin = 64;

//        ushort[,] temperatureMatix;
//        Data.Simmulate.Trend[,] trends;
//        Bitmap bitmapMatrix;
//        Bitmap bitmapRender;
//        Rectangle rectangleEmpty;
//        int width;
//        int height;
//        Color[] colorRanges;
//        Font font;
//        Rectangle rectangleMatrix;
//        StringFormat stringFormat;
//        ImageAttributes imageAttributes;
//        bool drawValue = false;
//        bool showTrend = false;

//        public Heatmap(int width, int height)
//        {
//            if (width <= widthMin || height <= heightMin)
//            {
//                throw new Exception();
//            }

//            this.width = width;
//            this.height = height;

//            temperatureMatix = new ushort[8, 8]
//            {
//                { 29, 30, 32, 30, 31, 27, 33, 31},
//                { 30, 27, 31, 31, 31, 27, 32, 31},
//                { 29, 27, 31, 31, 27, 27, 27, 29},
//                { 29, 27, 31, 31, 27, 27, 27, 29},
//                { 29, 27, 31, 31, 27, 27, 27, 29},
//                { 29, 27, 31, 31, 27, 27, 27, 29},
//                { 29, 27, 31, 31, 27, 27, 27, 29},
//                { 29, 27, 31, 31, 27, 27, 27, 29}
//            };

//            trends = new Data.Simmulate.Trend[8,8];

//            bitmapMatrix = new Bitmap(8, 8);
//            bitmapRender = new Bitmap(this.width, this.height);
//            rectangleEmpty = new Rectangle(Point.Empty, new Size(this.width, this.height));
//            colorRanges = new Color[Properties.Resources.colorBase.Width];
//            for (int i = 0; i < colorRanges.Length; i++)
//            {
//                colorRanges[i] = Properties.Resources.colorBase.GetPixel(i, 0);
//            }

//            font = new Font("Consolas", 12);
//            rectangleMatrix = new Rectangle(0, 0, this.width / 8, this.height / 8);
//            stringFormat = new StringFormat()
//            {
//                Alignment = StringAlignment.Center,
//                LineAlignment = StringAlignment.Center
//            };

//            imageAttributes = new ImageAttributes();
//        }

//        public void ShowValue(bool en)
//        {
//            drawValue = en;
//        }

//        public void ShowTrend(bool en)
//        {
//            showTrend = en;
//        }

//        public void TrendUpdate(int row, int col, Data.Simmulate.Trend trend)
//        {
//            trends[row, col] = trend;
//        }

//        public void UpdateMatrix(int row, int col, ushort value)
//        {
//            if (row >= 8 || col >= 8)
//            {
//                Debug.WriteLine("[Heatmap] Row or Col out-of index value");
//                throw new Exception();
//            }
//            temperatureMatix[row, col] = value;
//        }

//        public void Render()
//        {
//            ushort min = 0xffff;
//            ushort max = 0x0000;
//            for (int row = 0; row < 8; row++)
//            {
//                for (int col = 0; col < 8; col++)
//                {
//                    max = temperatureMatix[row, col] > max ? temperatureMatix[row, col] : max;
//                    min = temperatureMatix[row, col] < min ? temperatureMatix[row, col] : min;
//                }
//            }

//            float range = max - min;
//            float intensity;
//            int colorIndex;
//            for (int row = 0; row < 8; row++)
//            {
//                for (int col = 0; col < 8; col++)
//                {
//                    intensity = (temperatureMatix[row, col] - min) / range;
//                    if (intensity > 1)
//                    {
//                        intensity = 1;
//                    }

//                    colorIndex = (int)((colorRanges.Length - 1) * intensity);
//                    bitmapMatrix.SetPixel(col, 7 - row, colorRanges[colorIndex]);
//                }
//            }
//            imageAttributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
//            using (Graphics g = Graphics.FromImage(bitmapRender))
//            {
//                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
//                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
//                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
//                g.DrawImage(bitmapMatrix, rectangleEmpty, 0, 0, bitmapMatrix.Width, bitmapMatrix.Height, GraphicsUnit.Pixel, imageAttributes);

//                string dir = "-";
//                if (drawValue || showTrend)
//                {
//                    for (int row = 0; row < 8; row++)
//                    {
//                        for (int col = 0; col < 8; col++)
//                        {
//                            rectangleMatrix.Y = rectangleMatrix.Width * row;
//                            rectangleMatrix.X = rectangleMatrix.Height * col;

//                            if (trends[7 - row, col] == Data.Simmulate.Trend.RISING)
//                            {
//                                dir = "↑";
//                            }
//                            else if (trends[7 - row, col] == Data.Simmulate.Trend.FALLING)
//                            {
//                                dir = "↓";
//                            }
//                            else
//                            {
//                                dir = "-";
//                            }

//                            if (drawValue && showTrend)
//                            {
//                                g.DrawString(string.Format("{0} {1}", temperatureMatix[7 - row, col], dir), font, Brushes.Black, rectangleMatrix, stringFormat);
//                            }
//                            else if (drawValue && !showTrend)
//                            {
//                                g.DrawString(string.Format("{0}", temperatureMatix[7 - row, col]), font, Brushes.Black, rectangleMatrix, stringFormat);
//                            }
//                            else if (!drawValue && showTrend)
//                            {
//                                g.DrawString(string.Format("{0}", dir), font, Brushes.Black, rectangleMatrix, stringFormat);
//                            }

//                        }
//                    }
//                }
//            }
//        }

//        public Bitmap GetBitmap()
//        {
//            return bitmapRender;
//        }

//        public ushort GetMax()
//        {
//            ushort max = 0;
//            for (int row = 0; row < 8; row++)
//            {
//                for (int col = 0; col < 8; col++)
//                {
//                    max = temperatureMatix[row, col] > max ? temperatureMatix[row, col] : max;
//                }
//            }
//            return max;
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;

    using RuanYun.Logger;

    public class WaterMarkTest
    {
        private const int GrayScale = 200;

        private const int White = 255;

        /// <summary>
        /// 水印颜色，可以调整
        /// </summary>
        private const int WaterMarkColor = 252;

        private const string Content = "水印内容";

        /// <summary>
        ///  去除水印
        ///  如果太小，则返还Null
        /// </summary>
        /// <param name="file"></param>
        /// <returns>更新的图片 或为Null 太小</returns>
        public static Bitmap RemoveWaterMark(string file)
        {
            Bitmap bmp = new Bitmap(file);
            try
            {
                switch (bmp.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        Process24bppRgb(bmp);
                        break;
                    case PixelFormat.Format8bppIndexed:
                        bmp = Process8bppIndexImage(bmp);
                        break;
                    case PixelFormat.Format32bppArgb:
                        bmp = Process32bppArgb(bmp);
                        break;
                }
                return bmp;
            }
            catch (Exception e)
            {
                Log.Write("图片添加水印失败", MessageType.Error, typeof(WaterMarkTest), e);
                return bmp;
            }
        }

        /// <summary>
        /// 添加水印返回文件流
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Stream AddWaterMark(string file)
        {
            var bmp = RemoveWaterMark(file);
            var ms = new MemoryStream();
            bmp.Save(ms, bmp.RawFormat);
            ms.Position = 0;
            bmp.Dispose();
            return ms;
        }

        /// <summary>
        /// 替换图片背景色
        /// 将透明背景替换成白色
        /// 处理32位Argb图片
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private static Bitmap Process32bppArgb(Bitmap map)
        {
            Rectangle rect = new Rectangle(0, 0, map.Width, map.Height);
            var bmpData = map.LockBits(rect, ImageLockMode.ReadWrite, map.PixelFormat);
            for (int j = 0; j < map.Height; j++)
            {
                for (int i = 0; i < map.Width; i++)
                {
                    byte b = Marshal.ReadByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j));
                    byte g = Marshal.ReadByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 1);
                    byte r = Marshal.ReadByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 2);
                    byte a = Marshal.ReadByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 3);


                    if (a == 0 && b == 0 && g == 0 & r == 0)
                    {
                        Marshal.WriteByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j), White);
                        Marshal.WriteByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 1, White);
                        Marshal.WriteByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 2, White);
                        Marshal.WriteByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 3, White);
                    }
                    else
                    {
                        if (a > 0 && r > GrayScale && g > GrayScale && b > GrayScale)
                        {
                            Marshal.WriteByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j), White);
                            Marshal.WriteByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 1, White);
                            Marshal.WriteByte(bmpData.Scan0 + (i * 4 + bmpData.Stride * j) + 2, White);
                        }
                    }
                }
            }

            map.UnlockBits(bmpData);
            MergeImage(map);
            return map;
        }

        /// <summary>
        ///  处理8位图片 转成24位图进行处理
        /// </summary>
        /// <param name="bmp"></param>
        private static Bitmap Process8bppIndexImage(Bitmap bmp)
        {
            Bitmap newbmp = ConvertTo24Rgb(bmp);
            MergeImage(newbmp);
            bmp.Dispose();
            return newbmp;
        }

        /// <summary>
        /// 获取白色调色板索引值
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static int GetPaletteIndex(Bitmap bmp, int color)
        {
            for (int i = 0; i < bmp.Palette.Entries.Length; i++)
            {
                //注意：此行代码速度慢，不需要获取具体RGB值的时候请勿使用
                var p = bmp.Palette.Entries[i];
                if (p.R == color && p.G == color && p.B == color)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///  处理24位rgb格式
        /// </summary>
        /// <param name="bmp"></param>
        private static void Process24bppRgb(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    byte b = Marshal.ReadByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j));
                    byte g = Marshal.ReadByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j) + 1);
                    byte r = Marshal.ReadByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j) + 2);
                    if (r > GrayScale  && g > GrayScale  && b > GrayScale)
                    {
                        if (b != White)
                            Marshal.WriteByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j), White);
                        if (g != White)
                            Marshal.WriteByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j) + 1, White);
                        if (r != White)
                            Marshal.WriteByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j) + 2, White);
                    }
                }
            }
            bmp.UnlockBits(bmpData);
            MergeImage(bmp);
        }

        /// <summary>
        ///  添加水印
        /// </summary>
        /// <param name="bmp"></param>
        private static void MergeImage(Bitmap bmp)
        {
            if (bmp.Height > 100 && bmp.Width > 100)
            {
                int per = bmp.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;
                Bitmap whiteBmp = GetWaterMarkImage(bmp.Width, bmp.Height, bmp.PixelFormat);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
                var bmpDataWhite = whiteBmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
                for (int j = 0; j < bmp.Height; j++)
                {
                    for (int i = 0; i < bmp.Width; i++)
                    {
                        byte b = Marshal.ReadByte(bmpData.Scan0 + (i * per + bmpData.Stride * j));
                        byte g = Marshal.ReadByte(bmpData.Scan0 + (i * per + bmpData.Stride * j) + 1);
                        byte r = Marshal.ReadByte(bmpData.Scan0 + (i * per + bmpData.Stride * j) + 2);
                        byte b1 = Marshal.ReadByte(bmpDataWhite.Scan0 + (i * per + bmpDataWhite.Stride * j));
                        if (b >= 240 && g >= 240 && r >= 240 && b1 == WaterMarkColor)
                        {
                            Marshal.WriteByte(bmpData.Scan0 + (i * per + bmpData.Stride * j), WaterMarkColor);
                            Marshal.WriteByte(bmpData.Scan0 + (i * per + bmpData.Stride * j) + 1, WaterMarkColor);
                            Marshal.WriteByte(bmpData.Scan0 + (i * per + bmpData.Stride * j) + 2, WaterMarkColor);
                        }
                    }
                }
                bmp.UnlockBits(bmpData);
                whiteBmp.UnlockBits(bmpDataWhite);
                whiteBmp.Dispose();
            }
        }

        /// <summary>
        /// 8位深度图片添加水印
        /// 当8位添加失败的时候则转成24位处理
        /// </summary>
        /// <param name="bmp"></param>
        private static Bitmap AddWaterMarkForFormat8bppIndexed(Bitmap bmp)
        {
            if (bmp.Height > 100 && bmp.Width > 100)
            {
                var waterIndex = ProcessAndGetWaterColorIndex(bmp);
                if (waterIndex == -1)
                {
                    Bitmap newbmp = ConvertTo24Rgb(bmp);
                    MergeImage(newbmp);
                    bmp.Dispose();
                    return newbmp;
                }
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
                Bitmap whiteBmp = GetWaterMarkImage(bmpData.Width, bmp.Height, PixelFormat.Format24bppRgb);
                var bmpDataWhite = whiteBmp.LockBits(rect, ImageLockMode.ReadOnly, whiteBmp.PixelFormat);
                var whiteIndex = GetPaletteIndex(bmp, White);
                Log.Write("遍历图片像素开始", MessageType.Info);
                for (int j = 0; j < bmp.Height; j++)
                {
                    for (int i = 0; i < bmp.Width; i++)
                    {
                        byte b = Marshal.ReadByte(bmpDataWhite.Scan0 + (i * 3 + bmpDataWhite.Stride * j));
                        byte g = Marshal.ReadByte(bmpDataWhite.Scan0 + (i * 3 + bmpDataWhite.Stride * j) + 1);
                        byte r = Marshal.ReadByte(bmpDataWhite.Scan0 + (i * 3 + bmpDataWhite.Stride * j) + 2);
                        var b1 = Marshal.ReadByte(bmpData.Scan0 + i + bmpData.Stride * j);
                        if (bmp.Palette.Entries[b1].R > 240 && bmp.Palette.Entries[b1].R < 255
                                                           && bmp.Palette.Entries[b1].G > 240 && bmp.Palette.Entries[b1].G < 255
                                                           && bmp.Palette.Entries[b1].B > 240 && bmp.Palette.Entries[b1].B < 255)
                        {
                            Marshal.WriteByte(bmpData.Scan0 + i + bmpData.Stride * j, (byte)whiteIndex);
                        }
                        if (b == WaterMarkColor && g == WaterMarkColor && r == WaterMarkColor)
                        {
                            if (b1 == whiteIndex)
                                Marshal.WriteByte(bmpData.Scan0 + i + bmpData.Stride * j, (byte)waterIndex);
                        }
                    }
                }
                bmp.UnlockBits(bmpData);
                whiteBmp.UnlockBits(bmpDataWhite);
                whiteBmp.Dispose();
                Log.Write("遍历图片像素结束", MessageType.Info);
            }

            return bmp;
        }

        /// <summary>
        /// 获取水印颜色的调色板索引
        /// 不存在则找到空的添加一个索引
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private static int ProcessAndGetWaterColorIndex(Bitmap bmp)
        {
            int index = 0;
            for (index = 0; index < bmp.Palette.Entries.Length; index++)
            {
                var p = bmp.Palette.Entries[index];
                if (p.R == WaterMarkColor && p.G == WaterMarkColor && p.B == WaterMarkColor)
                {
                    return (byte)index;
                }
            }
            var result = bmp.Palette.Entries.ToList().FindIndex(p => p.R > 240 && p.R < 255 && p.G > 240 && p.G < 255 && p.B > 240 && p.B < 255);
            return result;
        }

        /// <summary>
        /// 获取水印图片
        /// 24位深度
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pixelFormat"></param>
        /// <returns></returns>
        private static Bitmap GetWaterMarkImage(int width, int height, PixelFormat pixelFormat)
        {
            Bitmap whiteBmp = new Bitmap(width, height, pixelFormat);
            Graphics graphics = Graphics.FromImage(whiteBmp);
            int x = width / 5;
            int y = height / 5;
            while (true)
            {
                if (y + 20 > height)
                {
                    break;
                }
                graphics.DrawString(Content, new Font(FontFamily.GenericSerif, 8.0f), new SolidBrush(Color.FromArgb(WaterMarkColor, WaterMarkColor, WaterMarkColor)), x, y);
                x = x + 150;
                if (x + 100 > width)
                {
                    x = width / 5;
                    y = y + 50;
                }

            }
            graphics.Dispose();
            return whiteBmp;
        }

        /// <summary>
        /// 将8位图片转成24位深度图片
        /// 同时去除原有水印
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private static Bitmap ConvertTo24Rgb(Bitmap bmp)
        {
            Bitmap newbmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            var newbmpData = newbmp.LockBits(rect, ImageLockMode.WriteOnly, newbmp.PixelFormat);
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {

                    var b1 = Marshal.ReadByte(bmpData.Scan0 + i + bmpData.Stride * j);
                    var entity = bmp.Palette.Entries[b1];
                    if (bmp.Palette.Entries[b1].R > GrayScale && bmp.Palette.Entries[b1].G > GrayScale 
                                                        && bmp.Palette.Entries[b1].B > GrayScale)
                    {
                        Marshal.WriteByte(newbmpData.Scan0 + (i * 3 + newbmpData.Stride * j), White);
                        Marshal.WriteByte(newbmpData.Scan0 + (i * 3 + newbmpData.Stride * j) + 1, White);
                        Marshal.WriteByte(newbmpData.Scan0 + (i * 3 + newbmpData.Stride * j) + 2, White);
                    }
                    else
                    {
                        Marshal.WriteByte(newbmpData.Scan0 + (i * 3 + newbmpData.Stride * j), entity.B);
                        Marshal.WriteByte(newbmpData.Scan0 + (i * 3 + newbmpData.Stride * j) + 1, entity.G);
                        Marshal.WriteByte(newbmpData.Scan0 + (i * 3 + newbmpData.Stride * j) + 2, entity.R);
                    }

                }
            }
            bmp.UnlockBits(bmpData);
            newbmp.UnlockBits(newbmpData);
            bmp.Dispose();
            return newbmp;
        }
    }
}

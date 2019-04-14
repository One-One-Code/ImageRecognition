using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvTest
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    public static class ImageHelper
    {
        /// <summary>
        /// 图片等比例缩放
        /// </summary>
        /// <param name="imgToResize"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static System.Drawing.Image ResizeImage(this System.Drawing.Image imgToResize, Size size)
        {
            //获取图片宽度
            int sourceWidth = imgToResize.Width;
            //获取图片高度
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //计算宽度的缩放比例
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //计算高度的缩放比例
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //期望的宽度
            int destWidth = (int)(sourceWidth * nPercent);
            //期望的高度
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //绘制图像
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }

        public static Bitmap CutImage(Bitmap rawImage, Rectangle rectangle)
        {
            var image = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format24bppRgb);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image);

            graphics.DrawImage(rawImage, new Rectangle(0, 0, image.Width, image.Height), rectangle, System.Drawing.GraphicsUnit.Pixel);
            return image;
        }

        public static Bitmap CutImageByBit(Bitmap rawImage, Rectangle rectangle)
        {
            var image = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format24bppRgb);
            //var bmpData = rawImage.LockBits(rectangle, ImageLockMode.ReadOnly, rawImage.PixelFormat);
            //var bmpDataNew = image.LockBits(new Rectangle(0,0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
            //for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++)
            //{
            //    for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++)
            //    {
            //        byte b = Marshal.ReadByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j));
            //        byte g = Marshal.ReadByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j) + 1);
            //        byte r = Marshal.ReadByte(bmpData.Scan0 + (i * 3 + bmpData.Stride * j) + 2);
            //        Marshal.WriteByte(bmpDataNew.Scan0 + ((i- rectangle.X) * 3 + bmpDataNew.Stride * (j- rectangle.Y)),b);
            //        Marshal.WriteByte(bmpDataNew.Scan0 + ((i - rectangle.X) * 3 + bmpDataNew.Stride * (j - rectangle.Y)+1), g);
            //        Marshal.WriteByte(bmpDataNew.Scan0 + ((i - rectangle.X) * 3 + bmpDataNew.Stride * (j - rectangle.Y)+2), r);
            //    }
              
            //}
            //rawImage.UnlockBits(bmpData);
            //image.UnlockBits(bmpDataNew);
            return image;
        }
    }
}

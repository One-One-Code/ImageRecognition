using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvTest
{
    using System.Drawing;

    using AForge.Imaging;
    using AForge.Imaging.Filters;

    /// <summary>
    /// 从大图中截取纸张区域，类似高拍仪拍照的图像截取
    /// </summary>
    public class GetPaperFromImage
    {
        /// <summary>
        /// 根据图片的纵横灰度情况截取图片
        /// 使用Aforge类库
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Rectangle GetByGrayPoint(Bitmap image)
        {
            //灰度化
            Grayscale g = new Grayscale(0.2125, 0.7154, 0.0721);
            // apply the filter
            var grayImg = g.Apply(image);
            //二值化
            var threshold = new Threshold();
            threshold.ThresholdValue = 80;
            var thresholdImage = threshold.Apply(grayImg);

            //找出纵横灰度值
            HorizontalIntensityStatistics hs = new HorizontalIntensityStatistics(thresholdImage);
            VerticalIntensityStatistics vs = new VerticalIntensityStatistics(thresholdImage);
            var vThreshold = (image.Height / 4) * 255;
            var hThreshold = (image.Width / 4) * 255;
            var hGrays = hs.Gray.Values.ToList();
            var vGrays = vs.Gray.Values.ToList();
            var hmin = FindPaperBorder(hGrays, true, hThreshold, 0);
            var hmax = FindPaperBorder(hGrays, false, hThreshold, 0);
            var vmin = FindPaperBorder(vGrays, true, vThreshold, 0); ;
            var vmax = FindPaperBorder(vGrays, false, vThreshold, 0);
            return new Rectangle(hmin, vmin, hmax - hmin, vmax - vmin);
        }

        /// <summary>
        /// 尝试查找纸张边界
        /// 考虑可能存在干扰直线的情况，查找到指定的白色直线后，再找40个宽度的直线，如果大部分为黑色则继续递归查找
        /// </summary>
        /// <param name="grays"></param>
        /// <param name="increase"></param>
        /// <param name="threshold"></param>
        /// <param name="firstIndex"></param>
        /// <returns></returns>
        private static int FindPaperBorder(List<int> grays, bool increase, int threshold, int firstIndex)
        {
            firstIndex = firstIndex == 0 ? (increase ? grays.FindIndex(p => p > threshold) : grays.FindLastIndex(p => p > threshold)) : firstIndex;
            if (increase && firstIndex > grays.Count / 3)
            {
                return firstIndex;
            }

            if (!increase && firstIndex < 2 * grays.Count / 3)
            {
                return firstIndex;
            }
            int blackCount = 0;
            int blackIndex = 0;
            int index = firstIndex;
            for (int i = 0; i < 40; i++)
            {
                index = increase ? index + 1 : index - 1;
                if (grays[index] <= threshold)
                {
                    blackCount++;
                    blackIndex = index;
                }
            }

            if (blackCount < 8)
            {
                return firstIndex;
            }
            return FindPaperBorder(grays, increase, threshold, increase ? blackIndex + 1 : blackIndex - 1);
        }
    }
}

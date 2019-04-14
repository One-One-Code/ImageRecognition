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
            threshold.ThresholdValue = 120;
            var thresholdImage = threshold.Apply(grayImg);

            //找出纵横灰度值
            HorizontalIntensityStatistics hs = new HorizontalIntensityStatistics(thresholdImage);
            VerticalIntensityStatistics vs = new VerticalIntensityStatistics(thresholdImage);
            var vThreshold = (image.Height / 4) * 255;
            var hThreshold = (image.Width / 4) * 255;
            var hGrays = hs.Gray.Values.ToList();
            var vGrays = vs.Gray.Values.ToList();
            var hmin = hGrays.FindIndex(p => p > hThreshold);
            var hmax = hGrays.FindLastIndex(p => p > hThreshold);
            var vmin = vGrays.FindIndex(p => p > vThreshold); ;
            var vmax = vGrays.FindLastIndex(p => p > vThreshold);
            return new Rectangle(hmin, vmin, hmax - hmin, vmax - vmin);
        }
    }
}

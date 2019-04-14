using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvTest
{
    using System.Drawing;
    using System.IO;

    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Structure;
    using Emgu.CV.Util;

    using ZXing;

    public class QrCodeHelper
    {
        /// <summary>
        /// 从Qr中获取内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetQrData(string path)
        {
            Image<Gray, Byte> image = new Image<Gray, Byte>(path),
                   dist1 = image.CopyBlank(),
                   dest2 = image.CopyBlank();

            Image<Gray, Byte> threshimg = new Image<Gray, Byte>(image.Width, image.Height);
            CvInvoke.Threshold(image, threshimg, 120, 255, ThresholdType.Binary);
            //threshimg.Save(string.Format(@"E:\data\image\testimage\{0}-threshimg.jpg", Path.GetFileNameWithoutExtension(path)));
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(threshimg, contours, dist1, RetrType.List, ChainApproxMethod.ChainApproxNone);
            var results = new List<Rectangle>();
            for (var k = 0; k < contours.Size; k++)
            {
                if (contours[k].Size < 50)
                {
                    continue;
                }

                var rectangle = CvInvoke.BoundingRectangle(contours[k]);

                float w = rectangle.Width;
                float h = rectangle.Height;
                float rate = Math.Min(w, h) / Math.Max(w, h);
                if (rate > 0.85)
                {
                    dest2.Draw(rectangle, new Gray(255));
                    results.Add(rectangle);
                }

            }
            dest2.Save(string.Format(@"E:\data\image\testimage\{0}-range.jpg", Path.GetFileNameWithoutExtension(path)));
            //按照二维码规则，过滤不符合规范的区域
            var filterResults = new List<Rectangle>();
            foreach (var rectangle in results)
            {
                //if (results.Exists(
                //    p => p.X > rectangle.X && p.X + p.Width < rectangle.X + rectangle.Width && p.Y > rectangle.Y && p.Y + p.Height < rectangle.Y + rectangle.Height))
                //{
                //    filterResults.Add(rectangle);
                //}
                filterResults.Add(rectangle);
            }
            //根据矩形面积进行分组，面积允许误差范围为10%
            var sizeGroups = new Dictionary<int, List<Rectangle>>();
            foreach (var filterResult in filterResults)
            {
                var size = filterResult.Height * filterResult.Width;
                foreach (var sizeGroup in sizeGroups)
                {
                    if (Math.Abs(sizeGroup.Key - size) <= sizeGroup.Key * 0.1)
                    {
                        sizeGroup.Value.Add(filterResult);
                    }
                }
                if (!sizeGroups.ContainsKey(size))
                {
                    var items = new List<Rectangle>() { filterResult };
                    sizeGroups[size] = items;
                }
            }
            //遍历分组，找出最符合二维码的组合
            var groupItems = new List<List<Rectangle>>();
            foreach (var sizeGroup in sizeGroups)
            {
                if (sizeGroup.Value.Count < 3)
                {
                    continue;
                }

                var combinationResults = GetCombinationList(sizeGroup.Value.Count, 3);
                foreach (var combinationResult in combinationResults)
                {
                    var r1 = sizeGroup.Value[combinationResult[0]];
                    var r2 = sizeGroup.Value[combinationResult[1]];
                    var r3 = sizeGroup.Value[combinationResult[2]];
                    if (Math.Abs(r1.Width - r2.Width) < 3 && Math.Abs(r1.Height - r2.Height) < 3
                                                          && Math.Abs(r2.Width - r3.Width) < 3
                                                          && Math.Abs(r2.Height - r3.Height) < 3)
                    {
                        int x = 0;
                        int y = 0;
                        if (Math.Abs(r1.X - r2.X) < r1.Width * 0.25)
                        {
                            y = Math.Abs(r1.Y - r2.Y);
                            var min = r1.Y > r2.Y ? r2 : r1;
                            if (Math.Abs(min.Y - r3.Y) > r1.Height * 0.25)
                            {
                                continue;
                            }
                            x = Math.Abs(r1.X - r3.X);
                        }
                        else if (Math.Abs(r1.Y - r2.Y) < r1.Height * 0.25)
                        {
                            y = Math.Abs(r1.Y - r3.Y);
                            var min = r1.X > r2.X ? r2 : r1;
                            if (Math.Abs(min.X - r3.X) > r1.Width * 0.25)
                            {
                                continue;
                            }
                            x = Math.Abs(r1.X - r2.X);
                        }
                        else if (Math.Abs(r1.Y - r3.Y) < r1.Height * 0.25)
                        {
                            y = Math.Abs(r1.Y - r2.Y);
                            var min = r1.X > r3.X ? r3 : r1;
                            if (Math.Abs(min.X - r2.X) > r1.Width * 0.25)
                            {
                                continue;
                            }
                            x = Math.Abs(r1.X - r3.X);
                        }
                        else if (Math.Abs(r1.X - r3.X) < r1.Width * 0.25)
                        {
                            y = Math.Abs(r1.Y - r3.Y);
                            var min = r1.Y > r3.Y ? r3 : r1;
                            if (Math.Abs(min.Y - r2.Y) > r1.Height * 0.25)
                            {
                                continue;
                            }
                            x = Math.Abs(r1.X - r2.X);
                        }
                        else
                        {
                            continue;
                        }
                        if (Math.Abs(x - y) < r1.Width * 0.25)
                        {
                            var item = new List<Rectangle>();
                            item.Add(r1);
                            item.Add(r2);
                            item.Add(r3);
                            groupItems.Add(item);
                        }
                    }
                }
            }

            if (groupItems.Count == 0)
            {
                return null;
            }
            var max = groupItems.OrderByDescending(p => (p[0].Height * p[0].Width)).ElementAt(0);
            //var newRects = max.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            //if (newRects[0].X != newRects[1].X && newRects[0].Y != newRects[2].Y)
            //{
            //   //三点仿射进行图片纠正
            //    var srcPoints = new PointF[]
            //                    {
            //                        new PointF(newRects[0].X, newRects[0].Y), new PointF(newRects[1].X, newRects[1].Y),
            //                        new PointF(newRects[2].X, newRects[2].Y)
            //                    };
            //    var destPoints = new PointF[]
            //                     {
            //                         new PointF(newRects[0].X, newRects[0].Y), new PointF(newRects[0].X, newRects[1].Y),
            //                         new PointF(newRects[2].X, newRects[0].Y)
            //                     };
            //    var warpImage = RotateHelper.WarpPerspective(image.Bitmap, srcPoints, destPoints);
            //    image = new Image<Gray, Byte>(warpImage);
            //    image.Save(string.Format(@"E:\data\image\testimage\{0}-Rotate.jpg", Path.GetFileNameWithoutExtension(path)));
            //}

            var minx = max.Min(p => p.X) - 10;
            var miny = max.Min(p => p.Y) - 10;
            var width = max.Max(p => (p.X + p.Width)) + 20;
            var height = max.Max(p => (p.Y + p.Height)) + 20;
            var code = ImageHelper.CutImage(image.Bitmap, new Rectangle(minx, miny, width - minx, height - miny));
            code.Save(string.Format(@"E:\data\image\testimage\{0}-qr.jpg", Path.GetFileNameWithoutExtension(path)));
            BarcodeReader reader = new BarcodeReader();
            reader.Options.CharacterSet = "utf-8";
            reader.Options.TryHarder = true;
            var decodeObj = reader.Decode(code);
            if (decodeObj != null)
            {
                return decodeObj.Text;

            }
            else
            {
                int count = 0;
                while (decodeObj == null)
                {
                    if (count > 2)
                    {
                        break;
                    }
                    code = (Bitmap)code.ResizeImage(
                        new Size((int)Math.Round(code.Width * 0.8), (int)Math.Round(code.Height * 0.8)));
                    decodeObj = reader.Decode(code);
                    if (decodeObj != null)
                    {
                        return decodeObj.Text;
                    }

                    count++;
                }

            }

            return null;

        }

        /// <summary>
        /// 获取排列组合的结果集
        /// </summary>
        /// <param name="count">总共多少元素</param>
        /// <param name="perCount">每组多少个</param>
        /// <returns></returns>
        private List<List<int>> GetCombinationList(int count, int perCount)
        {
            var result = new List<List<int>>();
            for (int i = 0; i < count - perCount + 1; i++)
            {
                for (int j = i + 1; j < count - perCount + 2; j++)
                {
                    for (int k = j + 1; k < count; k++)
                    {
                        var item = new List<int>();
                        item.Add(i);
                        item.Add(j);
                        item.Add(k);
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}

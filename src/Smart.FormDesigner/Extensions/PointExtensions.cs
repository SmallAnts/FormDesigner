using System;
using System.Drawing;

namespace Smart.FormDesigner
{
    internal static class PointExtensions
    {
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static int Distance(this Point p1, Point p2)
        {
            // 勾股定理计算
            int x = p1.X - p2.X;
            int y = p1.Y - p2.Y;
            return (int)Math.Sqrt(x * x + y * y);
        }
    }
}

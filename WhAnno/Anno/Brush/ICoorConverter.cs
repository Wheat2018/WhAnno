using System.Drawing;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义绘图坐标变换方法，允许绘图时进行坐标伸缩扭曲，旨在增加绘图灵活性。
    /// </summary>
    public interface ICoorConverter
    {
        /// <summary>
        /// 坐标转换。
        /// </summary>
        /// <param name="point">写进标注类型中的坐标点。</param>
        /// <returns>实际图面上的坐标点。</returns>
        Point Convert(Point point);

        /// <summary>
        /// 坐标反转换。
        /// </summary>
        /// <param name="point">实际图面上的坐标点。</param>
        /// <returns>写进标注类型中的坐标点。</returns>
        Point ReConvert(Point point);
    }
}

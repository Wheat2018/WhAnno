using System.Drawing;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义绘图坐标变换方法，允许绘图时进行坐标伸缩扭曲，旨在增加绘图灵活性。
    /// </summary>
    interface ICoorConverter
    {
        Point Convert(Point point);
    }
}

using System.Drawing;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义绘制标注的方法，给定一个图面和一个标注，接口定义具体绘制方式。
    /// </summary>
    public interface IAnnoPaintable
    {
        /// <summary>
        /// 给定GDI+绘图图面和标注实例，将实例绘制到指定图面中。
        /// </summary>
        /// <param name="g">GDI+绘图图面</param>
        /// <param name="anno">标注实例</param>
        /// <param name="cvt">坐标变换规则</param>
        /// 
        void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null);
    }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Utils;
using WhAnno.Anno.Base;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义画笔类的共有特性及约束，提供一些适用于所有画笔类的静态方法。
    /// </summary>
    abstract class BrushBase : PictureBox
    {
        //Properties
        /// <summary>
        /// 获取本画笔类型的标注类型。
        /// </summary>
        public Type AnnoType => GetAnnoTypeOf(GetType());

        /// <summary>
        /// 获取本画笔类型的标注类型的所有公共字段。
        /// </summary>
        public FieldInfo[] AnnoFields => AnnoType.GetFields();

        /// <summary>
        /// 获取或设置所有画笔的默认钢笔。
        /// </summary>
        public static Pen DefaultPen { get; set; } = new Pen(Color.Black, 2);

        /// <summary>
        /// 获取或设置所有画笔的默认字体。
        /// </summary>
        public static new Font DefaultFont { get; set; } = new Font("宋体", 9);

        //Fields
        /// <summary>
        /// 绘制标注时所用的GDI+钢笔。
        /// </summary>
        public Pen pen = DefaultPen;
        /// <summary>
        /// 绘制标注时所用的字体。
        /// </summary>
        public Font font = DefaultFont;

        //To Implement
        /// <summary>
        /// 给定GDI+绘图图面和标注实例，将实例绘制到指定图面中。
        /// </summary>
        /// <param name="g">GDI+绘图图面</param>
        /// <param name="anno">标注实例</param>
        /// <param name="cvt">坐标变换规则</param>
        /// 
        public abstract void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null);

        //Methods
        /// <summary>
        /// 默认构造。规定了画笔的Icon在更改大小时必须重绘。
        /// </summary>
        public BrushBase() => SetStyle(ControlStyles.ResizeRedraw, true);

        //Static Methods
        /// <summary>
        /// 判断类型是否是画笔类型，即在WhAnno.Anno.Brush命名空间下并继承了BrushBase。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static bool IsBrushType(Type type)
        {
            return type.FullName.Contains("WhAnno.Anno.Brush.") && type.BaseType.Name == "BrushBase";
        }

        /// <summary>
        /// 获取所有画笔类型，即WhAnno.Anno.Brush命名空间下继承了BrushBase的类型。
        /// </summary>
        /// <returns></returns>
        public static Type[] GetBrushTypes()
        {
            List<Type> types = new List<Type>();
            foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (IsBrushType(item))
                    types.Add(item);
            }
            return types.ToArray();
        }

        /// <summary>
        /// 获取目标画笔类型的标注类型，标注类型是一种值类型。若所给类型内没有标注
        /// 类型或所给非画笔类型，都会返回null。
        /// </summary>
        /// <param name="brushType">目标画笔类型</param>
        /// <returns></returns>
        public static Type GetAnnoTypeOf(Type brushType)
        {
            if (IsBrushType(brushType))
                return Assembly.GetExecutingAssembly().GetType(brushType.FullName + "+Annotation");
            return null;
        }

    }
}

/// <summary>
/// WhAnno画笔命名空间。该命名空间下的类名与System.Drawing下
/// 的类名高度重叠，通常不建议使用该命名空间。
/// </summary>
namespace WhAnno.Anno.Brush
{
    class Rectangle : BrushBase
    {
        /// <summary>
        /// 标注类型。
        /// </summary>
        public struct Annotation
        {
            public string file;
            public int x, y, width, height;
            public string category;
        }

        //Properties
        /// <summary>
        /// 绘制画笔图标的线条宽度。
        /// </summary>
        public int IconLineWidth { get; set; } = 2;

        //Abstract Implement
        public override void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null)
        {
            Annotation annotation = (Annotation)anno;
            g.SmoothingMode = SmoothingMode.HighQuality;

            //将矩形转换成点数组
            System.Drawing.Point[] rect = new System.Drawing.Rectangle(
                annotation.x, annotation.y, annotation.width, annotation.height).ConvertToQuadraPoints();

            //转换坐标
            if (cvt != null)
                for (int i = 0; i < rect.Length; i++) rect[i] = cvt.Convert(rect[i]);

            g.DrawPolygon(pen, rect);
        }

        //Icon Paint
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                ClientSize.Width / 6, ClientSize.Height / 4, ClientSize.Width * 2 / 3, ClientSize.Height / 2);
            pe.Graphics.DrawRectangle(new Pen(ForeColor, IconLineWidth), rect);
            base.OnPaint(pe);
        }

    }

    class Ellipse : BrushBase
    {
        /// <summary>
        /// 标注类型。
        /// </summary>
        public struct Annotation
        {
            public string file;
            public float major_axis_radius;
            public float minor_axis_radius;
            public float angle;
            public float center_x;
            public float center_y;
            public string category;
        }

        //Properties
        /// <summary>
        /// 绘制画笔图标的线条宽度。
        /// </summary>
        public int IconLineWidth { get; set; } = 2;

        //Abstract Implement
        public override void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null)
        {
            Annotation annotation = (Annotation)anno;
            g.SmoothingMode = SmoothingMode.HighQuality;

            throw new NotImplementedException();
        }

        //Icon Paint
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                ClientSize.Width / 6, ClientSize.Height / 4, ClientSize.Width * 2 / 3, ClientSize.Height / 2);
            pe.Graphics.DrawEllipse(new Pen(ForeColor, IconLineWidth), rect);
            base.OnPaint(pe);
        }

    }

    class Point : BrushBase
    {
        /// <summary>
        /// 标注类型。
        /// </summary>
        public struct Annotation
        {
            public string file;
            public int x, y;
            public string category;
        }

        //Properties
        /// <summary>
        /// 绘制画笔图标的线条宽度。
        /// </summary>
        public int IconLineWidth { get; set; } = 2;

        //Abstract Implement
        public override void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null)
        {
            throw new NotImplementedException();
        }

        //Icon Paint
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                ClientSize.Width / 2 - IconLineWidth, ClientSize.Height / 2 - IconLineWidth,
                IconLineWidth * 2, IconLineWidth * 2);
            pe.Graphics.FillEllipse(new SolidBrush(ForeColor), rect);
            base.OnPaint(pe);
        }
    }

}

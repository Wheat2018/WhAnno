using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno.Base;

namespace WhAnno.Anno.Base
{
    abstract class BrushBase : PictureBox
    {
        public BrushBase()
        {

        }

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
        public static Type[] GetTypes()
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

        /// <summary>
        /// 获取目标画笔类型的标注类型中所有公共字段。若所给类型没有标注类型或所给非
        /// 画笔类型，都会返回null；若所给类型标注类型没有公共字段，则返回空数组。
        /// </summary>
        /// <param name="brushType"></param>
        /// <returns></returns>
        public static FieldInfo[] GetAnnoFieldsOf(Type brushType)
        {
            Type annoType = GetAnnoTypeOf(brushType);
            if (annoType != null)
            {
                return annoType.GetFields();
            }
            return null;
        }
    }
}

namespace WhAnno.Anno.Brush
{
    class Rectangle : BrushBase
    {
        struct Annotation
        {
            public string file;
            public int x, y, width, height;
        }
        public int IconLineWidth { get; set; } = 2;

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }
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
        public int IconLineWidth { get; set; } = 2;

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }
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
        public int IconLineWidth { get; set; } = 2;

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }
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

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
using System.Diagnostics;
using WhAnno.Utils.Expand;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义画笔类的共有特性及约束，提供一些适用于所有画笔类的静态方法。
    /// </summary>
    /// <remarks>画笔类提供使用GDI+绘制标注的方法，以及一系列对<see cref="Canva"/>类消息的处理委托。</remarks>
    public abstract class BrushBase : PictureBox, IAnnoPaintable, IEventDelegable
    {
        //Properties
        /// <summary>
        /// 获取本画笔类型的标注类型。
        /// </summary>
        public Type AnnoType => GetAnnoTypeOf(GetType());

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
        /// 画笔初始化。
        /// </summary>
        /// <remarks>在准备使用画笔创建新标注时调用。</remarks>
        public abstract void Init();
        /// <summary>
        /// 给定GDI+绘图图面和标注实例，将实例绘制到指定图面中。
        /// </summary>
        /// <param name="g">GDI+绘图图面</param>
        /// <param name="anno">标注实例</param>
        /// <param name="cvt">坐标变换规则</param>
        /// 
        public abstract void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null);
        /// <summary>
        /// 生成当前画笔所构造的标注。
        /// </summary>
        /// <returns>当前画笔所构造的标注，如果画笔未准备好，将返回null</returns>
        public abstract AnnotationBase GenerateAnnotation();

        //Overridable
        public virtual bool DelegateMouseDown(object sender, MouseEventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateMouseMove(object sender, MouseEventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateMouseUp(object sender, MouseEventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateMouseClick(object sender, MouseEventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateMouseWheel(object sender, MouseEventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateMouseEnter(object sender, EventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateMouseLeave(object sender, EventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateMouseHover(object sender, EventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateClick(object sender, EventArgs e, ICoorConverter cvt = null) => true;
        public virtual bool DelegateProcessCmdKey(object sender, ref Message msg, Keys keyData, ICoorConverter cvt = null) => true;
        public virtual void DelegateKeyPress(object sender, KeyPressEventArgs e, ICoorConverter cvt = null) {; }
        public virtual void DelegatePaint(object sender, PaintEventArgs e, ICoorConverter cvt = null) {; }

        //Methods
        /// <summary>
        /// 默认构造。规定了画笔的Icon在更改大小时必须重绘。
        /// </summary>
        public BrushBase() => SetStyle(ControlStyles.ResizeRedraw, true);

        /// <summary>
        /// 创建一个标注类型实例。
        /// </summary>
        /// <returns></returns>
        public AnnotationBase CreatAnnotation() => Assembly.GetExecutingAssembly().CreateInstance(AnnoType.FullName) as AnnotationBase;

        //Static Methods
        /// <summary>
        /// 判断类型是否是画笔类型，即在WhAnno.Anno.Brush命名空间下并继承了BrushBase的类型。（运行时确定）
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static bool IsBrushType(Type type)
        {
            return type?.Namespace == "WhAnno.Anno.Brush" && type?.BaseType.Name == "BrushBase";
        }

        /// <summary>
        /// 获取目标画笔类型的标注类型。
        /// </summary>
        /// <param name="brushType">目标画笔类型</param>
        /// <returns>标注类型。若所给类型内没有标注类型或所给非画笔类型，为null。</returns>
        public static Type GetAnnoTypeOf(Type brushType)
        {
            if (IsBrushType(brushType) && AnnotationBase.IsAnnoType(brushType.GetNestedType("Annotation")))
                return brushType.GetNestedType("Annotation");
            return null;
        }

        /// <summary>
        /// 获取所有画笔类型。
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
        /// 使用画笔通用名称获取画笔类型。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetBrushType(string name)
        {
            var t = from type in GetBrushTypes()
                    where type.Name == name
                    select type;
            var enumerator = t.GetEnumerator();
            if (!enumerator.MoveNext()) return null;
            Type res = enumerator.Current;
            if (enumerator.MoveNext()) throw new Exception($"类型名称{name}具有多义性，找到{t.Count()}个匹配项");
            return t.First();
        }
    }

    public class AnnotationBase : ICloneable
    {
        //Properties
        /// <summary>
        /// 获取本标注类型的画笔类型。
        /// </summary>
        public Type BrushType => GetBrushTypeOf(GetType());

        /// <summary>
        /// 获取标注的子类字段。即非共有标注属性<see cref="file"/>、<see cref="category"/>等。
        /// </summary>
        public FieldInfo[] SubFields
        {
            get
            {
                var query = from field in GetType().GetFields()
                            where field.DeclaringType != typeof(AnnotationBase)
                            select field;
                return query.ToArray();
            }
        }

        //Fields
        /// <summary>
        /// 标注所在图像名。
        /// </summary>
        public string file = "";
        /// <summary>
        /// 标注类别。
        /// </summary>
        public string category = "default";

        //Methods
        /// <summary>
        /// 创建一个标注类型实例。
        /// </summary>
        /// <returns></returns>
        public BrushBase CreatBrush() => Assembly.GetExecutingAssembly().CreateInstance(BrushType.FullName) as BrushBase;

        /// <summary>
        /// 填充标注实例。
        /// </summary>
        /// <param name="fields">指定的若干个字段名</param>
        /// <param name="values">与给定字段名严格对应的字段值</param>
        /// <returns></returns>
        /// <remarks>
        /// <paramref name="values"/>可以为实现了<see cref="IConvertible"/>的任意类型，方法自动将其转换为字段的真实类型，若转换失败，会抛出异常。
        /// </remarks>
        public AnnotationBase SetFieldsValues(FieldInfo[] fields, object[] values)
        {
            //静态断言：fields长度理应等于values长度
            Debug.Assert(fields.Length == values.Length, $"fields长度({fields.Length})不等于values长度({values.Length})");

            for (int i = 0; i < fields.Length; i++)
            {
                if (values[i] == null) continue;
                fields[i].SetValue(this, Convert.ChangeType(values[i], fields[i].FieldType));
            }
            return this;
        }

        /// <summary>
        /// 获取标注实例的字段值。
        /// </summary>
        /// <param name="fields">指定字段，若保持null则获取全部字段。</param>
        /// <returns></returns>
        /// <remarks>
        /// 若所给实例不是标注类型，返回null
        /// </remarks>
        public object[] GetFieldsValues(FieldInfo[] fields = null)
        {
            if (fields is null) fields = GetType().GetFields();
            object[] result = new object[fields.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = fields[i].GetValue(this);
            return result;
        }

        //Overridable
        /// <summary>
        /// 在指定图面指定点绘制带框的类别标签。
        /// </summary>
        /// <param name="g">GDI+绘图图面</param>
        /// <param name="loc">文本绘制左下角</param>
        /// <param name="font">文字字体</param>
        /// <param name="rectColor">矩形颜色</param>
        /// <param name="strColor">文字颜色</param>
        public virtual void DrawCategory(Graphics g, Point loc, Font font, Color rectColor, Color strColor)
        {
            if (category.Length > 0)
            {
                SizeF size = g.MeasureString(category, font);
                RectangleF rectangle = new RectangleF(new PointF(loc.X, loc.Y - size.Height), size);
                using (System.Drawing.Brush brush = new SolidBrush(rectColor))
                {
                    g.FillRectangle(brush, rectangle);
                }
                using (System.Drawing.Brush brush = new SolidBrush(strColor))
                {
                    g.DrawString(category, font, brush, rectangle.Location);
                }

            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(category);
            builder.Append('{');
            foreach (object fieldValue in GetFieldsValues(SubFields))
                builder.Append(fieldValue.ToString()).Append(',');
            builder[builder.Length - 1] = '}';
            return builder.ToString();
        }


        //Static Methods
        /// <summary>
        /// 判断类型是否是标注类型，即内嵌在画笔类型下的、名为"Annotation"、继承了AnnotationBase的内嵌类型。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static bool IsAnnoType(Type type)
        {
            return BrushBase.IsBrushType(type?.ReflectedType) && 
                    type?.Name == "Annotation" && type?.BaseType.Name == "AnnotationBase";
        }

        /// <summary>
        /// 获取目标标注类型的画笔类型。
        /// </summary>
        /// <param name="annoType">标注类型</param>
        /// <returns>画笔类型。若所给类型不是标注类型或没有对应画笔类型，为null。</returns>
        public static Type GetBrushTypeOf(Type annoType)
        {
            if (IsAnnoType(annoType))
                return annoType.ReflectedType;
            return null;
        }

        /// <summary>
        /// 获取所有标注类型。
        /// </summary>
        /// <returns></returns>
        public static Type[] GetAnnoTypes()
        {
            List<Type> types = new List<Type>();
            foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (IsAnnoType(item))
                    types.Add(item);
            }
            return types.ToArray();
        }

        public object Clone()
        {
            AnnotationBase result = Assembly.GetExecutingAssembly().CreateInstance(GetType().FullName) as AnnotationBase;
            FieldInfo[] fields = GetType().GetFields();
            result.SetFieldsValues(fields, GetFieldsValues(fields));
            return result;
        }
    }
}

/// <summary>
/// WhAnno画笔命名空间。该命名空间下的类名与System.Drawing下的类名高度重叠，通常不建议使用该命名空间。
/// </summary>
namespace WhAnno.Anno.Brush
{
    class Rectangle : BrushBase
    {
        /// <summary>
        /// 标注类型。
        /// </summary>
        public class Annotation : AnnotationBase
        {
            public int x, y, width, height;
        }

        /// <summary>
        /// 指示<see cref="Rectangle"/>画笔当前状态。
        /// </summary>
        public enum BrushStatus 
        {
            /// <summary>
            /// 未生成任何标注。
            /// </summary>
            Free = 0,
            /// <summary>
            /// 正在建立标注中。
            /// </summary>
            Building = 1,
            /// <summary>
            /// 标注已建立，待调整。
            /// </summary>
            Tuning = 2
        }

        //Properties
        /// <summary>
        /// 绘制画笔图标的线条宽度。
        /// </summary>
        public int IconLineWidth { get; set; } = 2;
        public BrushStatus Status { get; private set; }
        public Annotation TempAnno { get; private set; }

        private System.Drawing.Point downPoint;
        private Annotation downTempAnno;

        //Methods
        public Rectangle() => Init();

        //Override
        public override bool DelegateMouseDown(object sender, MouseEventArgs e, ICoorConverter cvt = null)
        {
            if (Utils.Judge.Mouse.Left(e))
            {
                System.Drawing.Point point = e.Location;
                if (cvt != null) point = cvt.ReConvert(point);
                downPoint = point;
                downTempAnno = TempAnno.Clone() as Annotation;
            }

            return true;
        }

        public override bool DelegateMouseMove(object sender, MouseEventArgs e, ICoorConverter cvt = null)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control) return true;

            System.Drawing.Point point = e.Location;
            if (cvt != null) point = cvt.ReConvert(point);

            if (Utils.Judge.Mouse.Left(e))
            {
                if (Status == BrushStatus.Free || Status == BrushStatus.Building)
                {
                    Status = BrushStatus.Building;
                    System.Drawing.Rectangle rect = RectangleTransform.FromTwoPoints(downPoint, point);

                    TempAnno.x = rect.X;
                    TempAnno.y = rect.Y;
                    TempAnno.width = rect.Width;
                    TempAnno.height = rect.Height;
                }
                else
                {
                    Size delta = new Size(point.X - downPoint.X, point.Y - downPoint.Y);

                    TempAnno.x = downTempAnno.x + delta.Width;
                    TempAnno.y = downTempAnno.y + delta.Height;
                }
                (sender as Control).Invalidate();
                GlobalMessage.Add("status", TempAnno.ToString());
            }
            else if (e.Button == MouseButtons.None)
            {
                if (Status == BrushStatus.Free && TempAnno != null)
                {
                    TempAnno.x = point.X - TempAnno.width / 2;
                    TempAnno.y = point.Y - TempAnno.height / 2;

                    (sender as Control).Invalidate();
                    GlobalMessage.Add("status", TempAnno.ToString());
                }
            }

            return true;
        }

        public override bool DelegateMouseUp(object sender, MouseEventArgs e, ICoorConverter cvt = null)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (Status == BrushStatus.Free) Status = BrushStatus.Tuning;
                    else if (Status == BrushStatus.Building) Status = BrushStatus.Free;
                    break;
                case MouseButtons.Right:
                    if (Status == BrushStatus.Tuning) Status = BrushStatus.Free;
                    break;
            }
            (sender as Control).Invalidate();

            return true;
        }

        public override bool DelegateProcessCmdKey(object sender, ref Message msg, Keys keyData, ICoorConverter cvt = null)
        {
            //键值是否已处理。
            bool handled = false;
            if (TempAnno != null)
            {
                switch (keyData)
                {
                    case Keys.W:
                    case Keys.Up:
                        TempAnno.height++;
                        handled = true;
                        break;
                    case Keys.S:
                    case Keys.Down:
                        TempAnno.height = Math.Max(0, TempAnno.height - 1);
                        handled = true;
                        break;
                    case Keys.D:
                    case Keys.Right:
                        TempAnno.width++;
                        handled = true;
                        break;
                    case Keys.A:
                    case Keys.Left:
                        TempAnno.width = Math.Max(0, TempAnno.width - 1);
                        handled = true;
                        break;

                    case Keys.Back:
                        if (Status == BrushStatus.Tuning) Status = BrushStatus.Free;
                        handled = true;
                        break;
                    case Keys.Enter:
                    case Keys.Space:
                        if (Status == BrushStatus.Free)
                        {
                            Status = BrushStatus.Tuning;
                            handled = true;
                        }
                        break;
                }
            }

            if (handled) (sender as Control).Invalidate();
            return !handled;
        }

        public override void DelegatePaint(object sender, PaintEventArgs e, ICoorConverter cvt = null)
        {
            if (TempAnno != null)
            {
                if (Status == BrushStatus.Tuning) pen.DashStyle = DashStyle.Solid;
                else pen.DashStyle = DashStyle.Dash;
                PaintAnno(e.Graphics, TempAnno, cvt);

            }

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

        //Abstract Implement
        public override void Init()
        {
            TempAnno = new Annotation() { width = 10, height = 10 };
            Status = BrushStatus.Free;
        }

        public override void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null)
        {
            if (anno.GetType() != AnnoType) return;

            Annotation annotation = (Annotation)anno;
            g.SmoothingMode = SmoothingMode.HighQuality;

            //将矩形转换成点数组
            System.Drawing.Point[] rect = new System.Drawing.Rectangle(
                annotation.x, annotation.y, annotation.width, annotation.height).ConvertToQuadraPoints();

            //转换坐标
            if (cvt != null)
                for (int i = 0; i < rect.Length; i++) rect[i] = cvt.Convert(rect[i]);

            //打印类别文本
            annotation.DrawCategory(g, rect[0], font, pen.Color, pen.Color.GetReverse());

            //打印矩形
            g.DrawPolygon(pen, rect);
        }

        public override AnnotationBase GenerateAnnotation()
        {
            if (Status == BrushStatus.Tuning) return TempAnno;
            return null;
        }

    }

    class Ellipse : BrushBase
    {
        /// <summary>
        /// 标注类型。
        /// </summary>
        public class Annotation : AnnotationBase
        {
            public float major_axis_radius;
            public float minor_axis_radius;
            public float angle;
            public float center_x;
            public float center_y;
        }

        //Properties
        /// <summary>
        /// 绘制画笔图标的线条宽度。
        /// </summary>
        public int IconLineWidth { get; set; } = 2;

        //Abstract Implement
        public override void Init()
        {
            throw new NotImplementedException();
        }
        public override void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null)
        {
            if (anno.GetType() != AnnoType) return;

            //Annotation annotation = (Annotation)anno;
            g.SmoothingMode = SmoothingMode.HighQuality;

            throw new NotImplementedException();
        }
        public override AnnotationBase GenerateAnnotation()
        {
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
        public class Annotation : AnnotationBase
        {
            public int x, y;
        }

        //Properties
        /// <summary>
        /// 绘制画笔图标的线条宽度。
        /// </summary>
        public int IconLineWidth { get; set; } = 2;

        //Abstract Implement
        public override void Init()
        {
            throw new NotImplementedException();
        }
        public override void PaintAnno(Graphics g, object anno, ICoorConverter cvt = null)
        {
            throw new NotImplementedException();
        }

        public override AnnotationBase GenerateAnnotation()
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

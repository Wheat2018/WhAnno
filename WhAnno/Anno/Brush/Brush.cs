using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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

    }
}

namespace WhAnno.Anno.Brush
{
    class Rectangle : BrushBase
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

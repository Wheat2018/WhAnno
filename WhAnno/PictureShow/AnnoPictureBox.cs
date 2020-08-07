using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno.Base;
using WhAnno.Utils;
using WhAnno.Utils.Expand;

namespace WhAnno.PictureShow
{
    class AnnoPictureBox: PictureBox
    {
        //Properties
        /// <summary>
        /// 图片的文件名（不含后缀）。
        /// </summary>
        public string FileName => Path.GetFileNameWithoutExtension(FilePath);
        /// <summary>
        /// 图片的全名。
        /// </summary>
        public string FilePath { get => ImageLocation; set => ImageLocation = value; }
        /// <summary>
        /// 图片的标注。
        /// </summary>
        public List<AnnotationBase> Annotations { get; } = new List<AnnotationBase>();
        /// <summary>
        /// 要绘制的索引值。
        /// </summary>
        public int Index 
        { 
            get => index;
            set
            {
                index = value;
                //Index更改时触发重绘。
                Invalidate();
            }
        }
        private int index;

        //Style
        public Font paintFileNameFont;
        public Font paintIndexFont;

        public AnnoPictureBox()
        {
            Annotations.AsParallel();
            SizeMode = PictureBoxSizeMode.Zoom;
            BorderStyle = BorderStyle.FixedSingle;
            paintFileNameFont = paintIndexFont = Font;
            Paint += PaintText;
        }

        public AnnoPictureBox(string filePath) : this() => FilePath = filePath;

        static public bool CheckAnnotation(AnnotationBase annotation, string filePath)
        {
            return annotation != null && (annotation.file.Length == 0 || filePath.TailContains(annotation.file));
        }

        public bool AddAnnotation(AnnotationBase annotation)
        {
            if (!CheckAnnotation(annotation, FilePath)) return false;
            annotation.file = FilePath;
            Annotations.Add(annotation);
            Invalidate();
            return true;
        }

        /// <summary>
        /// 给定GDI+绘图图面，将标注绘制到指定图面中。
        /// </summary>
        /// <param name="g">GDI+绘图图面</param>
        /// <param name="cvt">坐标变换规则</param>
        /// 
        public void DrawAnnos(Graphics g, ICoorConverter cvt = null)
        {
            Color[] colors = ColorList.Linspace(Annotations.Count);
            for (int i = 0; i < Annotations.Count; i++)
            {
                BrushBase brush = Annotations[i].CreatBrush();
                brush.pen = new Pen(Color.FromArgb(200, colors[i]), 2);
                brush.PaintAnno(g, Annotations[i], cvt);

            }
            //foreach (AnnotationBase anno in Annotations)
            //    anno.CreatBrush().PaintAnno(g, anno, cvt);
        }

        /// <summary>
        /// 绘制<see cref="AnnoPictureBox"/>文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pe"></param>
        protected void PaintText(object sender ,PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            SizeF size = pe.Graphics.MeasureString(FileName, paintFileNameFont);
            float startX = 0;
            float startY = Height - size.Height;
            pe.Graphics.DrawString(FileName, paintFileNameFont, new SolidBrush(ForeColor), startX, startY);
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, Color.Orange)))
            {
                pe.Graphics.FillRectangle(brush,
                    new RectangleF(new PointF(0, 0),
                    pe.Graphics.MeasureString(Index.ToString(), paintIndexFont)));
                pe.Graphics.DrawString(Index.ToString(), paintIndexFont, new SolidBrush(ForeColor), 0, 0);
            }

            if (Annotations.Count > 0)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, Color.Red)))
                {
                    SizeF textSize = pe.Graphics.MeasureString(Annotations.Count.ToString(), paintIndexFont);
                    pe.Graphics.FillEllipse(brush, new RectangleF(Width - 1.2f * textSize.Width,
                                                                    -1.2f * textSize.Height,
                                                                    2.4f * textSize.Width,
                                                                    2.4f * textSize.Height));
                    pe.Graphics.DrawString(Annotations.Count.ToString(), paintIndexFont, new SolidBrush(ForeColor),
                                            Width - textSize.Width, 0);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image == null) Load();
            base.OnPaint(pe);
        }
    }
}

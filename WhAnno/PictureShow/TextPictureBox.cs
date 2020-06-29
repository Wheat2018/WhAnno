using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.PictureShow
{
    class TextPictureBox: PictureBox
    {
        /// <summary>
        /// 图片的文件名（不含后缀）。
        /// </summary>
        public string FileName { get => Path.GetFileNameWithoutExtension(FilePath); }
        /// <summary>
        /// 图片的全名。
        /// </summary>
        public string FilePath { get; private set; }
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

        public TextPictureBox()
        {
            SizeMode = PictureBoxSizeMode.Zoom;
            BorderStyle = BorderStyle.FixedSingle;
            paintFileNameFont = paintIndexFont = Font;
        }

        public TextPictureBox(string filePath) : this() => SetPicture(filePath);

        /// <summary>
        /// 加载图像。
        /// </summary>
        /// <param name="filePath"></param>
        public void SetPicture(string filePath)
        {
            FilePath = filePath;
            Image?.Dispose();
            //Image更改会自动触发重绘
            Image = new Bitmap(filePath);
        }

        /// <summary>
        /// 异步加载图像。
        /// </summary>
        /// <param name="filePath"></param>
        public async Task SetPictureAsync(string filePath)
        {
            FilePath = filePath;
            Image?.Dispose();
            //异步读取图像文件
            //Image更改会自动触发重绘
            Image = await Task.Run(() => new Bitmap(filePath));
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            SizeF size = pe.Graphics.MeasureString(FileName, paintFileNameFont);
            float startX = 0;
            float startY = Height - size.Height;
            pe.Graphics.DrawString(FileName, paintFileNameFont, new SolidBrush(ForeColor), startX, startY);
            using(SolidBrush brush = new SolidBrush(Color.FromArgb(150,Color.Orange)))
            {
                pe.Graphics.FillRectangle(brush,
                    new RectangleF(new PointF(0, 0),
                    pe.Graphics.MeasureString(Index.ToString(), paintIndexFont)));
            }
            pe.Graphics.DrawString(Index.ToString(), paintIndexFont, new SolidBrush(ForeColor), 0, 0);
        }
    }
}

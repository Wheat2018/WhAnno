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
        public string FileName { get; private set; }
        /// <summary>
        /// 图片的全名。
        /// </summary>
        public string FilePath 
        { 
            get => filePath;
            set
            {
                filePath = value;
                FileName = Path.GetFileNameWithoutExtension(filePath);
                Image?.Dispose();
                //Image更改会自动触发重绘
                Image = new Bitmap(filePath);
            }
        }
        private string filePath;
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

        public TextPictureBox(string filePath)
        {
            FilePath = filePath;

            SizeMode = PictureBoxSizeMode.Zoom;
            BorderStyle = BorderStyle.FixedSingle;
            paintFileNameFont = paintIndexFont = Font;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            SizeF size = pe.Graphics.MeasureString(FileName, Font);
            float startX = 0;
            float startY = Height - size.Height;
            pe.Graphics.DrawString(FileName, paintFileNameFont, new SolidBrush(ForeColor), startX, startY);
            pe.Graphics.DrawString(Index.ToString(), paintIndexFont, new SolidBrush(ForeColor), 0, 0);
        }
    }
}

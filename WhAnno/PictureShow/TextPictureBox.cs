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
        public string filePath;

        public string fileName;

        public int index;

        //Style
        public Font paintFileNameFont;
        public Font paintIndexFont;

        public TextPictureBox(string filePath)
        {
            this.filePath = filePath;

            fileName = Path.GetFileNameWithoutExtension(filePath);
            Image = new Bitmap(filePath);
            SizeMode = PictureBoxSizeMode.Zoom;
            BorderStyle = BorderStyle.FixedSingle;
            paintFileNameFont = this.paintIndexFont = Font;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            SizeF size = pe.Graphics.MeasureString(fileName, Font);
            float startX = 0;
            float startY = Height - size.Height;
            pe.Graphics.DrawString(fileName, paintFileNameFont, new SolidBrush(ForeColor), startX, startY);
            pe.Graphics.DrawString(index.ToString(), paintIndexFont, new SolidBrush(ForeColor), 0, 0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno
{
    class TextPictureBox: PictureBox
    {
        public string fileDir;

        public string fileName;
        public int index;

        //Style
        public Font paintFileNameFont;
        public Font paintIndexFont;

        public TextPictureBox(string fileDir)
        {
            this.fileDir = fileDir;

            this.fileName = Path.GetFileNameWithoutExtension(fileDir);
            this.Image = new Bitmap(fileDir);
            this.SizeMode = PictureBoxSizeMode.Zoom;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.paintFileNameFont = this.paintIndexFont = Font;
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

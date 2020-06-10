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
        
        public TextPictureBox(string fileDir)
        {
            this.fileDir = fileDir;

            this.fileName = Path.GetFileNameWithoutExtension(fileDir);
            this.Image = new Bitmap(fileDir);
            this.SizeMode = PictureBoxSizeMode.Zoom;
            this.BorderStyle = BorderStyle.FixedSingle;
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            SizeF size = pe.Graphics.MeasureString(fileName, Font);
            float startX = 0;
            float startY = Height - size.Height;
            base.OnPaint(pe);
            pe.Graphics.DrawString(fileName, Font, new SolidBrush(ForeColor), startX, startY);
        }
    }
}

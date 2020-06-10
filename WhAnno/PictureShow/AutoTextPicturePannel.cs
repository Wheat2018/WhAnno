using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.PictureShow
{
    class AutoTextPicturePannel:FlowLayoutPanel
    {
        public ArrayList textPics = new ArrayList();

        //Style
        public Font paintFileNameFont;
        public Font paintIndexFont;

        public AutoTextPicturePannel()
        {
            this.AutoScroll = true;
            this.paintFileNameFont = this.paintIndexFont = Font;
        }

    public void Add(TextPictureBox textPic)
        {
            textPics.Add(textPic);
            Controls.Add(textPic);
        }

        public void Add(string picFileDir)
        {
            Add(new TextPictureBox(picFileDir));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (int i = 0; i < textPics.Count; i++)
            {
                TextPictureBox textPic = textPics[i] as TextPictureBox;
                textPic.index = i;
                textPic.paintFileNameFont = paintFileNameFont;
                textPic.paintIndexFont = paintIndexFont;
                if(WrapContents)
                    textPic.Width = textPic.Height = Width - 25;
                else
                    textPic.Width = textPic.Height = Height - 25;
            }
            base.OnPaint(e);
        }
    }
}

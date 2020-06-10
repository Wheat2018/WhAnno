using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.PictureShow
{
    class AutoPicturePannel:FlowLayoutPanel
    {
        ArrayList textPics = new ArrayList();

        public AutoPicturePannel()
        {
            AutoScroll = true;
        }

        public void Add(TextPictureBox textPic)
        {
            textPics.Add(textPic);
            Controls.Add(textPic);
        }

        public void AddPicture(string picFileDir)
        {
            Add(new TextPictureBox(picFileDir));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach(TextPictureBox textPic in textPics)
            {
                if(WrapContents)
                    textPic.Width = textPic.Height = Width - 40;
                else
                    textPic.Width = textPic.Height = Height - 40;
            }
            base.OnPaint(e);
        }
    }
}

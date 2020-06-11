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
        TextPictureBox focusBox = null;

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
            textPic.index = textPics.Count;
            textPic.Click += SelectIndexChanged;
            textPics.Add(textPic);
            Controls.Add(textPic);
        }

        public void Add(string picFileDir)
        {
            Add(new TextPictureBox(picFileDir));
        }

        private void SelectIndexChanged(object sender, EventArgs e)
        {
            TextPictureBox nowFocusBox = sender as TextPictureBox;
            if(focusBox != null)
            {
                focusBox.BackColor = nowFocusBox.BackColor;
                focusBox.BorderStyle = nowFocusBox.BorderStyle;
            }
            nowFocusBox.BackColor = SystemColors.ActiveCaption;
            nowFocusBox.BorderStyle = BorderStyle.Fixed3D;
            focusBox = nowFocusBox;
            MessagePrint.PushMessage("status", "Selected: " + focusBox.fileName);
        }

        private void NextIndex()
        {
            SelectIndexChanged(textPics[(textPics.IndexOf(focusBox) + 1) % textPics.Count], null);
        }
        private void PrevIndex()
        {
            SelectIndexChanged(textPics[(textPics.Count + textPics.IndexOf(focusBox) - 1) % textPics.Count], null);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (int i = 0; i < textPics.Count; i++)
            {
                TextPictureBox textPic = textPics[i] as TextPictureBox;
                textPic.paintFileNameFont = paintFileNameFont;
                textPic.paintIndexFont = paintIndexFont;
                if(WrapContents)
                    textPic.Width = textPic.Height = Width - 25;
                else
                    textPic.Width = textPic.Height = Height - 25;
            }
            base.OnPaint(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {

            MessageBox.Show("AutoText" + e.ToString());
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Left:
                    NextIndex();
                    break;
                case Keys.Down:
                case Keys.Right:
                    PrevIndex();
                    break;
                default:
                    break;
            }
            base.OnKeyDown(e);
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.PictureShow
{
    class AutoTextPicturePannel:FlowLayoutPanel
    {
        public TextPictureBox focusBox = null;

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
            textPic.MouseDown += (sender, e) => OnMouseDown(ParentMouse.Get(sender, e));
            textPic.MouseMove += (sender, e) => OnMouseMove(ParentMouse.Get(sender, e));

            textPics.Add(textPic);
            Controls.Add(textPic);
        }

        public void Add(string picFileDir)
        {
            Add(new TextPictureBox(picFileDir));
        }

        private void SelectIndexChanged(object sender, EventArgs e)
        {
            if (e is MouseEventArgs && (e as MouseEventArgs).Button != MouseButtons.Left) return;

            TextPictureBox nowFocusBox = sender as TextPictureBox;
            if (focusBox != null)
            {
                focusBox.BackColor = nowFocusBox.BackColor;
                focusBox.BorderStyle = nowFocusBox.BorderStyle;
            }
            nowFocusBox.BackColor = SystemColors.ActiveCaption;
            nowFocusBox.BorderStyle = BorderStyle.Fixed3D;
            focusBox = nowFocusBox;
            ScrollControlIntoView(focusBox);
            MessagePrint.PushMessage("status", "选中: " + focusBox.fileName);
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.PushMessage("info", "鼠标: " + e.Location.ToString());
            bool sw_left = false, sw_right = false, sw_top = false, sw_bottom = false;
            switch (Dock)
            {
                case DockStyle.Top:
                    sw_bottom = true;
                    break;
                case DockStyle.Bottom:
                    sw_top = true;
                    break;
                case DockStyle.Left:
                    sw_right = true;
                    break;
                case DockStyle.Right:
                    sw_left = true;
                    break;
                case DockStyle.Fill:
                    break;
                case DockStyle.None:
                default:
                    sw_left = sw_right = sw_top = sw_bottom = true;
                    break;
            }
            base.OnMouseMove(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Down:
                case Keys.Left:
                    NextIndex();
                    break;
                case Keys.Up:
                case Keys.Right:
                    PrevIndex();
                    break;
                default:
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}

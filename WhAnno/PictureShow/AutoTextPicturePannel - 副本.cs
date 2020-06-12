using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.PictureShow
{
    class AutoTextPicturePannel : FlowLayoutPanel
    {
        //Properties
        /// <summary>
        /// 当前选中索引。
        /// </summary>
        public int Index { get => textPics.IndexOf(CurrentItem); }
        /// <summary>
        /// 当前选中项。
        /// </summary>
        public TextPictureBox CurrentItem { get; private set; } = null;

        //Event
        /// <summary>
        /// 选中项发生更改时触发的带数据事件。
        /// </summary>
        public event EventHandler<TextPictureBox> SelectedIndexChanged = new EventHandler<TextPictureBox>((sender, box) => { });
        
        //Style
        public Font paintFileNameFont;
        public Font paintIndexFont;

        private readonly ArrayList textPics = new ArrayList();
        public AutoTextPicturePannel()
        {
            this.AutoScroll = true;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.paintFileNameFont = this.paintIndexFont = Font;
        }

        /// <summary>
        /// 获取面板中指定索引的项
        /// </summary>
        /// <param name="index">索引值</param>
        /// <returns>返回项</returns>
        public TextPictureBox GetItem(int index) => textPics[index] as TextPictureBox;
        
        /// <summary>
        /// 添加项
        /// </summary>
        /// <param name="textPic">项</param>
        public void Add(TextPictureBox textPic)
        {
            textPic.index = textPics.Count;
            textPic.Click += Lambda.MouseLeft.Get((sender, e) => ChangeIndex(textPic));
            textPic.MouseDown += (sender, e) => OnMouseDown(ParentMouse.Get(sender, e));
            textPic.MouseMove += (sender, e) => OnMouseMove(ParentMouse.Get(sender, e));

            textPics.Add(textPic);
            Controls.Add(textPic);
        }

        /// <summary>
        /// 从文件添加项
        /// </summary>
        /// <param name="picFilePath">文件路径</param>
        public void Add(string picFilePath)
        {
            Add(new TextPictureBox(picFilePath));
        }

        private void ChangeIndex(TextPictureBox target)
        {
            if (CurrentItem != null)
            {
                CurrentItem.BackColor = target.BackColor;
                CurrentItem.BorderStyle = target.BorderStyle;
            }
            target.BackColor = SystemColors.ActiveCaption;
            target.BorderStyle = BorderStyle.Fixed3D;
            CurrentItem = target;
            ScrollControlIntoView(CurrentItem);
            SelectedIndexChanged(this, CurrentItem);
            MessagePrint.AddMessage("status", "选中: " + CurrentItem.fileName);
        }

        private void NextIndex()
        {
            ChangeIndex(textPics[(textPics.IndexOf(CurrentItem) + 1) % textPics.Count] as TextPictureBox);
        }
        private void PrevIndex()
        {
            ChangeIndex(textPics[(textPics.Count + textPics.IndexOf(CurrentItem) - 1) % textPics.Count] as TextPictureBox);
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
            MessagePrint.AddMessage("info", "鼠标: " + e.Location.ToString());
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

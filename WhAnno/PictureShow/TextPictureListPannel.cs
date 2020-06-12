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
    class TextPictureListPannel : ListPannel<TextPictureBox>
    {

        public TextPictureListPannel()
        {
            BorderStyle = BorderStyle.FixedSingle;
        }
        
        /// <summary>
        /// 从文件添加项
        /// </summary>
        /// <param name="picFilePath">文件路径</param>
        public void Add(string picFilePath)
        {
            Add(new TextPictureBox(picFilePath));
        }

        protected override void OnResize(EventArgs eventargs)
        {
            ForEachItem((item) =>
            {
                if (WrapContents)
                    item.Width = item.Height = Width - 25;
                else
                    item.Width = item.Height = Height - 25;
            });
            base.OnResize(eventargs);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            CurrentItem.BackColor = SystemColors.ActiveCaption;
            if (LastItem != default) 
                LastItem.BackColor = SystemColors.Control;
            MessagePrint.AddMessage("status", "选中: " + CurrentItem.fileName);
            base.OnSelectedIndexChanged(e);
        }

        protected override void OnItemAdded(TextPictureBox item)
        {
            item.index = Count;
            base.OnItemAdded(item);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.AddMessage("info", "鼠标: " + e.Location.ToString());
            MessagePrint.AddMessage("", "滚动条: " + AutoScrollPosition.ToString());
            base.OnMouseMove(e);
        }

    }
}

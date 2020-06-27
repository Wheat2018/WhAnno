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
using WhAnno.Utils;

namespace WhAnno.PictureShow
{
    class TextPictureListPannel : ListPannel<TextPictureBox>
    {

        public TextPictureListPannel()
        {
            AutoScroll = true;
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

        /// <summary>
        /// 异步从文件添加项
        /// </summary>
        /// <param name="picFilePath">文件路径</param>
        public async Task AddAsync(string picFilePath)
        {
            TextPictureBox textPictureBox = new TextPictureBox();
            Add(textPictureBox);
            await textPictureBox.SetPictureAsync(picFilePath);
        }

        /// <summary>
        /// 处理TextPictureBox特性：索引值
        /// </summary>
        /// <param name="item"></param>
        protected override void OnItemAdded(TextPictureBox item, EventArgs e)
        {
            item.Index = IndexOf(item);
            base.OnItemAdded(item, e);
        }

        /// <summary>
        /// 处理TextPictureBox特性：索引值
        /// </summary>
        /// <param name="item"></param>
        protected override void OnItemRemoved(TextPictureBox item, EventArgs e)
        {
            ForEachItem((_item) => _item.Index = IndexOf(_item));
            base.OnItemRemoved(item, e);
        }

        /// <summary>
        /// 选中项的视觉效果变更。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedIndexChanged(TextPictureBox item, EventArgs e)
        {
            if (LastItem != default)
            {
                LastItem.BackColor = CurrentItem.BackColor;
            }
            CurrentItem.BackColor = SystemColors.ActiveCaption;
            MessagePrint.Add("status", "选中: " + CurrentItem.FileName);
            base.OnSelectedIndexChanged(item, e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.Add("info", "鼠标: " + e.Location.ToString());
            base.OnMouseMove(e);
        }

    }
}

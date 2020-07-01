using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    class AnnoPictureListPannel : DynamicListPannel<AnnoPictureBox>
    {
        /// <summary>
        /// 提示文本。
        /// </summary>
        public ToolTip ToolTip { get; } = new ToolTip();

        /// <summary>
        /// 默认构造。设置滚动条和边框样式。
        /// </summary>
        public AnnoPictureListPannel()
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
            Add(new AnnoPictureBox(picFilePath));
        }

        /// <summary>
        /// 异步从文件添加项
        /// </summary>
        /// <param name="picFilePath">文件路径</param>
        /// <param name="completeCallBack">异步加载完成的回调委托</param>
        /// <remarks>图像I/O操作在异步线程上执行，完成时回调委托在主调方线程上排队执行。</remarks>
        public void AddAsync(string picFilePath, Action completeCallBack = null)
        {
            AnnoPictureBox annoPictureBox = new AnnoPictureBox();
            annoPictureBox.SetPictureAsync(picFilePath, completeCallBack);
            Add(annoPictureBox);
        }

        public void AddEmpty(string picFilePath)
        {
            Add(new AnnoPictureBox() { FilePath = picFilePath });
        }

        /// <summary>
        /// 处理<see cref="AnnoPictureBox.Index"/>特性
        /// </summary>
        /// <param name="item"></param>
        protected override void OnItemAdded(AnnoPictureBox item, EventArgs e)
        {
            item.Index = IndexOf(item);
            ToolTip.SetToolTip(item, Path.GetFileName(item.FilePath));
            base.OnItemAdded(item, e);
        }

        /// <summary>
        /// 处理<see cref="AnnoPictureBox.Index"/>特性
        /// </summary>
        /// <param name="item"></param>
        protected override void OnItemRemoved(AnnoPictureBox item, EventArgs e)
        {
            ForEachItem((_item) => _item.Index = IndexOf(_item));
            base.OnItemRemoved(item, e);
        }

        /// <summary>
        /// 选中项的视觉效果变更。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedIndexChanged(AnnoPictureBox item, EventArgs e)
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

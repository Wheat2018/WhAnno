using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        //Properties
        /// <summary>
        /// 获取或设置是否对<see cref="AnnoPictureBox"/>进行动态回收。
        /// </summary>
        /// <remarks>动态回收：在<see cref="AnnoPictureBox"/>远离绘图工作区时，将其图像资源释放。当其出现在绘图工作区时，<see cref="AnnoPictureBox"/>会自动从URL加载图像。</remarks>
        public bool IsDynamicDispose { get; set; } = false;

        /// <summary>
        /// 动态回收距离。
        /// </summary>
        /// <remarks>回收距离绘图工作区<see cref="DynamicDisposeDistance"/>项之前和<see cref="DynamicDisposeDistance"/>之后的项资源。</remarks>
        public int DynamicDisposeDistance { get; set; } = 5;

        /// <summary>
        /// 提示文本。
        /// </summary>
        public ToolTip ToolTip { get; } = new ToolTip();

        //Events
        /// <summary>
        /// 动态回收时发生。
        /// </summary>
        /// <remarks>动态回收：在<see cref="AnnoPictureBox"/>远离绘图工作区时，将其图像资源释放。当其出现在绘图工作区时，<see cref="AnnoPictureBox"/>会自动从URL加载图像。</remarks>
        public event CancelEventHandler DynamicDispose;

        //Methods
        /// <summary>
        /// 默认构造。设置滚动条和边框样式。
        /// </summary>
        public AnnoPictureListPannel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.AllPaintingInWmPaint, true);

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

        /// <summary>
        /// 引发适当的动态回收事件。
        /// </summary>
        /// <param name="e"></param>
        public void RaiseDynamicDispose(CancelEventArgs e) => OnDynamicDispose(e);


        //Override
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
        protected override void OnItemSelected(AnnoPictureBox item, EventArgs e)
        {
            item.BackColor = SystemColors.ActiveCaption;
            MessagePrint.Add("status", "选中: " + CurrentItem.FileName);
            base.OnItemSelected(item, e);
        }

        protected override void OnItemCanceled(AnnoPictureBox item, EventArgs e)
        {
            item.BackColor = SystemColors.Control;
            base.OnItemCanceled(item, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnDynamicDispose(new CancelEventArgs());
            base.OnPaint(e);
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.Add("info", "鼠标: " + e.Location.ToString());
            base.OnMouseMove(e);
        }

        //Overridable
        /// <summary>
        /// 引发<see cref="DynamicDispose"/>事件。
        /// </summary>
        protected virtual void OnDynamicDispose(CancelEventArgs e)
        {
            if (!e.Cancel && IsDynamicDispose)
            {
                if (InClientItemsRange.Item1 - DynamicDisposeDistance > 0)
                {
                    for (int i = 0; i < InClientItemsRange.Item1 - DynamicDisposeDistance; i++)
                    {
                        if (i == Index) continue;
                        GetItem(i).Image?.Dispose();
                        GetItem(i).Image = null;
                    }
                }
                if (InClientItemsRange.Item2 + DynamicDisposeDistance < Count - 1)
                {
                    for (int i = InClientItemsRange.Item2 + DynamicDisposeDistance + 1; i < Count; i++)
                    {
                        if (i == Index) continue;
                        GetItem(i).Image?.Dispose();
                        GetItem(i).Image = null;
                    }
                }
            }
            DynamicDispose?.Invoke(this, e);
        }

    }
}

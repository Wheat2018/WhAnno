﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.PictureShow
{
    public enum FlowMode
        {
            Horizon, Vertical
        }

    class ListPannel<ItemType> : FlowLayoutPanel where ItemType: Control
    {

        //Properties
        /// <summary>
        /// 获取项总数。
        /// </summary>
        public int Count { get => items.Count; }
        /// <summary>
        /// 分组数。
        /// </summary>
        public int Groups { get; set; } = 1;
        /// <summary>
        /// 获取当前选中索引。
        /// </summary>
        public int Index { get => IndexOf(CurrentItem); }
        /// <summary>
        /// 获取或设置绘制项的纵横比，默认为1.
        /// </summary>
        public float Aspect { get; set; } = 1;
        /// <summary>
        /// 获取当前选中项。
        /// </summary>
        public ItemType CurrentItem { get; private set; } = default;
        /// <summary>
        /// 获取上次选中项。
        /// </summary>
        public ItemType LastItem { get; private set; } = default;
        /// <summary>
        /// 获取或设置ListPanel的布局滚动方向
        /// </summary>
        public FlowMode FlowMode
        { 
            get => WrapContents == true ? FlowMode.Vertical : FlowMode.Horizon;
            set => WrapContents = (value == FlowMode.Vertical ? true : false);
        }

        /// <summary>
        /// 所有项
        /// </summary>
        protected readonly ArrayList items = new ArrayList();

        //Event
        public delegate void ItemEventHandle(object sender, ItemType item, EventArgs e);
        public delegate void ItemEventHandle<TEventArgs>(object sender, ItemType item, TEventArgs e);
        /// <summary>
        /// 更改选中项后发生。
        /// </summary>
        public event ItemEventHandle SelectedIndexChanged;
        /// <summary>
        /// 添加项后发生
        /// </summary>
        public event ItemEventHandle ItemAdded;
        /// <summary>
        /// 删除项后发生
        /// </summary>
        public event ItemEventHandle ItemRemoved;
        /// <summary>
        /// 单击项时发生
        /// </summary>
        public event ItemEventHandle ItemClick;
        /// <summary>
        /// 鼠标进入项时发生
        /// </summary>
        public event ItemEventHandle ItemMouseEnter;
        /// <summary>
        /// 鼠标离开项时发生
        /// </summary>
        public event ItemEventHandle ItemMouseLeave;
        /// <summary>
        /// 鼠标悬停项时发生
        /// </summary>
        public event ItemEventHandle ItemMouseHover;

        //Method
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="shareMouseEvent">指示ListPannel是否共享每个项的鼠标事件</param>
        public ListPannel(bool shareMouseEvent = true)
        {
            //绑定子控件触发ListPannel事件
            {
                void Click(object _sender, EventArgs _e) => OnItemClick(_sender as ItemType, _e);
                void MouseEnter(object _sender, EventArgs _e) => OnItemMouseEnter(_sender as ItemType, _e);
                void MouseHover(object _sender, EventArgs _e) => OnItemMouseHover(_sender as ItemType, _e);
                void MouseLeave(object _sender, EventArgs _e) => OnItemMouseLeave(_sender as ItemType, _e);
                ItemAdded += new ItemEventHandle((sender, item, e) =>
                {
                    item.Click += Click;
                    item.MouseEnter += MouseEnter;
                    item.MouseHover += MouseHover;
                    item.MouseLeave += MouseLeave;
                });
                ItemRemoved += new ItemEventHandle((sender, item, e) =>
                {
                    item.Click -= Click;
                    item.MouseEnter -= MouseEnter;
                    item.MouseHover -= MouseHover;
                    item.MouseLeave -= MouseLeave;
                });
            }

            //绑定子控件鼠标事件触发ListPannel鼠标事件
            if (shareMouseEvent)
            {
                void MouseClick(object _sender, MouseEventArgs _e) => OnMouseClick(ParentMouse.Get(this, _sender, _e));
                void MouseDoubleClick(object _sender, MouseEventArgs _e) => OnMouseDoubleClick(ParentMouse.Get(this, _sender, _e));
                void MouseDown(object _sender, MouseEventArgs _e) => OnMouseDown(ParentMouse.Get(this, _sender, _e));
                void MouseMove(object _sender, MouseEventArgs _e) => OnMouseMove(ParentMouse.Get(this, _sender, _e));
                void MouseUp(object _sender, MouseEventArgs _e) => OnMouseUp(ParentMouse.Get(this, _sender, _e));
                void MouseWheel(object _sender, MouseEventArgs _e) => OnMouseWheel(ParentMouse.Get(this, _sender, _e));

                void MouseCaptureChanged(object _sender, EventArgs _e) => OnMouseCaptureChanged(_e);
                void MouseEnter(object _sender, EventArgs _e) => OnMouseEnter(_e);
                void MouseHover(object _sender, EventArgs _e) => OnMouseHover(_e);
                void MouseLeave(object _sender, EventArgs _e) => OnMouseLeave(_e);
                ItemAdded += new ItemEventHandle((sender, item, e) =>
                {
                    item.MouseClick += MouseClick;
                    item.MouseDoubleClick += MouseDoubleClick;
                    item.MouseDown += MouseDown;
                    item.MouseMove += MouseMove;
                    item.MouseUp += MouseUp;
                    item.MouseWheel += MouseWheel;

                    item.MouseCaptureChanged += MouseCaptureChanged;
                    item.MouseEnter += MouseEnter;
                    item.MouseHover += MouseHover;
                    item.MouseLeave += MouseLeave;
                });
                ItemRemoved += new ItemEventHandle((sender, item, e) =>
                {
                    item.MouseClick -= MouseClick;
                    item.MouseDoubleClick -= MouseDoubleClick;
                    item.MouseDown -= MouseDown;
                    item.MouseMove -= MouseMove;
                    item.MouseUp -= MouseUp;
                    item.MouseWheel -= MouseWheel;

                    item.MouseCaptureChanged -= MouseCaptureChanged;
                    item.MouseEnter -= MouseEnter;
                    item.MouseHover -= MouseHover;
                    item.MouseLeave -= MouseLeave;
                });
            }
        }

        /// <summary>
        /// 获取面板中指定索引的项
        /// </summary>
        /// <param name="index">索引值</param>
        /// <returns>项</returns>
        public ItemType GetItem(int index) => items[index] as ItemType;

        /// <summary>
        /// 获取项的索引值
        /// </summary>
        /// <param name="item">项</param>
        /// <returns>索引值</returns>
        public int IndexOf(ItemType item) => items.IndexOf(item);

        /// <summary>
        /// 添加项。
        /// </summary>
        /// <param name="item">项</param>
        public void Add(ItemType item)
        {
            if (items.Contains(item)) return;

            items.Add(item);
            Controls.Add(item);

            OnItemAdded(item, new EventArgs());
        }

        /// <summary>
        /// 删除项。
        /// </summary>
        /// <param name="item"></param>
        public void Remove(ItemType item)
        {
            if (!items.Contains(item)) return;

            if (CurrentItem == item) CurrentItem = default;
            if (LastItem == item) LastItem = default;
            items.Remove(item);
            Controls.Remove(item);

            OnItemRemoved(item, new EventArgs());
        }

        /// <summary>
        /// 选中项。
        /// </summary>
        /// <param name="item"></param>
        public void Select(ItemType item)
        {
            if (CurrentItem == item || !items.Contains(item)) return;

            LastItem = CurrentItem;
            CurrentItem = item;
            OnSelectedIndexChanged(item, new EventArgs());
        }

        /// <summary>
        /// 清空所有项。
        /// </summary>
        public void Clear()
        {
            while (Count > 0) Remove(GetItem(0));
            GC.Collect();
        }

        /// <summary>
        /// 对每个项应用操作
        /// </summary>
        /// <param name="applyFunc">操作</param>
        public void ForEachItem(Action<ItemType> applyFunc)
        {
            foreach (ItemType item in items) applyFunc(item);
        }

        //Overridable
        /// <summary>
        /// 引发 ListPannel.SelectedIndexChanged 事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnSelectedIndexChanged(ItemType item, EventArgs e)
        {
            ScrollControlIntoView(item);
            SelectedIndexChanged?.Invoke(this, item, e);
        }

        /// <summary>
        /// 引发 ListPannel.ItemAdded 事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemAdded(ItemType item, EventArgs e) => ItemAdded?.Invoke(this, item, e);

        /// <summary>
        /// 引发 ListPannel.ItemRemoved 事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemRemoved(ItemType item, EventArgs e) => ItemRemoved?.Invoke(this, item, e);

        /// <summary>
        /// 引发 ListPannel.ItemClick 事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemClick(ItemType item, EventArgs e)
        {
            if(Judge.MouseEvent.Left(e)) Select(item);
            ItemClick?.Invoke(this, item, e);
        }

        /// <summary>
        /// 引发 ListPannel.ItemMouseEnter 事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemMouseEnter(ItemType item, EventArgs e) => ItemMouseEnter?.Invoke(this, item, e);

        /// <summary>
        /// 引发 ListPannel.ItemMouseLeave 事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemMouseLeave(ItemType item, EventArgs e) => ItemMouseLeave?.Invoke(this, item, e);

        /// <summary>
        /// 引发 ListPannel.ItemMouseHover 事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemMouseHover(ItemType item, EventArgs e) => ItemMouseHover?.Invoke(this, item, e);

        //Override
        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (Count > 0)
            {
                bool autoScrollFlag = AutoScroll;
                int hValue = 0, vValue = 0;
                if (autoScrollFlag)
                {
                    hValue = HorizontalScroll.Value;
                    vValue = VerticalScroll.Value;
                    AutoScroll = false;
                }
                ForEachItem((item) =>
                {
                    item.Width = EachBestDisplaySize.Width - item.Margin.Left - item.Margin.Right;
                    item.Height = EachBestDisplaySize.Height - item.Margin.Top - item.Margin.Bottom;
                });

                if (autoScrollFlag)
                {
                    AutoScroll = true;
                    HorizontalScroll.Value = hValue;
                    VerticalScroll.Value = vValue;
                }
            }
            base.OnLayout(levent);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            base.OnMouseDown(e);
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

        //Implement Details
        /// <summary>
        /// 获取当前项的布局范围，即每行最高有几项，每列最高有几项。
        /// </summary>
        protected virtual Size LayoutRange 
        { 
            get
            {
                if (FlowMode == FlowMode.Horizon)
                    return new Size((int)Math.Ceiling((double)Count / Groups), Groups);
                else
                    return new Size(Groups, (int)Math.Ceiling((double)Count / Groups));
            }
        }
        /// <summary>
        /// 获取最大工作区大小：包含滚动条的工作区大小。
        /// </summary>
        protected virtual Size MaxClientSize
        {
            get
            {
                return new Size(ClientSize.Width + (VScroll ? SystemScorllBar.VerticalWidth : 0),
                    ClientSize.Height + (HScroll ? SystemScorllBar.HorizonHeight : 0));
            }
        }
        /// <summary>
        /// 获取最小工作区大小：不包含滚动条的工作区大小。
        /// </summary>
        protected virtual Size MinClientSize
        {
            get
            {
                Size result = new Size(ClientSize.Width - (VScroll ? 0 : SystemScorllBar.VerticalWidth),
                    ClientSize.Height - (HScroll ? 0 : SystemScorllBar.HorizonHeight));
                result.Width = (result.Width < 1 ? 1 : result.Width);
                result.Height = (result.Height < 1 ? 1 : result.Height);
                return result;
            }
        }
        /// <summary>
        /// 获取最佳显示区大小
        /// </summary>
        protected virtual Size BestDisplaySize
        {
            get
            {
                SizeF displaySize = new SizeF(LayoutRange.Width, LayoutRange.Height * Aspect);
                if (FlowMode == FlowMode.Horizon)
                {
                    float height = displaySize.Height * MaxClientSize.Width / displaySize.Width;
                    if (height < MinClientSize.Height)
                        return new Size((int)(displaySize.Width * MinClientSize.Height / displaySize.Height), MinClientSize.Height);
                    else if (height > MaxClientSize.Height)
                        return new Size((int)(displaySize.Width * MaxClientSize.Height / displaySize.Height), MaxClientSize.Height);
                    else
                        return new Size(MaxClientSize.Width, (int)height);
                }
                else
                {
                    float width = displaySize.Width * MaxClientSize.Height / displaySize.Height;
                    if (width < MinClientSize.Width)
                        return new Size(MinClientSize.Width, (int)(displaySize.Height * MinClientSize.Width / displaySize.Width));
                    else if (width > MaxClientSize.Width)
                        return new Size(MaxClientSize.Width, (int)(displaySize.Height * MaxClientSize.Width / displaySize.Width));
                    else
                        return new Size((int)width, MaxClientSize.Height);
                }
            }
        }
        /// <summary>
        /// 获取对于每个项的最佳大小
        /// </summary>
        protected virtual Size EachBestDisplaySize
        {
            get
            {
                if (Count == 0) return BestDisplaySize;
                return new Size(BestDisplaySize.Width / LayoutRange.Width, BestDisplaySize.Height / LayoutRange.Height);
            }
        }

        //Private Implement Details
        private void NextIndex() => Select(GetItem((Index + 1) % Count));
        private void PrevIndex() => Select(GetItem((items.Count + Index - 1) % Count));

    }
}

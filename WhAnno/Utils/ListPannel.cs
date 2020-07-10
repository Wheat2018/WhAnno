using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.Utils
{
    /// <summary>
    /// <see cref="ListPannel{ItemType}"/>布局滚动方向。
    /// </summary>
    public enum FlowMode
    { 
        /// <summary>
        /// 水平滚动。
        /// </summary>
        Horizon,
        /// <summary>
        /// 垂直滚动。
        /// </summary>
        Vertical 
    }

    /// <summary>
    /// 具有自动排版功能的列表框。每一项必须继承自<see cref="Control"/>，以便列表框可以对齐进行排版。
    /// </summary>
    /// <typeparam name="ItemType">项类型：继承自<see cref="Control"/>的类型。</typeparam>
    class ListPannel<ItemType> : FlowLayoutPanel where ItemType : Control
    {
        private ItemType currentItem = null;

        //Properties
        /// <summary>
        /// 获取项总数。
        /// </summary>
        public int Count => Items.Count;
        /// <summary>
        /// 分组数。
        /// </summary>
        public int Groups { get; set; } = 1;
        /// <summary>
        /// 获取或设置当前选中索引，未选中项时返回-1。
        /// </summary>
        public int Index
        {
            get => IndexOf(CurrentItem);
            set
            {
                if (value >= 0 && value < Count)
                    Select(GetItem(value));
            }
        }
        /// <summary>
        /// 获取或设置绘制项的纵横比，默认为1.
        /// </summary>
        public float Aspect { get; set; } = 1;
        /// <summary>
        /// 获取或设置当前选中项，未选中项时为null。
        /// </summary>
        /// <value>获取的值或设置的值都应为为null或<see cref="Items"/>当中的项。设置为null取消当前选择项。</value>
        /// <remarks>选中非null项引发项的<see cref="ItemSelected"/>事件。上次选中非null，则引发上次项的<see cref="ItemCanceled"/>事件。</remarks>
        public ItemType CurrentItem
        { 
            get => currentItem;
            set
            {
                if (currentItem == value || (value != null && !Items.Contains(value))) return;

                ItemType lastItem = currentItem;
                currentItem = value;
                if (lastItem != null) OnItemCanceled(lastItem, new EventArgs());
                if (value != null) OnItemSelected(value, new EventArgs());
            }
        }
        /// <summary>
        /// 获取或设置ListPanel的布局滚动方向
        /// </summary>
        public FlowMode FlowMode
        {
            get => WrapContents == true ? FlowMode.Vertical : FlowMode.Horizon;
            set => WrapContents = value == FlowMode.Vertical;
        }
        /// <summary>
        /// 所有项。
        /// </summary>
        public ControlCollection Items => Controls;
        /// <summary>
        /// 选择项封送目标。
        /// </summary>
        public List<IItemAcceptable<ItemType>> Targets { get; } = new List<IItemAcceptable<ItemType>>();

        //Event
        public delegate void ItemEventHandle(object sender, ItemType item, EventArgs e);
        public delegate void ItemEventHandle<TEventArgs>(object sender, ItemType item, TEventArgs e);
        /// <summary>
        /// 选中项后发生。
        /// </summary>
        public event ItemEventHandle ItemSelected;
        /// <summary>
        /// 取消选中项后发生。
        /// </summary>
        public event ItemEventHandle ItemCanceled;
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
        /// 构造<see cref="ListPannel{ItemType}"/>。
        /// </summary>
        /// <param name="shareMouseEvent">指示ListPannel是否共享每个项的鼠标事件</param>
        public ListPannel(bool shareMouseEvent = true)
        {
            //获取焦点时的视觉边框
            {
                SetStyle(ControlStyles.ResizeRedraw, true);
                Paint += (sender, pe) =>
                {
                    if (Focused) pe.Graphics.DrawRectangle(new Pen(SystemColors.ActiveCaption, 4), ClientRectangle);
                };
                GotFocus += (sender, e) => Invalidate();
                LostFocus += (sender, e) => Invalidate();
            }

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
        /// 构造<see cref="ListPannel{ItemType}"/>并设置封送目标。
        /// </summary>
        /// <param name="target">封送目标</param>
        /// <param name="shareMouseEvent">指示ListPannel是否共享每个项的鼠标事件</param>
        public ListPannel(IItemAcceptable<ItemType> target, bool shareMouseEvent = true) : this(shareMouseEvent)
        {
            Targets.Add(target);
        }

        /// <summary>
        /// 构造<see cref="ListPannel{ItemType}"/>并设置封送目标组。
        /// </summary>
        /// <param name="targets">封送目标数组</param>
        /// <param name="shareMouseEvent">指示ListPannel是否共享每个项的鼠标事件</param>
        public ListPannel(IItemAcceptable<ItemType>[] targets, bool shareMouseEvent = true) : this(shareMouseEvent)
        {
            Targets.AddRange(targets);
        }

        /// <summary>
        /// 获取面板中指定索引的项
        /// </summary>
        /// <param name="index">索引值</param>
        /// <returns>项</returns>
        public ItemType GetItem(int index) => Items[index] as ItemType;

        /// <summary>
        /// 获取项的索引值
        /// </summary>
        /// <param name="item">项</param>
        /// <returns>索引值</returns>
        public int IndexOf(ItemType item) => Items.IndexOf(item);

        /// <summary>
        /// 添加项。
        /// </summary>
        /// <param name="item">项</param>
        public void Add(ItemType item)
        {
            if (item == null || Items.Contains(item)) return;

            Items.Add(item);

            OnItemAdded(item, new EventArgs());
        }

        /// <summary>
        /// 添加项数组。
        /// </summary>
        /// <param name="items">项数组</param>
        public void AddRange(ItemType[] items)
        {
            foreach (ItemType item in items)
                Add(item);
        }

        /// <summary>
        /// 删除项。
        /// </summary>
        /// <param name="item"></param>
        public void Remove(ItemType item)
        {
            if (!Items.Contains(item)) return;

            if (CurrentItem == item) CurrentItem = null;
            Items.Remove(item);

            OnItemRemoved(item, new EventArgs());
        }

        /// <summary>
        /// 选中项。
        /// </summary>
        /// <param name="item"></param>
        public void Select(ItemType item) => CurrentItem = item;

        /// <summary>
        /// 清空所有项。
        /// </summary>
        /// <param name="fastClear">是否快速清空，不触发<see cref="ItemRemoved"/>事件。通常在被清空项即将被回收时使用。</param>
        public void Clear(bool fastClear = false)
        {
            if (fastClear)
            {
                CurrentItem = null;
                CurrentItem = null;
                Items.Clear();
            }
            else
                while (Count > 0) Remove(GetItem(0));
            GC.Collect();
        }


        /// <summary>
        /// 对每个项应用操作
        /// </summary>
        /// <param name="applyFunc">操作</param>
        public void ForEachItem(Action<ItemType> applyFunc)
        {
            foreach (ItemType item in Items) applyFunc(item);
        }

        //Overridable
        /// <summary>
        /// 引发<see cref="ItemSelected"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemSelected(ItemType item, EventArgs e)
        {
            ScrollControlIntoView(item);
            Targets.ForEach((target) => target.Accept(item));
            ItemSelected?.Invoke(this, item, e);
        }

        /// <summary>
        /// 引发<see cref="ItemCanceled"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemCanceled(ItemType item, EventArgs e)
        {
            Targets.ForEach((target) => target.Cancel(item));
            ItemCanceled?.Invoke(this, item, e);
        }

        /// <summary>
        /// 引发<see cref="ItemAdded"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemAdded(ItemType item, EventArgs e) => ItemAdded?.Invoke(this, item, e);

        /// <summary>
        /// 引发<see cref="ItemRemoved"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemRemoved(ItemType item, EventArgs e) => ItemRemoved?.Invoke(this, item, e);

        /// <summary>
        /// 引发<see cref="ItemClick"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemClick(ItemType item, EventArgs e)
        {
            if (Judge.MouseEvent.Left(e)) Select(item);
            ItemClick?.Invoke(this, item, e);
        }

        /// <summary>
        /// 引发<see cref="ItemMouseEnter"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemMouseEnter(ItemType item, EventArgs e) => ItemMouseEnter?.Invoke(this, item, e);

        /// <summary>
        /// 引发<see cref="ItemMouseLeave"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemMouseLeave(ItemType item, EventArgs e) => ItemMouseLeave?.Invoke(this, item, e);

        /// <summary>
        /// 引发<see cref="ItemMouseHover"/>事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected virtual void OnItemMouseHover(ItemType item, EventArgs e) => ItemMouseHover?.Invoke(this, item, e);

        //Override
        protected override void OnLayout(LayoutEventArgs levent)
        {
            ForEachItem((item) =>
            {
                item.Width = EachBestDisplaySize.Width - item.Margin.Left - item.Margin.Right;
                item.Height = EachBestDisplaySize.Height - item.Margin.Top - item.Margin.Bottom;
            });
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
                case Keys.Right:
                    NextIndex();
                    break;
                case Keys.Up:
                case Keys.Left:
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
        /// <remarks>显示区是可滚动面板的理论虚拟面板大小。</remarks>
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
        /// <summary>
        /// 当前位于绘图工作区的项的索引范围。
        /// </summary>
        /// <value>(int, int)元组，第一个值为起始索引，第二个值为终止索引。若<see cref="Count"/>为0，则为(-1,-1)</value>
        protected virtual (int, int) InClientItemsRange
        {
            get
            {
                int first = -1, last = -1;
                ItemType item;
                for (int i = 0; i < Count; i++)
                {
                    item = GetItem(i);
                    first = i;
                    while (new Rectangle(item.Location, item.Size).IntersectsWith(ClientRectangle))
                    {
                        last = i++;
                        if (i >= Count) break;
                        item = GetItem(i);
                    }
                    if (last >= 0) break;
                }
                return (first, last);
            }
        }

        /// <summary>
        /// 选择下一项。
        /// </summary>
        /// <remarks>该方法在响应键盘方向键时被调用。</remarks>
        protected void NextIndex() => Index = Math.Min(Index + 1, Count - 1);
        /// <summary>
        /// 选择上一项。
        /// </summary>
        /// <remarks>该方法在响应键盘方向键时被调用。</remarks>
        protected void PrevIndex() => Index = Math.Max(Index - 1, 0);

    }
}

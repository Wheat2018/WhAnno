using System;
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

    class ListPannel<ItemType> : FlowLayoutPanel where ItemType: Control
    {
        public enum LayoutFlowMode
        {
            Horizon, Vertical
        }

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
        public LayoutFlowMode FlowMode
        { 
            get => WrapContents == true ? LayoutFlowMode.Vertical : LayoutFlowMode.Horizon;
            set => WrapContents = (value == LayoutFlowMode.Vertical ? true : false);
        }


        //Event
        /// <summary>
        /// 更改选中项后触发事件。
        /// </summary>
        public event EventHandler SelectedIndexChanged;
        /// <summary>
        /// 添加项后触发事件
        /// </summary>
        public event EventHandler<ItemType> ItemAdded;

        //Method
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
        /// 默认构造函数
        /// </summary>
        /// <param name="shareMouseEvent">指示ListPannel是否共享每个项的鼠标事件</param>
        public ListPannel(bool shareMouseEvent = true)
        {
            AutoScroll = true;

            if (shareMouseEvent)
            {
                ItemAdded += new EventHandler<ItemType>((sender, item) =>
                {
                    item.MouseClick += (_sender, _e) => OnMouseClick(ParentMouse.Get(_sender, _e));
                    item.MouseDoubleClick += (_sender, _e) => OnMouseDoubleClick(ParentMouse.Get(_sender, _e));
                    item.MouseDown += (_sender, _e) => OnMouseDown(ParentMouse.Get(_sender, _e));
                    item.MouseMove += (_sender, _e) => OnMouseMove(ParentMouse.Get(_sender, _e));
                    item.MouseUp += (_sender, _e) => OnMouseUp(ParentMouse.Get(_sender, _e));
                    item.MouseWheel += (_sender, _e) => OnMouseWheel(ParentMouse.Get(_sender, _e));
                    
                    item.MouseCaptureChanged += (_sender, _e) => OnMouseCaptureChanged(_e);
                    item.MouseEnter += (_sender, _e) => OnMouseEnter(_e);
                    item.MouseHover += (_sender, _e) => OnMouseHover(_e);
                    item.MouseLeave += (_sender, _e) => OnMouseLeave(_e);
                });
            }
        }

        /// <summary>
        /// 添加项
        /// </summary>
        /// <param name="item">项</param>
        public void Add(ItemType item)
        {
            if (items.Contains(item)) return;

            items.Add(item);
            Controls.Add(item);
            OnItemAdded(item);
        }

        public delegate void Apply(ItemType item);
        /// <summary>
        /// 对每个项应用操作
        /// </summary>
        /// <param name="applyFunc">操作</param>
        public void ForEachItem(Apply applyFunc)
        {
            foreach (ItemType item in items)
            {
                applyFunc(item);
            }
        }

        private void ChangeSelection(ItemType target)
        {
            LastItem = CurrentItem;
            CurrentItem = target;
            OnSelectedIndexChanged(new EventArgs());
        }

        private void NextIndex() => ChangeSelection(GetItem((Index + 1) % Count));
        private void PrevIndex() => ChangeSelection(GetItem((items.Count + Index - 1) % Count));

        /// <summary>
        /// 引发ListPannel.SelectedIndexChanged事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            ScrollControlIntoView(CurrentItem);
            SelectedIndexChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 引发ListPannel.ItemAdded事件
        /// </summary>
        /// <param name="item">添加的项</param>
        protected virtual void OnItemAdded(ItemType item)
        {
            item.Click += Lambda.MouseLeft.Get((_sender, _e) => ChangeSelection(item));
            ItemAdded?.Invoke(this, item);
        }

        protected override void OnResize(EventArgs e)
        {
            if (Count > 0)
            {
                Size eachSize = new Size(BestDisplaySize.Width / LayoutRange.Width, BestDisplaySize.Height / LayoutRange.Height);
                ForEachItem((item) =>
                {
                    item.Width = eachSize.Width - item.Margin.Left - item.Margin.Right;
                    item.Height = eachSize.Height - item.Margin.Top - item.Margin.Bottom;
                });
            }
            base.OnResize(e);
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

        /// <summary>
        /// 所有项
        /// </summary>
        protected readonly ArrayList items = new ArrayList();

        //Implement Details
        /// <summary>
        /// 获取当前项的布局范围，即每行最高有几项，每列最高有几项。
        /// </summary>
        private Size LayoutRange 
        { 
            get
            {
                if (FlowMode == LayoutFlowMode.Horizon)
                    return new Size((int)Math.Ceiling((double)Count / Groups), Groups);
                else
                    return new Size(Groups, (int)Math.Ceiling((double)Count / Groups));
            }
        }
        /// <summary>
        /// 获取最大工作区大小：包含滚动条的工作区大小。
        /// </summary>
        private Size MaxClientSize
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
        private Size MinClientSize
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
        private Size BestDisplaySize
        {
            get
            {
                SizeF displaySize = new SizeF(LayoutRange.Width, LayoutRange.Height * Aspect);
                if (FlowMode == LayoutFlowMode.Horizon)
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

    }
}

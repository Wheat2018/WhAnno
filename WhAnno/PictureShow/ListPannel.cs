using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.PictureShow
{
    class ListPannel<ItemType> : FlowLayoutPanel where ItemType: Control
    {
        //Properties
        /// <summary>
        /// 项总数
        /// </summary>
        public int Count { get => items.Count; }
        /// <summary>
        /// 当前选中索引。
        /// </summary>
        public int Index { get => IndexOf(CurrentItem); }
        /// <summary>
        /// 当前选中项。
        /// </summary>
        public ItemType CurrentItem { get; private set; } = default;
        /// <summary>
        /// 上次选中项。
        /// </summary>
        public ItemType LastItem { get; private set; } = default;

        //Event
        /// <summary>
        /// 更改选中项后触发事件。
        /// </summary>
        public event EventHandler SelectedIndexChanged;
        /// <summary>
        /// 添加项后触发事件
        /// </summary>
        public event EventHandler<ItemType> ItemAdded;

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

            SelectedIndexChanged = new EventHandler((sender, e) => { });
            ItemAdded = new EventHandler<ItemType>((sender, item) => { });

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
            SelectedIndexChanged(this, e);
        }

        /// <summary>
        /// 引发ListPannel.ItemAdded事件
        /// </summary>
        /// <param name="item">添加的项</param>
        protected virtual void OnItemAdded(ItemType item)
        {
            item.Click += Lambda.MouseLeft.Get((_sender, _e) => ChangeSelection(item));
            ItemAdded(this, item);
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

    }
}

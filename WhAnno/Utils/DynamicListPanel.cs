using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.Utils
{
    /// <summary>
    /// 对于加载项数目巨大时，<see cref="ListPanel{ItemType}"/>会耗费大量时间在添加操作上。<see cref="DynamicListPanel{ItemType}"/>动态进行添加操作，当实际需要显示后续项时，才添加后续项。
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    class DynamicListPanel<ItemType> : ListPanel<ItemType> where ItemType : Control
    {
        //Properties
        /// <summary>
        /// 获取或设置是否动态添加控件。
        /// </summary>
        /// <remarks>动态项：添加项时不立刻添加到面板，当控件位置接近显示区时实际添加。</remarks>
        public bool IsDynamicAdd { get; set; } = false;
        /// <summary>
        /// 动态加载数。
        /// </summary>
        /// <remarks>当<see cref="DynamicListPanel{ItemType}"/>触发动态添加时，加载项的数目。</remarks>
        public int DynamicNum { get; set; } = 10;
        /// <summary>
        /// 动态加载容忍项数。
        /// </summary>
        /// <remarks>在绘图工作区距离最后一个已添加项最多还有<see cref="DynamicTolerate"/>项的距离时，触发动态添加。</remarks>
        public int DynamicTolerate { get; set; } = 3;
        /// <summary>
        /// 获取包含动态项的项总数。
        /// </summary>
        /// <value><see cref="Items"/>与<see cref="DynamicItems"/>项总和。</value>
        public int AllCount => Items.Count + DynamicItems.Count;
        /// <summary>
        /// 动态项。
        /// </summary>
        /// <remarks>动态项：添加项时不立刻添加到面板，当控件位置接近显示区时实际添加。</remarks>
        public List<ItemType> DynamicItems { get; } = new List<ItemType>();

        //Events
        /// <summary>
        /// 动态添加时发生
        /// </summary>
        public event EventHandler DynamicAdd;

        //Rewrite
        /// <summary>
        /// 添加项。
        /// </summary>
        /// <param name="item">项</param>
        public new void Add(ItemType item)
        {
            if (Contains(item)) return;

            if (IsDynamicAdd)
            {
                DynamicItems.Add(item);
                OnDynamicAdd(EventArgs.Empty);
            }
            else base.Add(item);
        }

        /// <summary>
        /// 添加项数组。
        /// </summary>
        /// <param name="items">项数组</param>
        public new void AddRange(ItemType[] items)
        {
            if (IsDynamicAdd)
            {
                DynamicItems.AddRange(items);
                OnDynamicAdd(EventArgs.Empty);
            }
            else base.AddRange(items);
        }

        /// <summary>
        /// 删除项。
        /// </summary>
        /// <param name="item"></param>
        public new void Remove(ItemType item)
        {
            base.Remove(item);
            DynamicItems.Remove(item);
        }

        /// <summary>
        /// 清空所有项。
        /// </summary>
        /// <param name="fastClear">是否快速清空，不触发<see cref="ItemRemoved"/>事件。通常在被清空项即将被回收时使用。</param>
        public new void Clear(bool fastClear = false)
        {
            base.Clear(fastClear);
            DynamicItems.Clear();
            GC.Collect();
        }

        public new void ForEachItem(Action<ItemType> applyFunc)
        {
            base.ForEachItem(applyFunc);
            DynamicItems.ForEach(applyFunc);
        }

        //Methods
        /// <summary>
        /// 确定指定项是否已添加或待添加。
        /// </summary>
        /// <param name="item">项</param>
        /// <returns>true若<see cref="ListPanel{ItemType}.Items"/>或<see cref="DynamicItems"/>包含此项，否则为false。</returns>
        public bool Contains(ItemType item) => Items.Contains(item) || DynamicItems.Contains(item);

        /// <summary>
        /// 引发适当的动态添加事件。
        /// </summary>
        /// <param name="e"></param>
        public void RaiseDynamicAdd(EventArgs e) => OnDynamicAdd(e);

        //Override
        protected override void OnPaint(PaintEventArgs e)
        {
            OnDynamicAdd(EventArgs.Empty);
            base.OnPaint(e);
        }

        //Overridable
        /// <summary>
        /// 引发<see cref="DynamicAdd"/>事件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDynamicAdd(EventArgs e)
        {
            if (IsDynamicAdd && InClientItemsRange.Item2 >= Count - 1 - DynamicTolerate)
            {
                int count = Math.Min(DynamicNum, DynamicItems.Count);
                base.AddRange(DynamicItems.GetRange(0, count).ToArray());
                DynamicItems.RemoveRange(0, count);
            }
            DynamicAdd?.Invoke(this, e);
        }

    }
}

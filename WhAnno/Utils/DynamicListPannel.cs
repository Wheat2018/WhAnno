using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.Utils
{
    class DynamicListPannel<ItemType> : ListPannel<ItemType> where ItemType : Control
    {
        /// <summary>
        /// 获取或设置是否动态添加控件。
        /// </summary>
        /// <remarks>动态项：添加项时不立刻添加到面板，当控件位置接近显示区时实际添加。</remarks>
        public bool IsDynamicAdd { get; set; } = false;
        /// <summary>
        /// 动态加载数。
        /// </summary>
        /// <remarks>当<see cref="DynamicListPannel{ItemType}"/>触发动态添加时，加载项的数目。</remarks>
        public int DynamicNum { get; set; } = 10;
        /// <summary>
        /// 动态加载容忍项数。
        /// </summary>
        /// <remarks>在显示区距离最后一个已添加项还有<see cref="DynamicTolerate"/>项的距离时，触发动态添加。</remarks>
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
        public List<Control> DynamicItems { get; } = new List<Control>();


        /// <summary>
        /// 添加项。
        /// </summary>
        /// <param name="item">项</param>
        public new void Add(ItemType item)
        {
            if (Contains(item)) return;

            if (IsDynamicAdd) DynamicItems.Add(item);
            else base.Add(item);
        }

        /// <summary>
        /// 添加项数组。
        /// </summary>
        /// <param name="items">项数组</param>
        public new void AddRange(ItemType[] items)
        {
            foreach (ItemType item in items)
                Add(item);
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
        /// 确定指定项是否已添加或待添加。
        /// </summary>
        /// <param name="item">项</param>
        /// <returns>true若<see cref="ListPannel{ItemType}.Items"/>或<see cref="DynamicItems"/>包含此项，否则为false。</returns>
        public bool Contains(ItemType item) => Items.Contains(item) || DynamicItems.Contains(item);


        protected override void OnPaint(PaintEventArgs e)
        {
            if (Count > 0)
            {
                MessagePrint.Add("exception", ScrollToControl(Items[Count - 1]).ToString() + VerticalScroll.Value.ToString());
            }
            base.OnPaint(e);
        }
    }
}

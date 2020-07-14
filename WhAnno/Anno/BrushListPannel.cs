using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Utils;
using WhAnno.Anno.Base;
using WhAnno.PictureShow;

namespace WhAnno.Anno
{
    class BrushListPannel : ListPannel<BrushBase>
    {
        /// <summary>
        /// 禁用ListPanel的动态工作区大小功能
        /// </summary>
        protected override Size MinClientSize => MaxClientSize;
        /// <summary>
        /// 每个项之间的间隔（相对于每项最佳区域大小）的倒数，默认为1/8。
        /// </summary>
        public Padding ItemMargin { get; set; } = new Padding(8);

        public BrushListPannel()
        {
            BorderStyle = BorderStyle.FixedSingle;
        }

        /// <summary>
        /// 鼠标移过子控件的视觉效果
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected override void OnItemMouseEnter(BrushBase item, EventArgs e)
        {
            if (item != CurrentItem)
            {
                item.BackColor = SystemColors.ActiveBorder;
            }
            base.OnItemMouseEnter(item, e);
        }

        /// <summary>
        /// 鼠标移出子控件的视觉效果
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected override void OnItemMouseLeave(BrushBase item, EventArgs e)
        {
            if (item != CurrentItem)
            {
                item.BackColor = SystemColors.Control;
            }
            base.OnItemMouseLeave(item, e);
        }

        protected override void OnItemSelected(BrushBase item, EventArgs e)
        {
            item.BackColor = SystemColors.ActiveCaption;
            item.BorderStyle = BorderStyle.Fixed3D;
            GlobalMessage.Add("status", "选中: " + CurrentItem.GetType().Name);
            base.OnItemSelected(item, e);
        }

        protected override void OnItemCanceled(BrushBase item, EventArgs e)
        {
            item.BackColor = SystemColors.Control;
            item.BorderStyle = BorderStyle.None;
            base.OnItemCanceled(item, e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            ForEachItem((item) =>
            {
                item.Margin = new Padding(
                    EachBestDisplaySize.Width / ItemMargin.Left,
                    EachBestDisplaySize.Height / ItemMargin.Top,
                    EachBestDisplaySize.Width / ItemMargin.Right,
                    EachBestDisplaySize.Height / ItemMargin.Bottom);
            });
            base.OnLayout(levent);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            GlobalMessage.Add("info", "鼠标: " + e.Location.ToString());
            base.OnMouseMove(e);
        }

    }
}

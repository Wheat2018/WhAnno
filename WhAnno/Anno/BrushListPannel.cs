using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno.Base;
using WhAnno.PictureShow;

namespace WhAnno.Anno
{
    class BrushListPannel : ListPannel<BrushBase>
    {
        /// <summary>
        /// 禁用ListPanel的动态工作区大小功能
        /// </summary>
        protected override Size MinClientSize { get => MaxClientSize; }
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
            MessagePrint.Add("", "MouseEnter " + item.GetType().Name);
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
            MessagePrint.Add("", "MouseLeave " + item.GetType().Name);
            base.OnItemMouseLeave(item, e);
        }

        protected override void OnSelectedIndexChanged(BrushBase item, EventArgs e)
        {
            if (LastItem != default)
            {
                LastItem.BackColor = SystemColors.Control;
                LastItem.BorderStyle = BorderStyle.None;
            }
            CurrentItem.BackColor = SystemColors.ActiveCaption;
            CurrentItem.BorderStyle = BorderStyle.Fixed3D;
            MessagePrint.Add("status", "选中: " + CurrentItem.GetType().Name);
            base.OnSelectedIndexChanged(item, e);
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
            MessagePrint.Add("info", "鼠标: " + e.Location.ToString());
            base.OnMouseMove(e);
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno.Base;
using WhAnno.PictureShow;
using WhAnno.Utils;
using WhAnno.Utils.Expand;

namespace WhAnno.AnnoList
{
    class AnnoLinkListPanel : DynamicListPanel<AnnoLink>, IItemAcceptable<AnnoPictureBox>
    {

        public AnnoPictureBox AnnoPicture { get; private set; }

        public AnnoLinkListPanel()
        {
            IsDynamicAdd = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BorderStyle = BorderStyle.FixedSingle;
            AutoScroll = true;
        }

        public void FreshAnnoLinkItem()
        {
            //Items.Clear();
            //Items.AddRange(AnnoPicture.Annotations.ToStringArray());
            Clear(true);
            foreach (AnnotationBase annotation in AnnoPicture.Annotations)
            {
                Add(new AnnoLink(annotation));  
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            //GlobalMessage.Add(BestDisplaySize.ToString());
            base.OnLayout(levent);
        }

        /// <summary>
        /// 鼠标移过子控件的视觉效果
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected override void OnItemMouseEnter(AnnoLink item, EventArgs e)
        {
            if (item != CurrentItem)
            {
                item.BackColor = SystemColors.ActiveCaption;
            }
            base.OnItemMouseEnter(item, e);
        }

        /// <summary>
        /// 鼠标移出子控件的视觉效果
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected override void OnItemMouseLeave(AnnoLink item, EventArgs e)
        {
            if (item != CurrentItem)
            {
                item.BackColor = SystemColors.Control;
            }
            base.OnItemMouseLeave(item, e);
        }


        void IItemAcceptable<AnnoPictureBox>.Accept(object sender, AnnoPictureBox item)
        {
            AnnoPicture = item;
            FreshAnnoLinkItem();
        }

        void IItemAcceptable<AnnoPictureBox>.Cancel(object sender, AnnoPictureBox item)
        {
            if (AnnoPicture == item) AnnoPicture = null;
        }
    }
}

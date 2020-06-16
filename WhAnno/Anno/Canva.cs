using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.Anno
{
    class Canva : Panel
    {
        //Properties
        /// <summary>
        /// 显示的图像
        /// </summary>
        public Image Image 
        {
            get => PicBox.Image;
            set
            {
                OnImageChanging(new EventArgs());
                //此处必须创建image的副本。因为Image类会为Bitmap动图创建某种线程，
                //而多次传递Image实例，会导致动图绑定多个线程，造成卡顿和跳帧。
                PicBox.Image?.Dispose();
                PicBox.Image = value.Clone() as Image;
                ImageBounds = ImageZoomDefaultBounds;
                OnImageChanged(new EventArgs());
            }
        }
        /// <summary>
        /// 显示图像边界
        /// </summary>
        public Rectangle ImageBounds { get => PicBox.Bounds; set => PicBox.Bounds = value; }
        public SizeF ImageScale 
        { 
            get => new SizeF((float)ImageBounds.Width / Image.Width, (float)ImageBounds.Height / Image.Height);
            //set
            //{
            //    ImageBounds = new Rectangle(PicBox.Clien);
            //}
        }

        /// <summary>
        /// 图像控件
        /// </summary>
        protected PictureBox PicBox { get; set; } = new PictureBox();

        //Event
        /// <summary>
        /// 图像更改前触发事件
        /// </summary>
        public event EventHandler ImageChanging;
        /// <summary>
        /// 图像更改后触发事件
        /// </summary>
        public event EventHandler ImageChanged;

        public Canva()
        {
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(PicBox);
            PicBox.SizeMode = PictureBoxSizeMode.StretchImage;
            PicBox.MouseDown += (sender, e) => OnMouseDown(ParentMouse.Get(this, sender, e));
            PicBox.MouseMove += (sender, e) => OnMouseMove(ParentMouse.Get(this, sender, e));
            PicBox.MouseUp += (sender, e) => OnMouseUp(ParentMouse.Get(this, sender, e));
        }

        public void ResetImageBounds()
        {
            if (PicBox.Image == null) return;

            PicBox.Bounds = new Rectangle();
        }

        //Overridable
        /// <summary>
        /// 引发 Canva.ImageChanging 事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnImageChanging(EventArgs e)
        {
            ImageChanging?.Invoke(this, e);
        }
        /// <summary>
        /// 引发 Canva.ImageChanged 事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnImageChanged(EventArgs e)
        {
            ImageChanged?.Invoke(this, e);
        }

        //Override
        protected override void OnMouseDown(MouseEventArgs e)
        {
            MouseDownArgs = e;

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.AddMessage("info", "Canva鼠标：" + PicBox.PointToClient(PointToScreen(e.Location)).ToString() + PicBox.ClientRectangle.ToString());
            if (Judge.MouseEvent.Left(MouseDownArgs))
                MessagePrint.AddMessage("info", "Canva鼠标：" + e.Location.ToString());

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            MouseDownArgs = null;

            MessagePrint.AddMessage("", "MouseUp：" + e.Location.ToString());
            base.OnMouseUp(e);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            MessagePrint.AddMessage("exception", "Bound" + Size.ToString() + ImageZoomDefaultBounds.ToString());
            base.OnResize(eventargs);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            MessagePrint.AddMessage("", "滚轮" + e.Delta.ToString());
            base.OnMouseWheel(e);
        }

        //Implement Details
        /// <summary>
        /// 以Zoom形式填充的图像控件大小
        /// </summary>
        protected Size ImageZoomSize
        {
            get
            {
                if (Image == null) return new Size();

                if (Width * Image.Height < Height * Image.Width)
                    return new Size(Width, Image.Height * Width / Image.Width);
                else
                    return new Size(Image.Width * Height / Image.Height, Height);
            }
        }
        /// <summary>
        /// 以Zoom形式填充的图像控件默认边界框
        /// </summary>
        protected Rectangle ImageZoomDefaultBounds
        {
            get => new Rectangle(
                (Width - ImageZoomSize.Width) / 2,
                (Height - ImageZoomSize.Height) / 2,
                ImageZoomSize.Width,
                ImageZoomSize.Height
                );
        }

        //Private Implement Details
        /// <summary>
        /// 鼠标在当前控件按下，此属性记录该鼠标参数，并在鼠标松开后销毁参数
        /// </summary>
        private MouseEventArgs MouseDownArgs = null;
    }
}

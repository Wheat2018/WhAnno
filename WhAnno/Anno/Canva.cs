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
            get => PictureBox.Image;
            set
            {
                OnImageChanging(new EventArgs());
                //此处必须创建image的副本。因为Image类会为Bitmap动图创建某种线程，
                //而多次传递Image实例，会导致动图绑定多个线程，造成卡顿和跳帧。
                PictureBox.Image?.Dispose();
                PictureBox.Image = value.Clone() as Image;
                ImageBounds = ImageZoomDefaultBounds;
                OnImageChanged(new EventArgs());
            }
        }
        /// <summary>
        /// 显示图像边界，由 ImageLocation 和 ImageSize 组成。
        /// </summary>
        public Rectangle ImageBounds { get => PictureBox.Bounds; set => PictureBox.Bounds = value; }
        /// <summary>
        /// 显示图像位置
        /// </summary>
        public Point ImageLocation { get => PictureBox.Location; set => PictureBox.Location = value; }
        /// <summary>
        /// 显示图像大小。
        /// </summary>
        public Size ImageSize { get => PictureBox.Size; set => PictureBox.Size = value; }
        /// <summary>
        /// 显示图像相对原图的放缩比。依赖于 ImageSize 和 Image.Size，
        /// 若PicBox边框非None，比例会有误差。
        /// </summary>
        public SizeF ImageScale 
        {
            get => PreviewScaleFromSize(ImageSize);
            set
            {
                if (Image != null) ImageSize = PreviewSizeFromScale(value);
            }
        }

        /// <summary>
        /// 图像控件
        /// </summary>
        public PictureBox PictureBox { get; private set; } = new PictureBox();

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
            Controls.Add(PictureBox);
            PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            PictureBox.MouseDown += (sender, e) => OnMouseDown(ParentMouse.Get(this, sender, e));
            PictureBox.MouseMove += (sender, e) => OnMouseMove(ParentMouse.Get(this, sender, e));
            PictureBox.MouseUp += (sender, e) => OnMouseUp(ParentMouse.Get(this, sender, e));
        }

        public void ResetImageBounds()
        {
            if (PictureBox.Image == null) return;

            PictureBox.Bounds = new Rectangle();
        }

        //Overridable
        /// <summary>
        /// 引发 Canva.ImageChanging 事件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnImageChanging(EventArgs e)
        {
            ImageChanging?.Invoke(this, e);
        }
        /// <summary>
        /// 引发 Canva.ImageChanged 事件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnImageChanged(EventArgs e)
        {
            ImageChanged?.Invoke(this, e);
        }

        //Override
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.Add("info", "原始坐标：" + PointToRawImage(e.Location).ToString() + e.Location.ToString() + PointToCanvaClient(PointToRawImage(e.Location)).ToString());
            if (Judge.MouseEvent.Left(e))
                MessagePrint.Add("info", "Canva鼠标：" + e.Location.ToString() + ImageScale.ToString());

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            MessagePrint.Add("", "MouseUp：" + e.Location.ToString());
            base.OnMouseUp(e);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            MessagePrint.Add("info", "Bound" + Image?.Size.ToString() + PictureBox.ClientSize.ToString());
            base.OnResize(eventargs);
        }

        /// <summary>
        /// 图像缩放。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            PointF anchorPoint = PointToRawImageF(e.Location);

            SizeF scale = new SizeF(ImageScale.Width * (1.0f + e.Delta / 1200f), ImageScale.Height * (1.0f + e.Delta / 1200f));
            if (scale.Width > 0.1 && scale.Width < 20 &&
                scale.Height > 0.1 && scale.Height < 20) 
                ImageScale = scale;

            Point anchorPointToClient = PointFToCanvaClient(anchorPoint);
            ImageLocation = new Point(ImageLocation.X - (anchorPointToClient.X - e.X), ImageLocation.Y - (anchorPointToClient.Y - e.Y));
            MessagePrint.Add("", "放大" + ImageScale.ToString());
            base.OnMouseWheel(e);
        }

        //Implement Details
        /// <summary>
        /// 以Zoom形式填充的图像控件大小。
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
        /// 以Zoom形式填充的图像控件默认边界框。
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
        /// <summary>
        /// 预览按比例缩放后的图像大小。
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        protected Size PreviewSizeFromScale(SizeF scale)
        {
            if (Image == null)
                return new Size();
            return new Size((int)(Image.Width * scale.Width), (int)(Image.Height * scale.Height));
        }
        /// <summary>
        /// 预览目标大小相对原图像的放缩比。依赖于 ImageSize 和 Image.Size，
        /// 若PicBox边框非None，比例会有误差。
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        protected SizeF PreviewScaleFromSize(Size size)
        {
            if (Image == null)
                return new SizeF();
            return new SizeF((float)size.Width / Image.Width, (float)size.Height / Image.Height);
        }
        /// <summary>
        /// 将 Canva 上的坐标转换为原图像坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Point PointToRawImage(Point point)
        {
            PointF result = PointToRawImageF(point);
            return new Point((int)result.X, (int)result.Y);
        }
        /// <summary>
        /// 将 Canva 上的坐标转换为原图像坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected PointF PointToRawImageF(Point point)
        {
            if (Image == null) return new PointF();

            point = PictureBox.PointToClient(PointToScreen(point));
            return new PointF((float)Image.Width * point.X / PictureBox.ClientSize.Width, (float)Image.Height * point.Y / PictureBox.ClientSize.Height);
        }
        /// <summary>
        /// 将原图像坐标转换为 Canva 上的坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Point PointToCanvaClient(Point point)
        {
            return PointFToCanvaClient(point);
        }
        /// <summary>
        /// 将原图像坐标转换为 Canva 上的坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Point PointFToCanvaClient(PointF point)
        {
            if (Image == null) return new Point();

            point = new PointF(PictureBox.ClientSize.Width * point.X / Image.Width, PictureBox.ClientSize.Height * point.Y / Image.Height);
            Point result = new Point((int)point.X, (int)point.Y);
            return PointToClient(PictureBox.PointToScreen(result));
        }

    }
}

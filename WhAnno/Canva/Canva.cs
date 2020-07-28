using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Utils;
using WhAnno.Anno.Base;
using System.Drawing.Imaging;
using System.Threading;
using WhAnno.PictureShow;

namespace WhAnno.Anno
{
    class Canva : Panel, ICoorConverter, IItemAcceptable<BrushBase>, IItemAcceptable<AnnoPictureBox>
    {
        //Properties
        /// <summary>
        /// 与<see cref="Canva"/>绑定的<see cref="AnnoPictureBox"/>实例。
        /// </summary>
        public AnnoPictureBox AnnoPicture { get; private set; }

        public BrushBase AnnoBrush { get; private set; }

        /// <summary>
        /// 获取显示的图像。
        /// </summary>
        public Image Image
        {
            get
            {
                if (AnnoPicture == null) return null;
                
                if(AnnoPicture.Image == null) AnnoPicture.Load();
                return AnnoPicture.Image;
            }
        }

        /// <summary>
        /// 获取或设置显示图像边界，由<see cref="ImageLocation"/>和<see cref="ImageSize"/>组成。
        /// </summary>
        public Rectangle ImageBounds
        {
            get => new Rectangle(ImageLocation, ImageSize);
            set
            {
                ImageLocation = value.Location;
                ImageSize = value.Size;
            }
        }
        /// <summary>
        /// 获取或设置显示图像位置。
        /// </summary>
        public Point ImageLocation 
        { 
            get => imageLocation; 
            set
            {
                imageLocation = value;
                if (Image != null) Invalidate();
            }
        }
        /// <summary>
        /// 获取或设置显示图像大小。
        /// </summary>
        public Size ImageSize
        {
            get => imageSize;
            set
            {
                imageSize = value;
                if (Image != null) Invalidate();
            }
        }

        /// <summary>
        /// 显示图像相对原图的放缩比。依赖于<see cref="ImageSize"/>和<see cref="Image.Size"/>。
        /// </summary>
        public SizeF ImageScale
        {
            get => PreviewScaleFromSize(ImageSize);
            set => ImageSize = PreviewSizeFromScale(value);
        }
        
        //Fields
        private Point imageLocation = new Point();
        private Size imageSize = new Size();
        private MouseEventArgs imageTranslationTemp = null;

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
            SetStyle(ControlStyles.ResizeRedraw
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint
                     | ControlStyles.Selectable,
                true);
            BorderStyle = BorderStyle.FixedSingle;

            //获取焦点时的视觉边框
            ControlFocusStyle.SetFocusStyle(this);
        }

        public void ResetImageBounds() => ImageBounds = ImageZoomDefaultBounds;

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
            Invalidate();
            ImageChanged?.Invoke(this, e);
        }

        //Override
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image != null) pe.Graphics.DrawImage(Image, ImageBounds);

            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            AnnoPicture?.DrawAnnos(pe.Graphics, this);

            AnnoBrush?.DelegatePaint(this, pe, this);
            base.OnPaint(pe);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseWheel(this, e, this)) return;

            //图像缩放
            {
                PointF anchorPoint = PointToRawImageF(e.Location);

                float sca = ImageScale.Width * (1.0f + e.Delta / 1200f);
                SizeF scale = new SizeF(sca, sca);
                //if (scale.Width > 0.1 && scale.Width < 20 &&
                //    scale.Height > 0.1 && scale.Height < 20) 
                    ImageScale = scale;

                Point anchorPointToClient = PointFToCanvaClient(anchorPoint);
                ImageLocation = new Point(ImageLocation.X - (anchorPointToClient.X - e.X), ImageLocation.Y - (anchorPointToClient.Y - e.Y));
                GlobalMessage.Add("", "放大" + ImageScale.ToString());
            }
            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseDown(this, e, this)) return;

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseMove(this, e, this)) return;

            //图像平移
            {
                if (Utils.Judge.Mouse.Middle(e))
                {
                    Cursor = Cursors.SizeAll;
                    if (imageTranslationTemp != null)
                    {
                        Size delta = new Size(e.X - imageTranslationTemp.X, e.Y - imageTranslationTemp.Y);
                        ImageLocation = Point.Add(ImageLocation, delta);
                        GlobalMessage.Add("info", "Delta：" + delta.ToString());
                    }
                    imageTranslationTemp = e;
                }
                else
                {
                    Cursor = Cursors.Default;
                    imageTranslationTemp = null;
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseUp(this, e, this)) return;

            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseClick(this, e, this)) return;

            base.OnMouseClick(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseEnter(this, e, this)) return;

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseLeave(this, e, this)) return;

            base.OnMouseLeave(e);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateMouseHover(this, e, this)) return;

            base.OnMouseHover(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateClick(this, e, this)) return;

            base.OnClick(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            AnnoBrush?.DelegateKeyPress(this, e, this);

            base.OnKeyPress(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (AnnoBrush != null && !AnnoBrush.DelegateProcessCmdKey(this, ref msg, keyData, this)) return true;

            bool handled = false;
            switch (keyData)
            {
                case Keys.Escape:
                    GlobalMessage.Add("brush cancel", null);
                    handled = true;
                    break;
                case Keys.Enter:
                case Keys.Space:
                    AnnoPicture.AddAnnotation(AnnoBrush?.GenerateAnnotation());
                    AnnoBrush?.Init();
                    handled = true;
                    break;
            }
            if (handled) Invalidate();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        //Implement Details
        /// <summary>
        /// 获取以<see cref="PictureBoxSizeMode.Zoom"/>形式填充的图像控件大小。
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
        /// 获取以<see cref="PictureBoxSizeMode.Zoom"/>形式填充的图像控件默认边界框。
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
            Size result = new Size((int)(Image.Width * scale.Width), (int)(Image.Height * scale.Height));
            if (result.Width < 1) result.Width = 1;
            if (result.Height < 1) result.Height = 1;
            return result;
        }
        /// <summary>
        /// 预览目标大小相对原图像的放缩比。依赖于<see cref="ImageSize"/>和<see cref="Image.Size"/>。
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
        /// 将<see cref="Canva"/>上的坐标转换为原图像坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Point PointToRawImage(Point point)
        {
            PointF result = PointToRawImageF(point);
            return new Point((int)(result.X + 0.5), (int)(result.Y + 0.5));
        }
        /// <summary>
        /// 将<see cref="Canva"/>上的坐标转换为原图像坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected PointF PointToRawImageF(Point point)
        {
            if (Image == null) return new PointF();

            point = new Point(point.X - ImageLocation.X, point.Y - ImageLocation.Y);
            return new PointF((float)Image.Width * point.X / ImageSize.Width, (float)Image.Height * point.Y / ImageSize.Height);
        }
        /// <summary>
        /// 将原图像坐标转换为<see cref="Canva"/>上的坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Point PointToCanvaClient(Point point)
        {
            return PointFToCanvaClient(point);
        }
        /// <summary>
        /// 将原图像坐标转换为<see cref="Canva"/>上的坐标。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected Point PointFToCanvaClient(PointF point)
        {
            if (Image == null) return new Point();

            point = new PointF(ImageSize.Width * point.X / Image.Width, ImageSize.Height * point.Y / Image.Height);
            Point result = new Point((int)(point.X + 0.5), (int)(point.Y + 0.5));
            return new Point(result.X + ImageLocation.X, result.Y + ImageLocation.Y);
        }
        /// <summary>
        /// 与<see cref="AnnoPicture"/>图像同步刷新。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnnoPicturePaintSync(object sender, PaintEventArgs e) => Invalidate();

        //Interface Implement
        Point ICoorConverter.Convert(Point point) => PointToCanvaClient(point);

        Point ICoorConverter.ReConvert(Point point) => PointToRawImage(point);

        void IItemAcceptable<BrushBase>.Accept(object sender, BrushBase item)
        {
            AnnoBrush = item;
            Invalidate();
        }

        void IItemAcceptable<BrushBase>.Cancel(object sender, BrushBase item)
        {
            if (AnnoBrush == item) AnnoBrush = null;
        }

        void IItemAcceptable<AnnoPictureBox>.Accept(object sender, AnnoPictureBox item)
        {
            OnImageChanging(EventArgs.Empty);

            AnnoPicture = item;
            item.Paint += AnnoPicturePaintSync;
            ResetImageBounds();
            AnnoBrush?.Init();

            OnImageChanged(EventArgs.Empty);
        }

        void IItemAcceptable<AnnoPictureBox>.Cancel(object sender, AnnoPictureBox item)
        {
            OnImageChanging(EventArgs.Empty);

            item.Paint -= AnnoPicturePaintSync;
            if (AnnoPicture == item)
            {
                AnnoPicture.AddAnnotation(AnnoBrush?.GenerateAnnotation());
                AnnoBrush?.Init();
                AnnoPicture = null;
                Invalidate();
            }

            OnImageChanged(EventArgs.Empty);
        }

    }
}

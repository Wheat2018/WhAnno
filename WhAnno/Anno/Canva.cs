﻿using System;
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

namespace WhAnno.Anno
{
    class Canva : Panel, ICoorConverter
    {
        //Properties
        /// <summary>
        /// 显示的图像。
        /// </summary>
        public Image Image 
        {
            get => image;
            set
            {
                OnImageChanging(new EventArgs());
                image = value;
                ResetImageBounds();
                OnImageChanged(new EventArgs());
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
        private Image image = null;
        private Point imageLocation = new Point();
        private Size imageSize = new Size();

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
            SetStyle(
                ControlStyles.ResizeRedraw | 
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint, 
                true);
            BorderStyle = BorderStyle.FixedSingle;

            //图像平移
            MouseEventArgs mouseDownEventArgs = null;
            Point imageLocation = default;
            MouseDown += (sender, e) =>
            {
                mouseDownEventArgs = e;
                imageLocation = ImageLocation;
            };
            MouseMove += (sender, e) =>
            {
                if (Utils.Judge.MouseEvent.Middle(e))
                {
                    Cursor = Cursors.SizeAll;
                    Point delta = new Point(e.X - mouseDownEventArgs.X, e.Y - mouseDownEventArgs.Y);
                    ImageLocation = new Point(imageLocation.X + delta.X, imageLocation.Y + delta.Y);
                    MessagePrint.Add("info", "Delta：" + delta.ToString());
                }
                else
                    Cursor = Cursors.Default;

            };
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
            ImageChanged?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image != null) pe.Graphics.DrawImage(Image, ImageBounds);

            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            Rectangle rect = new Rectangle(
                ClientSize.Width / 6, ClientSize.Height / 4, ClientSize.Width * 2 / 3, ClientSize.Height / 2);
            pe.Graphics.DrawRectangle(new Pen(ForeColor, 2), rect);

            base.OnPaint(pe);
        }

        //Override
        /// <summary>
        /// 图像缩放。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            PointF anchorPoint = PointToRawImageF(e.Location);

            float sca = ImageScale.Width * (1.0f + e.Delta / 1200f);
            SizeF scale = new SizeF(sca, sca);
            //if (scale.Width > 0.1 && scale.Width < 20 &&
            //    scale.Height > 0.1 && scale.Height < 20) 
                ImageScale = scale;

            Point anchorPointToClient = PointFToCanvaClient(anchorPoint);
            ImageLocation = new Point(ImageLocation.X - (anchorPointToClient.X - e.X), ImageLocation.Y - (anchorPointToClient.Y - e.Y));
            MessagePrint.Add("", "放大" + ImageScale.ToString());
            base.OnMouseWheel(e);
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
            return new Point((int)result.X, (int)result.Y);
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
            Point result = new Point((int)point.X, (int)point.Y);
            return new Point(result.X + ImageLocation.X, result.Y + ImageLocation.Y);
        }

        //Interface Implement
        Point ICoorConverter.Convert(Point point) => PointToCanvaClient(point);
    }
}

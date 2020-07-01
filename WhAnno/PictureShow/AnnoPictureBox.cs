using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno.Base;
using WhAnno.Utils;

namespace WhAnno.PictureShow
{
    class AnnoPictureBox: PictureBox
    {
        //Properties
        /// <summary>
        /// 图片的文件名（不含后缀）。
        /// </summary>
        public string FileName => Path.GetFileNameWithoutExtension(FilePath);
        /// <summary>
        /// 图片的全名。
        /// </summary>
        public string FilePath { get => ImageLocation; set => ImageLocation = value; }
        /// <summary>
        /// 图片的标注。
        /// </summary>
        public List<object> Annotations { get; } = new List<object>();
        /// <summary>
        /// 要绘制的索引值。
        /// </summary>
        public int Index 
        { 
            get => index;
            set
            {
                index = value;
                //Index更改时触发重绘。
                Invalidate();
            }
        }
        private int index;

        //Style
        public Font paintFileNameFont;
        public Font paintIndexFont;

        public AnnoPictureBox()
        {
            SizeMode = PictureBoxSizeMode.Zoom;
            BorderStyle = BorderStyle.FixedSingle;
            paintFileNameFont = paintIndexFont = Font;
        }

        public AnnoPictureBox(string filePath) : this() => SetPicture(filePath);

        /// <summary>
        /// 加载图像。
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void SetPicture(string filePath)
        {
            FilePath = filePath;
            //Image更改会自动触发重绘
            Image = new Bitmap(filePath);
        }

        /// <summary>
        /// 异步加载图像。
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="completeCallBack">异步加载完成的回调委托</param>
        /// <remarks>图像I/O操作在异步线程上执行，完成时回调委托在主调方线程上排队执行。</remarks>
        public async void SetPictureAsync(string filePath, Action completeCallBack = null)
        {
            FilePath = filePath;
            //异步读取图像文件
            //Image更改会自动触发重绘
            Image = await Task.Run(Process.CatchAction(() =>
            {
                Bitmap bitmap = new Bitmap(filePath);
                return bitmap.GetThumbnailImage(Width, Height, null, IntPtr.Zero);
            }));
            completeCallBack?.Invoke();
        }

        //public bool CheckAnnotation(object annotation)
        //{
        //    if (!BrushBase.IsAnnoType(annotation.GetType())) return false;

        //}


        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            SizeF size = pe.Graphics.MeasureString(FileName, paintFileNameFont);
            float startX = 0;
            float startY = Height - size.Height;
            pe.Graphics.DrawString(FileName, paintFileNameFont, new SolidBrush(ForeColor), startX, startY);
            using(SolidBrush brush = new SolidBrush(Color.FromArgb(150,Color.Orange)))
            {
                pe.Graphics.FillRectangle(brush,
                    new RectangleF(new PointF(0, 0),
                    pe.Graphics.MeasureString(Index.ToString(), paintIndexFont)));
            }
            pe.Graphics.DrawString(Index.ToString(), paintIndexFont, new SolidBrush(ForeColor), 0, 0);
        }
    }
}

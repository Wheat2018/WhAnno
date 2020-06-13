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
        public PictureBox Pic { get; private set; } = new PictureBox();

        public Canva()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(Pic);
            Pic.SizeMode = PictureBoxSizeMode.Zoom;
            Pic.Dock = DockStyle.Fill;
            Pic.MouseDown += (sender, e) => OnMouseDown(ParentMouse.Get(sender, e));
            Pic.MouseMove += (sender, e) => OnMouseMove(ParentMouse.Get(sender, e));
        }

        public void SetImage(Image image)
        {
            //此处必须创建image的副本。因为Image类会为Bitmap动图创建某种线程，
            //而多次传递Image实例，会导致动图绑定多个线程，造成卡顿和跳帧。
            Pic.Image?.Dispose();
            Pic.Image = image.Clone() as Image;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.AddMessage("info", "Canva鼠标：" + e.Location.ToString());
            base.OnMouseMove(e);
        }
    }
}

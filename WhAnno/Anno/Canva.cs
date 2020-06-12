using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno.Anno
{
    class Canva:Panel
    {
        public Canva()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            MessagePrint.PushMessage("info", "Canva鼠标：" + e.Location.ToString());
            base.OnMouseMove(e);
        }
    }
}

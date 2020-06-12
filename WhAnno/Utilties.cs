using System.Drawing;
using System.Windows.Forms;

namespace WhAnno
{
    class ParentMouse
    {
        /// <summary>
        /// 获取相对父控件位置的鼠标事件
        /// </summary>
        /// <param name="sender">子控件</param>
        /// <param name="e">鼠标事件</param>
        /// <returns>由子空间在父控件的Location计算父控件鼠标事件</returns>
        public static MouseEventArgs Get(object sender, MouseEventArgs e)
        {
            Point loc = (sender as Control).Location;
            return new MouseEventArgs(e.Button, e.Clicks, e.X + loc.X, e.Y + loc.Y, e.Delta);
        }
    }

}

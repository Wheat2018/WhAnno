using System.Windows.Forms;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义重绘消息的委托处理方法，允许或希望控件<see cref="Control"/>调用接口中的方法代为处理重绘消息。
    /// </summary>
    public interface IPaintEventDelegable
    {
        /// <summary>
        /// 重绘事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">重绘事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        void DelegatePaint(object sender, PaintEventArgs e, ICoorConverter cvt = null);
    }
}

using System.Windows.Forms;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义按键消息的委托处理方法，允许或希望控件<see cref="Control"/>调用接口中的方法代为处理按键消息。
    /// </summary>
    public interface IKeyEventDelegable
    {
        /// <summary>
        /// 命令键按下事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="msg">Windows消息。</param>
        /// <param name="keyData">按键信息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateProcessCmdKey(object sender, ref Message msg, Keys keyData, ICoorConverter cvt = null);
        /// <summary>
        /// 键盘按键按下事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">按键事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        void DelegateKeyPress(object sender, KeyPressEventArgs e, ICoorConverter cvt = null);
    }
}

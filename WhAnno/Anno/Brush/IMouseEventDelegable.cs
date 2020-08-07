using System;
using System.Windows.Forms;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义鼠标消息的委托处理方法，允许或希望控件<see cref="Control"/>调用接口中的方法代为处理鼠标消息。
    /// </summary>
    /// <remarks>接口中方法的返回值中，true表示希望继续执行原先的事件，false表示希望屏蔽原先事件，<see cref="Control"/>在调用接口时应注意这一规则。</remarks>
    public interface IMouseEventDelegable
    {
        /// <summary>
        /// 鼠标按键按下事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">鼠标事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseDown(object sender, MouseEventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 鼠标移动事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">鼠标事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseMove(object sender, MouseEventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 鼠标按键释放事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">鼠标事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseUp(object sender, MouseEventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 鼠标点击事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">鼠标事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseClick(object sender, MouseEventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 鼠标滚动事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">鼠标事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseWheel(object sender, MouseEventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 鼠标进入控件区域事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseEnter(object sender, EventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 鼠标离开控件区域事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseLeave(object sender, EventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 鼠标悬停事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateMouseHover(object sender, EventArgs e, ICoorConverter cvt = null);
        /// <summary>
        /// 单击事件委托。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="e">事件消息。</param>
        /// <param name="cvt">坐标变换规则。</param>
        /// <returns>true表示希望继续执行原先的事件，false表示希望屏蔽原先事件。</returns>
        bool DelegateClick(object sender, EventArgs e, ICoorConverter cvt = null);

    }
}

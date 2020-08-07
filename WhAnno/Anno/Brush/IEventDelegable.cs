using System.Windows.Forms;

namespace WhAnno.Anno.Base
{
    /// <summary>
    /// 定义多种消息的委托处理方法，允许或希望控件<see cref="Control"/>调用接口中的方法代为处理消息。
    /// </summary>
    /// <remarks>接口中方法若以布尔值为返回值，则true表示希望继续执行原先的事件，false表示希望屏蔽原先事件，<see cref="Control"/>在调用接口时应注意这一规则。</remarks>
    public interface IEventDelegable : IMouseEventDelegable, IPaintEventDelegable, IKeyEventDelegable
    {

    }
}

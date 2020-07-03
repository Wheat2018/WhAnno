using System.Windows.Forms;

namespace WhAnno.Utils
{
    /// <summary>
    /// 允许<see cref="ListPannel{ItemType}"/>将每次的选择项封送到接口实例中
    /// </summary>
    /// <typeparam name="ItemType">项类型：继承自<see cref="Control"/>的类型。</typeparam>
    public interface IItemAcceptable<in ItemType> where ItemType : Control
    {
        /// <summary>
        /// 接收来自<see cref="ListPannel{ItemType}"/>的选择项更改。
        /// </summary>
        /// <param name="item">当前选择的项。</param>
        /// <remarks>随<see cref="ListPannel{ItemType}.ItemSelected"/>引发。</remarks>
        void Accept(ItemType item);

        /// <summary>
        /// 取消选择项。
        /// </summary>
        /// <param name="item">已被取消的项。</param>
        /// <remarks>随<see cref="ListPannel{ItemType}.ItemCanceled"/>引发。</remarks>
        void Cancel(ItemType item);
    }
}

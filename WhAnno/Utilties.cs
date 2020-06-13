using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace WhAnno
{
    class MessagePrint
    {
        struct Message
        {
            public string describe;
            public object data;
        }

        private static ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();
        private static Thread thread = null;
        private static AutoResetEvent thread_suspend = new AutoResetEvent(false);

        public delegate void DelegateSolveMethod(string describe, object data);
        public static event DelegateSolveMethod SolveMethods = new DelegateSolveMethod(DefaultSolveMethod);

        /// <summary>
        /// 将新的待打印消息添加到队列
        /// </summary>
        /// <param name="describe">消息归类</param>
        /// <param name="data">消息内容</param>
        public static void AddMessage(string describe, object data)
        {
            if (thread == null)
            {
                lock (thread_suspend)
                {
                    if (thread == null)
                    {
                        thread = new Thread(new ThreadStart(SolveMessage));
                        thread.Name = "MessagePrint Unique Thread";
                        thread.Priority = ThreadPriority.Lowest;
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
            }
            Message message = new Message();
            message.describe = describe;
            message.data = data;
            messages.Enqueue(message);
            thread_suspend.Set();
        }

        private static void DefaultSolveMethod(string describe, object data)
        {
            Console.WriteLine(describe + ":" + data.ToString());
        }

        private static void SolveMessage()
        {
            while (true)
            {
                if (messages.Count == 0) thread_suspend.WaitOne();
                Message message;
                if (messages.TryDequeue(out message))
                    SolveMethods?.Invoke(message.describe, message.data);
            }
        }
    }

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

    class SystemScorllBar
    {
        /// <summary>
        /// 系统垂直滚动条宽度
        /// </summary>
        public static int VerticalWidth { get => SystemInformation.VerticalScrollBarWidth; }
        /// <summary>
        /// 系统水平滚动条高度
        /// </summary>
        public static int HorizonHeight { get => SystemInformation.HorizontalScrollBarHeight; }
    }

    namespace Lambda
    {
        class MouseLeft
        {
            /// <summary>
            /// 重新生成鼠标事件，仅当鼠标为左击时才触发目标事件
            /// </summary>
            /// <param name="func">目标事件</param>
            /// <returns>重新生成的鼠标事件</returns>
            public static EventHandler Get(EventHandler func)
            {
                return new EventHandler((sender, e) =>
                {
                    if (e is MouseEventArgs && (e as MouseEventArgs).Button == MouseButtons.Left)
                        func(sender, e);
                });
            }

        }
    }
}

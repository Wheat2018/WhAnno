using System;
using System.Collections.Concurrent;
using System.Drawing;
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
        public static DelegateSolveMethod solveMethods = new DelegateSolveMethod(DefaultSolveMethod);

        public static void PushMessage(string describe, object data)
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
                    solveMethods(message.describe, message.data);
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

}

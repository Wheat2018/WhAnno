﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;
using System.Windows.Forms;

namespace WhAnno
{
    class MessagePrint
    {
        /// <summary>
        /// 消息类
        /// </summary>
        struct Message
        {
            public string describe;
            public object data;
            public Message(string describe, object data)
            {
                this.describe = describe;
                this.data = data;
            }
        }

        private static ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();
        private static Thread thread = null;
        private static AutoResetEvent thread_suspend = new AutoResetEvent(false);

        public delegate void MessageSolveMethod(string describe, object data);
        /// <summary>
        /// 消息处理方法
        /// </summary>
        public static event MessageSolveMethod SolveMessage;

        /// <summary>
        /// 将新的待打印消息添加到队列。
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
                        thread = new Thread(new ThreadStart(Solve));
                        thread.Name = "MessagePrint Unique Thread";
                        thread.Priority = ThreadPriority.Lowest;
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
            }
            messages.Enqueue(new Message(describe, data));
            thread_suspend.Set();
        }

        /// <summary>
        /// 引发 MessagePrint.SolveMessage 事件。
        /// </summary>
        /// <param name="describe">消息归类</param>
        /// <param name="data">消息内容</param>
        private static void OnSolveMessage(string describe, object data)
        {
            Console.WriteLine(describe + ":" + data.ToString());
            SolveMessage?.Invoke(describe, data);
        }

        /// <summary>
        /// 处理消息线程主函数。
        /// </summary>
        private static void Solve()
        {
            while (true)
            {
                Message message;
                if (messages.TryDequeue(out message))
                    OnSolveMessage(message.describe, message.data);
                else
                    thread_suspend.WaitOne();
            }
        }
    }

    class ParentMouse
    {
        /// <summary>
        /// 获取相对父控件位置的鼠标事件
        /// </summary>
        /// <param name="parent">父控件</param>
        /// <param name="children">子控件</param>
        /// <param name="e">鼠标事件</param>
        /// <returns>由子空间在父控件的Location计算父控件鼠标事件</returns>
        public static MouseEventArgs Get(object parent, object children, MouseEventArgs e)
        {
            Point loc = (parent as Control).PointToClient((children as Control).PointToScreen(e.Location));
            return new MouseEventArgs(e.Button, e.Clicks, loc.X, loc.Y, e.Delta);
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

    namespace Judge
    {
        class MouseEvent
        {
            /// <summary>
            /// 判断事件是否为鼠标左击事件
            /// </summary>
            /// <typeparam name="EventType"></typeparam>
            /// <param name="e"></param>
            /// <returns></returns>
            public static bool Left(EventArgs e)
            {
                return e is MouseEventArgs && (e as MouseEventArgs).Button == MouseButtons.Left;
            }
            /// <summary>
            /// 判断事件是否为鼠标右击事件
            /// </summary>
            /// <typeparam name="EventType"></typeparam>
            /// <param name="e"></param>
            /// <returns></returns>
            public static bool Right(EventArgs e)
            {
                return e is MouseEventArgs && (e as MouseEventArgs).Button == MouseButtons.Right;
            }
            /// <summary>
            /// 判断事件是否为鼠标中击事件
            /// </summary>
            /// <typeparam name="EventType"></typeparam>
            /// <param name="e"></param>
            /// <returns></returns>
            public static bool Middle(EventArgs e)
            {
                return e is MouseEventArgs && (e as MouseEventArgs).Button == MouseButtons.Middle;
            }
        }
    }
}

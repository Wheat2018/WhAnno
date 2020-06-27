using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// 提供一些实用的方法或控件。可以根据需求功能重新实现它们。
/// </summary>
namespace WhAnno.Utils
{
    public static class MessagePrint
    {
        /// <summary>
        /// 消息类
        /// </summary>
        private struct Message
        {
            public string describe;
            public object data;
            public Message(string describe, object data)
            {
                this.describe = describe;
                this.data = data;
            }
        }

        private static readonly ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();
        private static Thread thread = null;
        private static readonly AutoResetEvent thread_suspend = new AutoResetEvent(false);

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
        public static void Add(string describe, object data)
        {
            if (thread == null)
            {
                lock (thread_suspend)
                {
                    if (thread == null)
                    {
                        thread = new Thread(new ThreadStart(Solve))
                        {
                            Name = "MessagePrint Unique Thread",
                            Priority = ThreadPriority.Lowest,
                            IsBackground = true
                        };
                        thread.Start();
                    }
                }
            }

            messages.Enqueue(new Message(describe, data));
            thread_suspend.Set();
        }

        /// <summary>
        /// 使用主调方线程立即处理待打印消息。
        /// </summary>
        /// <param name="describe"></param>
        /// <param name="data"></param>
        public static void Apply(string describe, object data) => OnSolveMessage(describe, data);

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
                if (messages.TryDequeue(out Message message))
                    OnSolveMessage(message.describe, message.data);
                else
                    thread_suspend.WaitOne();
            }
        }
    }

    public static class AsyncProcess
    {
        /// <summary>
        /// 异步并发出新线程处理任务。
        /// </summary>
        /// <param name="action">要处理的任务</param>
        public async static void Now(Action action, Action callback = null)
        {
            try
            {

                await Task.Run(action);
                callback?.Invoke();
            }
            catch (Exception ex)
            {
                MessagePrint.Add("exception", ex.Message);
            }
        }
        /// <summary>
        /// 异步并发出新线程，线程在毫秒级延时后处理任务。
        /// </summary>
        /// <param name="millisecondsDelay">延时毫秒数</param>
        /// <param name="action">要处理的任务</param>
        public static void Delay(int millisecondsDelay, Action action, Action callback = null)
        {
            Now(() =>
            {
                Thread.Sleep(millisecondsDelay);
                action();
            },callback);
        }
    }

    /// <summary>
    /// 使用单个专有线程异步处理任务
    /// </summary>
    public class UniqueAsyncProcess : IDisposable
    {
        /// <summary>
        /// 用于UniqueAsyncProcess的处理任务提前终止委托，委托指示了任务何时应当
        /// 尽快终止。任务应当在会消耗大量时间的语句块中不断判断该委托返回值，并
        /// 在检测到返回为true时尽快结束任务。
        /// </summary>
        /// <param name="shouldBreak">是否需要尽快结束任务</param>
        public delegate bool ProcessAbort();

        //Methods
        /// <summary>
        /// 默认构造。
        /// </summary>
        public UniqueAsyncProcess()
        {
            thread = new Thread(new ThreadStart(Run))
            {
                Name = "AsyncProcess Unique Thread",
                Priority = ThreadPriority.Normal,
                IsBackground = true
            };
            thread.Start();
        }

        /// <summary>
        /// 请求上个任务尽快结束，并安排任务进行处理。若在下一次调用本方法前，
        /// 本次安排的任务还未得到执行，则本次任务会被取消。
        /// </summary>
        /// <param name="action">待处理任务</param>
        public void Add(Action<ProcessAbort> action)
        {
            RequireEnd();
            Interlocked.Exchange(ref nextHandle, action);
            thread_suspend.Set();
        }

        /// <summary>
        /// 请求上个任务尽快结束。
        /// </summary>
        public void RequireEnd()
        {
            lock (abortLock)
            {
                abort = true;
            }
        }

        /// <summary>
        /// 在执行完当前任务后结束线程，不再执行后续任务。
        /// </summary>
        public void End()
        {
            exit = true;
            RequireEnd();
            thread_suspend.Set();
            thread.Join();
        }

        //Implement Details
        private readonly Thread thread;
        private readonly AutoResetEvent thread_suspend = new AutoResetEvent(false);
        private bool exit = false;

        private bool abort = false;
        private object abortLock = new object();

        private Action<ProcessAbort> nextHandle = null;
        private bool disposedValue;

        /// <summary>
        /// 处理任务线程主函数。
        /// </summary>
        private void Run()
        {
            while (!exit)
            {
                if (nextHandle != null)
                {
                    lock (abortLock)
                    {
                        abort = false;
                    }
                    Action<ProcessAbort> nowHandle = nextHandle;
                    Interlocked.Exchange(ref nextHandle, null);
                    try
                    {
                        nowHandle.Invoke(() => abort);
                    }
                    catch (Exception ex)
                    {
                        MessagePrint.Add("exception", ex.Message);
                    }
                }
                else if (!exit)
                    thread_suspend.WaitOne();
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    thread_suspend.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                End();
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~UniqueAsyncProcess()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class ParentMouse
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

    public static class SystemScorllBar
    {
        /// <summary>
        /// 获取系统垂直滚动条宽度
        /// </summary>
        public static int VerticalWidth => SystemInformation.VerticalScrollBarWidth;

        /// <summary>
        /// 获取系统水平滚动条高度
        /// </summary>
        public static int HorizonHeight => SystemInformation.HorizontalScrollBarHeight;
    }

    namespace Judge
    {
        public static class MouseEvent
        {
            /// <summary>
            /// 判断事件是否为鼠标左击事件
            /// </summary>
            /// <typeparam name="EventType"></typeparam>
            /// <param name="e"></param>
            /// <returns></returns>
            public static bool Left(EventArgs e) => e is MouseEventArgs && (e as MouseEventArgs).Button == MouseButtons.Left;

            /// <summary>
            /// 判断事件是否为鼠标右击事件
            /// </summary>
            /// <typeparam name="EventType"></typeparam>
            /// <param name="e"></param>
            /// <returns></returns>
            public static bool Right(EventArgs e) => e is MouseEventArgs && (e as MouseEventArgs).Button == MouseButtons.Right;

            /// <summary>
            /// 判断事件是否为鼠标中击事件
            /// </summary>
            /// <typeparam name="EventType"></typeparam>
            /// <param name="e"></param>
            /// <returns></returns>
            public static bool Middle(EventArgs e) => e is MouseEventArgs && (e as MouseEventArgs).Button == MouseButtons.Middle;
        }
    }

    public enum ProgressBarColor { Green = 1, Red, Yellow }
    /// <summary>
    /// 拓展ProgressBar的SetColor方法
    /// </summary>
    public static class ModifyProgressBarColor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

        /// <summary>
        /// 设置进度条颜色。
        /// </summary>
        /// <param name="bar">进度条实例</param>
        /// <param name="color">进度条颜色</param>
        public static void SetColor(this ProgressBar bar, ProgressBarColor color)
        {
            SendMessage(bar.Handle, 1040, (IntPtr)color, IntPtr.Zero);
        }
    }

    /// <summary>
    /// 为string拓展一些方法
    /// </summary>
    public static class StringMethod
    {
        /// <summary>
        /// 获取目标索引的字符在整个字符串中位于第几行(Y)第几列(X)。
        /// </summary>
        /// <param name="str">字符串实例</param>
        /// <param name="index">目标索引</param>
        /// <returns></returns>
        public static Point CoorOf(this string str, int index)
        {
            int rows = 0, feedPoint = 0;
            for (int i = 0; i < index; i++)
            {
                if (str[i] == '\n')
                {
                    rows++;
                    feedPoint = i;
                }
            }
            return new Point(index - feedPoint, rows);
        }
    }

    /// <summary>
    /// 为GDI+的Rectangle矩形类拓展一种转换方式。
    /// </summary>
    public static class RectangleConverter
    {
        /// <summary>
        /// 将矩形按逆时针转换成四个点的数组
        /// </summary>
        /// <param name="rect">矩形实例</param>
        /// <returns></returns>
        public static Point[] ConvertToQuadraPoints(this Rectangle rect)
        {
            Point[] result = new Point[4];
            result[0] = new Point(rect.X, rect.Y);
            result[2] = new Point(rect.X + rect.Width, rect.Y + rect.Height);
            result[1] = new Point(result[2].X, result[0].Y);
            result[3] = new Point(result[0].X, result[2].Y);
            return result;
        }
    }
}

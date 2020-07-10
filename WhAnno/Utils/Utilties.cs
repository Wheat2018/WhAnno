using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
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
    /// <summary>
    /// 全局消息打印托管类。
    /// </summary>
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
        /// 将新的待打印消息添加到队列。
        /// </summary>
        /// <param name="data">消息内容</param>
        public static void Add(object data) => Add("", data);

        /// <summary>
        /// 使用主调方线程立即处理待打印消息。
        /// </summary>
        /// <param name="describe"></param>
        /// <param name="data"></param>
        public static void Apply(string describe, object data) => OnSolveMessage(describe, data);

        /// <summary>
        /// 使用主调方线程立即处理待打印消息。
        /// </summary>
        /// <param name="data"></param>
        public static void Apply(object data) => Apply("", data);

        /// <summary>
        /// 引发 MessagePrint.SolveMessage 事件。
        /// </summary>
        /// <param name="describe">消息归类</param>
        /// <param name="data">消息内容</param>
        private static void OnSolveMessage(string describe, object data)
        {
            Console.WriteLine(describe + ":" + data?.ToString());
            try
            {
                SolveMessage?.Invoke(describe, data);
            }
            catch (Exception ex)
            {
                Process.CatchExceptionHandle?.Invoke(ex);
            }
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

        /// <summary>
        /// 报告进度类。
        /// </summary>
        /// <typeparam name="ReportType">用于报告的进度类型</typeparam>
        /// <typeparam name="InterType">用于计算进度类型的中间类型</typeparam>
        public class Progress : IProgress<int>
        {
            //Properties
            /// <summary>
            /// 获取或设置当前值。
            /// </summary>
            public int Value { get => incValue; set => Interlocked.Exchange(ref incValue, value); }

            /// <summary>
            /// 获取或设置最大值。
            /// </summary>
            public int MaxValue { get; set; } = 100;

            /// <summary>
            /// 获取或设置最大进度。
            /// </summary>
            public int MaxProgress { get; set; } = 100;

            /// <summary>
            /// 获取或设置仅一次报告。
            /// </summary>
            /// <value>为true，仅当进度未报告过，才尝试报告。为false，每次调用Report都尝试报告。</value>
            public bool ReportOnce { get; set; } = true;

            /// <summary>
            /// 处理中报告格式串。
            /// </summary>
            /// <value>默认为"{0}%"，即进度为100打印"100%"</value>
            /// <remarks>格式串中，{0}替换为进度值，{1}替换为传入值, {2}替换为<see cref="MaxValue"/></remarks>
            public string ProgressingFormatString { get; set; } = "{0}%";

            /// <summary>
            /// 处理完毕报告字符串。
            /// </summary>
            public string ProgressedString { get; set; } = "就绪";

            /// <summary>
            /// 打印进度的回调委托。
            /// </summary>
            /// <remarks>默认为<see cref="Add(string, object)"/></remarks>
            public Action<string, object> Print { get; set; } = Add;

            //Fields
            private int lastProgress = -1;
            private int incValue = 0;

            //Methods
            /// <summary>
            /// 构造Progress并设置最大值。
            /// </summary>
            /// <param name="maxValue"><see cref="MaxValue"/></param>
            public Progress(int maxValue = 100) => MaxValue = maxValue;

            /// <summary>
            /// 自增计数器并报告。
            /// </summary>
            public void IncReport() => Report(Value + 1);


            //Interface Implements
            public void Report(int value)
            {
                Value = value;
                int progress = MaxValue == 0 ? MaxProgress : value * MaxProgress / MaxValue;
                if (progress == lastProgress) return;
                Interlocked.Exchange(ref lastProgress, progress);

                Print?.Invoke("progress", progress);
                if (progress < MaxProgress) Print?.Invoke("status", string.Format(ProgressingFormatString, progress, value, MaxValue));
                else Print?.Invoke("status", ProgressedString);
            }
        }
    }

    /// <summary>
    /// 提供一系列异步处理任务类和方法。
    /// </summary>
    public static class Process
    {
        /// <summary>
        /// 捕获异常时应用的默认异常处理方法。
        /// </summary>
        public static Action<Exception> CatchExceptionHandle { set; get; } = (ex) => { MessagePrint.Add("exception", ex.Message); };

        /// <summary>
        /// 生成异常安全（使用try-catch包围）的Action，并使用给定的方式处理异常。
        /// </summary>
        /// <param name="action">需要包围的委托</param>
        /// <param name="exception">异常处理方式</param>
        /// <returns></returns>
        public static Action CatchAction(Action action, ExceptionHandleEnum exception = ExceptionHandleEnum.Catch)
        {
            if (action == null) return null;
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    switch (exception)
                    {
                        case ExceptionHandleEnum.Throw:
                            throw;
                        case ExceptionHandleEnum.Catch:
                            CatchExceptionHandle?.Invoke(ex);
                            break;
                        case ExceptionHandleEnum.Ignore:
                            break;
                    }
                }
            };
        }
        /// <summary>
        /// 生成异常安全（使用try-catch包围）的带返回值Action，并使用给定的方式处理异常。
        /// </summary>
        /// <param name="action">需要包围的委托</param>
        /// <param name="exception">异常处理方式</param>
        /// <returns></returns>
        public static Func<T> CatchAction<T>(Func<T> action, ExceptionHandleEnum exception = ExceptionHandleEnum.Catch)
        {
            if (action == null) return null;
            return () =>
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    switch (exception)
                    {
                        case ExceptionHandleEnum.Throw:
                            throw;
                        case ExceptionHandleEnum.Catch:
                            CatchExceptionHandle?.Invoke(ex);
                            break;
                        case ExceptionHandleEnum.Ignore:
                            break;
                    }
                }
                return default;
            };
        }


        /// <summary>
        /// 处理任务提前终止委托，委托指示了任务何时应当尽快终止。任务应当在会消耗大量时间的语句块中不断判断该委托返回值，并在检测到返回为true时尽快结束任务。
        /// </summary>
        public delegate bool Abort();

        /// <summary>
        /// 任务取消异常。
        /// </summary>
        public class ProcessAbortException : Exception
        {
            public override string Message => "任务取消。";
        }

        /// <summary>
        /// 表示任务抛出异常时的处理方式，带有该类型参数的函数，应当是异常安全的。
        /// </summary>
        /// <remarks>
        /// 尽管可以捕获基于<see cref="Exception"/>的全部异常，但某些调试器仍然会对某些异常触发中断（详情参考各调试器异常规则）。因此调试时的try-catch并不能完全保证程序不会被中断。
        /// </remarks>
        public enum ExceptionHandleEnum
        {
            /// <summary>
            /// 将异常重新抛出。
            /// </summary>
            Throw = 0,
            /// <summary>
            /// 捕获异常，并调用<see cref="CatchExceptionHandle"/>处理异常内容。
            /// </summary>
            Catch = 1,
            /// <summary>
            /// 忽略异常。
            /// </summary>
            Ignore = 2
        };

        /// <summary>
        /// 异步并发多线程处理任务。
        /// </summary>
        public static class Async
        {
            /// <summary>
            /// 异步并发出新线程处理任务。
            /// </summary>
            /// <param name="action">要处理的任务</param>
            /// <param name="callback">处理完任务的回调方法</param>
            /// <param name="exception">异常处理方法</param>
            /// <remarks><paramref name="action"/>由异步线程执行，<paramref name="callback"/>由主调方线程排队执行。</remarks>
            public async static Task Now(Action action, Action callback = null, ExceptionHandleEnum exception = ExceptionHandleEnum.Catch)
            {
                await Task.Run(CatchAction(action, exception));
                CatchAction(callback, exception)?.Invoke();
            }

            /// <summary>
            /// 异步并发出新线程，线程在毫秒级延时后处理任务。
            /// </summary>
            /// <param name="millisecondsDelay">延时毫秒数</param>
            /// <param name="action">要处理的任务</param>
            /// <param name="callback">处理完任务的回调方法</param>
            /// <param name="exception">异常处理方法</param>
            /// <remarks>延时和<paramref name="action"/>由异步线程执行，<paramref name="callback"/>由主调方线程排队执行。</remarks>
            public static void Delay(int millisecondsDelay, Action action, Action callback = null, ExceptionHandleEnum exception = ExceptionHandleEnum.Catch)
            {
                _ = Now(() =>
                {
                    Thread.Sleep(millisecondsDelay);
                    action?.Invoke();
                }, callback, exception);
            }

            /// <summary>
            /// 毫秒级延时后由主调方线程排队处理任务。
            /// </summary>
            /// <param name="millisecondsDelay">延时毫秒数</param>
            /// <param name="action">要处理的任务</param>
            /// <param name="exception">异常处理方法</param>
            public async static void DelayInvoke(int millisecondsDelay, Action action, ExceptionHandleEnum exception = ExceptionHandleEnum.Catch)
            {
                await Now(() =>
                {
                    Thread.Sleep(millisecondsDelay);
                }, action, exception);
            }
        }

        /// <summary>
        /// 使用单个专有线程异步处理任务
        /// </summary>
        public class UniqueAsync : IDisposable
        {
            //Properties
            /// <summary>
            /// 获取或设置任务抛出异常时的处理方式。
            /// </summary>
            public ExceptionHandleEnum Exception { get; set; } = ExceptionHandleEnum.Catch;

            //Methods
            /// <summary>
            /// 默认构造。
            /// </summary>
            public UniqueAsync()
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
            public void Add(Action<Abort> action)
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
            private readonly object abortLock = new object();

            private Action<Abort> nextHandle = null;

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
                        Action<Abort> nowHandle = nextHandle;
                        Interlocked.Exchange(ref nextHandle, null);
                        try
                        {
                            nowHandle.Invoke(() => abort);
                        }
                        catch (Exception ex)
                        {
                            switch (Exception)
                            {
                                case ExceptionHandleEnum.Throw:
                                    throw;
                                case ExceptionHandleEnum.Catch:
                                    CatchExceptionHandle?.Invoke(ex);
                                    break;
                                case ExceptionHandleEnum.Ignore:
                                    break;
                            }
                        }
                    }
                    else if (!exit)
                        thread_suspend.WaitOne();
                }
            }

            public void Dispose() => End();
        }
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

    public static class ColorList
    {
        /// <summary>
        /// 获取线性等分的颜色表。
        /// </summary>
        /// <param name="n">数目</param>
        /// <returns>数量为<paramref name="n"/>的颜色表</returns>
        public static Color[] Linspace(int n)
        {
            return Linspace(Color.Red, Color.Cyan, n);
        }

        /// <summary>
        /// 获取线性等分的颜色表。
        /// </summary>
        /// <param name="color1">起始颜色</param>
        /// <param name="color2">终止颜色</param>
        /// <param name="n">数目</param>
        /// <returns>数量为<paramref name="n"/>的颜色表</returns>
        public static Color[] Linspace(Color color1, Color color2, int n)
        {
            Color[] result = new Color[n];
            int first = color1.ToArgb();
            int last = color2.ToArgb();
            for (int i = 0; i < n; i++)
            {
                result[i] = Color.FromArgb((int)(first + (i + 1) * (long)(first - last) / n));
            }

            return result;
        }

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

    /// <summary>
    /// 为某些类拓展一些方法。
    /// </summary>
    namespace Expend
    {
        /// <summary>
        /// <see cref="ProgressBar"/>颜色。
        /// </summary>
        public enum ProgressBarColor { Green = 1, Red, Yellow }
        /// <summary>
        /// 拓展<see cref="ProgressBar"/>的SetColor方法
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
        /// 为<see cref="string"/>拓展一些方法
        /// </summary>
        public static class StringMethods
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

            /// <summary>
            /// 检测目标字符串是否为该字符串的结尾子字符串。
            /// </summary>
            /// <param name="str">字符串实例</param>
            /// <param name="dst">目标字符串</param>
            /// <returns></returns>
            public static bool TailContains(this string str, string dst)
            {
                if (dst.Length > str.Length) return false;
                return str.IndexOf(dst) == str.Length - dst.Length;
            }

            /// <summary>
            /// 获取字符串中每一行。
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string[] GetLines(this string str)
            {
                List<string> result = new List<string>();
                int lastIndex = 0;
                do
                {
                    int index = str.IndexOf('\n', lastIndex);
                    if (index == -1)
                    {
                        if (lastIndex <= str.Length) result.Add(str.Substring(lastIndex, str.Length - lastIndex));
                        break;
                    }
                    result.Add(str.Substring(lastIndex, index - lastIndex));
                    lastIndex = index + 1;
                } while (lastIndex <= str.Length);
                return result.ToArray();
            }

            /// <summary>
            /// 返回表示每个对象的字符串数组。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="objs"></param>
            /// <returns></returns>
            public static string[] ToStringArray<T>(this T objs) where T : ICollection
            {
                string[] result = new string[objs.Count];
                int i = 0;
                foreach (object obj in objs)
                {
                    if (obj == null) result[i] = "";
                    else result[i] = obj.ToString();
                    i++;
                }
                return result;
            }
        }

        /// <summary>
        /// 为<see cref="Array"/>扩展一些方法
        /// </summary>
        public static class ArrayMethods
        {
            /// <summary>
            /// 获取子数组，其中每个对象都为浅拷贝。
            /// </summary>
            /// <param name="objs"></param>
            /// <param name="startIndex">起始索引值</param>
            /// <param name="length">子数组长度</param>
            /// <returns></returns>
            public static T[] SubArray<T>(this T[] objs, int startIndex, int length)
            {
                T[] result = new T[length];
                for (int i = 0; i < length; i++)
                    result[i] = objs[startIndex + i];
                return result;
            }

            /// <summary>
            /// 获取子数组，其中每个对象都为浅拷贝。
            /// </summary>
            /// <param name="objs"></param>
            /// <param name="length">子数组长度</param>
            /// <returns></returns>
            public static T[] SubArray<T>(this T[] objs, int length) => objs.SubArray(0, length);

            /// <summary>
            /// 在一个一维数组中以特定比对方法搜索指定对象，并返回其首个匹配项的索引。
            /// </summary>
            /// <typeparam name="T">数组元素的类型。</typeparam>
            /// <param name="array">要搜索的从零开始的一维数组。</param>
            /// <param name="value">要在 array 中查找的对象。</param>
            /// <param name="equals">比对两个对象是否相等的方法。</param>
            /// <returns>如果在整个 array 中找到 value 的第一个匹配项，则为该项的从零开始的索引；否则为 -1。</returns>
            /// <exception cref="ArgumentNullException">array 为 null。</exception>
            public static int IndexOf<T>(T[] array, T value, Func<T, T, bool> equals)
            {
                if (array is null) throw new ArgumentException("array 为 null。");
                for (int i = 0; i < array.Length; i++)
                    if (equals(array[i], value)) return i;
                return -1;
            }
        }

        /// <summary>
        /// 为<see cref="Rectangle"/>矩形类拓展一种转换方式。
        /// </summary>
        public static class RectangleTransform
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

            public static Rectangle FromTwoPoints(Point point1, Point point2)
            {
                return new Rectangle(
                    Math.Min(point1.X, point2.X),
                    Math.Min(point1.Y, point2.Y),
                    Math.Abs(point1.X - point2.X),
                    Math.Abs(point1.Y - point2.Y)
                    );
            }
        }

        /// <summary>
        /// 字段排序方式。
        /// </summary>
        public enum FieldsOrder 
        {
            /// <summary>
            /// 子类字段在前，基类字段在后。
            /// </summary>
            SubToBase = 0,
            /// <summary>
            /// 基类字段在前，子类字段在后。
            /// </summary>
            BaseToSub = 1
        }
        /// <summary>
        /// 为<see cref="Type.GetFields"/>方法添加一种重载，支持返回的字段数组按照指定顺序排序。
        /// </summary>
        public static class TypeGetFields
        {
            public static FieldInfo[] GetFields(this Type type) => type.GetFields(FieldsOrder.BaseToSub);

            /// <summary>
            /// 按照指定排序顺序获取当前<see cref="Type"/>的所有字段
            /// </summary>
            /// <param name="type"><see cref="Type"/>实例</param>
            /// <param name="order">排序方式</param>
            /// <returns></returns>
            public static FieldInfo[] GetFields(this Type type, FieldsOrder order)
            {
                if (order == FieldsOrder.SubToBase) return type.GetFields();

                List<Type> tree = new List<Type>();
                Type temp = type;
                while(temp != null)
                {
                    tree.Add(temp);
                    temp = temp.BaseType;
                }
                tree.Reverse();

                List<FieldInfo> result = new List<FieldInfo>();
                foreach (Type item in tree)
                {
                    foreach (FieldInfo field in type.GetFields())
                        if (field.DeclaringType == item)
                            result.Add(field);
                }
                return result.ToArray();
            }
        }
    }
}

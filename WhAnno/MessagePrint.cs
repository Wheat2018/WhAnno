using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            if(thread == null)
            {
                lock (thread_suspend)
                {
                    if(thread == null)
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
                else
                {
                    Message message;
                    if(messages.TryDequeue(out message))
                        solveMethods(message.describe, message.data);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno;
using WhAnno.Utils;
using WhAnno.Anno.Base;
using System.Collections;

namespace WhAnno
{
    public partial class AnnoLoaderForm : Form
    {
        public object[] Annotations { get; private set; }

        private readonly string AnnoFilePath;
        private string FileContext;

        private readonly BrushListPannel brushListPanel = new BrushListPannel();
        private readonly Process.UniqueAsync UniqueAsyncProcess = new Process.UniqueAsync();
        public AnnoLoaderForm(string annoFilePath)
        {
            AnnoFilePath = annoFilePath;

            DoubleBuffered = true;
            Controls.Add(brushListPanel);
            InitializeComponent();
        }

        private async void AnnoLoaderForm_Load(object sender, EventArgs e)
        {
            {
                toolStripProgressBar1.ProgressBar.SetColor(ProgressBarColor.Yellow);
                toolStripProgressBar1.Visible = false;
                //注册消息打印
                MessagePrint.SolveMessage += PrintStatus;
            }
            {
                textBox1.MouseUp += TextBox1_MouseUp; ;
            }
            {
                brushListPanel.Dock = DockStyle.Left;
                brushListPanel.Width = 30;
                foreach (Type item in BrushBase.GetBrushTypes())
                {
                    brushListPanel.Add(Assembly.GetExecutingAssembly().CreateInstance(item.FullName) as BrushBase);
                }

                brushListPanel.SelectedIndexChanged += (_sender, _item, _e) =>
                {
                    if (_item == null) return;
                    listView1.Columns.Clear();
                    listView1.Groups.Clear();
                    listView1.Items.Clear();
                    foreach (FieldInfo field in _item.AnnoType.GetFields())
                    {
                        listView1.Columns.Add($"{field.Name}<{field.FieldType.Name}>");
                    }
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    TextBox2_TextChanged(this, new EventArgs());
                };
                brushListPanel.Index = 0;
            }

            try
            {
                using (StreamReader sr = new StreamReader(AnnoFilePath))
                {
                    MessagePrint.Add("status", "正在读取文本...");
                    FileContext = await sr.ReadToEndAsync();
                    MessagePrint.Add("status", "正在加载文本...");
                    string display = FileContext.Substring(0, Math.Min(textBox1.MaxLength, FileContext.Length)).Replace("\n", "\r\n");
                    textBox1.Text = display;
                    if(FileContext.Length > textBox1.MaxLength)
                        MessagePrint.Add("status", $"就绪，显示文本前{ textBox1.MaxLength }个字符");
                    else
                        MessagePrint.Add("status", "就绪");
                }
            }
            catch (FileNotFoundException ex)
            {
                MessagePrint.Add("exception", ex.Message);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (statusStrip1 == null) return;

            const int interval = 5;
            textBox2.Location = new Point(
                brushListPanel.Location.X + brushListPanel.Width,
                textBox2.Location.Y
                );
            textBox3.Location = new Point(
                brushListPanel.Location.X + brushListPanel.Width,
                textBox2.Location.Y + textBox2.Height + interval
                );

            listView1.Location = new Point(
                brushListPanel.Location.X + brushListPanel.Width,
                textBox3.Location.Y + textBox3.Height + interval
                );
            listView1.Height = statusStrip1.Location.Y - listView1.Location.Y;


            listView1.Width = textBox1.Location.X - listView1.Location.X - interval;
            textBox3.Width = textBox1.Location.X - textBox3.Location.X - interval;
            textBox2.Width = textBox1.Location.X - textBox2.Location.X - interval;
        }

        private void TextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Point coor = textBox1.Text.CoorOf(textBox1.SelectionStart);
            string str = "行: " + coor.Y.ToString() + ", 列: " + coor.X.ToString();
            if (textBox1.SelectionLength > 0)
                str += ", 选中" + textBox1.SelectionLength.ToString() + "个字符";
            MessagePrint.Add("status", str);
        }

        protected override void OnClosed(EventArgs e)
        {
            //移除消息打印
            MessagePrint.SolveMessage -= PrintStatus;
            //结束UniqueAsyncProcess
            UniqueAsyncProcess.End();
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //询问是否返回数据
            switch (MessageBox.Show("是否应用标注？", Text, MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.Yes:
                    string[] patterns = textBox2.Text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    MatchCollection matches = Regex.Matches(FileContext, patterns[0]);
                    Annotations = GetAnnoFromRegex(brushListPanel.CurrentItem, matches.ToStringArray(),
                                                   patterns.SubArray(1, patterns.Length - 1),
                                                   progress: new MessagePrint.Progress() { Print = PrintStatus });
                    break;
                case DialogResult.No:
                    break;
            }
            base.OnClosing(e);
        }

        private void PrintStatus(string describe, object data)
        {
            //事件调用该函数，执行线程并不是创建状态栏控件的线程
            //需将打印任务交给创建状态栏的线程，否则可能出现异常
            Action action = new Action(() =>
            {
                try
                {
                    //if (!CanFocus) return;
                    switch (describe)
                    {
                        case "progress":
                            toolStripProgressBar1.Visible = true;
                            toolStripProgressBar1.Value = (int)data;
                            if (toolStripProgressBar1.Value == 100)
                            {
                                toolStripProgressBar1.ProgressBar.SetColor(ProgressBarColor.Green);
                                //延时一段时间后将进度条设为不可见。（存在情况：一段时间后可能进度条已被销毁。此时抛出异常，无需处理）
                                Process.Async.Delay(1000, () =>
                                {
                                    Invoke(new Action(() =>
                                    {
                                        toolStripProgressBar1.Visible = false;
                                        toolStripProgressBar1.ProgressBar.SetColor(ProgressBarColor.Yellow);
                                    }));
                                },exception: Process.ExceptionHandleEnum.Ignore);
                            }
                            break;
                        case "status":
                            toolStripStatusLabel1.Text = data as string;
                            break;
                        case "exception":
                            toolStripStatusLabel2.Text = data as string;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessagePrint.Add("exception", ex.Message);
                }
            });
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = "";
            listView1.Groups.Clear();
            listView1.Items.Clear();
            UniqueAsyncProcess.Add(CalRegex);
        }

        /// <summary>
        /// 计算正则表达式并写入textBox3中
        /// </summary>
        /// <param name="abort"></param>
        private void CalRegex(Process.Abort abort)
        {
            try
            {
                if (textBox2.Text.Length > 0)
                {
                    //显示的最大匹配数
                    const int max = 100;

                    //报告相关
                    MessagePrint.Add("status", "计算正则...");

                    string[] patterns = textBox2.Text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    MatchCollection matches = Regex.Matches(FileContext, patterns[0]);

                    //报告相关
                    if(matches.Count > max)
                        MessagePrint.Add("status", $"匹配项过多，加载前{max}项");

                    StringBuilder matchResult = new StringBuilder("");
                    int realCount = Math.Min(max, matches.Count);

                    //报告相关
                    MessagePrint.Progress progress = new MessagePrint.Progress(realCount)
                    {
                        ProgressedString = matches.Count > max ? 
                        $"就绪 {realCount}/{matches.Count}个匹配项" : 
                        $"就绪 {matches.Count}个匹配项"
                    };

                    ListViewGroup[] groups = new ListViewGroup[realCount];
                    for (int i = 0; i < realCount; i++)
                    {
                        if (abort != null && abort()) return;

                        matchResult.Append(matches[i].Value.Replace("\n",@"\n") + "\r\n");
                        object[] annos = GetAnnoFromRegex(brushListPanel.CurrentItem,
                                                          new string[1] { matches[i].Value },
                                                          patterns.SubArray(1, patterns.Length - 1),
                                                          abort);
                        if (annos == null) return;

                        ListViewGroup group = new ListViewGroup($"Match {i} ({annos.Length})");
                        foreach (object anno in annos)
                        {
                            object[] values = BrushBase.GetAnnoFielsValues(anno);
                            group.Items.Add(
                                new ListViewItem(values.ToStringArray())
                            );
                        }
                        groups[i] = group;

                        //报告相关
                        progress.Report(i + 1);
                    }

                    Invoke(new Action(() =>
                    {
                        textBox3.Text = matchResult.ToString().Substring(0, Math.Min(textBox3.MaxLength, matchResult.Length));
                        listView1.Groups.AddRange(groups);
                        foreach (ListViewGroup group in groups)
                        {
                            listView1.Items.AddRange(group.Items);
                        }
                    }));

                    GC.Collect();
                }
                else
                    MessagePrint.Add("status", "就绪");

                MessagePrint.Add("exception", "");
            }
            catch (Exception ex)
            {
                MessagePrint.Add("status", "正则计算错误");
                MessagePrint.Add("exception", ex.Message);
            }
        }

        /// <summary>
        /// 给定若干个字符串和若干个正则表达式，生成若干个标注实例。
        /// </summary>
        /// <param name="brush">用来确定标注类型的画笔类实例</param>
        /// <param name="inputs">给定字符串</param>
        /// <param name="patterns">正则表达式</param>
        /// <param name="abort">提前终止条件</param>
        /// <param name="progress">进度报告</param>
        /// <returns>返回若干个标注实例</returns>
        /// <remarks>
        /// <para>生成规则（以单个字符串为例）：</para>
        /// <para>1.按照标注类型的字段顺序，逐一和正则表达式相对应。字段数小于表达式数时，多余表达式忽略；字段数大于表达式数时，无表达式对应的字段始终为默认值。</para>
        /// <para>2.每个正则表达式多次提取，假设最终生成n个标注实例，则理论上每个正则都应匹配到1个或n个结果。</para>
        /// <para>3.若某个正则表达式匹配到1个结果，则n个标注实例的该字段都为该结果。</para>
        /// <para>4.若某个正则表达式匹配到1＜x＜n个结果，则前x个标注实例的该字段对应赋值，后(n-x)个标注实例该字段为空。</para>
        /// </remarks>
        private object[] GetAnnoFromRegex(BrushBase brush, string[] inputs, string[] patterns,
                                          Process.Abort abort = null, IProgress<int> progress = null)
        {
                if (brush == null) return null;

                //标注类型的所有字段
                FieldInfo[] fields = brush.AnnoType.GetFields();
                //所有有效的正则表达式产生的匹配结果（集合长度等同于有效字段个数）
                MatchCollection[] matches = new MatchCollection[Math.Min(fields.Length, patterns.Length)];

                ArrayList result = new ArrayList();

                foreach (string input in inputs)
                {
                    if (abort != null && abort()) return null;

                    //正则匹配
                    int maxLength = 0;
                    for (int i = 0; i < matches.Length; i++)
                    {
                        if (abort != null && abort()) return null;

                        matches[i] = Regex.Matches(input, patterns[i]);
                        if (matches[i].Count > maxLength) maxLength = matches[i].Count;
                    }


                    for (int i = 0; i < maxLength; i++)
                    {
                        if (abort != null && abort()) return null;

                        object[] values = new object[fields.Length];
                        for (int j = 0; j < values.Length; j++)
                        {
                            if (j >= matches.Length) continue;

                            if (i < matches[j].Count)
                                values[j] = matches[j][i].Value;
                            else if (matches[j].Count == 1)
                                values[j] = matches[j][0].Value;
                        }
                        result.Add(brush.CreatAnnotation(fields, values));
                    }

                    if (progress != null) progress.Report((Array.IndexOf(inputs, input) + 1) * 100 / inputs.Length);
                }

                return result.ToArray();
        }
    }


}

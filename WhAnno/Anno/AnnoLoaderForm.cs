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

namespace WhAnno
{
    public partial class AnnoLoaderForm : Form
    {
        private readonly string AnnoFilePath;
        private string FileContext;

        private readonly BrushListPannel brushListPanel = new BrushListPannel();

        private readonly UniqueAsyncProcess UniqueAsyncProcess = new UniqueAsyncProcess();
        public AnnoLoaderForm(string annoFilePath)
        {
            AnnoFilePath = annoFilePath;

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
            textBox3.Location = new Point(
                brushListPanel.Location.X + brushListPanel.Width,
                statusStrip1.Location.Y - textBox3.Height
                );
            textBox3.Width = textBox1.Location.X - textBox3.Location.X - 10;

            textBox2.Location = new Point(
                brushListPanel.Location.X + brushListPanel.Width,
                textBox2.Location.Y
                );
            textBox2.Height = textBox3.Location.Y - textBox2.Location.Y - 10;
            textBox2.Width = textBox1.Location.X - textBox2.Location.X - 10;
        }

        private void TextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Point coor = textBox1.Text.CoorOf(textBox1.SelectionStart);
            string str = "行: " + coor.Y.ToString() + ", 列: " + coor.X.ToString();
            if (textBox1.SelectionLength > 0)
                str += ", 选中" + textBox1.SelectionLength.ToString() + "个字符";
            MessagePrint.Add("status", str);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //移除消息打印
            MessagePrint.SolveMessage -= PrintStatus;
            //结束UniqueAsyncProcess
            UniqueAsyncProcess.End();
            base.OnClosing(e);
        }

        private void PrintStatus(string describe, object data)
        {
            //事件调用该函数，执行线程并不是创建状态栏控件的线程
            //需将打印任务交给创建状态栏的线程，否则可能出现异常
            BeginInvoke(new Action(() =>
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
                                //延时一段时间后将进度条设为不可见
                                AsyncProcess.Delay(1000, () =>
                                {
                                    Invoke(new Action(() =>
                                    {
                                        toolStripProgressBar1.Visible = false;
                                        toolStripProgressBar1.ProgressBar.SetColor(ProgressBarColor.Yellow);
                                    }));
                                });
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
            }));
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = "";
            UniqueAsyncProcess.Add(CalRegex);
        }

        /// <summary>
        /// 计算正则表达式并写入textBox3中
        /// </summary>
        /// <param name="abort"></param>
        private void CalRegex(UniqueAsyncProcess.ProcessAbort abort)
        {
            try
            {
                if (textBox2.Text.Length > 0)
                {
                    string[] patterns = textBox2.Text.Split("\r\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                    MessagePrint.Add("status", "计算正则...");
                    MatchCollection matches = Regex.Matches(FileContext, patterns[0]);
                    const int max = 10000;
                    if(matches.Count > max)
                        MessagePrint.Add("status", $"匹配项过多，加载前{max}项");
                    
                    StringBuilder matchResult = new StringBuilder("");
                    int realCount = Math.Min(max, matches.Count);
                    int progress = 0;
                    for (int i = 0; i < realCount; i++)
                    {
                        if (abort()) return;
                        matchResult.Append(matches[i].Value.Replace("\n",@"\n") + "\r\n");
                        FieldInfo[] fields = brushListPanel.CurrentItem.AnnoFields;
                        for (int j = 0; j < fields.Length; j++)
                        {
                            if (j + 1 < patterns.Length)
                            {
                                MatchCollection fieldMatches = Regex.Matches(matches[i].Value, patterns[j + 1]);
                                matchResult.Append($"[{fields[j].Name}<{fields[j].FieldType.Name}>({fieldMatches.Count})]");
                                for (int k = 0; k < fieldMatches.Count; k++)
                                {
                                    matchResult.Append(fieldMatches[k].Value.Replace("\n", @"\n"));
                                    if (k < fieldMatches.Count - 1) matchResult.Append(',');
                                }
                                matchResult.Append("\r\n");
                            }
                            else
                            {
                                matchResult.Append($"[{fields[j].Name}<{fields[j].FieldType.Name}>]\r\n");
                            }
                        }
                        if (realCount < 100 || (i + 1) % (realCount / 100) == 0)
                        {
                            progress = (int)((float)(i + 1) / realCount * 100);
                            MessagePrint.Add("progress", progress);
                        }
                    }
                    if (progress > 0 && progress < 100)
                        MessagePrint.Add("progress", 100);

                    MessagePrint.Add("status", "加载正则结果...");
                    Invoke(new Action(() =>
                    {
                        textBox3.Text = matchResult.ToString().Substring(0, Math.Min(textBox3.MaxLength, matchResult.Length));
                    }));

                    if (matches.Count > max)
                        MessagePrint.Add("status", $"就绪 {realCount}/{matches.Count}个匹配项");
                    else
                        MessagePrint.Add("status", $"就绪 {matches.Count}个匹配项");

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
    }
}

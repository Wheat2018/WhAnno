using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno
{
    public partial class AnnoLoaderForm : Form
    {
        private string annoFilePath;
        public AnnoLoaderForm(string annoFilePath)
        {
            this.annoFilePath = annoFilePath;
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
            try
            {
                using (StreamReader sr = new StreamReader(annoFilePath))
                {
                    MessagePrint.Add("status", "正在读取文本...");
                    string line = await sr.ReadToEndAsync();
                    MessagePrint.Add("status", "正在加载文本...");
                    string display = line.Substring(0, Math.Min(textBox1.MaxLength, line.Length)).Replace("\n", "\r\n");
                    textBox1.Text = display;
                    MessagePrint.Add("status", "就绪");
                }
            }
            catch (FileNotFoundException ex)
            {
                MessagePrint.Add("exception", ex.Message);
            }

            {
                textBox1.Click += (_sender, _e) =>
                {
                    int rows = 0, feedPoint = 0;
                    for (int i = 0; i < textBox1.SelectionStart; i++)
                    {
                        if (textBox1.Text[i] == '\n')
                        {
                            rows++;
                            feedPoint = i;
                        }
                    }
                    MessagePrint.Add("status", "行: " + rows.ToString() + ", 列: " + (textBox1.SelectionStart - feedPoint).ToString());
                };
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            //移除消息打印
            MessagePrint.SolveMessage -= PrintStatus;
            base.OnClosed(e);
        }

        private void PrintStatus(string describe, object data)
        {
            //事件调用该函数，执行线程并不是创建状态栏控件的线程
            //需将打印任务交给创建状态栏的线程，否则可能出现异常
            Invoke(new Action(() =>
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
                    }
                }
                catch (Exception ex)
                {
                    MessagePrint.Add("exception", ex.Message);
                }
            }));
        }

    }
}

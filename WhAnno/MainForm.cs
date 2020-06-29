﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno;
using WhAnno.Utils;
using WhAnno.Anno.Base;
using WhAnno.PictureShow;

namespace WhAnno
{
    public partial class MainForm : Form
    {
        string workspace;

        private readonly TextPictureListPannel textPicturePannel = new TextPictureListPannel();
        private readonly BrushListPannel brushListPanel = new BrushListPannel();
        private readonly Canva canva = new Canva();

        public MainForm()
        {
            Controls.Add(textPicturePannel);
            Controls.Add(canva);
            Controls.Add(brushListPanel);

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            {
                toolStripProgressBar1.ProgressBar.SetColor(ProgressBarColor.Yellow);
                toolStripProgressBar1.Visible = false;
                //注册消息打印
                MessagePrint.SolveMessage += PrintStatus;
            }
            {
                textPicturePannel.Dock = DockStyle.Right;
                textPicturePannel.Width = 200;
            }
            {
                brushListPanel.Dock = DockStyle.Right;
                brushListPanel.Width = 30;
                foreach (Type item in BrushBase.GetBrushTypes())
                {
                    brushListPanel.Add(Assembly.GetExecutingAssembly().CreateInstance(item.FullName) as BrushBase);
                }
            }
            {
                canva.Dock = DockStyle.Left;
                canva.Paint += (_sender, _pe) =>
                {
                    Anno.Brush.Rectangle.Annotation annotation = new Anno.Brush.Rectangle.Annotation();
                    annotation.x = 10;
                    annotation.y = 10;
                    annotation.width = 50;
                    annotation.height = 50;

                    if (brushListPanel.CurrentItem != null)
                    brushListPanel.CurrentItem?.PaintAnno(_pe.Graphics, annotation, canva);
                };
                textPicturePannel.SelectedIndexChanged += (_sender, _item, _e) => canva.Image = _item.Image;
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
            Action action = new Action(() =>
            {
                try
                {
                    if (!CanFocus) return;
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
                                }, exception: Process.ExceptionHandleEnum.Ignore);
                            }
                            break;
                        case "status":
                            toolStripStatusLabel1.Text = data as string;
                            break;
                        case "info":
                            toolStripStatusLabel2.Text = data as string;
                            break;
                        case "exception":
                            toolStripStatusLabel3.Text = data as string;
                            break;
                        default:
                            toolStripStatusLabel4.Text = data as string;
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

        private void 工作区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            worksapceFolderDialog.Description = "选择标注工作区，加载其中.png|.gif|.jpg|.bmp|.jpeg|.wmf图像文件";
            if (worksapceFolderDialog.ShowDialog() == DialogResult.OK)
            {
                MessagePrint.Add("status", "加载中");
                workspace = worksapceFolderDialog.SelectedPath;
                DirectoryInfo wkDir = new DirectoryInfo(workspace);
                List<FileInfo> files = new List<FileInfo>();
                foreach (FileInfo file in wkDir.GetFiles())
                {
                    switch (file.Extension.ToLower())
                    {
                        case ".png":
                        case ".gif":
                        case ".jpg":
                        case ".bmp":
                        case ".jpeg":
                        case ".wmf":
                            files.Add(file);
                            break;
                        default:
                            break;
                    }
                }

                textPicturePannel.Clear();

                //异步加载所有图像，并发送进度消息。
                MessagePrint.Progress progress = new MessagePrint.Progress(files.Count) { ProgressingFormatString = "已加载{0}%" };
                int count = 0;
                foreach (FileInfo file in files)
                {
                    //Invoke调用一个异步Lambda，将很快返回，且发出异步线程处理。
                    //因此，该循环短时间内并发出与文件数相同的异步线程数，进行
                    //包含IO操作的图像加载过程。
                    Invoke(new Action(async () =>
                    {
                        //等待图像加载完成。
                        await textPicturePannel.AddAsync(file.FullName);
                        //发送进度消息。
                        Interlocked.Increment(ref count);
                        progress.Report(count);
                    }));
                }

                textPicturePannel.ForEachItem((item) => item.paintIndexFont = new Font(item.paintIndexFont.FontFamily, 15));

            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            canva.Width = textPicturePannel.Location.X - canva.Location.X;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void 标注ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            annoFileDialog.Title = "载入标注文本文件";
            if (annoFileDialog.ShowDialog() == DialogResult.OK)
            {
                AnnoLoaderForm annoLoaderForm = new AnnoLoaderForm(annoFileDialog.FileName);
                annoLoaderForm.ShowDialog();
                annoLoaderForm.Dispose();
            }
        }

        private void TestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.ShowDialog();
        }
    }
}

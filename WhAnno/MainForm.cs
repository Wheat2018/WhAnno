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
using WhAnno.Utils.Expand;
using WhAnno.AnnoList;

namespace WhAnno
{
    public partial class MainForm : Form
    {
        string workspace;
        AnnotationBase[] Annotations { get; set; } = null;

        private readonly AnnoPictureListPanel annoPictureListPanel = new AnnoPictureListPanel();
        private readonly BrushListPanel brushListPanel = new BrushListPanel();
        private readonly AnnoLinkListPanel annoLinkListPanel = new AnnoLinkListPanel();
        private readonly Canva canva = new Canva();

        private readonly List<AnnoPictureBox> collect = new List<AnnoPictureBox>();
        public MainForm()
        {
            Controls.Add(annoPictureListPanel);
            Controls.Add(canva);
            Controls.Add(brushListPanel);
            Controls.Add(annoLinkListPanel);

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            {
                toolStripProgressBar1.ProgressBar.SetColor(ProgressBarColor.Yellow);
                toolStripProgressBar1.Visible = false;
                //注册消息打印
                GlobalMessage.Handlers += PrintStatus;
            }
            {
                annoLinkListPanel.Dock = DockStyle.Left;
                annoLinkListPanel.Aspect = 0;
            }
            {
                annoPictureListPanel.Dock = DockStyle.Right;
                annoPictureListPanel.Width = 200;
                annoPictureListPanel.Targets.Add(canva);
                annoPictureListPanel.Targets.Add(annoLinkListPanel);
            }
            {
                brushListPanel.Dock = DockStyle.Right;
                brushListPanel.Width = 30;
                foreach (Type item in BrushBase.GetBrushTypes())
                {
                    brushListPanel.Add(Assembly.GetExecutingAssembly().CreateInstance(item.FullName) as BrushBase);
                }
                brushListPanel.Targets.Add(canva);
                //注册取消画笔消息接收
                GlobalMessage.Handlers += BrushCancel;
            }
            {
                canva.Dock = DockStyle.Left;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            //移除消息打印
            GlobalMessage.Handlers -= PrintStatus;
            GlobalMessage.Handlers -= BrushCancel;
            base.OnClosed(e);
        }

        private void BrushCancel(string describe, object data)
        {
            if (describe == "brush cancel")
            {
                if (InvokeRequired) BeginInvoke(Process.CatchAction(() => brushListPanel.Cancel()));
                else brushListPanel.Cancel();
            }
        }

        private void PrintStatus(string describe, object data)
        {
            Action action = new Action(() =>
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
                            Process.Async.DelayInvoke(1000, () =>
                            {
                                if (toolStripProgressBar1.IsDisposed) return; //Fuck the debugger!
                                toolStripProgressBar1.Visible = false;
                                toolStripProgressBar1.ProgressBar.SetColor(ProgressBarColor.Yellow);
                            }, exception: Process.ExceptionHandleEnum.Ignore);
                        }
                        break;
                    case "status":
                        toolStripStatusLabel1.Text = data as string;
                        break;
                    case "status delay":
                        toolStripStatusLabel1.Text = data as string;
                        //延时一段时间后清空文本。（存在情况：一段时间后可能文本框已被销毁。此时抛出异常，无需处理）
                        Process.Async.DelayInvoke(1000, () => toolStripStatusLabel1.Text = "",
                                                  exception: Process.ExceptionHandleEnum.Ignore);
                        break;
                    case "info":
                        toolStripStatusLabel2.Text = data as string;
                        break;
                    case "info delay":
                        //延时一段时间后清空文本。（存在情况：一段时间后可能文本框已被销毁。此时抛出异常，无需处理）
                        toolStripStatusLabel2.Text = data as string;
                        Process.Async.DelayInvoke(1000, () => toolStripStatusLabel2.Text = "",
                                                  exception: Process.ExceptionHandleEnum.Ignore);
                        break;
                    case "exception":
                        toolStripStatusLabel3.Text = "错误：" + data;
                        break;
                    case "exception delay":
                        //延时一段时间后清空文本。（存在情况：一段时间后可能文本框已被销毁。此时抛出异常，无需处理）
                        toolStripStatusLabel3.Text = "错误：" + data;
                        Process.Async.DelayInvoke(1000, () => toolStripStatusLabel3.Text = "",
                                                  exception: Process.ExceptionHandleEnum.Ignore);
                        break;
                    case "delay":
                        //延时一段时间后清空文本。（存在情况：一段时间后可能文本框已被销毁。此时抛出异常，无需处理）
                        toolStripStatusLabel4.Text = data as string;
                        Process.Async.DelayInvoke(1000, () => toolStripStatusLabel4.Text = "",
                                                  exception: Process.ExceptionHandleEnum.Ignore);
                        break;
                    default:
                        toolStripStatusLabel4.Text = data as string;
                        break;
                }
                statusStrip1.Refresh();
            });
            //事件调用该函数，执行线程可能并不是创建状态栏控件的线程
            //需将打印任务交给创建状态栏的线程，否则可能引发异常
            if (InvokeRequired) BeginInvoke(Process.CatchAction(action));
            else action();

        }

        private void 工作区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            worksapceFolderDialog.Description = "选择标注工作区，加载其中.png|.gif|.jpg|.bmp|.jpeg|.wmf图像文件";
            if (worksapceFolderDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GlobalMessage.Apply("status", "加载中");
                    workspace = worksapceFolderDialog.SelectedPath;
                    DirectoryInfo wkDir = new DirectoryInfo(workspace);
                    List<FileInfo> wkFiles = new List<FileInfo>();

                    wkFiles.AddRange(wkDir.GetFiles());
                    foreach (DirectoryInfo subDir in wkDir.GetDirectories("*", SearchOption.AllDirectories))
                        wkFiles.AddRange(subDir.GetFiles());

                    List<FileInfo> files = new List<FileInfo>();
                    foreach (FileInfo file in wkFiles)
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
                    GlobalMessage.Apply("status", $"共{files.Count}张图像");

                    annoPictureListPanel.Clear(true);
                    collect.ForEach((ctl) => { ctl.Image?.Dispose(); ctl.Dispose(); });
                    annoPictureListPanel.IsDynamicAdd = files.Count > 100;
                    annoPictureListPanel.IsDynamicDispose = files.Count > 100;

                    //虚加载所有图像，并发送进度消息。
                    GlobalMessage.Progress progress = new GlobalMessage.Progress(files.Count)
                    {
                        ProgressingFormatString = "已加载{1}",
                        ProgressedString = $"就绪，共{files.Count}张图像",
                        Print = PrintStatus
                    };
                    AnnoPictureBox[] annoPictures = new AnnoPictureBox[files.Count];
                    for (int i = 0; i < files.Count; i++)
                    {
                        annoPictures[i] = new AnnoPictureBox(files[i].FullName);
                        progress.Report(i + 1);
                    }
                    annoPictureListPanel.AddRange(annoPictures);
                    collect.AddRange(annoPictures);

                    annoPictureListPanel.ForEachItem((item) => item.paintIndexFont = new Font("微软雅黑", 15));

                }
                catch (Exception ex)
                {
                    GlobalMessage.Add("exception", ex.Message);
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            canva.Width = annoPictureListPanel.Location.X - canva.Location.X;
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
                using (AnnoLoaderForm annoLoaderForm = new AnnoLoaderForm(annoFileDialog.FileName))
                {
                    if (annoLoaderForm.ShowDialog() == DialogResult.Yes)
                    {
                        Annotations = annoLoaderForm.Annotations;
                        void ItemAdd(object _sender, AnnoPictureBox _item, EventArgs _e)
                        {
                            string filePath = _item.FilePath;
                            _ = Process.Async.Now(() =>
                            {
                                for (int i = 0; i < Annotations.Length; i++)
                                {
                                    if (AnnoPictureBox.CheckAnnotation(Annotations[i], filePath))
                                    {
                                        _item.Annotations.Add(Annotations[i]);
                                        Annotations[i] = null;
                                    }
                                }
                            }, () => _item.Invalidate());
                        }

                        GlobalMessage.Progress progress = new GlobalMessage.Progress(annoPictureListPanel.Count)
                        {
                            ProgressingFormatString = "正在处理第{1}项，共{2}项",
                            Print = PrintStatus
                        };
                        (annoPictureListPanel as ListPanel<AnnoPictureBox>).ForEachItem((item) =>
                        {
                            ItemAdd(null, item, null);
                            progress.IncReport();
                        });
                        annoPictureListPanel.ItemAdded += ItemAdd;
                    }
                }
            }
        }

        private void TestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setting.Global.Categories.Add("A");
            Setting.Global.Categories.Add("B");
            Setting.Global.Categories.Add("C");
            Setting.Global.Categories.Add("D");

            Setting.Global.Categories["A"].Brush.pen = new Pen(Color.AliceBlue, 10);
            Setting.Global.Categories["B"].Brush.font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 20);
            Setting.Global.Save("test.xml");
        }

        private void Test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setting.Global.Load("test.xml");
            GlobalMessage.Add(Setting.Global);
        }

    }
}

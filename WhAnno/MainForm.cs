using System;
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

        private readonly AnnoPictureListPannel annoPicturePannel = new AnnoPictureListPannel();
        private readonly BrushListPannel brushListPanel = new BrushListPannel();
        private readonly Canva canva = new Canva();

        public MainForm()
        {
            Controls.Add(annoPicturePannel);
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
                annoPicturePannel.Dock = DockStyle.Right;
                annoPicturePannel.Width = 200;
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
                    Anno.Brush.Rectangle.Annotation annotation = new Anno.Brush.Rectangle.Annotation
                    {
                        x = 10,
                        y = 10,
                        width = 50,
                        height = 50
                    };

                    if (brushListPanel.CurrentItem != null)
                    brushListPanel.CurrentItem?.PaintAnno(_pe.Graphics, annotation, canva);
                };
                annoPicturePannel.SelectedIndexChanged += (_sender, _item, _e) => canva.AnnoPicture = _item;
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
            if (InvokeRequired) BeginInvoke(action);
            else action();

        }

        private void 工作区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            worksapceFolderDialog.Description = "选择标注工作区，加载其中.png|.gif|.jpg|.bmp|.jpeg|.wmf图像文件";
            if (worksapceFolderDialog.ShowDialog() == DialogResult.OK)
            {
                MessagePrint.Apply("status", "加载中");
                workspace = worksapceFolderDialog.SelectedPath;
                DirectoryInfo wkDir = new DirectoryInfo(workspace);
                List<FileInfo> wkFiles = new List<FileInfo>();

                wkFiles.AddRange(wkDir.GetFiles());
                foreach (DirectoryInfo subDir in wkDir.GetDirectories())
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
                MessagePrint.Apply("status", $"共{files.Count}张图像");

                annoPicturePannel.Clear(true);

                //虚加载所有图像，并发送进度消息。
                MessagePrint.Progress progress = new MessagePrint.Progress(files.Count) { ProgressingFormatString = "已加载{1}", Print = PrintStatus };
                AnnoPictureBox[] annoPictures = new AnnoPictureBox[files.Count];
                for (int i = 0; i < files.Count; i++)
                {
                    annoPictures[i] = new AnnoPictureBox() { FilePath = files[i].FullName };
                    progress.Report(i + 1);
                }
                annoPicturePannel.AddRange(annoPictures);

                annoPicturePannel.ForEachItem((item) => item.paintIndexFont = new Font(item.paintIndexFont.FontFamily, 15));
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            canva.Width = annoPicturePannel.Location.X - canva.Location.X;
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
                if (annoLoaderForm.ShowDialog() == DialogResult.Yes)
                {
                    ;
                }
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

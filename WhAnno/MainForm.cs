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
using WhAnno.Anno.Base;
using WhAnno.PictureShow;

namespace WhAnno
{
    public partial class MainForm : Form
    {
        string workspace;

        private TextPictureListPannel textPicturePannel = new TextPictureListPannel();
        private BrushListPannel brushListPanel = new BrushListPannel();
        private Canva canva = new Canva();

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
                textPicturePannel.Dock = DockStyle.Right;
                textPicturePannel.Width = 200;
            }
            {
                brushListPanel.Dock = DockStyle.Right;
                brushListPanel.Width = 30;
                foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (item.FullName.Contains("WhAnno.Anno.Brush."))
                    {
                        brushListPanel.Add(Assembly.GetExecutingAssembly().CreateInstance(item.FullName) as BrushBase);
                    }
                }
            }
            {
                canva.Dock = DockStyle.Left;
                textPicturePannel.SelectedIndexChanged += (_sender, _item, _e) => canva.Image = _item.Image;
            }
            MessagePrint.SolveMessage += PrintStatus;
        }

        private void PrintStatus(string describe, object data)
        {
            //事件调用该函数，执行线程并不是创建状态栏控件的线程
            //需将打印任务交给创建状态栏的线程，否则可能出现异常
            try
            {
                Action solve = new Action(() =>
                {
                    switch (describe)
                    {
                        case "progress":
                            toolStripProgressBar1.Visible = true;
                            toolStripProgressBar1.Value = (int)data;
                            statusStrip1.Refresh();
                            if (toolStripProgressBar1.Value == 100)
                            {
                                //延时一段时间后将进度条设为不可见
                                InvokeProcess.Delay(1000, () =>
                                {
                                     Invoke(new Action(() => { toolStripProgressBar1.Visible = false; }));
                                });
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
                });
                if (InvokeRequired) BeginInvoke(solve);
                else solve();
            }
            catch (Exception e)
            {
                MessagePrint.Add("exception", e.Message);
            }
        }

        private void 打开工作区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "选择标注工作区";
            if (folder.ShowDialog() == DialogResult.OK)
            {
                MessagePrint.Add("status", "加载中");
                workspace = folder.SelectedPath;
                DirectoryInfo wkDir = new DirectoryInfo(workspace);
                FileInfo[] files = wkDir.GetFiles();

                textPicturePannel.Clear();

                InvokeProcess.Now(() =>
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        switch (files[i].Extension)
                        {
                            case ".png":
                            case ".gif":
                            case ".jpg":
                            case ".bmp":
                                Invoke(new Action(() =>
                                {
                                    textPicturePannel.Add(files[i].FullName);
                                }));
                                break;
                            default:
                                break;
                        }
                        int progress = (int)((float)(i + 1) / files.Length * 100);
                        MessagePrint.Add("progress", progress);
                        MessagePrint.Add("status", "加载中" + progress.ToString() + "%");
                    }
                    MessagePrint.Add("status", "就绪");
                });

                textPicturePannel.ForEachItem((item) => item.paintIndexFont = new Font(item.paintIndexFont.FontFamily, 15));

            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            canva.Width = textPicturePannel.Location.X - canva.Location.X;
        }

        private void 退出ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            textPicturePannel.Remove(textPicturePannel.GetItem(2));
        }
    }
}

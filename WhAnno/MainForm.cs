using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
                textPicturePannel.SelectedIndexChanged += (sender, item, e) => canva.SetImage(item.Image);
            }
            MessagePrint.SolveMethods += PrintStatus;
        }

        private void PrintStatus(string describe, object data)
        {
            //事件调用该函数，执行线程并不是创建状态栏控件的线程
            //需将打印任务交给创建状态栏的线程，否则可能出现异常
            try
            {
                Invoke(new Action(() =>
                {
                    switch (describe)
                    {
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
                }));
            }
            catch (Exception e)
            {
                MessagePrint.AddMessage("exception", e.Message);
            }
        }

        private void 打开工作区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "选择标注工作区";
            if (folder.ShowDialog() == DialogResult.OK)
            {
                workspace = folder.SelectedPath;
                DirectoryInfo wkDir = new DirectoryInfo(workspace);
                FileInfo[] files = wkDir.GetFiles();

                textPicturePannel.Clear();

                foreach (FileInfo file in files)
                {
                    switch (file.Extension)
                    {
                        case ".png":
                        case ".gif":
                        case ".jpg":
                        case ".bmp":
                            textPicturePannel.Add(file.FullName);
                            break;
                        default:
                            break;
                    }
                }
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

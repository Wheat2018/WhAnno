using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhAnno
{
    public partial class MainForm : Form
    {
        string workspace;
        

        public MainForm()
        {
            InitializeComponent();
            TextPictureBox textPictureBox = new TextPictureBox(@"C:\Users\88033\Pictures\QQ图片202.png");
            this.Controls.Add(textPictureBox);
            textPictureBox.Height = 100;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void 打开工作区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "选择标注工作区";
            if (folder.ShowDialog() == DialogResult.OK)
            {
                workspace = folder.SelectedPath;
                
            }
        }

        private void 退出ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}

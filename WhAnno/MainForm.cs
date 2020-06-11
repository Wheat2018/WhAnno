﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.PictureShow;

namespace WhAnno
{
    public partial class MainForm : Form
    {
        string workspace;
        

        public MainForm()
        {
            InitializeComponent();
            MessagePrint.solveMethods += PrintStatus;
            this.flowLayoutPanel1.Focus();

            AutoTextPicturePannel textPicturePannel = new AutoTextPicturePannel();
            textPicturePannel.Dock = DockStyle.Right;
            this.Controls.Add(textPicturePannel);
            textPicturePannel.Add(@"C:\Users\88033\Pictures\QQ图片202.png");
            textPicturePannel.Add(@"C:\Users\88033\Pictures\car.gif");
            textPicturePannel.Add(@"C:\Users\88033\Pictures\QQ截图20200529222914.png");
            textPicturePannel.paintIndexFont = new Font(textPicturePannel.paintIndexFont.FontFamily, 15);
            textPicturePannel.Width = 300;
        }

        private void PrintStatus(string describe, object data)
        {
            switch (describe)
            {
                case "status":
                    toolStripStatusLabel1.Text = data as string;
                    break;
                case "info":
                    toolStripStatusLabel2.Text = data as string;
                    break;
                default:
                    break;
            }
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

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

            textBox1.Text = annoFilePath;
        }
    }
}

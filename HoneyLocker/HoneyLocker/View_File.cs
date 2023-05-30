using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HoneyLocker
{
    public partial class View_File : Form
    {
        public View_File()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void View_File_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // 또는 FormBorderStyle.FixedDialog
            // 바탕화면 경로
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // .Honey 확장자를 가진 모든 파일 가져오기
            string[] honeyFiles = Directory.GetFiles(desktopPath, "*.Honey");

            // 가져온 파일들을 리스트박스에 추가
            listBox1.Items.AddRange(honeyFiles);
        }
    }
}

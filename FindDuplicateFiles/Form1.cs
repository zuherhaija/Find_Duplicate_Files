using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindDuplicateFiles
{
    public partial class frmMain : Form
    {
        public static string folderPath;
        public static int counter = 0;
        public static int result = 0;
        public static List<string> fileInfo = new List<string>();
        public static List<string> fileName = new List<string>();
        public static List<string> filePath = new List<string>();
        public static List<long> fileSize = new List<long>();
        public static bool finding = false;
        public static bool stoping = false;

        public frmMain()
        {
            InitializeComponent();

            // lvwResult初始化
            this.lvwResult.Columns.Add("文件名", 200, System.Windows.Forms.HorizontalAlignment.Left);
            this.lvwResult.Columns.Add("位置", 460, System.Windows.Forms.HorizontalAlignment.Left);
            this.lvwResult.Columns.Add("大小", 100, System.Windows.Forms.HorizontalAlignment.Left);
        }

        // 选择目标文件夹
        private void btnFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();

            // 隐藏“新建文件夹”按钮
            folderDlg.ShowNewFolderButton = false;
            folderDlg.Description = "选择需要查重的文件夹";

            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                // 获取路径
                folderPath = folderDlg.SelectedPath;
                // 显示路径
                txtPath.Text = folderPath;
            }
            // 释放
            folderDlg.Dispose();
        }

        // 遍历文件夹
        private void ListFiles(FileSystemInfo info)
        {
            // 中止
            if (stoping)
            {
                return;
            }

            // 路径错误
            if (!info.Exists)
            {
                MessageBox.Show("文件不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DirectoryInfo dir = info as DirectoryInfo;

            // 不是目录
            if (dir == null)
            {
                return;
            }

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;

                // 文件
                if (file != null)
                {
                    // 计算MD5数值
                    FileStream fileStream = file.OpenRead();
                    MD5CryptoServiceProvider fileMD5 = new MD5CryptoServiceProvider();
                    byte[] hashByte = fileMD5.ComputeHash(fileStream);
                    string str = System.BitConverter.ToString(hashByte);
                    str = str.Replace("-", "");
                    //str = str.ToLower();

                    // 释放流
                    fileStream.Close();
                    
                    // 记录
                    counter++;
                    fileInfo.Add(str);
                    fileName.Add(file.Name);
                    filePath.Add(file.DirectoryName);
                    fileSize.Add(file.Length);

                    // 显示
                    lblCounter.Text = Convert.ToString(counter);
                    lblProcess.Text = "正在查找：" + file.DirectoryName + "\\" + file.Name;
                    lblCounter.Update();
                    lblProcess.Update();
                }
                // 文件夹
                else
                {
                    ListFiles(files[i]);
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // 初始化
            counter = 0;
            result = 0;
            fileInfo.Clear();
            fileName.Clear();
            filePath.Clear();
            fileSize.Clear();
            lvwResult.Clear();
            this.lvwResult.Columns.Add("文件名", 200, System.Windows.Forms.HorizontalAlignment.Left);
            this.lvwResult.Columns.Add("位置", 460, System.Windows.Forms.HorizontalAlignment.Left);
            this.lvwResult.Columns.Add("大小", 100, System.Windows.Forms.HorizontalAlignment.Left);

            if (folderPath != null)
            {
                finding = true;
                stoping = false;
                btnStart.Text = "中止";
                btnStart.Update();

                ListFiles(new DirectoryInfo(folderPath));

                // 判断
                int f = fileName.Count();

                for (int i = 0; i < f; i++)
                {
                    for (int j = i + 1; j < f; j++)
                    {
                        if (fileInfo[i] == fileInfo[j] && fileSize[i] == fileSize[j])
                        {
                            result++;

                            // 显示结果
                            ListViewItem item1 = new ListViewItem();
                            item1.SubItems.Clear();
                            item1.SubItems[0].Text = fileName[i];
                            item1.SubItems.Add(filePath[i]);
                            item1.SubItems.Add(fileSize[i].ToString());

                            ListViewItem item2 = new ListViewItem();
                            item2.SubItems.Clear();
                            item2.SubItems[0].Text = fileName[j];
                            item2.SubItems.Add(filePath[j]);
                            item2.SubItems.Add(fileSize[j].ToString());

                            lvwResult.Items.Add(item1);
                            lvwResult.Items.Add(item2);
                        }
                    }
                }

                lblProcess.Text = "查找完毕，已找到" + result.ToString() + "个重复文件";

                if (result == 0)
                {
                    MessageBox.Show("未找到重复文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }

                // 复位
                finding = false;
                btnStart.Text = "开始";
            }
            else
            {
                MessageBox.Show("请选择目标文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }
    }
}

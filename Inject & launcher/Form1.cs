﻿using System.Diagnostics;

namespace Inject___launcher {
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    public partial class Form1 : Form {
        public Form1() {
            this.InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            if (File.Exists("path.dat")) {
                this.textBox1.Text = File.ReadAllText("path.dat");
            }
        }

        [DllImport("kernel32.dll")] //声明API函数
        public static extern IntPtr VirtualAllocEx(IntPtr hwnd, IntPtr lpaddress, int size, int type, int tect);

        [DllImport("kernel32.dll")]
        public static extern int WriteProcessMemory(IntPtr hwnd, IntPtr baseaddress, string buffer, int nsize, int filewriten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hwnd, string lpname);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandleA(string name);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hwnd, IntPtr attrib, int size, IntPtr address, IntPtr par, int flags, IntPtr threadid);


        [DllImport("KERNEL32.DLL ")]
        public static extern int CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern long GetLastError();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) {
            this.listBox1.Items.Clear();
            this.button1.Enabled = false;
            try {
                new Thread(
                           () => {
                               this.listBox1.Items.Add(Log.FormatLog("准备启动游戏..."));
                               this.listBox1.Items.Add(Log.FormatLog("剔除检测: " + (this.checkBox1.Checked ? "已启用" : "未启用")));
                               if (this.checkBox1.Checked) {
                                   this.listBox1.Items.Add(Log.FormatLog("正在剔除检测..."));
                                   if (this.textBox1.Text == "") {
                                       this.listBox1.Items.Add(Log.FormatLog("未选择游戏目录!"));
                                       this.button1.Enabled = true;
                                       return;
                                   }

                                   this.listBox1.Items.Add(Log.FormatLog("正在替换文件..."));
                                   try { File.Copy("KS_Diagnostics_Process.dll", this.textBox1.Text + "\\Phasmophobia_Data\\Plugins\\x86_64\\KS_Diagnostics_Process.dll", true); } catch (UnauthorizedAccessException ex) {
                                       // TODO: Handle the System.UnauthorizedAccessException
                                       MessageBox.Show("复制文件出错\n" + ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                       this.listBox1.Items.Add(Log.FormatLog("复制文件出错!"));
                                       this.button1.Enabled = true;
                                   }

                                   this.listBox1.Items.Add(Log.FormatLog("复制文件成功!"));
                                   this.listBox1.Items.Add(Log.FormatLog("剔除检测成功!"));
                               }
                               this.listBox1.Items.Add(Log.FormatLog("正在启动游戏..."));

                                   string  url = "steam://rungameid/739630";
                                   Process p   = new Process();
                                   p.StartInfo.FileName               = "cmd.exe";
                                   p.StartInfo.UseShellExecute        = false; //不使用shell启动
                                   p.StartInfo.RedirectStandardInput  = true;  //喊cmd接受标准输入
                                   p.StartInfo.RedirectStandardOutput = false; //不想听cmd讲话所以不要他输出
                                   p.StartInfo.RedirectStandardError  = true;  //重定向标准错误输出
                                   p.StartInfo.CreateNoWindow         = true;  //不显示窗口
                                   p.Start();

                                   //向cmd窗口发送输入信息 后面的&exit告诉cmd运行好之后就退出
                                   p.StandardInput.WriteLine("start " + url + "&exit");
                                   p.StandardInput.AutoFlush = true;
                                   p.WaitForExit(); //等待程序执行完退出进程
                                   p.Close();

                           findProcess:
                               Thread.Sleep(5000);
                               var process = Process.GetProcessesByName("Phasmophobia");
                               if (process == null || process.Length == 0) {
                                   goto findProcess;
                               }

                               Int32 SYNCHRONIZE= 0x00100000;

                               Int32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;

                               Int32 PROCESS_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF);

                                string dllname = System.AppDomain.CurrentDomain.BaseDirectory + "Phasmophobia.dll";
                               this.listBox1.Items.Add(Log.FormatLog("游戏ID: " + process[0].Id));
                               nint handle = OpenProcess(PROCESS_ALL_ACCESS, false, process[0].Id);
                               this.listBox1.Items.Add(Log.FormatLog("游戏句柄: " + handle));
                               this.listBox1.Items.Add(Log.FormatLog("正在注入..."));
                               IntPtr allocBaseAddress = VirtualAllocEx(handle, 0, dllname.Length, 4096, 4);
                               if (allocBaseAddress == 0) {
                                   MessageBox.Show("内存分配失败", "错误");
                                   this.listBox1.Items.Add(Log.FormatLog("内存分配失败!"));
                                   this.button1.Enabled = true;
                                   return;
                               }

                               this.listBox1.Items.Add(Log.FormatLog("已分配内存地址: " + allocBaseAddress));

                               if (WriteProcessMemory(handle, allocBaseAddress, dllname, dllname.Length, 0) == 0) {
                                   MessageBox.Show("DLL写入失败 error:" + GetLastError(), "错误", 0);
                                   this.listBox1.Items.Add(Log.FormatLog("DLL写入失败!"));
                                   this.button1.Enabled = true;
                                   return;
                               }

                               MessageBox.Show("DLL写入失败 error:" + GetLastError(), "错误", 0);

                               IntPtr loadaddr = GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryA");
                               if (loadaddr == 0) {
                                   MessageBox.Show("取得LoadLibraryA的地址失败");
                                   this.listBox1.Items.Add(Log.FormatLog("取得LoadLibraryA的地址失败!"));
                                   this.button1.Enabled = true;
                                   return;
                               }

                               this.listBox1.Items.Add(Log.FormatLog("LoadLibraryA内存地址: " + loadaddr));

                               IntPtr ThreadHwnd = CreateRemoteThread(handle, 0, 0, loadaddr, allocBaseAddress, 0, 0);
                               if (ThreadHwnd == IntPtr.Zero) {
                                   MessageBox.Show("创建远程线程失败");
                                   this.listBox1.Items.Add(Log.FormatLog("创建远程线程失败!"));
                                   this.button1.Enabled = true;
                                   return;
                               }

                               MessageBox.Show("DLL写入失败 error:" + GetLastError(), "错误", 0);

                               this.listBox1.Items.Add(Log.FormatLog("远程线程句柄: " + ThreadHwnd));

                               this.listBox1.Items.Add(Log.FormatLog("注入成功!"));
                               this.button1.Enabled = true;
                           }).Start();
            } catch (ThreadStateException ex) {
                // TODO: Handle the System.Threading.ThreadStateException
                MessageBox.Show("创建启动线程失败\n" + ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.listBox1.Items.Add(Log.FormatLog("创建启动线程失败!"));
                this.button1.Enabled = true;
            } catch (Win32Exception ex) {
                // TODO: Handle the System.ComponentModel.Win32Exception
                MessageBox.Show("启动游戏失败\n" + ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.listBox1.Items.Add(Log.FormatLog("启动游戏失败!"));
                this.button1.Enabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e) {
            this.textBox1.PlaceholderText = "路径...";
            this.listBox1.Items.Add(Log.FormatLog("启动器启动!"));
        }

        private void button2_Click(object sender, EventArgs e) {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择一个目录作为路径：";
            dialog.ShowNewFolderButton = true;
            dialog.RootFolder = Environment.SpecialFolder.ApplicationData;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            this.textBox1.Text = dialog.SelectedPath;
            try {
                if (!File.Exists("path.dat")) {
                    File.Create("path.dat").Close();
                }
                File.WriteAllText("path.dat", this.textBox1.Text);
            } catch (DirectoryNotFoundException ex) {
                // TODO: Handle the System.IO.DirectoryNotFoundException
                MessageBox.Show("写出目录失败\n" + ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            string  url = "https://github.com/issuimo/PhasmophobiaCheat/tree/main";
            Process p   = new Process();
            p.StartInfo.FileName               = "cmd.exe";
            p.StartInfo.UseShellExecute        = false; //不使用shell启动
            p.StartInfo.RedirectStandardInput  = true;  //喊cmd接受标准输入
            p.StartInfo.RedirectStandardOutput = false; //不想听cmd讲话所以不要他输出
            p.StartInfo.RedirectStandardError  = true;  //重定向标准错误输出
            p.StartInfo.CreateNoWindow         = true;  //不显示窗口
            p.Start();

            //向cmd窗口发送输入信息 后面的&exit告诉cmd运行好之后就退出
            p.StandardInput.WriteLine("start " + url + "&exit");
            p.StandardInput.AutoFlush = true;
            p.WaitForExit(); //等待程序执行完退出进程
            p.Close();
        }
    }
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace cyusbDemo
{
    public partial class WinUpan : Form
    {
        private string IniFilePath;
        int count = 0;     //u盘个数(中间量)
        int cNumber = 0;    //测试计数
        int num_Alcor = 8;   // 待测试安国本体数量
        int delay = 150;      //延时时间
        int loopTime = 1;   // 空格次数
        string Section = "Information";
        int cN = 0;

        double min, hour, min2;

        #region U盘设备相关常量
        public const int WM_DEVICECHANGE = 0x219;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEARRIVAL = 0x8000;  //就是用来表示U盘可用的。一个设备或媒体已被插入一块，现在可用。
        public const int DBT_CONFIGCHANGECANCELED = 0x0019;  //要求更改当前的配置（或取消停靠码头）已被取消。
        public const int DBT_CONFIGCHANGED = 0x0018;  //当前的配置发生了变化，由于码头或取消固定。
        public const int DBT_CUSTOMEVENT = 0x8006; //自定义的事件发生。 的Windows NT 4.0和Windows 95：此值不支持。
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;  //审批要求删除一个设备或媒体作品。任何应用程序也不能否认这一要求，并取消删除。
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;  //请求删除一个设备或媒体片已被取消。
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;  //一个设备或媒体一块即将被删除。不能否认的。
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;  //一个设备特定事件发生。
        public const int DBT_DEVNODES_CHANGED = 0x0007;  //一种设备已被添加到或从系统中删除。
        public const int DBT_QUERYCHANGECONFIG = 0x0017;  //许可是要求改变目前的配置（码头或取消固定）。
        public const int DBT_USERDEFINED = 0xFFFF;  //此消息的含义是用户定义的
        public const uint GENERIC_READ = 0x80000000;
        public const int GENERIC_WRITE = 0x40000000;
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
        #endregion

        public WinUpan()
        {
            InitializeComponent();
            count = 0;
            this.Height = 110;
            //Combobox.SelectedIndex = Combobox.Items.IndexOf(“默认选中文本”);
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("200");
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf("0");
            comboBox3.SelectedIndex = comboBox3.Items.IndexOf("8");
            comboBox4.SelectedIndex = comboBox4.Items.IndexOf("1");
            //=====判断进程法：(修改程序名字后依然能执行)=====
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (process.MainModule.FileName
                    == current.MainModule.FileName)
                    {
                        MessageBox.Show("程序已经运行！", Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        Process.GetCurrentProcess().Kill();
                        //return;
                    }
                }
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            count = 0;
            cN = 0;


            button1_Click(sender, e);
            //ShowTime();
            lbl_Time.Text = DateTime.Now.ToString("f"); // + " " + DateTime.Now.ToString("dddd");


        }

        private void ShowTime()
        {
            //创建线程
            System.Threading.Thread P_thread = new System.Threading.Thread(
             () =>   //使用Lambda表达式
             {
                 while (true)
                 {
                     this.Invoke(
           (MethodInvoker)delegate()  //操作窗体线程,使用匿名方法
           {
               this.Refresh();  //刷新窗体
               Graphics P_Graphics = CreateGraphics(); //创建绘图对象

               // 绘制出系统时间
               P_Graphics.DrawString(
               DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分 ss 秒"),
                   new Font("宋体", 8), Brushes.DarkBlue, new Point(0, 5));
           }
                                );
                     System.Threading.Thread.Sleep(1000);  //线程挂起1秒钟
                 }
             }
                          );
            P_thread.IsBackground = true;  //线程设置为后台线程 
            P_thread.Start();     //线程开始执行
        }

        //获取配置文件中的值 
        private void GetValue(string section, string key, out string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            GetPrivateProfileString(section, key, "", stringBuilder, 1024, IniFilePath);
            value = stringBuilder.ToString();
        }

        protected override void WndProc(ref Message m)
        {

            try
            {

                if (m.Msg == WM_DEVICECHANGE)
                {

                    cN++;
                    DateTime dt_start = DateTime.Now;


                    switch (m.WParam.ToInt32())
                    {
                        case DBT_DEVNODES_CHANGED:



                            break;

                        case DBT_DEVICEARRIVAL:

                            DriveInfo[] s = DriveInfo.GetDrives();
                            s.Any(t =>
                            {
                                if (t.DriveType == DriveType.Removable)
                                {
                                    count++;
                                    return true;
                                }

                                return false;
                            });
                            if (count == num_Alcor)
                            {
                                Thread.Sleep(delay);
                                SendKeys.Send(" ");

                            }
                            else if ((count < 0) && (count > 8))
                            {
                                count = 0;
                            }

                            break;
                        case DBT_DEVICEREMOVECOMPLETE:

                            count--;


                            if (count < 0)
                            {
                                count = 0;
                            }
                            break;
                        default:

                            break;
                    }




                    for (int i = 0; i < loopTime; i++)
                    {

                        Thread.Sleep(delay);
                        SendKeys.Send(" ");
                        Thread.Sleep(delay);

                    }

                    // usb检测每6次进行一次
                    if (cN % 7 == 0)
                    {
                        // 检测时间判断
                        DateTime dt_stop = DateTime.Now;
                        TimeSpan ts_start_stop = dt_stop - dt_start;
                        if (ts_start_stop.Milliseconds > 400)
                        {
                            cNumber++;
                            lbl_Number.Text = cNumber.ToString();
                            DateTime dtone = Convert.ToDateTime(lbl_Time.Text);
                            DateTime dttwo = DateTime.Now;
                            TimeSpan ts = dttwo - dtone;
                            min2 = Math.Round(ts.TotalMilliseconds / 1000.0 / 60.0);
                            min = min2 == 0 ? 1 : min2;

                            hour = ts.Hours;
                            Console.WriteLine("分钟:{0},小时数:{1}", min, hour);
                            lbl_m.Text = (cNumber / min).ToString("f0");

                        }




                        cN = 0;
                        Thread.Sleep(delay);

                    }

                    WritePrivateProfileString(Section, "cNumber", lbl_Number.Text, IniFilePath);


                }

            }
            catch (Exception)
            {

                return;
            }


            base.WndProc(ref m);



        }
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
         string lpFileName,
         uint dwDesireAccess,
         uint dwShareMode,
         IntPtr SecurityAttributes,
         uint dwCreationDisposition,
         uint dwFlagsAndAttributes,
         IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );
        [DllImport("kernel32.dll")]

        private static extern long WritePrivateProfileString(string section, string key, string value, string filepath);



        [DllImport("kernel32.dll")]

        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnvalue, int buffersize, string filepath);


        #region 无连框窗体拖动
        //using System.Runtime.InteropServices;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;


        #endregion




        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;

        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void WinUpan_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();  //点X只隐藏主窗体
            e.Cancel = true;

        }
        #region 单击按钮写入到配置文件

        private void button1_Click(object sender, EventArgs e)
        {
            btn_Click(sender, e);
            string outString;
            IniFilePath = Application.StartupPath + "\\Config.ini";
            lbl_Time.Text = DateTime.Now.ToString("f");
            try
            {

                GetValue("Information", "Delay", out outString);
                delay = Convert.ToInt32(outString);
                GetValue("Information", "cNumber", out outString);
                cNumber = Convert.ToInt32(outString);
                lbl_Number.Text = cNumber.ToString();
                GetValue("Information", "num_Alcor", out outString);
                num_Alcor = Convert.ToInt32(outString);
                GetValue("Information", "loopTime", out outString);
                loopTime = Convert.ToInt32(outString);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void btn_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text.Trim() != "")
            {
                string Section = "Information";
                try
                {

                    WritePrivateProfileString(Section, "Delay", comboBox1.Text, IniFilePath);
                    WritePrivateProfileString(Section, "cNumber", comboBox2.Text, IniFilePath);
                    WritePrivateProfileString(Section, "num_Alcor", comboBox3.Text, IniFilePath);
                    WritePrivateProfileString(Section, "loopTime", comboBox4.Text, IniFilePath);


                }
                catch (Exception ee)
                {

                    MessageBox.Show(ee.Message);
                }

            }
            else
            {
                MessageBox.Show("不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion



        private void WinUpan_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);

        }

        private void lbl_Number_Click(object sender, EventArgs e)
        {
            this.Height = 210;
            //Thread.Sleep(30000);

        }

        private void WinUpan_MouseLeave(object sender, EventArgs e)
        {
            this.Height = 100;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            this.Height = 100;

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        public int min1 { get; set; }

        private void WinUpan_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Height = 210;
        }

        private void WinUpan_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

         //重写ProcessCmdKey的方法 |实现按ESC键退出
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int WM_KEYDOWN = 256;
            int WM_SYSKEYDOWN = 260;
            if (msg.Msg == WM_KEYDOWN | msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        if (MessageBox.Show("你真的要退出测试程序?", "退出", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            this.Close();
                            Process.GetCurrentProcess().Kill();
                        }
                        break;
                }
            }
            return false;
        }


    }
    
    
}


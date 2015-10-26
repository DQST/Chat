using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public partial class Main : Form
    {
        private UDPSender udpSender;
        private string hostName = null;
        private int port1 = 25, port2 = 81;

        public Main()
        {
            InitializeComponent();
            hostName = Properties.Settings.Default.Local_IP;
            port1 = int.Parse(Properties.Settings.Default.Input);
            port2 = int.Parse(Properties.Settings.Default.Output);
            
            udpSender = new UDPSender(hostName, port1, port2);
            udpSender.setLogFun = Log;
            udpSender.setAddUserFun = AddUser;
            udpSender.setDelUserFun = DelUser;
            udpSender.setDownloadFun = DownloadFun;
        }

        /* 
         * #################################################################
         * Функции для вывода сообщений
         * #################################################################
         */

        private void Log(string m)              //функция вывода сообщений
        {
            richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(m+"\n")));
        }

        private void AddUser(string user_name)  //функция добавления пользователей
        {
            listBox1.Invoke(new Action(()=>listBox1.Items.Add(user_name)));
        }

        private void DelUser(string user_name)  //функция удаления пользователей
        {
            listBox1.Invoke(new Action(() => listBox1.Items.Remove(user_name)));
        }

        private void DownloadFun(int start, int end, int step)
        {
            this.Invoke(new Action(() => downloadBar.Minimum = start));
            this.Invoke(new Action(() => downloadBar.Maximum = end));
            for (int i = 0; i < end; i += step)
                this.Invoke(new Action(() => downloadBar.Value = i));
            Thread.Sleep(2000);
            this.Invoke(new Action(() => downloadBar.Value = 0));
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            udpSender.Recieve();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void очиститьИсториюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings f2 = new Settings();
            f2.Show(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.usersTableAdapter.Fill(this.baseDataSet.Users);
            backgroundWorker1.RunWorkerAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundWorker1.CancelAsync();
            richTextBox1.SaveFile(Application.StartupPath + "\\history.txt", RichTextBoxStreamType.PlainText);
            Application.Exit();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Users users = new Users();
            users.Show(this);
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(listBox1.SelectedIndex > -1)
                udpSender.KickUser(listBox1.Items[listBox1.SelectedIndex].ToString());
        }

        private void отключитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
                udpSender.KickUser(listBox1.Items[listBox1.SelectedIndex].ToString());
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void aboutProgram_Click(object sender, EventArgs e)
        {
            Process.Start(Application.StartupPath + "\\help.chm");
        }
    }
}
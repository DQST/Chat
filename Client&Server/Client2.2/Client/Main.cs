using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace Client
{
    public partial class Main : Form
    {
        private string hostName = null, remoteName = null, user_name = null;
        private int port1 = 25, port2 = 81;
        private bool connect = false;
        private UdpClient udp;
        private Form f;

        public Main(Form f)
        {
            InitializeComponent();
            this.f = f;
        }

        public void LoadSettings()
        {
            hostName = Properties.Settings.Default.Host_name;
            remoteName = Properties.Settings.Default.Remote_name;
            port1 = int.Parse(Properties.Settings.Default.Input);
            port2 = int.Parse(Properties.Settings.Default.Output);
            user_name = Properties.Settings.Default.UserName;
        }

        private void AddUser(string name)
        {
            listBox1.Invoke(new Action(() => listBox1.Items.Add(name)));
        }

        private void ClearUserList()
        {
            listBox1.Invoke(new Action(() => listBox1.Items.Clear()));
        }

        private void Log(string m)
        {
            richTextBox2.Invoke(new Action(() => richTextBox2.AppendText(m + "\n")));
        }

        private void Parse(ref string m)
        {
            string[] str = m.Split('/');
            switch (str[0])
            {
                case "ok":      if (str[1] == user_name) connect = true; break;
                case "neok":    if (str[1] == user_name) connect = false; break;
                case "add":     if (str[1] != null) AddUser(str[1]); break;
                case "clear":   if (str[1] == "all") ClearUserList(); break;      // очищаем список клиентов
                case "msg":     if(str[2]!=null) Log(str[1]+"> "+str[2]); break;
                case "kick":    this.Invoke(new Action(() => this.connect = false)); 
                                MessageBox.Show("Вы отключены от сервера!","Информация",MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                case "entok":   this.Invoke(new Action(() => this.Show())); f.Invoke(new Action(() => f.Hide())); 
                                break;
                case "entnok":  MessageBox.Show(str[1],"Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error); break;
                case "regok":   MessageBox.Show(str[1], "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                case "regnok":  MessageBox.Show(str[1], "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
                case "f":       DialogResult rez = MessageBox.Show("Сохранить файл "+str[1]+'?',"Файл",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                                if (rez == DialogResult.Yes)
                                {
                                    string name = Properties.Settings.Default.SaveDirectory + "\\" + str[1];
                                    Log(">Загрузка файла: " + str[1]);
                                    BinaryWriter b = new BinaryWriter(File.Open(name, FileMode.OpenOrCreate));
                                    foreach (var i in str[2])
                                        b.Write(i);
                                    b.Close();
                                    Log(">Файл загружен!");
                                }
                                break;
            }
        }

        /*
         *  Методы отправки сообщений 
         */
        private void Send(string msg, string ip, int port)
        {
            byte[] data = Encoding.Default.GetBytes(msg);
            udp.Send(data,data.Length,ip,port);
        }

        public void Send(string msg)
        {
            Send(msg, remoteName, port2);
        }

        private void Send(byte[] data)
        {
            udp.Send(data, data.Length, remoteName, port2);
        }

        //прием сообщений
        private void Recieve()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(hostName), port1);
                udp = new UdpClient(endPoint);
                while (true)
                {
                    IPEndPoint end = null;
                    byte[] data = udp.Receive(ref end);
                    string msg = Encoding.Default.GetString(data);
                    Parse(ref msg);
                }
            }
            catch {
                MessageBox.Show("Соединение не было установлено!\nИзмените настройки и перезапустите приложение.","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void Connect()              //соединение с сервером
        {
            Send("connect/" + user_name);
        }

        private void Disconnect()           //разъединение
        {
            Send("disconnect/" + user_name);
            connect = false;
        }

        //запуска второго потока
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Recieve();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            this.Text = "Клиент: " + user_name;
            backgroundWorker1.RunWorkerAsync();
        }

        /*###############################################################
                            Стандартные функции формы
        ###############################################################*/

        private void button1_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Length > 0 && connect == true)
            {
                Send("msg/"+user_name+"/"+richTextBox1.Text);
                richTextBox1.Clear();
                richTextBox1.Focus();
            }
        }

        /*
         * При закрытии формы отключаем клиента от сервера
         */
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connect == true)
                Disconnect();
            backgroundWorker1.CancelAsync();    //остановка потока
            Application.Exit();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(connect == true)
                Disconnect();
            Application.Exit();
        }

        //показываем окно настроек пользователю
        public void ShowSettings()
        {
            Settings f2 = new Settings();
            f2.Show(this);
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettings();
        }

        //обновление данных о пользователе
        private void timer1_Tick(object sender, EventArgs e)
        {
            user_name = Properties.Settings.Default.UserName;
            this.Text = "Клиент: " + user_name;
            connectLabel.Text = "Connect: " + connect.ToString();
            
            if (connect == true)
            {
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                //обновление данных о подключенных пользователях
                Send("get/");
            }
            else {
                connectButton.Enabled = true;
                disconnectButton.Enabled = false;
            }
        }

        /*
         * Тут происходит вызов функции соединения и разъединения с сервером
         */
        private void connectButton_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Disconnect();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                if (richTextBox1.Text.Length > 0 && connect == true)
                {
                    Send("msg/" + user_name + "/" + richTextBox1.Text);
                    richTextBox1.Clear();
                    richTextBox1.Focus();
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Send("disconnect/" + user_name);
            f.Show();
            this.Hide();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            richTextBox2.ScrollToCaret();
        }

        //показываем диалоговое окно для выбора файла
        private void ShowFileDialog()
        {
            DialogResult r = fileDialog.ShowDialog(this);
            if (r == DialogResult.OK && connect == true)
            {
                UdpFile file = new UdpFile(fileDialog.FileName);
                Log("Отправка файла: "+file.FileName + "; размер" + file.FileLen + " байт");
                Send(file.getBytes());
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ShowFileDialog();
        }

        private void отправитьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFileDialog();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Application.StartupPath+"\\help.chm");
        }
    }
}

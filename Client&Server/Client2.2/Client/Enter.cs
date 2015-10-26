using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class Enter : Form
    {
        private Main main;
        private bool registerMode = false;

        public Enter()
        {
            InitializeComponent();
        }

        private void Enter_Load(object sender, EventArgs e)
        {
            main = new Main(this);
            main.Show();
            main.Hide();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (Properties.Settings.Default.SaveLoginAndPass == true)
            {
                textBox1.Text = Properties.Settings.Default.UserName;
                textBox2.Text = Properties.Settings.Default.UserPassword;
                checkBox1.Checked = Properties.Settings.Default.SaveLoginAndPass;
            }
            else
                textBox1.Focus();
        }

        private string getHash(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                Properties.Settings.Default.UserName = textBox1.Text;
                Properties.Settings.Default.UserPassword = textBox2.Text;
                Properties.Settings.Default.SaveLoginAndPass = true;
                Properties.Settings.Default.Save();
            }

            if (registerMode == false)
                main.Send("entuser/" + textBox1.Text + "/" + getHash(textBox2.Text));
            else
                main.Send("reguser/" + textBox1.Text + "/" + getHash(textBox2.Text));
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            registerMode = !registerMode;
            if (registerMode == true)
            {
                this.Text = "Регистрация";
                checkBox1.Visible = false;
                linkLabel1.Text = "Назад...";
                button1.Text = "Создать аккаунт";
                textBox1.Clear();
                textBox2.Clear();
                textBox1.Focus();
            }
            else {
                this.Text = "Вход";
                checkBox1.Visible = true;
                linkLabel1.Text = "Регистрация";
                button1.Text = "Вход";
                LoadSettings();
            }
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            main.ShowSettings();
        }
    }
}

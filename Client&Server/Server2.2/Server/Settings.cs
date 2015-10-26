using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Server
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Local_IP = textBox1.Text;
            Properties.Settings.Default.Input = textBox2.Text;
            Properties.Settings.Default.Output = textBox3.Text;
            Properties.Settings.Default.Save();

            DialogResult rez = MessageBox.Show("Хотите перезапустить приложение сейчас?","Информация",MessageBoxButtons.YesNo,MessageBoxIcon.Information);
            if (rez == DialogResult.Yes)
            {
                string s = Application.StartupPath + "\\server.exe";
                Process.Start(s);
                Application.Exit();
            }
            else
                this.Hide();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.Local_IP;
            textBox2.Text = Properties.Settings.Default.Input;
            textBox3.Text = Properties.Settings.Default.Output;
            textBox4.Text = Properties.Settings.Default.SaveDirectory;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult rez = folderBrowserDialog1.ShowDialog(this);
            if (rez == DialogResult.OK)
            {
                Properties.Settings.Default.SaveDirectory = folderBrowserDialog1.SelectedPath;
                textBox4.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }
    }
}

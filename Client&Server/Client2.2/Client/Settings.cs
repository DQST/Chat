using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Client
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.Host_name;
            textBox2.Text = Properties.Settings.Default.Remote_name;
            textBox3.Text = Properties.Settings.Default.Input;
            textBox4.Text = Properties.Settings.Default.Output;
            textBox5.Text = Properties.Settings.Default.UserName;
            textBox6.Text = Properties.Settings.Default.SaveDirectory;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Host_name = textBox1.Text;
            Properties.Settings.Default.Remote_name = textBox2.Text;
            Properties.Settings.Default.Input = textBox3.Text;
            Properties.Settings.Default.Output = textBox4.Text;
            Properties.Settings.Default.Save();

            DialogResult rez = MessageBox.Show("Хотите перезапустить приложение сейчас?", "Информация", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (rez == System.Windows.Forms.DialogResult.Yes)
            {
                string s = Application.StartupPath + "\\client.exe";
                Process.Start(s);
                Application.Exit();
            }
            else
                this.Hide();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult rez = folderBrowser.ShowDialog(this);
            if (rez == DialogResult.OK)
            {
                Properties.Settings.Default.SaveDirectory = folderBrowser.SelectedPath;
                textBox6.Text = folderBrowser.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public partial class Users : Form
    {
        public Users()
        {
            InitializeComponent();
        }

        private void Users_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'baseDataSet.Users' table. You can move, or remove it, as needed.
            this.usersTableAdapter.Fill(this.baseDataSet.Users);
        }

        private void Users_FormClosing(object sender, FormClosingEventArgs e)
        {
            usersBindingSource.EndEdit();
            usersTableAdapter.Update(baseDataSet);
            baseDataSet.AcceptChanges();
            this.Hide();
        }
    }
}

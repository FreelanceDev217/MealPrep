using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MealPrep
{
    public partial class SimpleInputFrm : Form
    {
        public string content = "";
        public SimpleInputFrm()
        {
            InitializeComponent();
            this.ActiveControl = txt_name;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
        }

        ~SimpleInputFrm() {  }
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
        }

        private void txt_pwd_TextChanged(object sender, EventArgs e)
        {
            content = txt_name.Text;
        }

        private void PasswordForm_Load(object sender, EventArgs e)
        {

        }
    }
}

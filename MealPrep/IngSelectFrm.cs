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
    public partial class IngSelectFrm : Form
    {
        public string content = "";

        ~IngSelectFrm() {  }
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            content = com_ing.Text;
        }

        private void IngSelectFrm_Load(object sender, EventArgs e)
        {

        }
    }
}

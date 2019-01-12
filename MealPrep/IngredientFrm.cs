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
    public partial class IngredientFrm : Form
    {
        // ui
        Color error_col = Color.FromArgb(255, 192, 192);
        Color normal_col = Color.White;

        public string name = "";
        public string category = "";
        public string price = "";
        public string cal = "";
        public string fat = "";
        public string protein = "";
        public string carb = "";

        public MainFrm m_parent;
        public string m_id;
        public IngredientFrm(MainFrm par, string id = "")
        {
            InitializeComponent();
            this.ActiveControl = txt_name;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;

            m_parent = par;
            m_id = id;

            init_ui();
            if(m_id != "")
            {
                fill_form();
            }
        }

        void init_ui()
        {
            foreach (KeyValuePair<string, string> entry in m_parent.m_cat_dict)
            {
                com_category.Items.Add(entry.Key);
            }
            com_category.SelectedIndex = 0;
        }
        void fill_form()
        {
            DataTable dt = m_parent.m_sqlite.ExecuteQuery("select A.name as Name, B.name as Category, A.calorie as Calories, A.price as Price, A.fat as Fat, A.protein as Protein, A.carbo  as Carbo from tbl_ingredient A left join tbl_category B on A.category = B.id where A.del_flag is 0 AND B.del_flag is 0 AND A.id = " + m_id);

            txt_name.Text = dt.Rows[0][0].ToString();
            com_category.SelectedIndex = com_category.Items.IndexOf(dt.Rows[0][1].ToString());
            txt_price.Text = dt.Rows[0][2].ToString();
            txt_cal.Text = dt.Rows[0][3].ToString();
            txt_fat.Text = dt.Rows[0][4].ToString();
            txt_protein.Text = dt.Rows[0][5].ToString();
            txt_carb.Text = dt.Rows[0][6].ToString();
        }

        ~IngredientFrm() {  }
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            if(name == "")
            {
                MessageBox.Show("Name is not valid.");
                txt_name.BackColor = error_col;
                return;
            }
            else
                txt_name.BackColor = normal_col;
        }

        private void txt_name_TextChanged(object sender, EventArgs e)
        {
            name = txt_name.Text;
        }

        private void PasswordForm_Load(object sender, EventArgs e)
        {

        }

        private void txt_price_TextChanged(object sender, EventArgs e)
        {
            price = txt_price.Text;
        }

        private void txt_cal_TextChanged(object sender, EventArgs e)
        {
            cal = txt_cal.Text;
        }

        private void txt_fat_TextChanged(object sender, EventArgs e)
        {
            fat = txt_fat.Text;
        }

        private void txt_protein_TextChanged(object sender, EventArgs e)
        {
            protein = txt_protein.Text;
        }

        private void txt_carb_TextChanged(object sender, EventArgs e)
        {
            carb = txt_carb.Text;
        }

        private void com_category_SelectedIndexChanged(object sender, EventArgs e)
        {
            category = com_category.SelectedItem.ToString();
        }

        private void IngredientFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (txt_name.BackColor == error_col)
                e.Cancel = true;
        }
    }
}

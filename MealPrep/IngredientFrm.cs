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

}

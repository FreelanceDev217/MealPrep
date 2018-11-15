using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBUtil;
using XlsFile;

namespace MealPrep
{
    public partial class MainFrm : Form
    {
        public xlsf m_xls = new xlsf();

        public SQLiteWrapper m_sqlite = new SQLiteWrapper();
        public Dictionary<string, string> m_cat_dict = new Dictionary<string, string>();
        public Dictionary<string, string> m_ing_dict = new Dictionary<string, string>();
        public Dictionary<string, string> m_ing_cat_dict = new Dictionary<string, string>();
        public List<List<string>> m_finder = new List<List<string>>();

        public DataTable m_dt = new DataTable();
        public DataRow m_work_row;
        public float m_price_sum = 0;
        public float m_cal_sum = 0;

        public float m_fat_sum = 0;
        public float m_pro_sum = 0;
        public float m_carbo_sum = 0;

        public float m_min_price = 0;
        public float m_max_price = 0;
        public float m_min_cal = 0;
        public float m_max_cal = 0;

        public List<string> m_like_cat = new List<string>();
        public MainFrm()
        {
            InitializeComponent();
            init_connection();
        }

        void init_connection()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "meal.db";
            m_sqlite.CreatConnection(path);
        }

        void init_ui()
        {
            DataGridViewCellStyle cell_style = new DataGridViewCellStyle();
            cell_style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cell_style.ForeColor = Color.Black;
            cell_style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid_ingred.ColumnHeadersDefaultCellStyle = cell_style;
            grid_ingred.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            refresh_category();
            refresh_ingredient();
        }

        void refresh_ingredient()
        {
            m_ing_dict.Clear();
            m_ing_cat_dict.Clear();
            DataTable dt = m_sqlite.ExecuteQuery("select A.id as ID, A.name as Name, B.name as Category, A.calorie as Calories, A.price as Price, A.fat as Fat, A.protein as Protein, A.carbo  as Carbo from tbl_ingredient A left join tbl_category B on A.category = B.id where A.del_flag is 0 AND B.del_flag is 0");
            grid_ingred.DataSource = dt;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                m_ing_dict.Add(dt.Rows[i][1].ToString(), dt.Rows[i][0].ToString());
                m_ing_cat_dict.Add(dt.Rows[i][1].ToString(), dt.Rows[i][2].ToString());
            }
        }

        void refresh_category()
        {
            m_cat_dict.Clear();
            lis_category.Items.Clear();
            DataTable dt = m_sqlite.ExecuteQuery("select id, name from tbl_category where del_flag is 0");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                m_cat_dict.Add(dt.Rows[i][1].ToString(), dt.Rows[i][0].ToString());
                lis_category.Items.Add(dt.Rows[i][1].ToString());
            }
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            init_ui();
        }

        private void btn_del_category_Click(object sender, EventArgs e)
        {
            var sel = lis_category.SelectedItem;
            if (sel == null)
                return;
            string name = sel.ToString();
            if (MessageBox.Show(String.Format("Are you sure to remove the selected category: {0}", name), "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            string id;
            if(m_cat_dict.TryGetValue(name, out id))
            {
                m_sqlite.ExecuteNonQuery("update tbl_category set del_flag = 1 where id = " + id);
                lis_category.Items.Remove(sel);
                m_cat_dict.Remove(name);
                refresh_ingredient();
            }
        }

        private void btn_add_category_Click(object sender, EventArgs e)
        {
            SimpleInputFrm frm = new SimpleInputFrm();
            if(frm.ShowDialog() == DialogResult.OK && frm.content != "")
            {
                DataTable dt = m_sqlite.ExecuteQuery(String.Format("select count(id) from tbl_category where del_flag=0 and name='" + frm.content + "'"));
                if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() != "0")
                {
                    MessageBox.Show("Category with that name already exists.");
                    return;
                }
                m_sqlite.ExecuteNonQuery(String.Format("insert into tbl_category (name, del_flag) values ('{0}',0)", frm.content));
                refresh_category();
            }
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_xls.excelApp.ActiveWorkbook != null)
            {
                m_xls.CloseFile();
            }
        }
    }
}

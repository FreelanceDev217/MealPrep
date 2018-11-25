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

        private void lis_category_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sel = lis_category.SelectedItem;
            if (sel == null)
                return;
            string name = sel.ToString();
            if (MessageBox.Show(String.Format("Are you sure to remove the selected category: {0}", name), "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            string id;
        }

        private void btn_add_ingred_Click(object sender, EventArgs e)
        {
            if(m_cat_dict.Count <= 0)
            {
                MessageBox.Show("There's no valid category. Please add categories first.");
                return;
            }
            IngredientFrm frm = new IngredientFrm(this);
            if(frm.ShowDialog() == DialogResult.OK)
            {
                string cat_id;
                if (m_cat_dict.TryGetValue(frm.category, out cat_id) == false)
                    return;
                m_sqlite.ExecuteNonQuery(String.Format("insert into tbl_ingredient (name, category, calorie, price, fat, protein, carbo, del_flag) values ('{0}',{1}, {2}, {3}, {4}, {5}, {6}, 0)", 
                                                                                frm.name, cat_id, str2flt(frm.cal), str2flt(frm.price), str2flt(frm.fat), str2flt(frm.protein), str2flt(frm.carb)));
                refresh_ingredient();
            }
        }

        float str2flt(string str)
        {
            float ret;
            try
            {
                if (float.TryParse(str, out ret) == false)
                    return 0;
            }
            catch (Exception ex)
            {
                ret = 0;
            }
            return ret;
        }

        private void btn_del_ingred_Click(object sender, EventArgs e)
        {
            if (grid_ingred.SelectedRows.Count <= 0)
                return;
            if(MessageBox.Show("Are you sure to remove the selected ingredients?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                for(int i = 0; i < grid_ingred.SelectedRows.Count; i ++)
                {
                    string id = grid_ingred.SelectedRows[i].Cells[0].Value.ToString();
                    m_sqlite.ExecuteNonQuery("update tbl_ingredient set del_flag = 1 where id = " + id);
                    refresh_ingredient();
                }
            }
        }

        private void btn_edit_ingred_Click(object sender, EventArgs e)
        {
            if (grid_ingred.SelectedRows.Count != 1)
                return;
            string id = grid_ingred.SelectedRows[0].Cells[0].Value.ToString();
            IngredientFrm frm = new IngredientFrm(this, id);
            if(frm.ShowDialog() == DialogResult.OK)
            {
                string cat_id;
                if (m_cat_dict.TryGetValue(frm.category, out cat_id) == false)
                    return;
                m_sqlite.ExecuteNonQuery(String.Format("update tbl_ingredient set name='{0}', category='{1}', calorie='{2}', price='{3}', fat='{4}', protein='{5}', carbo='{6}' where id='{7}'",
                                                                                frm.name, cat_id, str2flt(frm.cal), str2flt(frm.price), str2flt(frm.fat), str2flt(frm.protein), str2flt(frm.carb), id));
                refresh_ingredient();
            }
        }

        private void btn_add_like_Click(object sender, EventArgs e)
        {
            IngSelectFrm ing_frm = new IngSelectFrm(m_ing_dict);
            if(ing_frm.ShowDialog() == DialogResult.OK)
            {
                if(lis_like.Items.IndexOf(ing_frm.content) >= 0)
                {
                    MessageBox.Show("You already have that category in your like list");
                    return;
                }
                lis_like.Items.Add(ing_frm.content);
                update_like_cat();
            }
        }

        private void btn_del_like_Click(object sender, EventArgs e)
        {
            var sel = lis_like.SelectedItem;
            if (sel == null)
                return;
            string name = sel.ToString();
            if (MessageBox.Show(String.Format("Are you sure to remove the selected category: {0}", name), "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            lis_like.Items.Remove(sel);
            update_like_cat();
        }

        private void btn_add_dislike_Click(object sender, EventArgs e)
        {
            IngSelectFrm ing_frm = new IngSelectFrm(m_ing_dict);
            if (ing_frm.ShowDialog() == DialogResult.OK)
            {
                if (lis_dislike.Items.IndexOf(ing_frm.content) >= 0)
                {
                    MessageBox.Show("You already have that category in your dislike list");
                    return;
                }
                lis_dislike.Items.Add(ing_frm.content);
            }
        }

        private void btn_del_dislike_Click(object sender, EventArgs e)
        {
            var sel = lis_dislike.SelectedItem;
            if (sel == null)
                return;
            string name = sel.ToString();
            if (MessageBox.Show(String.Format("Are you sure to remove the selected category: {0}", name), "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            lis_dislike.Items.Remove(sel);
        }

        private void update_like_cat()
        {
            m_like_cat.Clear();
            for(int i = 0; i < lis_like.Items.Count; i ++)
            {
                string cat;
                if (m_ing_cat_dict.TryGetValue(lis_like.Items[i].ToString(), out cat) == true)
                    m_like_cat.Add(cat);
            }
        }

        private void btn_find_plan_Click(object sender, EventArgs e)
        {
            if (lis_category.Items.Count == 0)
                return;

            m_finder.Clear();
            m_dt = new DataTable();
            m_dt.Columns.Add("No");
            for (int i = 0; i < lis_category.Items.Count; i++)
            {
                string cat_name = lis_category.Items[i].ToString();
                m_dt.Columns.Add(cat_name);

                List<string> ing = new List<string>();
                DataTable dt = m_sqlite.ExecuteQuery(String.Format("SELECT a.id, a.name FROM tbl_ingredient a LEFT JOIN tbl_category b ON a.category = b.id WHERE b.name = '{0}'  AND a.del_flag=0 AND b.del_flag=0", cat_name));
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    if (m_like_cat.IndexOf(cat_name) >= 0 && lis_like.Items.IndexOf(dt.Rows[j][1].ToString()) < 0)
                        continue;
                    ing.Add(dt.Rows[j][0].ToString());
                }
                if(ing.Count == 0)
                {
                    MessageBox.Show("Input data not complete yet.");
                    return;
                }
                m_finder.Add(ing);
            }
            m_dt.Columns.Add("Price");
            m_dt.Columns.Add("Calories");
            m_dt.Columns.Add("Fat");
            m_dt.Columns.Add("Protein");
            m_dt.Columns.Add("Carbo");

            if (txt_cal_from.Text != "")
                float.TryParse(txt_cal_from.Text, out m_min_cal);
            else
                m_min_cal = float.MinValue;

            if (txt_cal_to.Text != "")
                float.TryParse(txt_cal_to.Text, out m_max_cal);
            else
                m_max_cal = float.MaxValue;

            if (txt_price_from.Text != "")
                float.TryParse(txt_price_from.Text, out m_min_price);
            else
                m_min_price = float.MinValue;

            if (txt_price_to.Text != "")
                float.TryParse(txt_price_to.Text, out m_max_price);
            else
                m_max_price = float.MaxValue;

            m_work_row = m_dt.NewRow();

            find_plan(0);

            grid_plan.DataSource = m_dt;
        }

        private void find_plan(int cat_id)
        {
            if(cat_id == m_finder.Count)
            {
                m_work_row["No"] = m_dt.Rows.Count + 1;
                m_work_row["Price"] = m_price_sum;
                m_work_row["Calories"] = m_cal_sum;
                m_work_row["Fat"] = m_fat_sum;
                m_work_row["Protein"] = m_pro_sum;
                m_work_row["Carbo"] = m_carbo_sum;
                m_dt.Rows.Add(m_work_row);
                m_work_row = m_dt.NewRow();
            }
            else
            {
                for(int i = 0; i < m_finder[cat_id].Count; i ++)
                {
                    string ing_id = m_finder[cat_id][i];
                    DataTable dt = m_sqlite.ExecuteQuery(String.Format("select name, price, calorie, fat, protein, carbo from tbl_ingredient where del_flag = 0 and id = {0}", ing_id));
                    if (dt.Rows.Count == 0)
                        continue;
                    string name = dt.Rows[0][0].ToString();
                    float price, cal, fat, pro, carbo;
                    if(float.TryParse(dt.Rows[0][1].ToString(), out price) == false ||
                        float.TryParse(dt.Rows[0][2].ToString(), out cal) == false ||
                        float.TryParse(dt.Rows[0][3].ToString(), out fat) == false ||
                        float.TryParse(dt.Rows[0][4].ToString(), out pro) == false ||
                        float.TryParse(dt.Rows[0][5].ToString(), out carbo) == false)
                        continue;

                    if (price + m_price_sum < m_min_price || price + m_price_sum > m_max_price ||
                        cal + m_cal_sum < m_min_cal || cal + m_cal_sum > m_max_cal)
                        continue;

                    if (lis_dislike.Items.IndexOf(name) >= 0)
                        continue;

                    m_price_sum += price;
                    m_cal_sum += cal;
                    m_fat_sum += fat;
                    m_pro_sum += pro;
                    m_carbo_sum += carbo;
                    m_work_row[cat_id+1] = name;
                    find_plan(cat_id + 1);
                    m_price_sum -= price;
                    m_cal_sum -= cal;
                    m_fat_sum -= fat;
                    m_pro_sum -= pro;
                    m_carbo_sum -= carbo;

                }
            }
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

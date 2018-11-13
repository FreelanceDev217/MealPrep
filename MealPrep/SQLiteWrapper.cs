using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.SQLite;
using System.IO;

namespace DBUtil
{
    public class SQLiteWrapper
    {
        public SQLiteConnection     sql_con;
        public SQLiteCommand        sql_cmd;
        private System.Object locker = new System.Object();
        public bool log = false;
        public SQLiteWrapper() { }
        public void OutLog(string str)
        {
            if (log == false)
                return;
            using (StreamWriter writer = new StreamWriter("mysql_log.txt", true))
            {
                writer.WriteLine(str);
            }
        }
        public void CreatConnection(string file_name)
        {
            OutLog(">> Create SQLite Connection");
            try
            {
                //string conn = "Data Source=" + file_name + ";Version=3;New=True;Compress=True; Password=" + Constants.HARD_PASS;
                string conn = "Data Source=" + file_name + ";Version=3;New=True;Compress=True;";
                sql_con = new SQLiteConnection(conn);
            }
            catch
            {
                OutLog("!! Create SQLite Connection Failed");
            }
            OutLog("<< Create SQLite Connection");
        }

        public void Open()
        {
            sql_con.Open();
        }

        public void Close()
        {
            sql_con.Close();
        }

        public ArrayList GetTables()
        {
            OutLog(">> Get Tables");
            string query = "SELECT name FROM sqlite_master " +
                                        "WHERE type IN ('table','view') AND name NOT LIKE 'sqlite_%'" +
                                        "UNION ALL " +
                                        "SELECT name FROM sqlite_temp_master " +
                                        "WHERE type IN ('table','view') " +
                                        "ORDER BY 1";
            DataTable table = ExecuteQuery(query);

            ArrayList list = new ArrayList();
            foreach (DataRow row in table.Rows)
            {
                list.Add(row.ItemArray[0].ToString());
            }
            OutLog("<< Get Tables : " + list.Count.ToString());
            return list;
        }

        public void ExecuteNonQuery(string txtQuery)
        {
            lock (locker)
            {
                OutLog("\t\t#LOCAL# Execute SQLite NonQuery: " + txtQuery);
                Open();
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = txtQuery;
                sql_cmd.ExecuteNonQuery();
                Close();
            }
        }

        public DataTable ExecuteQuery(string txtQuery)
        {
            lock (locker)
            {
                Open();
                DataTable dt = new DataTable();
                sql_cmd = sql_con.CreateCommand();
                SQLiteDataAdapter DB = new SQLiteDataAdapter(txtQuery, sql_con);
                DB.SelectCommand.CommandType = CommandType.Text;
                DB.Fill(dt);
                Close();
                OutLog("\t\t#LOCAL# Execute SQLite Query: " + txtQuery + " -> " + dt.Rows.Count.ToString());
                return dt;
            }
        }
    }
}
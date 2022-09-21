using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File
{
    internal class Search_from_Mysql
    {
        StringBuilder stringHistory;
        StringBuilder stringHistoryTemp;
        
        public StringBuilder SearchT30FromMysql(MySqlConnection conn)
        {
            stringHistoryTemp = new StringBuilder();

            // 查詢資料表全部資料
            string sql = "SELECT * From t30.t30";
            try
            {
                using (MySqlCommand mySqlCommand = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                    {
                        if (mySqlDataReader.HasRows)
                        {
                            while (mySqlDataReader.Read())
                            {
                                stringHistoryTemp.AppendLine(String.Format("股票編號: {0} \t 股票名字: {1}", mySqlDataReader["STOCK-NO"], mySqlDataReader["STOCK-NAME"]));
                            }
                            stringHistory = stringHistoryTemp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return stringHistory;
        }
        public StringBuilder SearchHFP085FromMysql(MySqlConnection conn)
        {
            stringHistoryTemp = new StringBuilder();

            // 查詢資料表全部資料
            string sql = "SELECT * From t30.hfp085";
            try
            {
                using (MySqlCommand mySqlCommand = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                    {
                        if (mySqlDataReader.HasRows) // 有資料就讀
                        {
                            while (mySqlDataReader.Read())
                            {
                                stringHistoryTemp.AppendLine(String.Format("證券代號: {0} \t 市場別: {1}", mySqlDataReader["CFM02"], mySqlDataReader["CFM03"]));
                            }
                            stringHistory = stringHistoryTemp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return stringHistory;
        }
    }
}

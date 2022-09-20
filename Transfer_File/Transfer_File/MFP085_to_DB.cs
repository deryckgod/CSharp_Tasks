using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File
{
    class MFP085_to_DB : Txt_to_DB
    {
        ESMP.HFP085DataTable hFP085Rows;
        HFP085_Data hfp085_data_get_set;
        Move_File move_File;
        StringBuilder stringHistoryTemp;

        string path = ConfigurationManager.AppSettings["path"];
        string destinationPath = ConfigurationManager.AppSettings["destinationPath"];
        public override StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, string fileString)
        {
            List<string> fileStringList = new List<string>();
            byte[] currentByteString = new byte[154]; // 存取當前100個byte資料

            hFP085Rows = new ESMP.HFP085DataTable(); // xsd 裝載
            hfp085_data_get_set = new HFP085_Data();
            move_File = new Move_File();
            stringHistoryTemp = new StringBuilder();

            if (ReadFileToString(fileString, ref fileStringList, false))
            {
                // 在文字框顯示資料
                stringHistoryTemp.AppendLine(String.Format("收到檔案: {0} \r", fileString));

                foreach (string mainString in fileStringList)
                {
                    byte[] lineString = Encoding.GetEncoding(950).GetBytes(mainString);

                    int count = 0;
                    // 插入資料到DB
                    using (MySqlCommand mySqlCommand = mySqlConnection.CreateCommand())
                    {
                        mySqlCommand.CommandText = "insert into t30.hfp085  (CFM01,CFM02,CFM03,CFM04,CFM05,CFM06,CFM07,CFM08,CFM09,CFM10,CFM11,CFM12,CFM13,CFM14,CFM15,CFM16,CFM17,CFM18,CFM19,CFM20,CFM21,CFM22,CFM23,CFM24,CFM25) values (@cfm01,@cfm02,@cfm03,@cfm04,@cfm05,@cfm06,@cfm07,@cfm08,@cfm09,@cfm10,@cfm11,@cfm12,@cfm13,@cfm14,@cfm15,@cfm16,@cfm17,@cfm18,@cfm19,@cfm20,@cfm21,@cfm22,@cfm23,@cfm24,@cfm25)";

                        // 每個資料都是100位元，所以以100為底跳著讀
                        for (int totalLength = 0; totalLength < lineString.Length; totalLength += 153)
                        {
                            Array.Copy(lineString, totalLength, currentByteString, 0, 153);

                            hfp085_data_get_set.Cfm01 = Encoding.GetEncoding(950).GetString(currentByteString, 0, 6);
                            hfp085_data_get_set.Cfm02 = Encoding.GetEncoding(950).GetString(currentByteString, 6, 9);
                            hfp085_data_get_set.Cfm03 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 15, 9));
                            hfp085_data_get_set.Cfm04 = Encoding.GetEncoding(950).GetString(currentByteString, 24, 9);
                            hfp085_data_get_set.Cfm05 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 33, 8));
                            hfp085_data_get_set.Cfm06 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 41, 1));
                            hfp085_data_get_set.Cfm07 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 42, 1));
                            hfp085_data_get_set.Cfm08 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 43, 1));
                            hfp085_data_get_set.Cfm09 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 44, 1));
                            hfp085_data_get_set.Cfm10 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 44, 1));
                            hfp085_data_get_set.Cfm11 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 44, 1));
                            hfp085_data_get_set.Cfm12 = Encoding.GetEncoding(950).GetString(currentByteString, 49, 1);

                            // STOCK_NAME
                            hfp085_data_get_set.Cfm13 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));

                            // MARK_W_DETAILS
                            hfp085_data_get_set.Cfm14 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 66, 3));
                            hfp085_data_get_set.Cfm15 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 69, 6));
                            hfp085_data_get_set.Cfm16 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 75, 6));
                            hfp085_data_get_set.Cfm17 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            hfp085_data_get_set.Cfm18 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            hfp085_data_get_set.Cfm19 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            hfp085_data_get_set.Cfm20 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            hfp085_data_get_set.Cfm21 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            hfp085_data_get_set.Cfm22 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            hfp085_data_get_set.Cfm23 = Encoding.GetEncoding(950).GetString(currentByteString, 88, 12);
                            hfp085_data_get_set.Cfm24 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            hfp085_data_get_set.Cfm25 = Encoding.GetEncoding(950).GetString(currentByteString, 88, 12);
                            //stringHistoryTemp.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));

                            mySqlCommand.Parameters.Clear(); // 每次插入都先清除引數
                            try
                            {
                                // 待處理
                                //mySqlCommand.Parameters.Add(new MySqlParameter("cfm01", hfp085_data_get_set.Stock_No));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("BULL_PRICE", hfp085_data_get_set.Bull_Price));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("LDC_PRICE", hfp085_data_get_set.Ldc_Price));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("BEAR_PRICE", hfp085_data_get_set.Bear_Price));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("LAST_MTH_DATE", hfp085_data_get_set.Last_Mth_Date));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("SETTYPE", hfp085_data_get_set.SetType));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MARK_W", hfp085_data_get_set.Mark_W));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MARK_P", hfp085_data_get_set.Mark_P));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MARK_L", hfp085_data_get_set.Mark_L));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("IND_CODE", hfp085_data_get_set.Ind_Code));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("STK_CODE", hfp085_data_get_set.Stk_Code));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MARK_M", hfp085_data_get_set.Mark_M));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NAME", hfp085_data_get_set.Stock_Name));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MATCH_INTERVAL", hfp085_data_get_set.Match_Interval));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("ORDER_LIMIT", hfp085_data_get_set.Order_Limit));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("ORDERS_LIMIT", hfp085_data_get_set.Orders_Limit));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("PREPAY_RATE", hfp085_data_get_set.Prepay_Rate));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MARK_S", hfp085_data_get_set.Mark_S));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MARK_F", hfp085_data_get_set.Mark_F));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("MARK_DAY_TRADE", hfp085_data_get_set.Mark_Day_Trade));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("STK_CTGCD", hfp085_data_get_set.Stk_CTGCD));
                                //mySqlCommand.Parameters.Add(new MySqlParameter("FILLER", hfp085_data_get_set.Filler));
                                //t30_xsd_rows.AddT30Row(hfp085_data_get_set.Stock_No, hfp085_data_get_set.Bull_Price, hfp085_data_get_set.Ldc_Price, hfp085_data_get_set.Bear_Price,
                                //    hfp085_data_get_set.Last_Mth_Date, hfp085_data_get_set.SetType, hfp085_data_get_set.Mark_W, hfp085_data_get_set.Mark_P, hfp085_data_get_set.Mark_L,
                                //    hfp085_data_get_set.Ind_Code, hfp085_data_get_set.Stk_Code, hfp085_data_get_set.Mark_M, hfp085_data_get_set.Stock_Name, hfp085_data_get_set.Match_Interval,
                                //    hfp085_data_get_set.Order_Limit, hfp085_data_get_set.Orders_Limit, hfp085_data_get_set.Prepay_Rate, hfp085_data_get_set.Mark_S, hfp085_data_get_set.Mark_F,
                                //    hfp085_data_get_set.Mark_Day_Trade, hfp085_data_get_set.Stk_CTGCD, hfp085_data_get_set.Filler);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }

                            if (mySqlCommand.ExecuteNonQuery() > 0)
                            {
                                count++;
                            }
                        }
                    }
                    stringHistoryTemp.AppendLine(String.Format("{0} 存入DB完畢 共存入{1}筆\r", fileString, count));
                    stringHistoryTemp.AppendLine(move_File.MoveFile(fileString, destinationPath)); // 轉移處理完的檔案 並讓stringHistoryTemp暫存轉移的log

                    string xsdFile = @"D:\Desktop\ALPED\Systex\Git_Repository\CSharp_Tasks\deryckgod\CSharp_Tasks\Transfer_File\Transfer_File\ESMP.xsd";
                    hFP085Rows.WriteXmlSchema(xsdFile);
                    string xmlFile = @"D:\Desktop\ALPED\Systex\Git_Repository\CSharp_Tasks\deryckgod\CSharp_Tasks\Transfer_File\Transfer_File\ESMP.xml";
                    hFP085Rows.WriteXml(xmlFile);
                }
                return stringHistoryTemp;
            }
            else
            {
                MessageBox.Show("Error");
                return null;
            }
        }
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File.File_to_DB
{
    internal class T30_to_DB : Txt_to_DB
    {
        string destinationPath = ConfigurationManager.AppSettings["destinationPath"];
        public override StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, string fileString)
        {
            List<string> fileStringList = new List<string>();
            byte[] currentByteString = new byte[101]; // 存取當前100個byte資料

            Move_File move_file = new Move_File();
            StringBuilder stringHistoryTemp = new StringBuilder();
            ESMP.T30DataTable t30_xsd_rows = new ESMP.T30DataTable(); // xsd 裝載
            var t30Rows = t30_xsd_rows.NewT30Row();
            
            if (ReadFileToString(fileString, ref fileStringList, false))
            {
                // 在文字框顯示資料
                stringHistoryTemp.AppendLine(string.Format("收到檔案: {0} \r", fileString));
                int count = 0;
                foreach (string mainString in fileStringList)
                {
                    byte[] lineString = Encoding.GetEncoding(950).GetBytes(mainString);

                    // 插入資料到DB
                    using (MySqlCommand mySqlCommand = mySqlConnection.CreateCommand())
                    {
                        mySqlCommand.CommandText = "insert into t30.t30  (`STOCK-NO`,`BULL-PRICE`,`LDC-PRICE`,`BEAR-PRICE`, `LAST-MTH-DATE`,`SETTYPE`,`MARK-W`,`MARK-P`,`MARK-L`,`IND-CODE`,`STK-CODE`,`MARK-M`,`STOCK-NAME`,`MATCH-INTERVAL`, `ORDER-LIMIT`,`ORDERS-LIMIT`,`PREPAY-RATE`,`MARK-S`,`MARK-F`,`MARK-DAY-TRADE`,`STK-CTGCD`,`FILLER`) values (@STOCK_NO, @BULL_PRICE, @LDC_PRICE, @BEAR_PRICE, @LAST_MTH_DATE, @SETTYPE, @MARK_W, @MARK_P, @MARK_L, @IND_CODE, @STK_CODE, @MARK_M, @STOCK_NAME, @MATCH_INTERVAL, @ORDER_LIMIT, @ORDERS_LIMIT, @PREPAY_RATE, @MARK_S, @MARK_F,@MARK_DAY_TRADE, @STK_CTGCD, @FILLER)";

                        // 每個資料都是100位元，所以以100為底跳著讀
                        for (int totalLength = 0; totalLength < lineString.Length; totalLength += 100)
                        {
                            Array.Copy(lineString, totalLength, currentByteString, 0, 100);
                            #region 變數賦值
                            t30Rows._STOCK_NO= Encoding.GetEncoding(950).GetString(currentByteString, 0, 6);
                            t30Rows._BULL_PRICE = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 6, 9)) / 10000;
                            t30Rows._LDC_PRICE = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 15, 9)) / 10000;
                            t30Rows._BEAR_PRICE = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 24, 9)) / 10000;
                            t30Rows._LAST_MTH_DATE = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 33, 8));
                            t30Rows.SETTYPE= Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 41, 1));
                            t30Rows._MARK_W= Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 42, 1));
                            t30Rows._MARK_P = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 43, 1));
                            t30Rows._MARK_L = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 44, 1));
                            t30Rows._IND_CODE = Encoding.GetEncoding(950).GetString(currentByteString, 45, 2);
                            t30Rows._STK_CODE = Encoding.GetEncoding(950).GetString(currentByteString, 47, 2);
                            t30Rows._MARK_M = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 49, 1));

                            // STOCK_NAME
                            t30Rows._STOCK_NAME = Encoding.GetEncoding(950).GetString(currentByteString, 50, 16);

                            // MARK_W_DETAILS
                            t30Rows._MATCH_INTERNAL = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 66, 3));
                            t30Rows._ORDER_LIMIT = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 69, 6));
                            t30Rows._ORDERS_LIMIT = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 75, 6));
                            t30Rows._PREPAY_RATE = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 81, 3));
                            t30Rows._MARK_S = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 84, 1));
                            t30Rows._MARK_F = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 85, 1));
                            t30Rows._MARK_DAY_TRADE = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 86, 1));
                            t30Rows._STK_CTGCD = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 87, 1));
                            t30Rows.FILLER = Encoding.GetEncoding(950).GetString(currentByteString, 88, 12);

                            //stringHistoryTemp.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));
                            #endregion
                            mySqlCommand.Parameters.Clear();

                            #region 添加參數
                            try
                            {
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NO", t30Rows._STOCK_NO));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BULL_PRICE", t30Rows._BULL_PRICE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LDC_PRICE", t30Rows._LDC_PRICE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BEAR_PRICE", t30Rows._BEAR_PRICE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LAST_MTH_DATE", t30Rows._LAST_MTH_DATE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("SETTYPE", t30Rows.SETTYPE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_W", t30Rows._MARK_W));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_P", t30Rows._MARK_P));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_L", t30Rows._MARK_L));
                                mySqlCommand.Parameters.Add(new MySqlParameter("IND_CODE", t30Rows._IND_CODE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CODE", t30Rows._STK_CODE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_M", t30Rows._MARK_M));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NAME", t30Rows._STOCK_NAME));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MATCH_INTERVAL", t30Rows._MATCH_INTERNAL));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDER_LIMIT", t30Rows._ORDER_LIMIT));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDERS_LIMIT", t30Rows._ORDERS_LIMIT));
                                mySqlCommand.Parameters.Add(new MySqlParameter("PREPAY_RATE", t30Rows._PREPAY_RATE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_S", t30Rows._MARK_S));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_F", t30Rows._MARK_F));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_DAY_TRADE", t30Rows._MARK_DAY_TRADE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CTGCD", t30Rows._STK_CTGCD));
                                mySqlCommand.Parameters.Add(new MySqlParameter("FILLER", t30Rows.FILLER));
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                            #endregion

                            if (mySqlCommand.ExecuteNonQuery() > 0)
                            {
                                count++;
                            }
                        }
                    }
                }
                stringHistoryTemp.AppendLine(string.Format("{0} 存入DB完畢 共存入{1}筆\r", fileString, count));
                stringHistoryTemp.AppendLine(move_file.MoveFile(fileString, destinationPath));
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

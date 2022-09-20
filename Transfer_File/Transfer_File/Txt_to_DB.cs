using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File
{
    internal class Txt_to_DB
    {
        T30_Data t30_Data_Get_Set;
        Move_File move_File;
        DirectoryInfo directoryInfo;
        StringBuilder stringHistory;
        StringBuilder stringHistoryTemp;

        string path = ConfigurationManager.AppSettings["path"];
        string destinationPath = ConfigurationManager.AppSettings["destinationPath"];

        public bool ReadFileToString(string fileName, ref List<string> fileStringList, bool noSpace = false)
        {
            fileStringList.Clear();
            if (File.Exists(fileName))
            {
                // 解決.NET Core簡化編碼問題， Big5錯誤訊息消失
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using (StreamReader streamReader = new StreamReader(fileName, Encoding.GetEncoding("big5")))
                {
                    string line = "";
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        fileStringList.Add(line);
                    }
                }
                return true;
            }
            return false;
        }

        public StringBuilder TxtToMysql(MySqlConnection mySqlConnection, ref bool checkFile)
        {
            stringHistoryTemp = new StringBuilder();

            directoryInfo = new DirectoryInfo(path);
            try
            {
                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.TXT"))
                {
                    checkFile = true;
                    InputDataToMysql(mySqlConnection, fileInfo.ToString());
                }
                stringHistory = stringHistoryTemp; // 全部掃完再輸出 也可以解決輸出一半的問題
                checkFile = false; // 原資料夾內處理完畢
            }
            catch(Exception e)
            {
                MessageBox.Show("匯入MySql例外 : "+e.Message);
            }
            return stringHistory;
        }

        public StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, String fileString)
        {
            List<string> fileStringList = new List<string>();
            byte[] currentByteString = new byte[101]; // 存取當前100個byte資料

            t30_Data_Get_Set = new T30_Data();
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
                        mySqlCommand.CommandText = "insert into t30.t30  (`STOCK-NO`,`BULL-PRICE`,`LDC-PRICE`,`BEAR-PRICE`, `LAST-MTH-DATE`,`SETTYPE`,`MARK-W`,`MARK-P`,`MARK-L`,`IND-CODE`,`STK-CODE`,`MARK-M`,`STOCK-NAME`,`MATCH-INTERVAL`, `ORDER-LIMIT`,`ORDERS-LIMIT`,`PREPAY-RATE`,`MARK-S`,`MARK-F`,`MARK-DAY-TRADE`,`STK-CTGCD`,`FILLER`) values (@STOCK_NO, @BULL_PRICE, @LDC_PRICE, @BEAR_PRICE, @LAST_MTH_DATE, @SETTYPE, @MARK_W, @MARK_P, @MARK_L, @IND_CODE, @STK_CODE, @MARK_M, @STOCK_NAME, @MATCH_INTERVAL, @ORDER_LIMIT, @ORDERS_LIMIT, @PREPAY_RATE, @MARK_S, @MARK_F,@MARK_DAY_TRADE, @STK_CTGCD, @FILLER)";

                        // 每個資料都是100位元，所以以100為底跳著讀
                        for (int totalLength = 0; totalLength < lineString.Length; totalLength += 100)
                        {
                            Array.Copy(lineString, totalLength, currentByteString, 0, 100);

                            t30_Data_Get_Set.Stock_No = Encoding.GetEncoding(950).GetString(currentByteString, 0, 6);
                            t30_Data_Get_Set.Bull_Price = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 6, 9)) / 10000);
                            t30_Data_Get_Set.Ldc_Price = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 15, 9)) / 10000);
                            t30_Data_Get_Set.Bear_Price = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 24, 9)) / 10000);
                            t30_Data_Get_Set.Last_Mth_Date = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 33, 8));
                            t30_Data_Get_Set.SetType = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 41, 1));
                            t30_Data_Get_Set.Mark_W = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 42, 1));
                            t30_Data_Get_Set.Mark_P = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 43, 1));
                            t30_Data_Get_Set.Mark_L = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  44, 1));
                            t30_Data_Get_Set.Ind_Code = Encoding.GetEncoding(950).GetString(currentByteString, 45, 2);
                            t30_Data_Get_Set.Stk_Code = Encoding.GetEncoding(950).GetString(currentByteString, 47, 2);
                            t30_Data_Get_Set.Mark_M = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  49, 1));

                            // STOCK_NAME
                            t30_Data_Get_Set.Stock_Name = Encoding.GetEncoding(950).GetString(currentByteString, 50, 16);

                            // MARK_W_DETAILS
                            t30_Data_Get_Set.Match_Interval = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 66, 3));
                            t30_Data_Get_Set.Order_Limit = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString,  69, 6));
                            t30_Data_Get_Set.Orders_Limit = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString,  75, 6));
                            t30_Data_Get_Set.Prepay_Rate = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString,  81, 3));
                            t30_Data_Get_Set.Mark_S = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 84, 1));
                            t30_Data_Get_Set.Mark_F = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 85, 1));
                            t30_Data_Get_Set.Mark_Day_Trade = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  86, 1));
                            t30_Data_Get_Set.Stk_CTGCD = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  87, 1));
                            t30_Data_Get_Set.Filler = Encoding.GetEncoding(950).GetString(currentByteString, 88, 12);

                            //stringHistoryTemp.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));

                            mySqlCommand.Parameters.Clear(); // 每次插入都先清除引數
                            try
                            {
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NO", t30_Data_Get_Set.Stock_No));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BULL_PRICE", t30_Data_Get_Set.Bull_Price));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LDC_PRICE", t30_Data_Get_Set.Ldc_Price));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BEAR_PRICE", t30_Data_Get_Set.Bear_Price));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LAST_MTH_DATE", t30_Data_Get_Set.Last_Mth_Date));
                                mySqlCommand.Parameters.Add(new MySqlParameter("SETTYPE", t30_Data_Get_Set.SetType));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_W", t30_Data_Get_Set.Mark_W));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_P", t30_Data_Get_Set.Mark_P));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_L", t30_Data_Get_Set.Mark_L));
                                mySqlCommand.Parameters.Add(new MySqlParameter("IND_CODE", t30_Data_Get_Set.Ind_Code));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CODE", t30_Data_Get_Set.Stk_Code));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_M", t30_Data_Get_Set.Mark_M));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NAME", t30_Data_Get_Set.Stock_Name));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MATCH_INTERVAL", t30_Data_Get_Set.Match_Interval));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDER_LIMIT", t30_Data_Get_Set.Order_Limit));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDERS_LIMIT", t30_Data_Get_Set.Orders_Limit));
                                mySqlCommand.Parameters.Add(new MySqlParameter("PREPAY_RATE", t30_Data_Get_Set.Prepay_Rate));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_S", t30_Data_Get_Set.Mark_S));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_F", t30_Data_Get_Set.Mark_F));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_DAY_TRADE", t30_Data_Get_Set.Mark_Day_Trade));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CTGCD", t30_Data_Get_Set.Stk_CTGCD));
                                mySqlCommand.Parameters.Add(new MySqlParameter("FILLER", t30_Data_Get_Set.Filler));
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

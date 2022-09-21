using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer_File.DataTableFolder;

namespace Transfer_File
{
    internal class Txt_to_DB
    {
        T30_Data t30_data_get_set;
        Move_File move_file;
        DirectoryInfo directoryInfo;
        StringBuilder stringHistory;
        ESMP.T30DataTable t30_xsd_rows;
        MFP085_to_DB mfp085_to_db;

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

        public virtual StringBuilder TxtToMysql(MySqlConnection mySqlConnection, ref bool checkFile, ref bool checkOnCreated)
        {
            mfp085_to_db = new MFP085_to_DB();
            StringBuilder stringHistoryTemp = new StringBuilder(); // 不能設成全域變數否則在同個程式中new會洗掉之前MFP085的訊息

            directoryInfo = new DirectoryInfo(path);
            try
            {
                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.TXT"))
                {
                    checkFile = true;
                    // 避免create時 按btnConnect報錯
                    if (!checkOnCreated)
                    {
                        if (fileInfo.ToString().Contains("T30"))
                        {
                            try
                            {
                                stringHistoryTemp.AppendLine(this.InputDataToMysql(mySqlConnection, fileInfo.ToString()).ToString());
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                        }
                        else if (fileInfo.ToString().Contains("MFP085"))
                        {
                            try
                            {
                                stringHistoryTemp.AppendLine(mfp085_to_db.InputDataToMysql(mySqlConnection, fileInfo.ToString()).ToString());
                            }
                            catch(Exception e)
                            {
                                MessageBox.Show( e.Message);
                            }
                        }
                    }
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

        public virtual StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, String fileString)
        {
            List<string> fileStringList = new List<string>();
            byte[] currentByteString = new byte[101]; // 存取當前100個byte資料

            t30_xsd_rows = new ESMP.T30DataTable(); // xsd 裝載
            t30_data_get_set = new T30_Data();
            move_file = new Move_File();
            StringBuilder stringHistoryTemp = new StringBuilder();

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
                            #region 變數指定
                            t30_data_get_set.Stock_No = Encoding.GetEncoding(950).GetString(currentByteString, 0, 6);
                            t30_data_get_set.Bull_Price = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 6, 9)) / 10000);
                            t30_data_get_set.Ldc_Price = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 15, 9)) / 10000);
                            t30_data_get_set.Bear_Price = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 24, 9)) / 10000);
                            t30_data_get_set.Last_Mth_Date = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 33, 8));
                            t30_data_get_set.SetType = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 41, 1));
                            t30_data_get_set.Mark_W = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 42, 1));
                            t30_data_get_set.Mark_P = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 43, 1));
                            t30_data_get_set.Mark_L = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  44, 1));
                            t30_data_get_set.Ind_Code = Encoding.GetEncoding(950).GetString(currentByteString, 45, 2);
                            t30_data_get_set.Stk_Code = Encoding.GetEncoding(950).GetString(currentByteString, 47, 2);
                            t30_data_get_set.Mark_M = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  49, 1));

                            // STOCK_NAME
                            t30_data_get_set.Stock_Name = Encoding.GetEncoding(950).GetString(currentByteString, 50, 16);

                            // MARK_W_DETAILS
                            t30_data_get_set.Match_Interval = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 66, 3));
                            t30_data_get_set.Order_Limit = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString,  69, 6));
                            t30_data_get_set.Orders_Limit = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString,  75, 6));
                            t30_data_get_set.Prepay_Rate = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString,  81, 3));
                            t30_data_get_set.Mark_S = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 84, 1));
                            t30_data_get_set.Mark_F = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 85, 1));
                            t30_data_get_set.Mark_Day_Trade = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  86, 1));
                            t30_data_get_set.Stk_CTGCD = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString,  87, 1));
                            t30_data_get_set.Filler = Encoding.GetEncoding(950).GetString(currentByteString, 88, 12);

                            //stringHistoryTemp.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));
                            #endregion
                            mySqlCommand.Parameters.Clear(); // 每次插入都先清除引數

                            #region 添加參數
                            try
                            {
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NO", t30_data_get_set.Stock_No));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BULL_PRICE", t30_data_get_set.Bull_Price));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LDC_PRICE", t30_data_get_set.Ldc_Price));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BEAR_PRICE", t30_data_get_set.Bear_Price));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LAST_MTH_DATE", t30_data_get_set.Last_Mth_Date));
                                mySqlCommand.Parameters.Add(new MySqlParameter("SETTYPE", t30_data_get_set.SetType));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_W", t30_data_get_set.Mark_W));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_P", t30_data_get_set.Mark_P));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_L", t30_data_get_set.Mark_L));
                                mySqlCommand.Parameters.Add(new MySqlParameter("IND_CODE", t30_data_get_set.Ind_Code));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CODE", t30_data_get_set.Stk_Code));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_M", t30_data_get_set.Mark_M));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NAME", t30_data_get_set.Stock_Name));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MATCH_INTERVAL", t30_data_get_set.Match_Interval));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDER_LIMIT", t30_data_get_set.Order_Limit));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDERS_LIMIT", t30_data_get_set.Orders_Limit));
                                mySqlCommand.Parameters.Add(new MySqlParameter("PREPAY_RATE", t30_data_get_set.Prepay_Rate));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_S", t30_data_get_set.Mark_S));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_F", t30_data_get_set.Mark_F));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_DAY_TRADE", t30_data_get_set.Mark_Day_Trade));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CTGCD", t30_data_get_set.Stk_CTGCD));
                                mySqlCommand.Parameters.Add(new MySqlParameter("FILLER", t30_data_get_set.Filler));
                                t30_xsd_rows.AddT30Row(t30_data_get_set.Stock_No, t30_data_get_set.Bull_Price, t30_data_get_set.Ldc_Price, t30_data_get_set.Bear_Price,
                                    t30_data_get_set.Last_Mth_Date, t30_data_get_set.SetType, t30_data_get_set.Mark_W, t30_data_get_set.Mark_P, t30_data_get_set.Mark_L,
                                    t30_data_get_set.Ind_Code, t30_data_get_set.Stk_Code, t30_data_get_set.Mark_M, t30_data_get_set.Stock_Name, t30_data_get_set.Match_Interval,
                                    t30_data_get_set.Order_Limit, t30_data_get_set.Orders_Limit, t30_data_get_set.Prepay_Rate, t30_data_get_set.Mark_S, t30_data_get_set.Mark_F,
                                    t30_data_get_set.Mark_Day_Trade, t30_data_get_set.Stk_CTGCD, t30_data_get_set.Filler);
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
                    stringHistoryTemp.AppendLine(String.Format("{0} 存入DB完畢 共存入{1}筆\r", fileString, count));
                    stringHistoryTemp.AppendLine(move_file.MoveFile(fileString, destinationPath)); // 轉移處理完的檔案 並讓stringHistoryTemp暫存轉移的log
                    // 暫時用不到
                    //string xsdFile = @"D:\Desktop\ALPED\Systex\Git_Repository\CSharp_Tasks\deryckgod\CSharp_Tasks\Transfer_File\Transfer_File\ESMP.xsd";
                    //t30_xsd_rows.WriteXmlSchema(xsdFile);
                    //string xmlFile = @"D:\Desktop\ALPED\Systex\Git_Repository\CSharp_Tasks\deryckgod\CSharp_Tasks\Transfer_File\Transfer_File\ESMP.xml";
                    //t30_xsd_rows.WriteXml(xmlFile);
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

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

namespace Transfer_File.File_to_DB
{
    internal class Txt_to_DB
    {
        StringBuilder stringHistory;

        string path = ConfigurationManager.AppSettings["path"];

        public bool ReadFileToString(string fileName, ref List<string> fileStringList, bool noSpace = false)
        {
            fileStringList.Clear();
            if (File.Exists(fileName))
            {
                // 解決.NET Core簡化編碼問題， Big5錯誤訊息消失
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                try
                {
                    // StreamReader改用LINQ讀取
                    fileStringList = File.ReadLines(fileName, Encoding.GetEncoding("big5")).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return true;
            }
            return false;
        }

        public StringBuilder TxtToMysql(MySqlConnection mySqlConnection, ref bool checkFile, ref bool checkOnCreated)
        {
            T30_to_DB t30_to_db = new T30_to_DB();
            MFP085_to_DB mfp085_to_db = new MFP085_to_DB();
            StringBuilder stringHistoryTemp = new StringBuilder(); // 不能設成全域變數否則在同個程式中new會洗掉之前MFP085的訊息

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            try
            {
                if (!checkOnCreated)
                {
                    foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.TXT"))
                    {
                        checkFile = true;
                        // 避免有檔案create時 按btnConnect有衝突報錯
                    
                        if (fileInfo.ToString().Contains("T30"))
                        {
                            try
                            {
                                stringHistoryTemp.AppendLine(t30_to_db.InputDataToMysql(mySqlConnection, fileInfo.ToString()).ToString());
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
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                        }
                    }
                }
                stringHistory = stringHistoryTemp;
                checkFile = false; // 原資料夾內處理完畢
            }
            catch (Exception e)
            {
                MessageBox.Show("匯入MySql例外 : " + e.Message);
            }
            return stringHistory;
        }

        public virtual StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, string fileString)
        {
            return stringHistory;
        }
    }
}

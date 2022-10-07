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
    abstract class Txt_to_DB
    {
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
                    //MessageBox.Show("READ FILE THREAD ID : " + Thread.CurrentThread.ManagedThreadId.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return true;
            }
            return false;
        }
        abstract public StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, string fileString, string fileName);

    }
}

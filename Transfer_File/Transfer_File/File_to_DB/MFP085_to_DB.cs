using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer_File.DataTableFolder;

namespace Transfer_File.File_to_DB
{
    class MFP085_to_DB : Txt_to_DB
    {
        string destinationPath = ConfigurationManager.AppSettings["destinationPath"];

        public override StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, string fileString)
        {
            List<string> fileStringList = new List<string>();
            byte[] currentByteString = new byte[154];

            Move_File move_file = new Move_File();
            StringBuilder stringHistoryTemp = new StringBuilder();
            ESMP.MFP085DataTable mFP085_xsd_rows = new ESMP.MFP085DataTable(); // xsd 裝載
            var mfp085Rows = mFP085_xsd_rows.NewMFP085Row();

            if (ReadFileToString(fileString, ref fileStringList, false))
            {
                int count = 0;
                stringHistoryTemp.AppendLine(string.Format("收到檔案: {0} \r", fileString));

                foreach (string mainString in fileStringList)
                {
                    // 宣告字元陣列做處理
                    byte[] lineString = Encoding.GetEncoding(950).GetBytes(mainString);

                    // 插入資料到指定資料表
                    using (MySqlCommand mySqlCommand = mySqlConnection.CreateCommand())
                    {
                        mySqlCommand.CommandText = "insert into t30.mfp085  (CFM01,CFM02,CFM03,CFM04,CFM05,CFM06,CFM07,CFM08,CFM09,CFM10,CFM11,CFM12,CFM13,CFM14,CFM15,CFM16,CFM17,CFM18,CFM19,CFM20,CFM21,CFM22,CFM23,CFM24,CFM25) values (@cfm01,@cfm02,@cfm03,@cfm04,@cfm05,@cfm06,@cfm07,@cfm08,@cfm09,@cfm10,@cfm11,@cfm12,@cfm13,@cfm14,@cfm15,@cfm16,@cfm17,@cfm18,@cfm19,@cfm20,@cfm21,@cfm22,@cfm23,@cfm24,@cfm25)";

                        // 每個資料都是153位元，所以以153為底跳著讀
                        for (int totalLength = 0; totalLength < lineString.Length; totalLength += 153)
                        {
                            Array.Copy(lineString, totalLength, currentByteString, 0, 153);

                            #region 變數賦值
                            mfp085Rows.CFM01 = Encoding.GetEncoding(950).GetString(currentByteString, 0, 8);
                            mfp085Rows.CFM02 = Encoding.GetEncoding(950).GetString(currentByteString, 8, 6);
                            mfp085Rows.CFM03 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 14, 1));
                            mfp085Rows.CFM04 = Encoding.GetEncoding(950).GetString(currentByteString, 15, 4);
                            mfp085Rows.CFM05 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 19, 9)) / 10000;
                            mfp085Rows.CFM06 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 28, 10));
                            mfp085Rows.CFM07 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 38, 3));
                            mfp085Rows.CFM08 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 41, 1));
                            mfp085Rows.CFM09 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 42, 3));
                            mfp085Rows.CFM10 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 45, 4));
                            mfp085Rows.CFM11 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 49, 3)) / 100;
                            mfp085Rows.CFM12 = Encoding.GetEncoding(950).GetString(currentByteString, 52, 1);
                            mfp085Rows.CFM13 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 53, 8));
                            mfp085Rows.CFM14 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 61, 8));
                            mfp085Rows.CFM15 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 69, 8));
                            mfp085Rows.CFM16 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 77, 8));
                            mfp085Rows.CFM17 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 85, 8));
                            mfp085Rows.CFM18 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 93, 8));
                            mfp085Rows.CFM19 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 101, 8));
                            mfp085Rows.CFM20 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 109, 8));
                            mfp085Rows.CFM21 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 117, 8));
                            mfp085Rows.CFM22 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 125, 8));
                            mfp085Rows.CFM23 = Encoding.GetEncoding(950).GetString(currentByteString, 133, 10);
                            mfp085Rows.CFM24 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 143, 8));
                            mfp085Rows.CFM25 = Encoding.GetEncoding(950).GetString(currentByteString, 151, 2);
                            //stringHistoryTemp.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));
                            #endregion
                            mySqlCommand.Parameters.Clear();

                            #region 添加參數
                            try
                            {
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm01", mfp085Rows.CFM01));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm02", mfp085Rows.CFM02));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm03", mfp085Rows.CFM03));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm04", mfp085Rows.CFM04));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm05", mfp085Rows.CFM05));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm06", mfp085Rows.CFM06));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm07", mfp085Rows.CFM07));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm08", mfp085Rows.CFM08));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm09", mfp085Rows.CFM09));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm10", mfp085Rows.CFM10));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm11", mfp085Rows.CFM11));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm12", mfp085Rows.CFM12));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm13", mfp085Rows.CFM13));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm14", mfp085Rows.CFM14));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm15", mfp085Rows.CFM15));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm16", mfp085Rows.CFM16));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm17", mfp085Rows.CFM17));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm18", mfp085Rows.CFM18));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm19", mfp085Rows.CFM19));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm20", mfp085Rows.CFM20));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm21", mfp085Rows.CFM21));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm22", mfp085Rows.CFM22));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm23", mfp085Rows.CFM23));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm24", mfp085Rows.CFM24));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm25", mfp085Rows.CFM25));
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

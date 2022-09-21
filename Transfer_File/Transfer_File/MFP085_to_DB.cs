using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer_File.DataTableFolder;

namespace Transfer_File
{
    class MFP085_to_DB : Txt_to_DB
    {
        string destinationPath = ConfigurationManager.AppSettings["destinationPath"];

        public override StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, string fileString)
        {
            List<string> fileStringList = new List<string>();
            byte[] currentByteString = new byte[154];

            ESMP.MFP085DataTable mFP085Rows = new ESMP.MFP085DataTable(); // xsd 裝載
            MFP085 mfp085_dto = new MFP085();
            Move_File move_file = new Move_File();
            StringBuilder stringHistoryTemp = new StringBuilder();

            if (ReadFileToString(fileString, ref fileStringList, false))
            {
                int count = 0;
                stringHistoryTemp.AppendLine(String.Format("收到檔案: {0} \r", fileString));

                foreach (string mainString in fileStringList)
                {
                    // 宣告字元陣列做處理
                    byte[] lineString = Encoding.GetEncoding(950).GetBytes(mainString);

                    // 插入資料到指定資料表
                    using (MySqlCommand mySqlCommand = mySqlConnection.CreateCommand())
                    {
                        mySqlCommand.CommandText = "insert into t30.hfp085  (CFM01,CFM02,CFM03,CFM04,CFM05,CFM06,CFM07,CFM08,CFM09,CFM10,CFM11,CFM12,CFM13,CFM14,CFM15,CFM16,CFM17,CFM18,CFM19,CFM20,CFM21,CFM22,CFM23,CFM24,CFM25) values (@cfm01,@cfm02,@cfm03,@cfm04,@cfm05,@cfm06,@cfm07,@cfm08,@cfm09,@cfm10,@cfm11,@cfm12,@cfm13,@cfm14,@cfm15,@cfm16,@cfm17,@cfm18,@cfm19,@cfm20,@cfm21,@cfm22,@cfm23,@cfm24,@cfm25)";

                        // 每個資料都是153位元，所以以153為底跳著讀
                        for (int totalLength = 0; totalLength < lineString.Length; totalLength += 153)
                        {
                            Array.Copy(lineString, totalLength, currentByteString, 0, 153);

                            #region 變數指定
                            mfp085_dto.Cfm01 = Encoding.GetEncoding(950).GetString(currentByteString, 0, 8);
                            mfp085_dto.Cfm02 = Encoding.GetEncoding(950).GetString(currentByteString, 8, 6);
                            mfp085_dto.Cfm03 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 14, 1));
                            mfp085_dto.Cfm04 = Encoding.GetEncoding(950).GetString(currentByteString, 15, 4);
                            mfp085_dto.Cfm05 = (decimal)Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 19, 9))/10000;
                            mfp085_dto.Cfm06 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 28, 10));
                            mfp085_dto.Cfm07 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 38, 3));
                            mfp085_dto.Cfm08 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 41, 1));
                            mfp085_dto.Cfm09 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 42, 3));
                            mfp085_dto.Cfm10 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 45, 4));
                            mfp085_dto.Cfm11 = (decimal)Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 49, 3))/100;
                            mfp085_dto.Cfm12 = Encoding.GetEncoding(950).GetString(currentByteString, 52, 1);
                            mfp085_dto.Cfm13 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 53, 8));
                            mfp085_dto.Cfm14 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 61, 8));
                            mfp085_dto.Cfm15 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 69, 8));
                            mfp085_dto.Cfm16 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 77, 8));
                            mfp085_dto.Cfm17 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 85, 8));
                            mfp085_dto.Cfm18 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 93, 8));
                            mfp085_dto.Cfm19 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 101, 8));
                            mfp085_dto.Cfm20 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 109, 8));
                            mfp085_dto.Cfm21 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 117, 8));
                            mfp085_dto.Cfm22 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 125, 8));
                            mfp085_dto.Cfm23 = Encoding.GetEncoding(950).GetString(currentByteString, 133, 10);
                            mfp085_dto.Cfm24 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 143, 8));
                            mfp085_dto.Cfm25 = Encoding.GetEncoding(950).GetString(currentByteString, 151, 2);
                            //stringHistoryTemp.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));
                            #endregion
                            mySqlCommand.Parameters.Clear(); 

                            #region 添加參數
                            try
                            {
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm01", mfp085_dto.Cfm01));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm02", mfp085_dto.Cfm02));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm03", mfp085_dto.Cfm03));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm04", mfp085_dto.Cfm04));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm05", mfp085_dto.Cfm05));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm06", mfp085_dto.Cfm06));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm07", mfp085_dto.Cfm07));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm08", mfp085_dto.Cfm08));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm09", mfp085_dto.Cfm09));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm10", mfp085_dto.Cfm10));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm11", mfp085_dto.Cfm11));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm12", mfp085_dto.Cfm12));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm13", mfp085_dto.Cfm13));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm14", mfp085_dto.Cfm14));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm15", mfp085_dto.Cfm15));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm16", mfp085_dto.Cfm16));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm17", mfp085_dto.Cfm17));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm18", mfp085_dto.Cfm18));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm19", mfp085_dto.Cfm19));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm20", mfp085_dto.Cfm20));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm21", mfp085_dto.Cfm21));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm22", mfp085_dto.Cfm22));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm23", mfp085_dto.Cfm23));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm24", mfp085_dto.Cfm24));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm25", mfp085_dto.Cfm25));
                                mFP085Rows.AddMFP085Row(mfp085_dto.Cfm01, mfp085_dto.Cfm02, mfp085_dto.Cfm03, mfp085_dto.Cfm04,
                                    mfp085_dto.Cfm05, mfp085_dto.Cfm06, mfp085_dto.Cfm07, mfp085_dto.Cfm08, mfp085_dto.Cfm09,
                                    mfp085_dto.Cfm10, mfp085_dto.Cfm11, mfp085_dto.Cfm12, mfp085_dto.Cfm13, mfp085_dto.Cfm14,
                                    mfp085_dto.Cfm15, mfp085_dto.Cfm16, mfp085_dto.Cfm17, mfp085_dto.Cfm18, mfp085_dto.Cfm19,
                                    mfp085_dto.Cfm20, mfp085_dto.Cfm21, mfp085_dto.Cfm22, mfp085_dto.Cfm23, mfp085_dto.Cfm24,
                                    mfp085_dto.Cfm25);
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
                stringHistoryTemp.AppendLine(String.Format("{0} 存入DB完畢 共存入{1}筆\r", fileString, count));
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

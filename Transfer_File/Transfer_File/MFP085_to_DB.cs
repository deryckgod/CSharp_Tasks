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
        MFP085_Data hfp085_data_get_set;
        Move_File move_File;
        StringBuilder stringHistoryTemp;

        string path = ConfigurationManager.AppSettings["path"];
        string destinationPath = ConfigurationManager.AppSettings["destinationPath"];
        public override StringBuilder InputDataToMysql(MySqlConnection mySqlConnection, string fileString)
        {
            List<string> fileStringList = new List<string>();
            byte[] currentByteString = new byte[154]; // 存取當前100個byte資料

            hFP085Rows = new ESMP.HFP085DataTable(); // xsd 裝載
            hfp085_data_get_set = new MFP085_Data();
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
                    // 插入資料到指定資料表
                    using (MySqlCommand mySqlCommand = mySqlConnection.CreateCommand())
                    {
                        mySqlCommand.CommandText = "insert into t30.hfp085  (CFM01,CFM02,CFM03,CFM04,CFM05,CFM06,CFM07,CFM08,CFM09,CFM10,CFM11,CFM12,CFM13,CFM14,CFM15,CFM16,CFM17,CFM18,CFM19,CFM20,CFM21,CFM22,CFM23,CFM24,CFM25) values (@cfm01,@cfm02,@cfm03,@cfm04,@cfm05,@cfm06,@cfm07,@cfm08,@cfm09,@cfm10,@cfm11,@cfm12,@cfm13,@cfm14,@cfm15,@cfm16,@cfm17,@cfm18,@cfm19,@cfm20,@cfm21,@cfm22,@cfm23,@cfm24,@cfm25)";

                        // 每個資料都是100位元，所以以100為底跳著讀
                        for (int totalLength = 0; totalLength < lineString.Length; totalLength += 153)
                        {
                            Array.Copy(lineString, totalLength, currentByteString, 0, 153);

                            hfp085_data_get_set.Cfm01 = Encoding.GetEncoding(950).GetString(currentByteString, 0, 8);
                            hfp085_data_get_set.Cfm02 = Encoding.GetEncoding(950).GetString(currentByteString, 8, 6);
                            hfp085_data_get_set.Cfm03 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 14, 1));
                            hfp085_data_get_set.Cfm04 = Encoding.GetEncoding(950).GetString(currentByteString, 15, 4);
                            hfp085_data_get_set.Cfm05 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 19, 9));
                            hfp085_data_get_set.Cfm06 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 28, 10));
                            hfp085_data_get_set.Cfm07 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 38, 3));
                            hfp085_data_get_set.Cfm08 = Convert.ToChar(Encoding.GetEncoding(950).GetString(currentByteString, 41, 1));
                            hfp085_data_get_set.Cfm09 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 42, 3));
                            hfp085_data_get_set.Cfm10 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 45, 4));
                            hfp085_data_get_set.Cfm11 = Convert.ToDecimal(Encoding.GetEncoding(950).GetString(currentByteString, 49, 3));
                            hfp085_data_get_set.Cfm12 = Encoding.GetEncoding(950).GetString(currentByteString, 52, 1);

                            // STOCK_NAME
                            hfp085_data_get_set.Cfm13 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 53, 8));

                            // MARK_W_DETAILS
                            hfp085_data_get_set.Cfm14 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 61, 8));
                            hfp085_data_get_set.Cfm15 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 69, 8));
                            hfp085_data_get_set.Cfm16 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 77, 8));
                            hfp085_data_get_set.Cfm17 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 85, 8));
                            hfp085_data_get_set.Cfm18 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 93, 8));
                            hfp085_data_get_set.Cfm19 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 101, 8));
                            hfp085_data_get_set.Cfm20 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 109, 8));
                            hfp085_data_get_set.Cfm21 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 117, 8));
                            hfp085_data_get_set.Cfm22 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 125, 8));
                            hfp085_data_get_set.Cfm23 = Encoding.GetEncoding(950).GetString(currentByteString, 133, 10);
                            hfp085_data_get_set.Cfm24 = Convert.ToInt32(Encoding.GetEncoding(950).GetString(currentByteString, 143, 8));
                            hfp085_data_get_set.Cfm25 = Encoding.GetEncoding(950).GetString(currentByteString, 151, 2);
                            //stringHistoryTemp.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));

                            mySqlCommand.Parameters.Clear(); // 每次插入都先清除引數
                            try
                            {
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm01", hfp085_data_get_set.Cfm01));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm02", hfp085_data_get_set.Cfm02));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm03", hfp085_data_get_set.Cfm03));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm04", hfp085_data_get_set.Cfm04));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm05", hfp085_data_get_set.Cfm05));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm06", hfp085_data_get_set.Cfm06));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm07", hfp085_data_get_set.Cfm07));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm08", hfp085_data_get_set.Cfm08));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm09", hfp085_data_get_set.Cfm09));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm10", hfp085_data_get_set.Cfm10));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm11", hfp085_data_get_set.Cfm11));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm12", hfp085_data_get_set.Cfm12));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm13", hfp085_data_get_set.Cfm13));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm14", hfp085_data_get_set.Cfm14));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm15", hfp085_data_get_set.Cfm15));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm16", hfp085_data_get_set.Cfm16));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm17", hfp085_data_get_set.Cfm17));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm18", hfp085_data_get_set.Cfm18));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm19", hfp085_data_get_set.Cfm19));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm20", hfp085_data_get_set.Cfm20));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm21", hfp085_data_get_set.Cfm21));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm22", hfp085_data_get_set.Cfm22));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm23", hfp085_data_get_set.Cfm23));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm24", hfp085_data_get_set.Cfm24));
                                mySqlCommand.Parameters.Add(new MySqlParameter("cfm25", hfp085_data_get_set.Cfm25));
                                hFP085Rows.AddHFP085Row(       hfp085_data_get_set.Cfm01, hfp085_data_get_set.Cfm02, hfp085_data_get_set.Cfm03, hfp085_data_get_set.Cfm04,
                                    hfp085_data_get_set.Cfm05, hfp085_data_get_set.Cfm06, hfp085_data_get_set.Cfm07, hfp085_data_get_set.Cfm08, hfp085_data_get_set.Cfm09,
                                    hfp085_data_get_set.Cfm10, hfp085_data_get_set.Cfm11, hfp085_data_get_set.Cfm12, hfp085_data_get_set.Cfm13, hfp085_data_get_set.Cfm14,
                                    hfp085_data_get_set.Cfm15, hfp085_data_get_set.Cfm16, hfp085_data_get_set.Cfm17, hfp085_data_get_set.Cfm18, hfp085_data_get_set.Cfm19,
                                    hfp085_data_get_set.Cfm20, hfp085_data_get_set.Cfm21, hfp085_data_get_set.Cfm22, hfp085_data_get_set.Cfm23, hfp085_data_get_set.Cfm24, 
                                    hfp085_data_get_set.Cfm25);
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

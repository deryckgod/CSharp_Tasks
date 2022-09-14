using System.IO;
using System;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Data;

namespace Transfer_File
{

    public partial class Form1 : Form
    {
        StringBuilder stringHistory;
        StringBuilder stringBuilder;
        bool checkFile = false;

        MySqlConnection conn;
        FileInfo fileInfo;
        DirectoryInfo directoryInfo;
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
        
        // 檔案參數 避免放在迴圈中一直宣告 浪費資源
        string STOCK_NO = "";
        decimal BULL_PRICE = 0;
        decimal LDC_PRICE = 0;
        decimal BEAR_PRICE = 0;
        int LAST_MTH_DATE = 0;
        char SETTYPE = '0';
        char MARK_W = '0';
        char MARK_P = '0';
        char MARK_L = '0';
        string IND_CODE = "";
        string STK_CODE = "";
        char MARK_M = '0';
        string STOCK_NAME = "";
        int MATCH_INTERVAL = 0;
        int ORDER_LIMIT = 0;
        int ORDERS_LIMIT = 0;
        int PREPAY_RATE = 0;
        char MARK_S = '0';
        char MARK_F = '0';
        char MARK_DAY_TRADE = '0';
        char STK_CTGCD = '0';
        string FILLER = "";

        private delegate void conditionShow(string conditionBox);

        string path = ConfigurationManager.AppSettings["path"];
        string destinationPath = ConfigurationManager.AppSettings["destinationPath"];

        public Form1()
        {
            InitializeComponent();
            MyFileSystemWatcher();
        }

        public MySqlConnection connect()
        {
            MySqlConnection mysqlConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionSource"].ConnectionString);

            if(mysqlConnection.State != ConnectionState.Open)
            {
                mysqlConnection.Open();
            }
           
            return mysqlConnection;
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            using (conn = connect())
            {
                txtToMysql(conn);
            }
            //if (conn.State != System.Data.ConnectionState.Open)
            //{
            //    MessageBox.Show("關了");
            //}
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            using (conn = connect())
            {
                searchFromMysql(conn);
            }             
        }
        private void searchFromMysql(MySqlConnection conn)
        {
            stringBuilder = new StringBuilder();

            // 查詢資料表全部資料
            string sql = "SELECT * From t30.t30";
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, conn))
            {
                using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                {
                    if (mySqlDataReader.HasRows)
                    {
                        while (mySqlDataReader.Read())
                        {
                            stringBuilder.AppendLine(String.Format("股票編號: {0} \t 股票名字: {1}", mySqlDataReader["STOCK-NO"], mySqlDataReader["STOCK-NAME"]));
                        }
                        stringHistory = stringBuilder;
                    }
                } 
            }   
        }
        private bool readFileToString(string fileName, ref List<string> fileStringList, bool noSpace = false)
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
        private void txtToMysql(MySqlConnection conn)
        {
            stringBuilder = new StringBuilder();

            directoryInfo = new DirectoryInfo(path);
            foreach (FileInfo file in directoryInfo.GetFiles("*.TXT"))
            {
                checkFile = true;
                inputDataToMysql(conn, file.ToString());
                //txtCondition.Text += stringBuilder.ToString();// 從原本的inputDataToMysql輸出移至txtToMysql避免跟thread輸出衝突
            }
            stringHistory = stringBuilder; // 全部掃完再輸出 也可以解決輸出一半的問題
            checkFile = false; // 原資料夾內處理完畢
        }
        private void inputDataToMysql(MySqlConnection conn, String fileString)
        {
            List<string> fileStringList = new List<string>();

            if (readFileToString(fileString, ref fileStringList, false))
            {
                // 在文字框顯示資料
                stringBuilder.AppendLine(String.Format("收到檔案: {0} \r", fileString));

                foreach (string mainString in fileStringList)
                {
                    byte[] lineString = Encoding.GetEncoding(950).GetBytes(mainString);
                    int count = 0;
                    // 插入資料到DB
                    using (MySqlCommand mySqlCommand = conn.CreateCommand())
                    {
                        mySqlCommand.CommandText = "insert into t30.t30  (`STOCK-NO`,`BULL-PRICE`,`LDC-PRICE`,`BEAR-PRICE`, `LAST-MTH-DATE`,`SETTYPE`,`MARK-W`,`MARK-P`,`MARK-L`,`IND-CODE`,`STK-CODE`,`MARK-M`,`STOCK-NAME`,`MATCH-INTERVAL`, `ORDER-LIMIT`,`ORDERS-LIMIT`,`PREPAY-RATE`,`MARK-S`,`MARK-F`,`MARK-DAY-TRADE`,`STK-CTGCD`,`FILLER`) values (@STOCK_NO, @BULL_PRICE, @LDC_PRICE, @BEAR_PRICE, @LAST_MTH_DATE, @SETTYPE, @MARK_W, @MARK_P, @MARK_L, @IND_CODE, @STK_CODE, @MARK_M, @STOCK_NAME, @MATCH_INTERVAL, @ORDER_LIMIT, @ORDERS_LIMIT, @PREPAY_RATE, @MARK_S, @MARK_F,@MARK_DAY_TRADE, @STK_CTGCD, @FILLER)";

                        // 每個資料都是100位元，所以以100為底跳著讀
                        for (int totalLength = 0; totalLength < lineString.Length; totalLength += 100)
                        {
                            STOCK_NO = Encoding.GetEncoding(950).GetString(lineString, totalLength, 6);
                            BULL_PRICE = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(lineString, totalLength + 6, 9)) / 10000);
                            LDC_PRICE = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(lineString, totalLength + 15, 9)) / 10000);
                            BEAR_PRICE = (decimal)(Convert.ToDecimal(Encoding.GetEncoding(950).GetString(lineString, totalLength + 24, 9)) / 10000);
                            LAST_MTH_DATE = Convert.ToInt32(Encoding.GetEncoding(950).GetString(lineString, totalLength + 33, 8));
                            SETTYPE = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 41, 1));
                            MARK_W = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 42, 1));
                            MARK_P = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 43, 1));
                            MARK_L = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 44, 1));
                            IND_CODE = Encoding.GetEncoding(950).GetString(lineString, totalLength + 45, 2);
                            STK_CODE = Encoding.GetEncoding(950).GetString(lineString, totalLength + 47, 2);
                            MARK_M = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 49, 1));

                            // STOCK_NAME
                            STOCK_NAME = Encoding.GetEncoding(950).GetString(lineString, totalLength + 50, 16);

                            // MARK_W_DETAILS
                            MATCH_INTERVAL = Convert.ToInt32(Encoding.GetEncoding(950).GetString(lineString, totalLength + 66, 3));
                            ORDER_LIMIT = Convert.ToInt32(Encoding.GetEncoding(950).GetString(lineString, totalLength + 69, 6));
                            ORDERS_LIMIT = Convert.ToInt32(Encoding.GetEncoding(950).GetString(lineString, totalLength + 75, 6));
                            PREPAY_RATE = Convert.ToInt32(Encoding.GetEncoding(950).GetString(lineString, totalLength + 81, 3));
                            MARK_S = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 84, 1));
                            MARK_F = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 85, 1));
                            MARK_DAY_TRADE = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 86, 1));
                            STK_CTGCD = Convert.ToChar(Encoding.GetEncoding(950).GetString(lineString, totalLength + 87, 1));
                            FILLER = Encoding.GetEncoding(950).GetString(lineString, totalLength + 88, 12);

                            stringBuilder.AppendLine(String.Format("第{0}筆解析完畢\r", count + 1));

                            mySqlCommand.Parameters.Clear(); // 每次插入都先清除引數
                            try
                            {
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NO", STOCK_NO));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BULL_PRICE", BULL_PRICE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LDC_PRICE", LDC_PRICE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("BEAR_PRICE", BEAR_PRICE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("LAST_MTH_DATE", LAST_MTH_DATE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("SETTYPE", SETTYPE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_W", MARK_W));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_P", MARK_P));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_L", MARK_L));
                                mySqlCommand.Parameters.Add(new MySqlParameter("IND_CODE", IND_CODE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CODE", STK_CODE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_M", MARK_M));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STOCK_NAME", STOCK_NAME));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MATCH_INTERVAL", MATCH_INTERVAL));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDER_LIMIT", ORDER_LIMIT));
                                mySqlCommand.Parameters.Add(new MySqlParameter("ORDERS_LIMIT", ORDERS_LIMIT));
                                mySqlCommand.Parameters.Add(new MySqlParameter("PREPAY_RATE", PREPAY_RATE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_S", MARK_S));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_F", MARK_F));
                                mySqlCommand.Parameters.Add(new MySqlParameter("MARK_DAY_TRADE", MARK_DAY_TRADE));
                                mySqlCommand.Parameters.Add(new MySqlParameter("STK_CTGCD", STK_CTGCD));
                                mySqlCommand.Parameters.Add(new MySqlParameter("FILLER", FILLER));
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }

                            if (mySqlCommand.ExecuteNonQuery() > 0)
                            {
                                count++;
                            }
                        }
                    }
                    stringBuilder.AppendLine(String.Format("{0} 存入DB完畢 共存入{1}筆\r", fileString, count));
                    moveFile(fileString); // 轉移處理完的檔案
                }
                //stringHistory = stringBuilder; // 這裡輸出可能只會輸出一部分資料夾內的檔案
            }
            else
            {
                MessageBox.Show("Error");
            }
        }
        private void MyFileSystemWatcher()
        {
            Thread threadStart = new Thread(storeHistory);
            threadStart.Start();
            
            // 設定所要監控的資料夾
            fileSystemWatcher.Path = path;

            // 設定所要監控的變更類型
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess
                                                | NotifyFilters.LastWrite
                                                | NotifyFilters.FileName
                                                | NotifyFilters.DirectoryName;

            // 設定所要監控的檔案，"*"為所有檔案
            fileSystemWatcher.Filter = "*";

            // 設定是否監控子資料夾
            fileSystemWatcher.IncludeSubdirectories = true;

            // 設定是否啟動元件，此部分必須要設定為true，不然事件是不會被觸發的
            fileSystemWatcher.EnableRaisingEvents = true;

            // 設定觸發事件
            fileSystemWatcher.Created += new FileSystemEventHandler(fileSystemWatcher_Created);
            fileSystemWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
            fileSystemWatcher.Renamed += new RenamedEventHandler(fileSystemWatcher_Renamed);
            fileSystemWatcher.Deleted += new FileSystemEventHandler(fileSystemWatcher_Deleted);
        }
        private void fileSystemWatcher_Created(object sender, FileSystemEventArgs events)
        {
            directoryInfo = new DirectoryInfo(events.FullPath.ToString()); // 當多個檔案同時轉入時可保留個檔案資訊
            while (true)
            {
                if (!checkFile)
                {
                    stringBuilder = new StringBuilder(); // 放在checkFile中是避免當前檔案處理中途有其他檔案轉入造成歷史資訊中斷
                    //MessageBox.Show("STOP");
                    stringBuilder.AppendLine("新建檔案於:" + directoryInfo.FullName.Replace(directoryInfo.Name, ""));
                    stringBuilder.AppendLine("新建檔案名稱:" + directoryInfo.Name);
                    stringBuilder.AppendLine("建立時間:" + directoryInfo.CreationTime.ToString());
                    stringBuilder.AppendLine("目錄下共有:" + directoryInfo.Parent.GetFiles().Count() + "檔案");
                    stringBuilder.AppendLine("目錄下共有:" + directoryInfo.Parent.GetDirectories().Count() + "資料夾");
                    // stringBuilder.AppendLine("內容:" + System.IO.File.ReadAllText(directoryInfo.FullName, Encoding.Default));
                    using (conn = connect())
                    {
                        inputDataToMysql(conn, directoryInfo.FullName.ToString()); // 進行解析
                    }
                    stringHistory = stringBuilder; // 從原本的inputDataToMysql輸出移至created下面輸出 避免跟直接txtConditional.Text輸出衝突
                    break;
                }
            }
            //MessageBox.Show(stringBuilder.ToString());
        }

        /// <summary>
        /// 當所監控的資料夾有文字檔檔案內容有異動時觸發
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs events)
        {
            stringBuilder = new StringBuilder();

            directoryInfo = new DirectoryInfo(events.FullPath.ToString());

            stringBuilder.AppendLine("被異動的檔名為:" + events.Name);
            stringBuilder.AppendLine("檔案所在位址為:" + events.FullPath.Replace(events.Name, ""));
            stringBuilder.AppendLine("異動內容時間為:" + directoryInfo.LastWriteTime.ToString());
            //stringBuilder.AppendLine("內容:" + System.IO.File.ReadAllText(directoryInfo.FullName, Encoding.Default));

            stringHistory = stringBuilder;
            //MessageBox.Show(stringBuilder.ToString());
        }

        /// <summary>
        /// 當所監控的資料夾有文字檔檔案重新命名時觸發
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs events)
        {
            stringBuilder = new StringBuilder();

            fileInfo = new FileInfo(events.FullPath.ToString());

            stringBuilder.AppendLine("檔名更新前:" + events.OldName.ToString());
            stringBuilder.AppendLine("檔名更新後:" + events.Name.ToString());
            stringBuilder.AppendLine("檔名更新前路徑:" + events.OldFullPath.ToString());
            stringBuilder.AppendLine("檔名更新後路徑:" + events.FullPath.ToString());
            stringBuilder.AppendLine("建立時間:" + fileInfo.LastAccessTime.ToString());

            stringHistory = stringBuilder;
            //MessageBox.Show(stringBuilder.ToString());
        }

        /// <summary>
        /// 當所監控的資料夾有文字檔檔案有被刪除時觸發
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs events)
        {
            stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("被刪除的檔名為:" + events.Name);
            stringBuilder.AppendLine("檔案所在位址為:" + events.FullPath.Replace(events.Name, ""));
            stringBuilder.AppendLine("刪除時間:" + DateTime.Now.ToString());

            stringHistory = stringBuilder;
            //MessageBox.Show(stringBuilder.ToString());
        }
        
        private void storeHistory()
        {
            while (true)
            {
                if (stringHistory != null)
                {
                    AddMessage(stringHistory.ToString());
                    stringHistory = null;
                }
            }
        }

        private void AddMessage(string sMessage)
        {
            if (this.InvokeRequired)
            {
                conditionShow condition = new conditionShow(AddMessage);
                this.Invoke(condition, sMessage);
            }
            else
            {
                this.txtCondition.Text += sMessage + Environment.NewLine;
            }
        }
        private void btnTransfer_Click(object sender, EventArgs e)
        {
            //FileStream fileStream = null;

            // 寫法二跟三都是用完關閉文字檔，寫法一最後需要Dispose才能關閉
            // 寫法一
            //fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open);
            // 寫法二
            //try { fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open); } catch { fileStream.Dispose(); }
            // 寫法三
            //using (fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open)) { }
            
            //moveFile();

        }
        private void moveFile(String fileFullPath)
        {
            string fileName = fileFullPath.Split('\\').Last();
            File.Move(fileFullPath, destinationPath + fileName);
            stringBuilder.AppendLine(String.Format("{0} \t 從{1} \r\n 轉移至新位置{2} \r\n", fileName, fileFullPath, (destinationPath + fileName)));
            // ABC.txt 轉出
            //string sourceFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt";
            //string destinationFile = @"D:\Desktop\ALPED\Systex\C#_VS\ABC.txt";
            //File.Move(sourceFile, destinationFile);
            //txtCondition.Text += String.Format("{0} 從{1} \r\n 轉移至新位置{2} \r\n", "ABC.txt", sourceFile, destinationFile);

            // CBA.txt 轉入
            //string otherFile = @"D:\Desktop\ALPED\Systex\C#_VS\CBA.txt";
            //string otherDesFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\CBA.txt";
            //File.Move(otherFile, otherDesFile);
            //txtCondition.Text += String.Format("{0} 從{1} \r\n 轉移至新位置{2} \r\n", "CBA.txt", otherFile, otherDesFile);
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            restoreFile();
        }

        private void restoreFile()
        {
            // ABC.txt 轉出
            string sourceFile = @"D:\Desktop\ALPED\Systex\C#_VS\ABC.txt";
            string destinationFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt";
            File.Move(sourceFile, destinationFile);
            txtCondition.Text += String.Format("{0} 從{1} \r\n 轉移至新位置{2} \r\n", "ABC.txt", sourceFile, destinationFile);

            // CBA.txt 轉入
            string otherFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\CBA.txt";
            string otherDesFile = @"D:\Desktop\ALPED\Systex\C#_VS\CBA.txt";
            File.Move(otherFile, otherDesFile);
            txtCondition.Text += String.Format("{0} 從{1} \r\n 轉移至新位置{2} \r\n", "CBA.txt", otherFile, otherDesFile);

        }

        private void txtExit_Click(object sender, EventArgs e)
        {
            //if(conn.State != ConnectionState.Closed) conn.Close(); // 前面使用conn 都用using釋放了
            Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
using System.IO;
using System;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Data;
using System.Reflection;
using Transfer_File.File_to_DB;

namespace Transfer_File
{

    public partial class Form1 : Form
    {
        #region 宣告變數
        StringBuilder stringHistory;
        StringBuilder stringHistoryTemp;
        bool checkFile = false; // 辨識有沒有尚存的檔案在資料夾
        bool checkOnCreated = false; // 辨識是否有新的檔案進入
        int checkCreateTime = 0; // 計算Create次數

        MySqlConnection mySqlConnection;
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

        private delegate void conditionShow(string conditionBox);

        string path = ConfigurationManager.AppSettings["path"];
        private static object _thisLock = new object();
        #endregion

        public Form1()
        {
            InitializeComponent();
            MyFileSystemWatcher();
        }
        public MySqlConnection Connect()
        {
            try
            {
                mySqlConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionSource"].ConnectionString);
                if (mySqlConnection.State != ConnectionState.Open)
                {
                    mySqlConnection.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
            return mySqlConnection;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (mySqlConnection = Connect())
                {
                    Btn_Insert btn_insert = new Btn_Insert();
                    stringHistory = btn_insert.TxtToMysql(mySqlConnection, ref checkFile, ref checkOnCreated);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("匯入資料例外 :" + ex.Message);
            }
            
        }
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                using (mySqlConnection = Connect())
                {
                    Search_from_Mysql search_From_Mysql = new Search_from_Mysql();
                    stringHistory = search_From_Mysql.SearchT30FromMysql(mySqlConnection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("T30搜尋例外 :" + ex.Message);
            }
                  
        }
        
        private void MyFileSystemWatcher()
        {
            Thread threadStart = new Thread(StoreHistory);
            threadStart.Start();
            MessageBox.Show("THREAD START THREAD ID : " + threadStart.ManagedThreadId.ToString());

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
            fileSystemWatcher.Created += new FileSystemEventHandler(FileSystemWatcher_Created);
            //MessageBox.Show("FILEWATCHER THREAD ID : " +  Thread.CurrentThread.ManagedThreadId.ToString());
            //MessageBox.Show("BUFFER SIZE " + fileSystemWatcher.InternalBufferSize);

        }
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs events)
        {
            checkCreateTime++;
            Thread current_thread = Thread.CurrentThread;
            DirectoryInfo directoryInfo = new DirectoryInfo(events.FullPath.ToString()); // 當多個檔案同時轉入時可保留個檔案資訊
            while (true)
            {
                if (!checkFile)
                {
                    checkOnCreated = true;
                    lock (_thisLock)
                    {
                        stringHistoryTemp = new StringBuilder(); // 放在checkFile中是避免當前檔案處理中途有其他檔案轉入造成歷史資訊中斷
                        stringHistoryTemp.AppendLine(String.Format("Thread [{0}], State : {1}\n HashCode : {2}", current_thread.ManagedThreadId, current_thread.ThreadState, current_thread.GetHashCode()));
                        //stringHistoryTemp.AppendLine("新建檔案於: " + directoryInfo.FullName.Replace(directoryInfo.Name, ""));
                        //stringHistoryTemp.AppendLine("新建檔案名稱: " + directoryInfo.Name);
                        stringHistoryTemp.AppendLine("檔案建立時間: " + directoryInfo.CreationTime.ToString());
                        stringHistoryTemp.AppendLine("檔案執行時間: " + DateTime.Now.ToString());
                        //stringHistoryTemp.AppendLine("目錄下共有: " + directoryInfo.Parent.GetFiles().Count() + "檔案");
                        //stringHistoryTemp.AppendLine("目錄下共有: " + directoryInfo.Parent.GetDirectories().Count() + "資料夾");
                        try
                        {
                            using (mySqlConnection = Connect())
                            {
                                if (directoryInfo.Name.ToString().Contains("T30"))
                                {
                                    // 轉檔
                                    Txt_to_DB t30_to_db = new T30_to_DB();
                                    stringHistoryTemp.AppendLine(t30_to_db.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString());
                                }
                                else if (directoryInfo.Name.ToString().Contains("MFP085"))
                                {
                                    // 轉檔
                                    Txt_to_DB mfp085_to_db = new MFP085_to_DB();
                                    stringHistoryTemp.AppendLine(mfp085_to_db.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString());
                                }
                            }
                            stringHistoryTemp.AppendLine("Create 次數:" + checkCreateTime.ToString());
                            stringHistory = stringHistoryTemp; // 從原本的inputDataToMysql輸出移至created下面輸出 避免跟直接txtConditional.Text輸出衝突
                            break;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("新增檔案例外 : " + e.Message);
                        }
                    }
                    checkOnCreated = false;
                }
            }
        }
        
        private void StoreHistory()
        {
            while (true)
            {
                if (stringHistory != null)
                {
                    AddMessage(stringHistory.ToString());
                    //MessageBox.Show("STORE HISTORY THREAD ID : " + Thread.CurrentThread.ManagedThreadId.ToString());
                    stringHistory = null;
                }
            }
        }
        private void AddMessage(string sMessage)
        {
            if (this.InvokeRequired)
            {
                conditionShow condition = new conditionShow(AddMessage);
                //MessageBox.Show("ADD MESSAGE THREAD ID : " + Thread.CurrentThread.ManagedThreadId.ToString());
                this.Invoke(condition, sMessage);
            }
            else
            {
                this.txtCondition.Text += sMessage + Environment.NewLine;
            }
        }
        
        private void TxtExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btnSearchHFP085_Click(object sender, EventArgs e)
        {
            try
            {
                using (mySqlConnection = Connect())
                {
                    Search_from_Mysql search_From_Mysql = new Search_from_Mysql();
                    stringHistory = search_From_Mysql.SearchHFP085FromMysql(mySqlConnection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HFP085搜尋例外 :" + ex.Message);
            }
        }
    }
}
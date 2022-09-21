using System.IO;
using System;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Data;
using System.Reflection;

namespace Transfer_File
{

    public partial class Form1 : Form
    {
        StringBuilder stringHistory;
        StringBuilder stringHistoryTemp;
        bool checkFile = false;
        bool checkOnCreated = false;

        Search_from_Mysql search_From_Mysql;
        Txt_to_DB txt_To_DB;
        MFP085_to_DB mfp085_to_db;
        MySqlConnection mySqlConnection;
        DirectoryInfo directoryInfo;
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

        private delegate void conditionShow(string conditionBox);

        string path = ConfigurationManager.AppSettings["path"];

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
                    txt_To_DB = new Txt_to_DB();
                    stringHistory = txt_To_DB.TxtToMysql(mySqlConnection, ref checkFile, ref checkOnCreated);
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
                    search_From_Mysql = new Search_from_Mysql();
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
        }
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs events)
        {
            txt_To_DB = new Txt_to_DB();
            mfp085_to_db = new MFP085_to_DB();
            directoryInfo = new DirectoryInfo(events.FullPath.ToString()); // 當多個檔案同時轉入時可保留個檔案資訊
            while (true)
            {
                if (!checkFile)
                {
                    try
                    {
                        checkOnCreated = true;
                        stringHistoryTemp = new StringBuilder(); // 放在checkFile中是避免當前檔案處理中途有其他檔案轉入造成歷史資訊中斷
                        stringHistoryTemp.AppendLine("新建檔案於:" + directoryInfo.FullName.Replace(directoryInfo.Name, ""));
                        stringHistoryTemp.AppendLine("新建檔案名稱:" + directoryInfo.Name);
                        stringHistoryTemp.AppendLine("建立時間:" + directoryInfo.CreationTime.ToString());
                        stringHistoryTemp.AppendLine("目錄下共有:" + directoryInfo.Parent.GetFiles().Count() + "檔案");
                        stringHistoryTemp.AppendLine("目錄下共有:" + directoryInfo.Parent.GetDirectories().Count() + "資料夾");
                        using (mySqlConnection = Connect())
                        {
                            if (directoryInfo.Name.ToString().Contains("T30"))
                            {
                                stringHistoryTemp.AppendLine(txt_To_DB.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString());
                            }
                            else if (directoryInfo.Name.ToString().Contains("MFP085"))
                            {
                                stringHistoryTemp.AppendLine(mfp085_to_db.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString());
                            }
                            //stringHistoryTemp.AppendLine(txt_To_DB.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString()); // 進行解析
                        }
                        stringHistory = stringHistoryTemp; // 從原本的inputDataToMysql輸出移至created下面輸出 避免跟直接txtConditional.Text輸出衝突
                        break;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("新增檔案例外 : "+e.Message);
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
                    search_From_Mysql = new Search_from_Mysql();
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
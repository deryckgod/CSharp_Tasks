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
        StringBuilder stringHistoryTemp;
        bool checkFile = false;

        ESMP eSMP;
        Search_from_Mysql search_From_Mysql;
        Txt_to_DB txt_To_DB;
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
        DataSet dataSet = new DataSet();
        public MySqlConnection Connect()
        {
            try
            {
                mySqlConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionSource"].ConnectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            if(mySqlConnection.State != ConnectionState.Open)
            {
                mySqlConnection.Open();
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
                    stringHistory = txt_To_DB.TxtToMysql(mySqlConnection, ref checkFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("�פJ��ƨҥ~ :" + ex.Message);
            }
            
        }
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                using (mySqlConnection = Connect())
                {
                    search_From_Mysql = new Search_from_Mysql();
                    stringHistory = search_From_Mysql.SearchFromMysql(mySqlConnection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("�j�M�ҥ~ :" + ex.Message);
            }
                  
        }
        
        private void MyFileSystemWatcher()
        {
            Thread threadStart = new Thread(StoreHistory);
            threadStart.Start();
            
            // �]�w�ҭn�ʱ�����Ƨ�
            fileSystemWatcher.Path = path;

            // �]�w�ҭn�ʱ����ܧ�����
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess
                                                | NotifyFilters.LastWrite
                                                | NotifyFilters.FileName
                                                | NotifyFilters.DirectoryName;

            // �]�w�ҭn�ʱ����ɮסA"*"���Ҧ��ɮ�
            fileSystemWatcher.Filter = "*";

            // �]�w�O�_�ʱ��l��Ƨ�
            fileSystemWatcher.IncludeSubdirectories = true;

            // �]�w�O�_�Ұʤ���A�����������n�]�w��true�A���M�ƥ�O���|�QĲ�o��
            fileSystemWatcher.EnableRaisingEvents = true;

            // �]�wĲ�o�ƥ�
            fileSystemWatcher.Created += new FileSystemEventHandler(FileSystemWatcher_Created);
        }
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs events)
        {
            txt_To_DB = new Txt_to_DB();
            directoryInfo = new DirectoryInfo(events.FullPath.ToString()); // ��h���ɮצP����J�ɥi�O�d���ɮ׸�T
            while (true)
            {
                if (!checkFile)
                {
                    try
                    {
                        stringHistoryTemp = new StringBuilder(); // ��bcheckFile���O�קK��e�ɮ׳B�z���~����L�ɮ���J�y�����v��T���_
                        stringHistoryTemp.AppendLine("�s���ɮש�:" + directoryInfo.FullName.Replace(directoryInfo.Name, ""));
                        stringHistoryTemp.AppendLine("�s���ɮצW��:" + directoryInfo.Name);
                        stringHistoryTemp.AppendLine("�إ߮ɶ�:" + directoryInfo.CreationTime.ToString());
                        stringHistoryTemp.AppendLine("�ؿ��U�@��:" + directoryInfo.Parent.GetFiles().Count() + "�ɮ�");
                        stringHistoryTemp.AppendLine("�ؿ��U�@��:" + directoryInfo.Parent.GetDirectories().Count() + "��Ƨ�");
                        using (mySqlConnection = Connect())
                        {
                            stringHistoryTemp.AppendLine(txt_To_DB.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString()); // �i��ѪR
                        }
                        stringHistory = stringHistoryTemp; // �q�쥻��inputDataToMysql��X����created�U����X �קK�򪽱�txtConditional.Text��X�Ĭ�
                        break;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("�s�W�ɮרҥ~ : "+e.Message);
                    }
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
            eSMP = new ESMP();
            eSMP.T30_Load(dataSet);
        }
    }
}
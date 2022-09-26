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
        #region �ŧi�ܼ�
        StringBuilder stringHistory;
        StringBuilder stringHistoryTemp;
        bool checkFile = false; // ���Ѧ��S���|�s���ɮצb��Ƨ�
        bool checkOnCreated = false; // ���ѬO�_���s���ɮ׶i�J
        int checkCreateTime = 0; // �p��Create����

        MySqlConnection mySqlConnection;
        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

        private delegate void conditionShow(string conditionBox);

        string path = ConfigurationManager.AppSettings["path"];
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
                MessageBox.Show("�פJ��ƨҥ~ :" + ex.Message);
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
                MessageBox.Show("T30�j�M�ҥ~ :" + ex.Message);
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
            checkCreateTime++;
            DirectoryInfo directoryInfo = new DirectoryInfo(events.FullPath.ToString()); // ��h���ɮצP����J�ɥi�O�d���ɮ׸�T
            while (true)
            {
                if (!checkFile)
                {
                    checkOnCreated = true; 
                    stringHistoryTemp = new StringBuilder(); // ��bcheckFile���O�קK��e�ɮ׳B�z���~����L�ɮ���J�y�����v��T���_
                    stringHistoryTemp.AppendLine("�s���ɮש�:" + directoryInfo.FullName.Replace(directoryInfo.Name, ""));
                    stringHistoryTemp.AppendLine("�s���ɮצW��:" + directoryInfo.Name);
                    stringHistoryTemp.AppendLine("�ɮ׫إ߮ɶ�:" + directoryInfo.CreationTime.ToString());
                    stringHistoryTemp.AppendLine("�ɮװ���ɶ�:" + DateTime.Now.ToString());
                    stringHistoryTemp.AppendLine("�ؿ��U�@��:" + directoryInfo.Parent.GetFiles().Count() + "�ɮ�");
                    stringHistoryTemp.AppendLine("�ؿ��U�@��:" + directoryInfo.Parent.GetDirectories().Count() + "��Ƨ�");
                    try
                    {    
                        using (mySqlConnection = Connect())
                        {
                            if (directoryInfo.Name.ToString().Contains("T30"))
                            {
                                // ����
                                Txt_to_DB t30_to_db = new T30_to_DB();
                                stringHistoryTemp.AppendLine(t30_to_db.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString());
                            }
                            else if (directoryInfo.Name.ToString().Contains("MFP085"))
                            {
                                // ����
                                Txt_to_DB mfp085_to_db = new MFP085_to_DB();
                                stringHistoryTemp.AppendLine(mfp085_to_db.InputDataToMysql(mySqlConnection, directoryInfo.FullName.ToString()).ToString());
                            }
                        }
                        stringHistoryTemp.AppendLine("Create ����:" + checkCreateTime.ToString());
                        stringHistory = stringHistoryTemp; // �q�쥻��inputDataToMysql��X����created�U����X �קK�򪽱�txtConditional.Text��X�Ĭ�
                        break;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("�s�W�ɮרҥ~ : "+e.Message);
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
                    Search_from_Mysql search_From_Mysql = new Search_from_Mysql();
                    stringHistory = search_From_Mysql.SearchHFP085FromMysql(mySqlConnection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HFP085�j�M�ҥ~ :" + ex.Message);
            }
        }
    }
}
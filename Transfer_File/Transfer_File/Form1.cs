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
using Transfer_File.Log4net_Converter_Layout;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "D:\\Desktop\\ALPED\\Systex\\Git_Repository\\CSharp_Tasks\\deryckgod\\CSharp_Tasks\\Transfer_File\\Transfer_File\\log4net.config", Watch = true)] // �w�]app.config �A���i�H�]�w��(ConfigFile="log4net.config", Watch=true)

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
        private static object _thisLock = new object();

        #endregion

        public Form1()
        {
            InitializeComponent();
            MyFileSystemWatcher();
        }
        public MySqlConnection Connect()
        {
            string executeStartTime = DateTime.Now.ToString("HH:mm:ss:fff");
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
                //MessageBox.Show(ex.Message);
                string executeEndTime = DateTime.Now.ToString("HH:mm:ss:fff");
                LogHelper.WriteError(LogHelper.BuildLogEntity(" ", executeStartTime, executeEndTime, String.Format("{0}, EX", "")),ex);
            }
            return mySqlConnection;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            string executeStartTime = DateTime.Now.ToString("HH:mm:ss:fff");
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
            string executeEndTime = DateTime.Now.ToString("HH:mm:ss:fff");
            LogHelper.WriteInfo(LogHelper.BuildLogEntity("TRANSFER_FILE", executeStartTime, executeEndTime, "�ոլ�INFO"));
            LogHelper.WriteDebug(LogHelper.BuildLogEntity("TRANSFER_FILE", executeStartTime, executeEndTime, "�ոլ�DEBUG"));
            LogHelper.WriteWarn(LogHelper.BuildLogEntity("TRANSFER_FILE", executeStartTime, executeEndTime, "�ոլ�WARN"), null);//, new Exception("���~�T���� : "));
            LogHelper.WriteError(LogHelper.BuildLogEntity("TRANSFER_FILE", executeStartTime, executeEndTime, "�ոլ�ERROR"), null);
            LogHelper.WriteFatal(LogHelper.BuildLogEntity("TRANSFER_FILE", executeStartTime, executeEndTime, "�ոլ�FATAL"), null);

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
            //MessageBox.Show("THREAD START THREAD ID : " + threadStart.ManagedThreadId.ToString());

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
            //MessageBox.Show("FILEWATCHER THREAD ID : " +  Thread.CurrentThread.ManagedThreadId.ToString());
            //MessageBox.Show("BUFFER SIZE " + fileSystemWatcher.InternalBufferSize);

        }
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs events)
        {
            // �s�W�}�l�p��
            string executeStartTime = DateTime.Now.ToString("HH:mm:ss:fff");
            checkCreateTime++;
            Thread current_thread = Thread.CurrentThread;
            DirectoryInfo directoryInfo = new DirectoryInfo(events.FullPath.ToString()); // ��h���ɮצP����J�ɥi�O�d���ɮ׸�T
            while (true)
            {
                if (!checkFile)
                {
                    checkOnCreated = true;
                    lock (_thisLock)
                    {
                        stringHistoryTemp = new StringBuilder(); // ��bcheckFile���O�קK��e�ɮ׳B�z���~����L�ɮ���J�y�����v��T���_
                        stringHistoryTemp.AppendLine(String.Format("Thread [{0}], State : {1}\n HashCode : {2}", current_thread.ManagedThreadId, current_thread.ThreadState, current_thread.GetHashCode()));
                        //stringHistoryTemp.AppendLine("�s���ɮש�: " + directoryInfo.FullName.Replace(directoryInfo.Name, ""));
                        //stringHistoryTemp.AppendLine("�s���ɮצW��: " + directoryInfo.Name);
                        stringHistoryTemp.AppendLine("�ɮ׫إ߮ɶ�: " + directoryInfo.CreationTime.ToString());
                        stringHistoryTemp.AppendLine("�ɮװ���ɶ�: " + DateTime.Now.ToString());
                        //stringHistoryTemp.AppendLine("�ؿ��U�@��: " + directoryInfo.Parent.GetFiles().Count() + "�ɮ�");
                        //stringHistoryTemp.AppendLine("�ؿ��U�@��: " + directoryInfo.Parent.GetDirectories().Count() + "��Ƨ�");
                        try
                        {
                            using (mySqlConnection = Connect())
                            {
                                if (directoryInfo.Name.ToString().Contains("T30"))
                                { 
                                    string executeEndTime = DateTime.Now.ToString("HH:mm:ss:fff");
                                    LogHelper.WriteInfo(LogHelper.BuildLogEntity(directoryInfo.Name, executeStartTime, executeEndTime, String.Format("{0}, ��J�}�l", directoryInfo.Name)));
                                    
                                    // ����
                                    Txt_to_DB t30_to_db = new T30_to_DB();
                                    stringHistoryTemp.AppendLine(t30_to_db.InputDataToMysql(mySqlConnection, directoryInfo.FullName, directoryInfo.Name).ToString());
                                }
                                else if (directoryInfo.Name.ToString().Contains("MFP085"))
                                {
                                    string executeEndTime = DateTime.Now.ToString("HH:mm:ss:fff");
                                    LogHelper.WriteInfo(LogHelper.BuildLogEntity(directoryInfo.Name, executeStartTime, executeEndTime, String.Format("{0}, ��J�}�l", directoryInfo.Name)));

                                    // ����
                                    Txt_to_DB mfp085_to_db = new MFP085_to_DB();
                                    stringHistoryTemp.AppendLine(mfp085_to_db.InputDataToMysql(mySqlConnection, directoryInfo.FullName, directoryInfo.Name).ToString());
                                }
                            }
                            stringHistoryTemp.AppendLine("Create ����:" + checkCreateTime.ToString());
                            stringHistory = stringHistoryTemp; // �q�쥻��inputDataToMysql��X����created�U����X �קK�򪽱�txtConditional.Text��X�Ĭ�
                            break;
                        }
                        catch (Exception ex)
                        {
                            string executeEndTime = DateTime.Now.ToString("HH:mm:ss:fff");
                            LogHelper.WriteError(LogHelper.BuildLogEntity(directoryInfo.Name, executeStartTime, executeEndTime, String.Format("{0}, �s�W�ɮרҥ~ : ", directoryInfo.Name)), ex);
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
                MessageBox.Show("HFP085�j�M�ҥ~ :" + ex.Message);
            }
        }
    }
}
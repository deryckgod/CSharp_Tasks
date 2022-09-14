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
        
        // �ɮװѼ� �קK��b�j�餤�@���ŧi ���O�귽
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
            //    MessageBox.Show("���F");
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

            // �d�߸�ƪ�������
            string sql = "SELECT * From t30.t30";
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, conn))
            {
                using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                {
                    if (mySqlDataReader.HasRows)
                    {
                        while (mySqlDataReader.Read())
                        {
                            stringBuilder.AppendLine(String.Format("�Ѳ��s��: {0} \t �Ѳ��W�r: {1}", mySqlDataReader["STOCK-NO"], mySqlDataReader["STOCK-NAME"]));
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
                // �ѨM.NET Core²�ƽs�X���D�A Big5���~�T������
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
                //txtCondition.Text += stringBuilder.ToString();// �q�쥻��inputDataToMysql��X����txtToMysql�קK��thread��X�Ĭ�
            }
            stringHistory = stringBuilder; // ���������A��X �]�i�H�ѨM��X�@�b�����D
            checkFile = false; // ���Ƨ����B�z����
        }
        private void inputDataToMysql(MySqlConnection conn, String fileString)
        {
            List<string> fileStringList = new List<string>();

            if (readFileToString(fileString, ref fileStringList, false))
            {
                // �b��r����ܸ��
                stringBuilder.AppendLine(String.Format("�����ɮ�: {0} \r", fileString));

                foreach (string mainString in fileStringList)
                {
                    byte[] lineString = Encoding.GetEncoding(950).GetBytes(mainString);
                    int count = 0;
                    // ���J��ƨ�DB
                    using (MySqlCommand mySqlCommand = conn.CreateCommand())
                    {
                        mySqlCommand.CommandText = "insert into t30.t30  (`STOCK-NO`,`BULL-PRICE`,`LDC-PRICE`,`BEAR-PRICE`, `LAST-MTH-DATE`,`SETTYPE`,`MARK-W`,`MARK-P`,`MARK-L`,`IND-CODE`,`STK-CODE`,`MARK-M`,`STOCK-NAME`,`MATCH-INTERVAL`, `ORDER-LIMIT`,`ORDERS-LIMIT`,`PREPAY-RATE`,`MARK-S`,`MARK-F`,`MARK-DAY-TRADE`,`STK-CTGCD`,`FILLER`) values (@STOCK_NO, @BULL_PRICE, @LDC_PRICE, @BEAR_PRICE, @LAST_MTH_DATE, @SETTYPE, @MARK_W, @MARK_P, @MARK_L, @IND_CODE, @STK_CODE, @MARK_M, @STOCK_NAME, @MATCH_INTERVAL, @ORDER_LIMIT, @ORDERS_LIMIT, @PREPAY_RATE, @MARK_S, @MARK_F,@MARK_DAY_TRADE, @STK_CTGCD, @FILLER)";

                        // �C�Ӹ�Ƴ��O100�줸�A�ҥH�H100��������Ū
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

                            stringBuilder.AppendLine(String.Format("��{0}���ѪR����\r", count + 1));

                            mySqlCommand.Parameters.Clear(); // �C�����J�����M���޼�
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
                    stringBuilder.AppendLine(String.Format("{0} �s�JDB���� �@�s�J{1}��\r", fileString, count));
                    moveFile(fileString); // �ಾ�B�z�����ɮ�
                }
                //stringHistory = stringBuilder; // �o�̿�X�i��u�|��X�@������Ƨ������ɮ�
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
            fileSystemWatcher.Created += new FileSystemEventHandler(fileSystemWatcher_Created);
            fileSystemWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
            fileSystemWatcher.Renamed += new RenamedEventHandler(fileSystemWatcher_Renamed);
            fileSystemWatcher.Deleted += new FileSystemEventHandler(fileSystemWatcher_Deleted);
        }
        private void fileSystemWatcher_Created(object sender, FileSystemEventArgs events)
        {
            directoryInfo = new DirectoryInfo(events.FullPath.ToString()); // ��h���ɮצP����J�ɥi�O�d���ɮ׸�T
            while (true)
            {
                if (!checkFile)
                {
                    stringBuilder = new StringBuilder(); // ��bcheckFile���O�קK��e�ɮ׳B�z���~����L�ɮ���J�y�����v��T���_
                    //MessageBox.Show("STOP");
                    stringBuilder.AppendLine("�s���ɮש�:" + directoryInfo.FullName.Replace(directoryInfo.Name, ""));
                    stringBuilder.AppendLine("�s���ɮצW��:" + directoryInfo.Name);
                    stringBuilder.AppendLine("�إ߮ɶ�:" + directoryInfo.CreationTime.ToString());
                    stringBuilder.AppendLine("�ؿ��U�@��:" + directoryInfo.Parent.GetFiles().Count() + "�ɮ�");
                    stringBuilder.AppendLine("�ؿ��U�@��:" + directoryInfo.Parent.GetDirectories().Count() + "��Ƨ�");
                    // stringBuilder.AppendLine("���e:" + System.IO.File.ReadAllText(directoryInfo.FullName, Encoding.Default));
                    using (conn = connect())
                    {
                        inputDataToMysql(conn, directoryInfo.FullName.ToString()); // �i��ѪR
                    }
                    stringHistory = stringBuilder; // �q�쥻��inputDataToMysql��X����created�U����X �קK�򪽱�txtConditional.Text��X�Ĭ�
                    break;
                }
            }
            //MessageBox.Show(stringBuilder.ToString());
        }

        /// <summary>
        /// ��Һʱ�����Ƨ�����r���ɮפ��e�����ʮ�Ĳ�o
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs events)
        {
            stringBuilder = new StringBuilder();

            directoryInfo = new DirectoryInfo(events.FullPath.ToString());

            stringBuilder.AppendLine("�Q���ʪ��ɦW��:" + events.Name);
            stringBuilder.AppendLine("�ɮשҦb��}��:" + events.FullPath.Replace(events.Name, ""));
            stringBuilder.AppendLine("���ʤ��e�ɶ���:" + directoryInfo.LastWriteTime.ToString());
            //stringBuilder.AppendLine("���e:" + System.IO.File.ReadAllText(directoryInfo.FullName, Encoding.Default));

            stringHistory = stringBuilder;
            //MessageBox.Show(stringBuilder.ToString());
        }

        /// <summary>
        /// ��Һʱ�����Ƨ�����r���ɮ׭��s�R�W��Ĳ�o
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs events)
        {
            stringBuilder = new StringBuilder();

            fileInfo = new FileInfo(events.FullPath.ToString());

            stringBuilder.AppendLine("�ɦW��s�e:" + events.OldName.ToString());
            stringBuilder.AppendLine("�ɦW��s��:" + events.Name.ToString());
            stringBuilder.AppendLine("�ɦW��s�e���|:" + events.OldFullPath.ToString());
            stringBuilder.AppendLine("�ɦW��s����|:" + events.FullPath.ToString());
            stringBuilder.AppendLine("�إ߮ɶ�:" + fileInfo.LastAccessTime.ToString());

            stringHistory = stringBuilder;
            //MessageBox.Show(stringBuilder.ToString());
        }

        /// <summary>
        /// ��Һʱ�����Ƨ�����r���ɮצ��Q�R����Ĳ�o
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs events)
        {
            stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("�Q�R�����ɦW��:" + events.Name);
            stringBuilder.AppendLine("�ɮשҦb��}��:" + events.FullPath.Replace(events.Name, ""));
            stringBuilder.AppendLine("�R���ɶ�:" + DateTime.Now.ToString());

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

            // �g�k�G��T���O�Χ�������r�ɡA�g�k�@�̫�ݭnDispose�~������
            // �g�k�@
            //fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open);
            // �g�k�G
            //try { fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open); } catch { fileStream.Dispose(); }
            // �g�k�T
            //using (fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open)) { }
            
            //moveFile();

        }
        private void moveFile(String fileFullPath)
        {
            string fileName = fileFullPath.Split('\\').Last();
            File.Move(fileFullPath, destinationPath + fileName);
            stringBuilder.AppendLine(String.Format("{0} \t �q{1} \r\n �ಾ�ܷs��m{2} \r\n", fileName, fileFullPath, (destinationPath + fileName)));
            // ABC.txt ��X
            //string sourceFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt";
            //string destinationFile = @"D:\Desktop\ALPED\Systex\C#_VS\ABC.txt";
            //File.Move(sourceFile, destinationFile);
            //txtCondition.Text += String.Format("{0} �q{1} \r\n �ಾ�ܷs��m{2} \r\n", "ABC.txt", sourceFile, destinationFile);

            // CBA.txt ��J
            //string otherFile = @"D:\Desktop\ALPED\Systex\C#_VS\CBA.txt";
            //string otherDesFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\CBA.txt";
            //File.Move(otherFile, otherDesFile);
            //txtCondition.Text += String.Format("{0} �q{1} \r\n �ಾ�ܷs��m{2} \r\n", "CBA.txt", otherFile, otherDesFile);
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            restoreFile();
        }

        private void restoreFile()
        {
            // ABC.txt ��X
            string sourceFile = @"D:\Desktop\ALPED\Systex\C#_VS\ABC.txt";
            string destinationFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt";
            File.Move(sourceFile, destinationFile);
            txtCondition.Text += String.Format("{0} �q{1} \r\n �ಾ�ܷs��m{2} \r\n", "ABC.txt", sourceFile, destinationFile);

            // CBA.txt ��J
            string otherFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\CBA.txt";
            string otherDesFile = @"D:\Desktop\ALPED\Systex\C#_VS\CBA.txt";
            File.Move(otherFile, otherDesFile);
            txtCondition.Text += String.Format("{0} �q{1} \r\n �ಾ�ܷs��m{2} \r\n", "CBA.txt", otherFile, otherDesFile);

        }

        private void txtExit_Click(object sender, EventArgs e)
        {
            //if(conn.State != ConnectionState.Closed) conn.Close(); // �e���ϥ�conn ����using����F
            Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
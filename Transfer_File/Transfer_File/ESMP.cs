using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;

namespace Transfer_File
{
}

namespace Transfer_File
{
    class ESMP
    {
        string path = ConfigurationManager.AppSettings["path"];
        public void T30_Load(DataSet dataSet)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ConnectionSource"].ConnectionString))
                {
                    MySqlDataAdapter mySqlDataAdapter_t30 = new MySqlDataAdapter("SELECT * From t30.t30", mySqlConnection);
                    mySqlDataAdapter_t30.Fill(dataSet);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}       
    
    









using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File.DataTableFolder
{
    internal class T30_Data
    {

        #region 檔案參數
        //  避免放在迴圈中一直宣告 浪費資源
        string stock_No = "";
        decimal bull_Price = 0;
        decimal ldc_Price = 0;
        decimal bear_Price = 0;
        int last_Mth_Date = 0;
        char setType = '0';
        char mark_W = '0';
        char mark_P = '0';
        char mark_L = '0';
        string ind_Code = "";
        string stk_Code = "";
        char mark_M = '0';
        string stock_Name = "";
        int match_Interval = 0;
        int order_Limit = 0;
        int orders_Limit = 0;
        int prepay_Rate = 0;
        char mark_S = '0';
        char mark_F = '0';
        char mark_Day_Trade = '0';
        char stk_CTGCD = '0';
        string filler = "";

        public string Stock_No { get => stock_No; set => stock_No = value; }
        public decimal Bull_Price { get => bull_Price; set => bull_Price = value; }
        public decimal Ldc_Price { get => ldc_Price; set => ldc_Price = value; }
        public decimal Bear_Price { get => bear_Price; set => bear_Price = value; }
        public int Last_Mth_Date { get => last_Mth_Date; set => last_Mth_Date = value; }
        public char SetType { get => setType; set => setType = value; }
        public char Mark_W { get => mark_W; set => mark_W = value; }
        public char Mark_P { get => mark_P; set => mark_P = value; }
        public char Mark_L { get => mark_L; set => mark_L = value; }
        public string Ind_Code { get => ind_Code; set => ind_Code = value; }
        public string Stk_Code { get => stk_Code; set => stk_Code = value; }
        public char Mark_M { get => mark_M; set => mark_M = value; }
        public string Stock_Name { get => stock_Name; set => stock_Name = value; }
        public int Match_Interval { get => match_Interval; set => match_Interval = value; }
        public int Order_Limit { get => order_Limit; set => order_Limit = value; }
        public int Orders_Limit { get => orders_Limit; set => orders_Limit = value; }
        public int Prepay_Rate { get => prepay_Rate; set => prepay_Rate = value; }
        public char Mark_S { get => mark_S; set => mark_S = value; }
        public char Mark_F { get => mark_F; set => mark_F = value; }
        public char Mark_Day_Trade { get => mark_Day_Trade; set => mark_Day_Trade = value; }
        public char Stk_CTGCD { get => stk_CTGCD; set => stk_CTGCD = value; }
        public string Filler { get => filler; set => filler = value; }
        #endregion

    }
}

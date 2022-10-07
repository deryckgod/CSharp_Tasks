using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File.Log4net_Converter_Layout
{
    internal class LogEntity
    {
        // Key 先填轉檔檔名
        public String Key { get; set; }

        // ServiceName(Class Name)
        public String ServiceName{ get; set; }

        // FunctionName 呼叫方法名
        public String FunctionName { get; set; }

        // ExecuteStartTime 程式啟動時間
        public String ExecuteStartTime { get ; set ; }

        // 程式結束時間
        public String ExecuteEndTime { get ; set ; }

        // Message訊息
        public String Message { get; set; }
    }
}

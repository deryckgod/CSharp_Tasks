using log4net.Core;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Transfer_File.Log4net_Converter_Layout
{
    public class PatternConvert : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        { 
            if(Option != null)
            {
                // 寫入指定的Key值
                WriteObject(writer, loggingEvent.Repository, LookupProperty(Option, loggingEvent));
            }
            else
            {
                // 寫入所有值
                WriteDictionary(writer, loggingEvent.Repository, loggingEvent.GetProperties());
            }
        }
        private object LookupProperty(string property, LoggingEvent loggingEvent)
        {
            object propertyValue = string.Empty;
            PropertyInfo propertyInfo = loggingEvent.MessageObject.GetType().GetProperty(property);

            if(propertyInfo != null)
            {
                propertyValue = propertyInfo.GetValue(loggingEvent.MessageObject, null);
            }

            return propertyValue;
        }
    }
}

/*
var executeTime = loggingEvent.MessageObject as LogEntity;
if (executeTime == null) return;

writer.Write(string.Format(" 開始 {0} | 結束 {1}", executeTime.ExecuteStartTime, executeTime.ExecuteEndTime));
*/

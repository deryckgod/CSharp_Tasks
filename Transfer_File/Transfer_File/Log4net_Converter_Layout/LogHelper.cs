using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File.Log4net_Converter_Layout
{
    internal class LogHelper
    {
        private static readonly log4net.ILog InfoLog = log4net.LogManager.GetLogger("InfoLogger");

        public static readonly log4net.ILog DebugLog = log4net.LogManager.GetLogger("DebugLogger");

        public static readonly log4net.ILog ErrorLog = log4net.LogManager.GetLogger("ErrorLogger");

        private static LogEntity logEntity = new LogEntity();

        public static LogEntity BuildLogEntity(string key_fileName, string startTime, string endTime, string message, [CallerMemberName] string method = "")
        {
            logEntity.Key = key_fileName;
            logEntity.ServiceName = (new StackTrace()).GetFrame(1).GetMethod().ReflectedType.Name;
            logEntity.FunctionName = method;
            logEntity.ExecuteStartTime = startTime;
            logEntity.ExecuteEndTime = endTime;
            logEntity.Message = message;
            return logEntity;
        }

        public static void WriteInfo(LogEntity logEntity)
        {
            try
            {
                if (InfoLog.IsInfoEnabled)
                {
                    InfoLog.Info(logEntity);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static void WriteDebug(LogEntity logEntity)
        {
            try
            {
                if (DebugLog.IsDebugEnabled)
                {
                    DebugLog.Debug(logEntity);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static void WriteWarn(LogEntity logEntity, Exception? ex)
        {
            try
            {
                if (ErrorLog.IsWarnEnabled)
                {
                    ErrorLog.Warn(logEntity, ex);
                }
            }
            catch { }
        }
        public static void WriteError(LogEntity logEntity, Exception? ex)
        {
            try
            {
                if (ErrorLog.IsErrorEnabled)
                {
                    ErrorLog.Error(logEntity, ex);
                }
            }
            catch { }
        }
        public static void WriteFatal(LogEntity logEntity, Exception? ex)
        {
            try
            {
                if (ErrorLog.IsFatalEnabled)
                {
                    ErrorLog.Fatal(logEntity, ex);
                }
            }
            catch { }
        }
    }
}

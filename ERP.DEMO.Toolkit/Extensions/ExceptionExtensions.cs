using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class ExceptionExtensions
    {
        public class Log
        {
            public DateTime Date { get; set; }
            public string Message { get; set; }
            public string Source { get; set; }
            public string ControllerSource { get; set; }
            public string ActionSource { get; set; }
            public string StackTrace { get; set; }
            public string Inner { get; set; }

            public Log() { }

            public Log(Exception ex)
            {
                Date = DateTime.UtcNow;
                Message = ex.Message;
                if (ex.TargetSite != null && ex.TargetSite.ReflectedType != null)
                    ControllerSource = ex.TargetSite.ReflectedType.FullName;
                if (ex.TargetSite != null)
                    ActionSource = ex.TargetSite.Name;
                if (ex.InnerException != null)
                    Inner = ex.InnerException.Message;
                StackTrace = ex.StackTrace;
                Source = ex.Source;
            }
        }

        public static void WriteLog(this Exception ex, string directoryPath)
        {
            var date = DateTime.UtcNow;
            var log = new Log(ex);
            List<Log> logs = null;

            var folder = new DirectoryInfo(directoryPath);
            if (!folder.Exists)
                folder.Create();

            var fileFullName = Path.Combine(directoryPath, "Log.xml");

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<Log>));
            if (new FileInfo(fileFullName).Exists)
                using (StreamReader rd = new StreamReader(fileFullName))
                    logs = xs.Deserialize(rd) as List<Log>;

            if (logs == null)
                logs = new List<Log>();
            logs.Where(x => x.Date <= DateTime.UtcNow.AddMonths(-3)).ToList().ForEach(x => logs.Remove(x));
            logs.Add(log);

            using (StreamWriter wr = new StreamWriter(fileFullName))
                xs.Serialize(wr, logs);
        }
    }
}

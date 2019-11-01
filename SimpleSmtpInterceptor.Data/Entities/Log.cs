using System;

namespace SimpleSmtpInterceptor.Data.Entities
{
    public class Log
    {
        public Log()
        {
            CreatedOnUtc = DateTime.UtcNow;
            MachineName = Environment.MachineName;
        }

        public int LogId { get; set; }

        public string MachineName { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public string Logger { get; set; }

        public string Properties { get; set; }

        public string Callsite { get; set; }

        public string Exception { get; set; }
    }
}

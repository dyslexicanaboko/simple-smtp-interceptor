using System;

namespace SimpleSmtpInterceptor.Data.Entities
{
    public class Email
    {
        public long EmailId { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public string HeaderJson { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}

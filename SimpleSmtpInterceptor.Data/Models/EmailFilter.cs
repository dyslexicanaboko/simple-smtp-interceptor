using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSmtpInterceptor.Data.Models
{
    public class EmailFilter
    {
        public int PageSize { get; set; }

        public string ToAddress { get; set; }

        public string FromAddress { get; set; }

        public string Subject { get; set; }
    }
}

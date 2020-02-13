using SimpleSmtpInterceptor.Data.Entities;
using System.Collections.Generic;

namespace SimpleSmtpInterceptor.Data.Models
{
    public class ParsedEmail
    {
        public Header Header { get; set; } = new Header();

        public Email Email { get; set; } = new Email();

        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}

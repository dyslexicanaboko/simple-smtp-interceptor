using SimpleSmtpInterceptor.Data.Entities;
using System.Collections.Generic;

namespace SimpleSmtpInterceptor.Data.Models
{
    public class ParsedEmail
    {
        public EmailHeader Header { get; set; } = new EmailHeader();

        public Email Email { get; set; } = new Email();

        public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
    }
}

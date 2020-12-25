using System.Runtime.Serialization;

namespace SimpleSmtpInterceptor.Data.Models
{
    [DataContract]
    public class Header
        : Content
    {
        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string From { get; set; }

        [DataMember]
        public string To { get; set; }

        [DataMember]
        public string Cc { get; set; }

        [DataMember]
        public string Bcc { get; set; }

        [DataMember]
        public string MimeVersion { get; set; }

        [DataMember]
        public string Date { get; set; }
    }
}

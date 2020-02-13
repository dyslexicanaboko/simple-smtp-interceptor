using System.Runtime.Serialization;

namespace SimpleSmtpInterceptor.Data.Models
{
    [DataContract]
    public class Content
    {
        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public string ContentTransferEncoding { get; set; }

        [DataMember]
        public string ContentDisposition { get; set; }
    }
}

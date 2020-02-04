using System;
using System.Runtime.Serialization;

namespace SimpleSmtpInterceptor.Data.Entities
{
    [DataContract]
    public class Email
    {
        [DataMember]
        public long EmailId { get; set; }

        [DataMember]
        public string From { get; set; }

        [DataMember]
        public string To { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string HeaderJson { get; set; }

        [DataMember]
        public DateTime CreatedOnUtc { get; set; }

        [DataMember]
        public int AttachmentCount { get; set; }

        [IgnoreDataMember]
        public byte[] AttachmentArchive { get; set; }
    }
}

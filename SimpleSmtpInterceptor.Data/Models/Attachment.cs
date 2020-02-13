using System.Runtime.Serialization;

namespace SimpleSmtpInterceptor.Data.Models
{
	[DataContract]
	public class Attachment
		: Content
	{
		[DataMember]
		public string Name { get; set; }
	
		[IgnoreDataMember]
		public string AttachmentBase64 { get; set; }
	}
}

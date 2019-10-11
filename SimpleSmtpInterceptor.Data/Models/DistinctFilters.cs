using System.Collections.Generic;

namespace SimpleSmtpInterceptor.Data.Models
{
    public class DistinctFilters
    {
        public List<string> ToAddresses { get; set; }
        public List<string> FromAddresses { get; set; }
        public List<string> Subjects { get; set; }
    }
}

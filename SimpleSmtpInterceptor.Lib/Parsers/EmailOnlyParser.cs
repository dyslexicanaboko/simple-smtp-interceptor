using System.IO;
using System.Text;

namespace SimpleSmtpInterceptor.Lib.Parsers
{
    public class EmailOnlyParser
        : EmailParser
    {
        public EmailOnlyParser(TextReader reader, bool verboseOutput)
            : base(reader, verboseOutput)
        {

        }

        private void ParseMessage()
        {
            var line = GetNextLine();

            var sb = new StringBuilder();

            //All message data - nothing else to find here
            while (line != null && line != ".")
            {
                line = RemoveTrailingEquals(line);

                sb.Append(line);

                line = GetNextLine();
            }

            var message = sb.ToString();

            if (ParsedEmail.Header.ContentTransferEncoding == ContentTransferEncodings.QuotedPrintable)
            {
                message = DecodeQuotedPrintable(ParsedEmail.Email.ContentType, message);
            }

            ParsedEmail.Email.Message = message;
        }

        protected override void SavePayloadHeaderContent()
        {
            ParsedEmail.Header.EmailContent = ParsedEmail.Email.CloneContent();
        }

        public override void ParseBody()
        {
            ParseMessage();

            SavePayloadHeaderContent();
        }
    }
}

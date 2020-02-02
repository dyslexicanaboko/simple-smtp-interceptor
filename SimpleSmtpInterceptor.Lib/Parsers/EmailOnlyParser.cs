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

            while (line != null && line != ".")
            {
                if (line.StartsWith(Headers.ContentTransferEncoding))
                {
                    //Save to Header Info
                }
                else
                {
                    sb.Append(line);
                }

                line = GetNextLine();
            }
	
            ParsedEmail.Email.Message = sb.ToString();
        }

        public override void ParseBody()
        {
            ParseMessage();
        }
    }
}

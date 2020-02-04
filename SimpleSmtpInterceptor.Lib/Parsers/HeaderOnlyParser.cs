using System.IO;

namespace SimpleSmtpInterceptor.Lib.Parsers
{
    public class HeaderOnlyParser
        : EmailParser
    {
        public HeaderOnlyParser(TextReader reader, bool verboseOutput) 
            : base(reader, verboseOutput)
        {
        }

        public override void ParseBody()
        {
            
        }
    }
}

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
            //There is nothing else to parse
        }

        protected override void SavePayloadHeaderContent()
        {
            //There is nothing else to save
        }
    }
}

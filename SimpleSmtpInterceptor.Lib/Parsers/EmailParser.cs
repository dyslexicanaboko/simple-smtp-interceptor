using SimpleSmtpInterceptor.Data.Entities;
using SimpleSmtpInterceptor.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleSmtpInterceptor.Lib.Parsers
{
    public abstract class EmailParser
        : CommonBase
	{
        protected ParsedEmail ParsedEmail { get; } = new ParsedEmail();

		protected readonly TextReader Reader;

		private readonly bool _verboseOutput;

		protected string GetNextLine()
        {
            var line = ReadNextLine();

			while (line == string.Empty)
			{
                line = ReadNextLine();
			}

			return line;
		}

        private string ReadNextLine()
        {
            var line = Reader.ReadLine();

            if(_verboseOutput) Console.WriteLine(line);

            return line;
        }

        protected EmailParser(TextReader reader, bool verboseOutput)
		{
			Reader = reader;

			_verboseOutput = verboseOutput;
		}

        public abstract void ParseBody();

		public static EmailParser GetEmailParser(TextReader reader, bool verboseOutput)
		{
			var obj = new EmailHeader();

            var line = reader.ReadLine();

            EmailParser parser = null;

			while (line != null && line != ".")
			{
                if (line == string.Empty)
                {
                    break;
                }

				if (line.StartsWith(Headers.Subject))
				{
					obj.Subject = line.Substring(Headers.Subject.Length);
				}
				else if (line.StartsWith(Headers.From))
				{
					obj.From = line.Substring(Headers.From.Length);
				}
				else if (line.StartsWith(Headers.To))
				{
					obj.To = line.Substring(Headers.To.Length);
				}
				else if (line.StartsWith(Headers.MimeVersion))
				{
					obj.MimeVersion = line.Substring(Headers.MimeVersion.Length);
				}
				else if (line.StartsWith(Headers.Date))
				{
					obj.Date = line.Substring(Headers.Date.Length);
				}
				else if (line.StartsWith(Headers.ContentType))
				{
                    obj.ContentType = line.Substring(Headers.ContentType.Length);

                    if (obj.ContentType == ContentFileTypes.MultiPartMixed)
                    {
                        parser = new BoundaryParser(reader, verboseOutput);
                    }
                    else
                    {
                        parser = new EmailOnlyParser(reader, verboseOutput);
                    }
                }
				else if (line.StartsWith(Headers.ContentTransferEncoding))
				{
					obj.ContentTransferEncoding = line.Substring(Headers.ContentTransferEncoding.Length);
				}
				else if(line == string.Empty)
				{
					break;
				}

                line = reader.ReadLine();
            }

            if(parser == null) parser = new HeaderOnlyParser(reader, verboseOutput);

            parser.ParsedEmail.Header = obj;

            return parser;
        }

        //TODO: Need to test this more - I don't like magic numbers
        protected string RemoveTrailingEquals(string line)
        {
            if (line.Length < 66) return line;

            if (!line.EndsWith("=")) return line;

            line = line.TrimEnd('=');

            return line;
        }

        //TODO: This should handle multiple charsets
        protected string DecodeQuotedPrintable(string input)
        {
            var occurrences = new Regex(@"(=[0-9A-Z][0-9A-Z])+", RegexOptions.Multiline);

            var matches = occurrences.Matches(input);

            foreach (Match m in matches)
            {
                var bytes = new byte[m.Value.Length / 3];

                for (var i = 0; i < bytes.Length; i++)
                {
                    var hex = m.Value.Substring(i * 3 + 1, 2);

                    var iHex = Convert.ToInt32(hex, 16);

                    bytes[i] = Convert.ToByte(iHex);
                }

                input = input.Replace(m.Value, Encoding.Default.GetString(bytes));
            }

            return input;
        }

        public Email GetEmail()
        {
            var h = ParsedEmail.Header;
            var e = ParsedEmail.Email;

            e.From = h.From;
            e.To = h.To;
            e.Subject = h.Subject;
            e.HeaderJson = SerializeAsJson(h);
            e.CreatedOnUtc = DateTime.UtcNow;

            return e;
        }

        public List<EmailAttachment> GetAttachments()
        {
            return ParsedEmail.Attachments;
        }
    }
}

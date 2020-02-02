using SimpleSmtpInterceptor.Data.Models;
using System;
using System.IO;
using SimpleSmtpInterceptor.Data.Entities;

namespace SimpleSmtpInterceptor.Lib
{
    public abstract class EmailParser
        : CommonBase
	{
        protected ParsedEmail ParsedEmail { get; } = new ParsedEmail();

		protected readonly TextReader Reader;

		private readonly bool _verboseOutput;

		protected string GetNextLine()
		{
			var line = Reader.ReadLine();

			while (line == string.Empty)
			{
				if(_verboseOutput) Console.WriteLine(line);

				line = Reader.ReadLine();
			}

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

                    parser.ParsedEmail.Header = obj;
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

            return parser;
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

        public EmailHeader GetHeader()
        {
            return ParsedEmail.Header;
        }
    }
}

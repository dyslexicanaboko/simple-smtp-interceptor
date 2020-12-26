using SimpleSmtpInterceptor.Data.Entities;
using SimpleSmtpInterceptor.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleSmtpInterceptor.Lib.Parsers
{
    //This application assumes "TO" address will always be provided
    public abstract class EmailParser
        : CommonBase
	{
        private static readonly Regex ReWhiteSpace = new Regex(@"\s+");

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
            return ReadNextLine(Reader, _verboseOutput);
        }

        private static string ReadNextLine(TextReader reader, bool verboseOutput)
        {
            var line = reader.ReadLine();

            if(verboseOutput) Console.WriteLine(line);

            return line;
        }

        protected EmailParser(TextReader reader, bool verboseOutput)
		{
			Reader = reader;

			_verboseOutput = verboseOutput;
		}

        public abstract void ParseBody();

        /// <summary>
        /// Build the header object while trying to determine what kind of email is this.
        /// Currently there is support for: Email Only, Email with Attachments and Header only
        /// </summary>
        /// <param name="reader">incoming email stream</param>
        /// <param name="verboseOutput">true prints out every line of the reader</param>
        /// <returns>Methodology to parse the remainder of the email</returns>
		public static EmailParser GetEmailParser(TextReader reader, List<string> rcptTo, bool verboseOutput)
		{
			var obj = new Header();

            string GetNextLine()
            {
                return ReadNextLine(reader, verboseOutput);
            };

            var line = GetNextLine();

            EmailParser parser = null;

			while (line != null && line != ".")
			{
                if (line == string.Empty)
                {
                    break;
                }

                if (TryGetAttribute(line, Headers.Subject, out var val))
				{
					obj.Subject = val;
				}
				else if (TryGetAttribute(line, Headers.From, out val))
				{
					obj.From = val;
				}
				else if (TryGetAttribute(line, Headers.To, out val))
				{
					obj.To = FormatEmailCsv(val);
				}
                else if (TryGetAttribute(line, Headers.Cc, out val))
                {
                    obj.Cc = FormatEmailCsv(val);
                }
                else if (TryGetAttribute(line, Headers.MimeVersion, out val))
				{
					obj.MimeVersion = val;
				}
				else if (TryGetAttribute(line, Headers.Date, out val))
				{
					obj.Date = val;
				}
				else if (TryGetAttribute(line, Headers.ContentType, out val))
				{
                    obj.ContentType = val;

                    if (obj.ContentType == ContentFileTypes.MultiPartMixed)
                    {
                        parser = new BoundaryParser(reader, verboseOutput);
                    }
                    else
                    {
                        parser = new EmailOnlyParser(reader, verboseOutput);
                    }
                }
				else if (TryGetAttribute(line, Headers.ContentTransferEncoding, out val))
				{
					obj.ContentTransferEncoding = val;
				}
				else if(line == string.Empty)
				{
					break;
				}

                line = GetNextLine();
            }

            //Attempt to extract BCC
            if (rcptTo.Any())
            {
                obj.Bcc = ResolveBccCsv(rcptTo, obj);
            }
            
            if (parser == null) parser = new HeaderOnlyParser(reader, verboseOutput);

            parser.ParsedEmail.Header = obj;

            return parser;
        }

        private static string ResolveBccCsv(List<string> rcptTo, Header header)
        {
            var h = header;

            var lstTo = GetEmailsAsList(h.To);

            var lstCc = GetEmailsAsList(h.Cc);

            var lstRemove = new List<string>(lstTo.Count + lstCc.Count);

            //Combine the lists to loop once
            lstRemove.AddRange(lstTo);
            lstRemove.AddRange(lstCc);

            //Remove the To and Cc address matches, what's left is BCC 
            foreach (var e in lstRemove)
            {
                //This will only remove the first occurrence which
                //will preserve duplicates on purpose
                rcptTo.Remove(e);
            }

            if (!rcptTo.Any()) return null;

            //Only take the remaining RCPT TO entries which is deduced to be BCC 
            var bcc = string.Join(';', rcptTo);

            return bcc;
        }

        private static List<string> GetEmailsAsList(string emailCsv)
        {
            if (string.IsNullOrWhiteSpace(emailCsv)) return new List<string>();

            var lst = emailCsv
                .Split(";")
                .ToList();

            return lst;
        }

        private static string FormatEmailCsv(string csv)
        {
            var str = csv.Replace(",", ";");

            str = ReWhiteSpace.Replace(str, string.Empty);

            return str;
        }

        protected static string TryGetCharSet(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return null;

            var arr = contentType.Split(new[] {';'});

            if (arr.Length < 2) return null;

            var token = arr[1];

            if (!TryGetAttribute(token, Headers.CharSet, out var charSet)) return null;

            return charSet;
        }

        //TODO: Need to test this more - I don't like magic numbers
        protected string RemoveTrailingEquals(string line)
        {
            if (line.Length < 66) return line;

            if (!line.EndsWith("=")) return line;

            line = line.TrimEnd('=');

            return line;
        }

        protected string DecodeQuotedPrintable(string contentType, string input)
        {
            var charSet = TryGetCharSet(contentType);

            Func<byte[], string> fEncoder;

            if (charSet == null)
            {
                //If no charset was found then use the default and hope it works
                fEncoder = (bytes) => Encoding.Default.GetString(bytes);
            }
            else
            {
                //If a charset was found then use that specific encoder
                fEncoder = (bytes) => Encoding.GetEncoding(charSet).GetString(bytes);
            }

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

                var strEncoded = fEncoder(bytes);

                input = input.Replace(m.Value, strEncoded);
            }

            return input;
        }

        public Email GetEmail()
        {
            var h = ParsedEmail.Header;
            var e = ParsedEmail.Email;

            e.From = h.From;
            e.To = h.To;
            e.Cc = h.Cc;
            e.Bcc = h.Bcc;
            e.Subject = h.Subject;
            e.HeaderJson = SerializeAsJson(h);
            e.CreatedOnUtc = DateTime.UtcNow;

            return e;
        }

        public List<Attachment> GetAttachments()
        {
            return ParsedEmail.Attachments;
        }
    }
}

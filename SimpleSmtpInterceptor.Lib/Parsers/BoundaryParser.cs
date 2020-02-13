using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SimpleSmtpInterceptor.Data.Models;

namespace SimpleSmtpInterceptor.Lib.Parsers
{
    public class BoundaryParser
        : EmailParser
    {
        private readonly string _boundaryOpen;
        private readonly string _boundaryClose;
        private readonly Regex _attachmentName;

        public BoundaryParser(TextReader reader, bool verboseOutput)
            : base(reader, verboseOutput)
        {
            var line = Reader.ReadLine();

            var boundaryValue = line.Substring(Headers.Boundary.Length);

            _boundaryOpen = "--" + boundaryValue;

            _boundaryClose = _boundaryOpen + "--";

            //In the group: Match all characters except soft quotes
            _attachmentName = new Regex("Content-Type: .+; name=\"?([^\"]+)\"?");
        }

        /// <summary>
        /// Parse the email message (body) first before parsing the attachments
        /// </summary>
        private void ParseMessage()
        {
            var line = GetNextLine();

            var sb = new StringBuilder();

            var boundaryCount = 0;

            var e = ParsedEmail.Email;

            while (line != null && boundaryCount < 2)
            {
                if (line == _boundaryOpen)
                {
                    boundaryCount++;

                    if (boundaryCount < 2)
                    {
                        line = GetNextLine();
                    }

                    continue;
                }

                if (TryGetAttribute(line, Headers.ContentTransferEncoding, out var val))
                {
                    e.ContentTransferEncoding = val;
                }
                else if (TryGetAttribute(line, Headers.ContentType, out val))
                {
                    e.ContentType = val;
                }
                else
                {
                    //Email message data
                    line = RemoveTrailingEquals(line);

                    sb.Append(line);
                }

                line = GetNextLine();
            }

            var message = sb.ToString();

            if (e.ContentTransferEncoding == ContentTransferEncodings.QuotedPrintable)
            {
                message = DecodeQuotedPrintable(e.ContentType, message);
            }

            e.Message = message;
        }

        /// <summary>
        /// Attachments are after the message
        /// </summary>
        protected void ParseAttachments()
        {
            var line = GetNextLine();

            var lst = new List<Attachment>();

            var endReached = false;

            while (!endReached)
            {
                var a = new Attachment();

                var sb = new StringBuilder();

                while (line != null)
                {
                    if (line == _boundaryClose)
                    {
                        endReached = true;

                        break;
                    }

                    if (line == _boundaryOpen)
                    {
                        break;
                    }

                    string val = null;

                    if (TryGetAttribute(line, Headers.ContentTransferEncoding, out val))
                    {
                        a.ContentTransferEncoding = val;
                    }
                    else if (TryGetAttribute(line, Headers.ContentType, out val))
                    {
                        //Save attachment name
                        var m = _attachmentName.Match(line);

                        a.Name = m.Success ? m.Groups[1].Value : Path.GetRandomFileName();
                    }
                    else if (TryGetAttribute(line, Headers.ContentDisposition, out val))
                    {
                        a.ContentDisposition = val;
                    }
                    else
                    {
                        sb.Append(line);
                    }

                    line = GetNextLine();
                }

                a.AttachmentBase64 = sb.ToString();

                lst.Add(a);

                line = GetNextLine();
            }

            ParsedEmail.Attachments = lst;
        }

        public override void ParseBody()
        {
            ParseMessage();

            ParseAttachments();
        }
    }
}
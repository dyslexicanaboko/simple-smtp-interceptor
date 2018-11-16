using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Models;

namespace SimpleSmtpInterceptor.Lib
{
    public class ClientHandler 
        : IDisposable
    {
        private readonly TcpClient _client;
        private const string Subject = "Subject: ";
        private const string From = "From: ";
        private const string To = "To: ";
        //private const string MimeVersion = "MIME-Version: ";
        //private const string Date = "Date: ";
        private const string ContentType = "Content-Type: ";
        private const string ContentTransferEncoding = "Content-Transfer-Encoding: ";

        private readonly bool _verboseOutput;

        public ClientHandler(TcpClient client, bool verboseOutput)
        {
            _client = client;

            _verboseOutput = verboseOutput;
        }

        public void HandleRequest()
        {
            using (var _stream = _client.GetStream())
            {
                using (var _writer = new StreamWriter(_stream))
                {
                    _writer.NewLine = "\r\n";
                    _writer.AutoFlush = true;

                    using (var _reader = new StreamReader(_stream))
                    {
                        _writer.WriteLine("220 localhost -- Fake proxy server");

                        try
                        {
                            var keepReading = true;

                            while (keepReading)
                            {
                                var line = _reader.ReadLine();

                                if (_verboseOutput)
                                    Console.Error.WriteLine("Read line {0}", line);

                                switch (line)
                                {
                                    case "DATA":
                                        _writer.WriteLine("354 Start input, end data with <CRLF>.<CRLF>");

                                        var data = new StringBuilder();
                                        var subject = string.Empty;
                                        var from = string.Empty;
                                        var to = string.Empty;
                                        //var mimeVersion = string.Empty;
                                        //var date = string.Empty;
                                        var contentType = string.Empty;
                                        var contentTransferEncoding = string.Empty;

                                        line = _reader.ReadLine();

                                        while (line != null && line != ".")
                                        {
                                            if (line.StartsWith(Subject))
                                            {
                                                subject = line.Substring(Subject.Length);
                                            }
                                            else if (line.StartsWith(From))
                                            {
                                                from = line.Substring(From.Length);
                                            }
                                            else if (line.StartsWith(To))
                                            {
                                                to = line.Substring(To.Length);
                                            }
                                            //else if (line.StartsWith(MimeVersion))
                                            //{
                                            //    mimeVersion = line.Substring(MimeVersion.Length);
                                            //}
                                            //else if (line.StartsWith(Date))
                                            //{
                                            //    date = line.Substring(Date.Length);
                                            //}
                                            else if (line.StartsWith(ContentType))
                                            {
                                                contentType = line.Substring(ContentType.Length);
                                            }
                                            else if (line.StartsWith(ContentTransferEncoding))
                                            {
                                                contentTransferEncoding =
                                                    line.Substring(ContentTransferEncoding.Length);
                                            }
                                            else
                                            {
                                                data.AppendLine(line);
                                            }

                                            line = _reader.ReadLine();
                                        }

                                        var message = data.ToString();

                                        var email = new Email
                                        {
                                            From = from,
                                            To = to,
                                            Subject = subject,
                                            Message = message,
                                            CreatedOnUtc = DateTime.UtcNow
                                        };

                                        WriteMessage(email, contentType, contentTransferEncoding);

                                        _writer.WriteLine("250 OK");
                                        break;

                                    case "QUIT":
                                        _writer.WriteLine("250 OK");

                                        keepReading = false;
                                        break;

                                    default:
                                        _writer.WriteLine("250 OK");
                                        break;
                                }
                            }
                        }
                        catch (IOException)
                        {
                            Console.WriteLine("Connection lost.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
        }

        private string DecodeQuotedPrintable(string input)
        {
            var occurences = new Regex(@"(=[0-9A-Z][0-9A-Z])+", RegexOptions.Multiline);

            var matches = occurences.Matches(input);

            foreach (Match m in matches)
            {
                byte[] bytes = new byte[m.Value.Length / 3];

                for (int i = 0; i < bytes.Length; i++)
                {
                    string hex = m.Value.Substring(i * 3 + 1, 2);
                    int iHex = Convert.ToInt32(hex, 16);
                    bytes[i] = Convert.ToByte(iHex);
                }

                input = input.Replace(m.Value, Encoding.Default.GetString(bytes));
            }

            return input.Replace("=\r\n", string.Empty);
        }

        private void WriteMessage(Email email, string contentType, string transferEncoding)
        {
            if (transferEncoding == "quoted-printable")
            {
                email.Message = DecodeQuotedPrintable(email.Message);
            }

            Console.WriteLine($"sent to -> {email.To}");

            SaveEmail(email);

            //Console.Error.WriteLine("===============================================================================");
            //Console.Error.WriteLine("Received ­email");
            //Console.Error.WriteLine("Type: " + contentType);
            //Console.Error.WriteLine("Encoding: " + transferEncoding);
            //Console.Error.WriteLine("From: " + from);
            //Console.Error.WriteLine("To: " + to);
            //Console.Error.WriteLine("Subject: " + subject);
            //Console.Error.WriteLine("-------------------------------------------------------------------------------");
            //Console.Error.WriteLine(message);
            //Console.Error.WriteLine("===============================================================================");
            //Console.Error.WriteLine(string.Empty);
        }

        private void SaveEmail(Email email)
        {
            using (var context = new InterceptorModelFactory().CreateDbContext(null))
            {
                context.Add(email);
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            if(_client == null) return;

            _client.Close();
            _client.Dispose();
        }
    }
}

using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Entities;

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

        private readonly InterceptorModel _context;

        public ClientHandler(TcpClient client, bool verboseOutput)
        {
            _client = client;

            _verboseOutput = verboseOutput;

            _context = new InterceptorModelFactory().CreateDbContext(null);
        }

        public void HandleRequest()
        {
            Email email = null;

            using (var stream = _client.GetStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.NewLine = "\r\n";
                    writer.AutoFlush = true;

                    using (var _reader = new StreamReader(stream))
                    {
                        writer.WriteLine("220 localhost -- Fake proxy server");

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
                                        writer.WriteLine("354 Start input, end data with <CRLF>.<CRLF>");

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

                                        email = new Email
                                        {
                                            From = from,
                                            To = to,
                                            Subject = subject,
                                            Message = message,
                                            CreatedOnUtc = DateTime.UtcNow
                                        };

                                        WriteMessage(email, contentType, contentTransferEncoding);

                                        writer.WriteLine("250 OK");
                                        break;

                                    case "QUIT":
                                        writer.WriteLine("250 OK");

                                        keepReading = false;
                                        break;

                                    default:
                                        writer.WriteLine("250 OK");
                                        break;
                                }
                            }
                        }
                        catch (IOException ioe)
                        {
                            Console.WriteLine("Connection lost.");

                            LogError(ioe, email);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);

                            LogError(ex, email);
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
            _context.Emails.Add(email);
            _context.SaveChanges();
        }

        private void LogError(Exception exception, Email email)
        {
            var log = new Log();

            log.Message = "Ex: " + exception.GetType().Name + " ExMsg: " + exception.Message;

            log.Exception = exception.ToString();

            log.Level = @"Error";

            if (email != null)
            {
                log.Properties = SerializeAsJson(email);
            }

            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        private string SerializeAsJson(object target)
        {
            var js = new DataContractJsonSerializer(target.GetType());

            using (var ms = new MemoryStream())
            {
                js.WriteObject(ms, target);

                ms.Position = 0;

                using (var sr = new StreamReader(ms))
                {
                    var json = sr.ReadToEnd();

                    return json;
                }
            }
        }

        public void Dispose()
        {
            _context?.Dispose();

            if(_client == null) return;

            _client.Close();
            _client.Dispose();
        }
    }
}

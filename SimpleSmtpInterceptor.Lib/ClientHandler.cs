using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Entities;
using SimpleSmtpInterceptor.Data.Models;

namespace SimpleSmtpInterceptor.Lib
{
    public class ClientHandler 
        : IDisposable
    {
        private const string Subject = "Subject: ";
        private const string From = "From: ";
        private const string To = "To: ";
        private const string MimeVersion = "MIME-Version: ";
        private const string Date = "Date: ";
        private const string ContentType = "Content-Type: ";
        private const string ContentTransferEncoding = "Content-Transfer-Encoding: ";

        private readonly TcpClient _client;
        
        private readonly bool _verboseOutput;

        private readonly InterceptorModel _context;

        public ClientHandler(TcpClient client, bool verboseOutput)
        {
            _client = client;

            _verboseOutput = verboseOutput;

            if (verboseOutput)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Verbose output is ON");
                Console.ResetColor();
            }

            _context = new InterceptorModelFactory().CreateDbContext(null);

            Console.WriteLine();
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

                    using (var reader = new StreamReader(stream))
                    {
                        writer.WriteLine("220 localhost -- Fake proxy server");

                        try
                        {
                            var keepReading = true;

                            while (keepReading)
                            {
                                var line = ReadNextLine(reader);

                                switch (line)
                                {
                                    case "DATA":
                                        writer.WriteLine("354 Start input, end data with <CRLF>.<CRLF>");

                                        //Read everything up to the blank line
                                        var header = ExtractHeaderInformation(reader);

                                        //Everything after the blank line is the message body
                                        var message = ExtractBody(reader);

                                        email = new Email
                                        {
                                            From = header.From,
                                            To = header.To,
                                            Subject = header.Subject,
                                            Message = message,
                                            HeaderJson = SerializeAsJson(header),
                                            CreatedOnUtc = DateTime.UtcNow
                                        };

                                        WriteMessage(header, email);

                                        SaveEmail(email);

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

        private string ReadNextLine(TextReader reader)
        {
            var line = reader.ReadLine();

            if(_verboseOutput) Console.WriteLine(line);

            return line;
        }

        private EmailHeader ExtractHeaderInformation(TextReader reader)
        {
            var obj = new EmailHeader();

            var line = ReadNextLine(reader);

            while (line != null && line != ".")
            {
                if (line.StartsWith(Subject))
                {
                    obj.Subject = line.Substring(Subject.Length);
                }
                else if (line.StartsWith(From))
                {
                    obj.From = line.Substring(From.Length);
                }
                else if (line.StartsWith(To))
                {
                    obj.To = line.Substring(To.Length);
                }
                else if (line.StartsWith(MimeVersion))
                {
                    obj.MimeVersion = line.Substring(MimeVersion.Length);
                }
                else if (line.StartsWith(Date))
                {
                    obj.Date = line.Substring(Date.Length);
                }
                else if (line.StartsWith(ContentType))
                {
                    obj.ContentType = line.Substring(ContentType.Length);
                }
                else if (line.StartsWith(ContentTransferEncoding))
                {
                    obj.ContentTransferEncoding = line.Substring(ContentTransferEncoding.Length);
                }
                else if(line == string.Empty)
                {
                    break;
                }

                line = ReadNextLine(reader);
            }

            return obj;
        }

        private string ExtractBody(TextReader reader)
        {
            var line = ReadNextLine(reader);

            var sb = new StringBuilder();

            while (line != null && line != ".")
            {
                sb.Append(line);

                line = ReadNextLine(reader);
            }

            var body = sb.ToString();

            return body;
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

        private void WriteMessage(EmailHeader header, Email email)
        {
            if (header.ContentTransferEncoding == "quoted-printable")
            {
                email.Message = DecodeQuotedPrintable(email.Message);
            }

            Console.WriteLine($"sent to -> {email.To}");
        }

        private void SaveEmail(Email email)
        {
            _context.Emails.Add(email);
            _context.SaveChanges();
        }

        private void LogError(Exception exception, Email email = null)
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

using Microsoft.EntityFrameworkCore.Internal;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Entities;
using SimpleSmtpInterceptor.Data.Models;
using SimpleSmtpInterceptor.Lib.Parsers;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using SimpleSmtpInterceptor.Lib.Services;

namespace SimpleSmtpInterceptor.Lib.Server
{
    public class ClientHandler 
        : CommonBase, IDisposable
    {
        private readonly TcpClient _client;
        
        private readonly bool _verboseOutput;

        private readonly InterceptorModel _context;

        public ClientHandler(TcpClient client, bool verboseOutput)
        {
            _client = client;

            _verboseOutput = verboseOutput;

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

                                        var parser = EmailParser.GetEmailParser(reader, _verboseOutput);

                                        if (parser == null)
                                        {
                                            throw new Exception("Parser was returned as null somehow... 0x202002020103");
                                        }

                                        parser.ParseBody();

                                        email = parser.GetEmail();

                                        var header = parser.GetHeader();

                                        var attachments = parser.GetAttachments();

                                        email.AttachmentCount = attachments.Count;

                                        if (attachments.Any())
                                        {
                                            var svc = new AttachmentCompressor(attachments);

                                            var rawFile = svc.SaveAsZipArchive(Path.GetRandomFileName());

                                            email.AttachmentArchive = rawFile.Contents;
                                        }

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
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine("Connection lost.");
                            Console.ResetColor();

                            LogError(ioe, email);
                        }
                        catch (Exception ex)
                        {
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

        private static string DecodeQuotedPrintable(string input)
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

            return input.Replace("=\r\n", string.Empty);
        }

        private void WriteMessage(EmailHeader header, Email email)
        {
            var isMessageNull = email.Message == null;

            if (!isMessageNull && header.ContentTransferEncoding == ContentTransferEncodings.QuotedPrintable)
            {
                email.Message = DecodeQuotedPrintable(email.Message);
            }

            var str = isMessageNull ? string.Empty : email.Message;

            var estimatedSize = email.AttachmentArchive.Length / 1024D;

            Console.Write("Sent @ ");
            PrintTimeStamp();
            Console.WriteLine();

            Console.Write("Sent to : ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(email.To);
            Console.ResetColor();

            Console.Write("Subject : ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(email.Subject);
            Console.ResetColor();

            Console.Write("\tMessage length  : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{str.Length:n0}");
            Console.ResetColor();

            Console.Write("\tAttachments     : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(email.AttachmentCount);
            Console.ResetColor();

            Console.Write("\tAttachment size : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{estimatedSize:n2} KB");
            Console.ResetColor();
        }

        private void SaveEmail(Email email)
        {
            _context.Emails.Add(email);
            _context.SaveChanges();
        }

        private void LogError(Exception exception, Email email = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(exception.ToString());
            Console.ResetColor();

            var log = new Log();

            log.Message = "Ex: " + exception.GetType().Name + " ExMsg: " + exception.Message;

            log.Exception = exception.ToString();

            log.Level = @"Error";

            if (email != null)
            {
                log.Properties = SerializeAsJson(email);
            }

            using (var context = new InterceptorModelFactory().CreateDbContext(null))
            {
                context.Logs.Add(log);
                context.SaveChanges();
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

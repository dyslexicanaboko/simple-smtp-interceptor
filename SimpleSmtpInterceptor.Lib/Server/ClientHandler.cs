using Microsoft.EntityFrameworkCore.Internal;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Entities;
using SimpleSmtpInterceptor.Lib.Parsers;
using SimpleSmtpInterceptor.Lib.Services;
using System;
using System.IO;
using System.Net.Sockets;

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

            _context = GetContext();

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

                                        WriteMessage(email);

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

        private void WriteMessage(Email email)
        {
            try
            {
                var messageSize = GetKiloBytes(email.Message);

                var archiveSize = GetKiloBytes(email.AttachmentArchive);

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
                Console.WriteLine($"{messageSize:n2} KB");
                Console.ResetColor();

                Console.Write("\tAttachments     : ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(email.AttachmentCount);
                Console.ResetColor();

                Console.Write("\tAttachment size : ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{archiveSize:n2} KB");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The method \"WriteMessage()\" encountered an exception, but this is not a critical error. Your message should have been saved. 0x202002082352\n\tError: {ex.Message}\n\tCheck the log for more details.");

                LogError(ex, email);
            }
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

            using (var context = GetContext())
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

using Microsoft.EntityFrameworkCore.Internal;
using SimpleSmtpInterceptor.Data.Entities;
using SimpleSmtpInterceptor.Lib.Exceptions;
using SimpleSmtpInterceptor.Lib.Parsers;
using SimpleSmtpInterceptor.Lib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SimpleSmtpInterceptor.Lib.Server
{
    public class ClientHandler 
        : CommonBase, IDisposable
    {
        private readonly TcpClient _client;
        
        private readonly bool _verboseOutput;

        private readonly LogService _log;

        public ClientHandler(TcpClient client, bool verboseOutput)
        {
            _client = client;

            _verboseOutput = verboseOutput;

            _log = new LogService();

            Console.WriteLine();
        }

        public void HandleRequest()
        {
            Email email = null;
            
            var lstRcptTo = new List<string>();

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

                                /* Each email address shows up one per line with "RCPT TO:" as a prefix 
                                 * It's a mix of To, Cc and Bcc. Bcc doesn't have a dedicated prefix 
                                 * so this is how I am dealing with it for now. */
                                CollectRcptTo(lstRcptTo, line);
                                
                                switch (line)
                                {
                                    case "DATA":
                                        writer.WriteLine("354 Start input, end data with <CRLF>.<CRLF>");

                                        var parser = EmailParser.GetEmailParser(reader, lstRcptTo, _verboseOutput);

                                        if (parser == null)
                                        {
                                            throw new Exception(
                                                "Parser was returned as null somehow... 0x202002020103");
                                        }

                                        parser.ParseBody();

                                        email = parser.GetEmail();

                                        var attachments = parser.GetAttachments();

                                        email.AttachmentCount = attachments.Count;

                                        if (attachments.Any())
                                        {
                                            var svc = new AttachmentCompressor(attachments);

                                            var rawFile = svc.SaveAsZipArchive(Path.GetRandomFileName());

                                            email.AttachmentArchive = rawFile.Contents;
                                        }

                                        WriteMessage(email);

                                        var svcEmail = new EmailService(email);

                                        svcEmail.ValidateEmail();

                                        svcEmail.SaveEmail();

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
                        catch (InvalidEmailException iee)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("This email cannot not be saved. Check log for more details.");
                            Console.ResetColor();

                            _log.LogError(iee, email);
                        }
                        catch (IOException ioe)
                        {
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine("Connection lost.");
                            Console.ResetColor();

                            _log.LogError(ioe, email);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, email);
                        }
                    }
                }
            }
        }

        private void CollectRcptTo(List<string> rcptTo, string line)
        {
            if (!TryGetAttribute(line, Headers.RcptTo, out var emailAddress)) return;

            //Remove angle brackets
            var strEmail = emailAddress
                .Replace("<", string.Empty)
                .Replace(">", string.Empty);

            rcptTo.Add(strEmail);
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

                Console.Write("Sent     @ ");
                PrintTimeStamp();
                Console.WriteLine();

                Console.Write("Sent To  : ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(email.To);
                Console.ResetColor();

                Console.Write("Sent Cc  : ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(email.Cc);
                Console.ResetColor();

                Console.Write("Sent Bcc : ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(email.Bcc);
                Console.ResetColor();

                Console.Write("Subject  : ");
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

                _log.LogError(ex, email);
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

using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Entities;
using SimpleSmtpInterceptor.Data.Models;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleSmtpInterceptor.Lib
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

                                        var parser = EmailParser.GetEmailParser(reader, _verboseOutput);

                                        if (parser == null)
                                        {
                                            throw new Exception("Parser was returned as null somehow... 0x202002020103");
                                        }

                                        parser.ParseBody();

                                        email = parser.GetEmail();

                                        var header = parser.GetHeader();

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
            if (header.ContentTransferEncoding == "quoted-printable")
            {
                email.Message = DecodeQuotedPrintable(email.Message);
            }

            Console.WriteLine($"sent to -> {email.To} : Message length: {email.Message.Length:n0}");
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

        public void Dispose()
        {
            _context?.Dispose();

            if(_client == null) return;

            _client.Close();
            _client.Dispose();
        }
    }
}

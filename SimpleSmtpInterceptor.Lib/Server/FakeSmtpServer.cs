using System;
using System.Net;

namespace SimpleSmtpInterceptor.Lib.Server
{
    public class FakeSmtpServer
    {
        private readonly MailListener _listener;

        public FakeSmtpServer(bool listenOnLoopback, int port = 25, bool verboseOutput = false)
        {
            var ip = listenOnLoopback ? IPAddress.Loopback : IPAddress.Any;

            _listener = new MailListener(ip, port, verboseOutput);
        }

        public void Start()
        {
            PrintMessage(@"started");
            
            _listener.StartListening();
        }

        public void Stop()
        {
            PrintMessage(@"stopped");

            _listener.StopListening();
        }

        private void PrintMessage(string operatingWord)
        {
            var dtm = DateTime.Now;

            var tz = TimeZoneInfo.Local;

            var strTimeZone = tz.IsDaylightSavingTime(dtm) ? tz.DaylightName : tz.StandardName;

            Console.Write($"\nFake SMTP Server {operatingWord} @ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" " + strTimeZone);
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}

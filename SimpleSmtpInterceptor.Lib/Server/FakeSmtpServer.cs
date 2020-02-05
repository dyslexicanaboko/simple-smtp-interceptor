using System;
using System.Net;

namespace SimpleSmtpInterceptor.Lib.Server
{
    public class FakeSmtpServer
        : CommonBase
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
            Console.Write($"\nFake SMTP Server {operatingWord} @ ");

            PrintTimeStamp();

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}

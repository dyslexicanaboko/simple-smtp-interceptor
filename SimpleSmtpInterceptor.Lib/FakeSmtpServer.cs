using System;
using System.Net;

namespace SimpleSmtpInterceptor.Lib
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
            Console.WriteLine($"Fake SMTP Server started @ {DateTime.Now}");

            _listener.StartListening();
        }

        public void Stop()
        {
            Console.WriteLine($"Fake SMTP Server stopped @ {DateTime.Now}");

            _listener.StopListening();
        }
    }
}

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleSmtpInterceptor.Lib;

namespace SimpleSmtpInterceptor.ConsoleApp
{
    public class Program
    {
        private static bool _listenOnLoopBack;

        public static void Main(string[] args)
        {
            BuildConfigs();

            SmtpServer();
        }

        private static void SmtpServer()
        {
            //var server = new FakeSmtpServer(_listenOnLoopBack);
            var server = new FakeSmtpServer(_listenOnLoopBack, 25, true);

            server.Start();
        }
        
        private static void BuildConfigs()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            _listenOnLoopBack = Convert.ToBoolean(configuration["ListenOnLoopback"]);

            Console.Write($"ListenOnLoopback : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(_listenOnLoopBack);
            Console.ResetColor();
        }
    }
}

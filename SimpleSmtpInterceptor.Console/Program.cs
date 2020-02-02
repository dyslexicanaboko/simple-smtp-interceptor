using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleSmtpInterceptor.Lib;

namespace SimpleSmtpInterceptor.ConsoleApp
{
    public class Program
    {
        private static bool _listenOnLoopBack;
        private static bool _verboseOutput;
        private static int _port;

        public static void Main(string[] args)
        {
            BuildConfigs();

            SmtpServer();
        }

        private static void SmtpServer()
        {
            var server = new FakeSmtpServer(_listenOnLoopBack, _port, _verboseOutput);

            server.Start();
        }
        
        private static void BuildConfigs()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            _listenOnLoopBack = Convert.ToBoolean(configuration["ListenOnLoopback"]);

            _port = Convert.ToInt32(configuration["Port"]);

            _verboseOutput = Convert.ToBoolean(configuration["VerboseOutput"]);

            PrintProperty("Listen on loop back", _listenOnLoopBack);
            PrintProperty("Listen on port     ", _port);
            PrintProperty("Verbose output     ", _listenOnLoopBack);
        }

        private static void PrintProperty(string key, object value)
        {
            Console.Write($"{key} : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(value);
            Console.ResetColor();
        }
    }
}

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
            //Console.WriteLine("Hello?");
            //Console.Read();

            BuildConfigs();

            SmtpServer();

            //ConfigTest();
        }

        private static void SmtpServer()
        {
            var server = new FakeSmtpServer(_listenOnLoopBack);

            server.Start();
        }
        
        private static void BuildConfigs()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            _listenOnLoopBack = Convert.ToBoolean(configuration["ListenOnLoopback"]);

            Console.WriteLine($"ListenOnLoopback: {_listenOnLoopBack}");
        }

        private static void ConfigTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            Console.WriteLine(configuration.GetConnectionString("SimpleSmtpInterceptor"));

            Console.Read();
        }
    }
}

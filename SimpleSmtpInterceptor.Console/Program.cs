using Microsoft.Extensions.Configuration;
using SimpleSmtpInterceptor.Lib.Server;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SimpleSmtpInterceptor.ConsoleApp
{
    public class Program
    {
        private static bool _listenOnLoopBack;
        private static bool _verboseOutput;
        private static int _port;

        public static void Main(string[] args)
        {
            DisplayProductInfo();

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

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===================================");
            Console.WriteLine("appsettings.json");
            Console.WriteLine("===================================");
            Console.ResetColor();

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

        private static readonly string[] AttributesToDisplay = new[]
        {
            "System.Reflection.AssemblyCompanyAttribute",
            "System.Reflection.AssemblyFileVersionAttribute",
            "System.Reflection.AssemblyProductAttribute"
        };

        private static void DisplayProductInfo()
        {
            var attributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes()
                .Where(x => AttributesToDisplay.Contains(x.ToString()))
                .ToList();

            if (!attributes.Any()) return;

            foreach (var a in attributes)
            {
                var fqdn = a.ToString();

                var t = Type.GetType(fqdn);

                if (t == null) continue;

                var arr = t.GetProperties();

                if (!arr.Any()) continue;

                var p = arr.First();

                PrintProperty(p.Name, p.GetValue(a));
            }

            Console.WriteLine();
        }
    }
}

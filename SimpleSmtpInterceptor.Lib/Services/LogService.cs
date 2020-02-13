using SimpleSmtpInterceptor.Data.Entities;
using System;

namespace SimpleSmtpInterceptor.Lib.Services
{
    internal class LogService
        : CommonBase
    {
        internal void LogError(Exception exception, Email email = null)
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
    }
}

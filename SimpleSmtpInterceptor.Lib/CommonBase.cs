using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using SimpleSmtpInterceptor.Data;

namespace SimpleSmtpInterceptor.Lib
{
    public abstract class CommonBase
    {
        protected const double KiloByte = 1024D;

        protected string SerializeAsJson(object target)
        {
            var js = new DataContractJsonSerializer(target.GetType());

            using (var ms = new MemoryStream())
            {
                js.WriteObject(ms, target);

                ms.Position = 0;

                using (var sr = new StreamReader(ms))
                {
                    var json = sr.ReadToEnd();

                    return json;
                }
            }
        }

        protected static bool TryGetAttribute(string text, string attribute, out string attributeValue)
        {
            attributeValue = null;

            if (!text.StartsWith(attribute)) return false;

            attributeValue = text.Substring(attribute.Length);

            return true;
        }

        protected void PrintTimeStamp()
        {
            var dtm = DateTime.Now;

            var tz = TimeZoneInfo.Local;

            var strTimeZone = tz.IsDaylightSavingTime(dtm) ? tz.DaylightName : tz.StandardName;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" " + strTimeZone);
            Console.ResetColor();
        }

        protected InterceptorModel GetContext()
        {
            var context = new InterceptorModelFactory().CreateDbContext(null);

            return context;
        }

        protected double GetKiloBytes(string characters)
        {
            if (characters == null) return 0D;

            //After testing a variety of ways to get the string size, settled on UTF8. This is for estimation only.
            var encoding = new UTF8Encoding();

            var bytes = encoding.GetBytes(characters);

            return GetKiloBytes(bytes);    
        }

        protected double GetKiloBytes(byte[] array)
        {
            var size = array?.Length / KiloByte ?? 0D;

            return size;
        }
    }
}

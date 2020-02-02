﻿using System.IO;
using System.Runtime.Serialization.Json;

namespace SimpleSmtpInterceptor.Lib
{
    public abstract class CommonBase
    {
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
    }
}
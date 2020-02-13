using System.Collections.Generic;
using SimpleSmtpInterceptor.Data.Entities;
using Iee = SimpleSmtpInterceptor.Lib.Exceptions.InvalidEmailException;

namespace SimpleSmtpInterceptor.Lib.Services
{
    public class EmailService
        : CommonBase
    {
        private readonly Email _email;

        public EmailService(Email email)
        {
            _email = email;
        }

        public void ValidateEmail()
        {
            var dict = new Dictionary<Iee.Part, string>()
            {
                {Iee.Part.From, _email.From},
                {Iee.Part.To, _email.To},
                {Iee.Part.Subject, _email.Subject}
            };

            foreach (var kvp in dict)
            {
                if (kvp.Value != null) continue;

                //If the value is null throw an exception ahead of time to prevent saving
                throw new Iee(kvp.Key);
            }
        }

        public void SaveEmail()
        {
            using (var context = GetContext())
            {
                context.Emails.Add(_email);
                context.SaveChanges();
            }
        }
    }
}

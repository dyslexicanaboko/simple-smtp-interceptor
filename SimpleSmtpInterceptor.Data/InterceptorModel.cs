using Microsoft.EntityFrameworkCore;
using SimpleSmtpInterceptor.Data.Mappings;
using SimpleSmtpInterceptor.Data.Entities;

namespace SimpleSmtpInterceptor.Data
{
    public partial class InterceptorModel 
        : DbContext
    {
        //"name=SimpleSmtpInterceptor"
        public InterceptorModel(DbContextOptions<InterceptorModel> options)
            : base(options)
        {
            //https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext
        }

        public virtual DbSet<Email> Emails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        //This link has the methodology I am using for putting together my Fluent API mapping layer
        //https://www.learnentityframeworkcore.com/configuration/fluent-api
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var m = modelBuilder;

            m.ApplyConfiguration(new EmailMap());
        }
    }
}

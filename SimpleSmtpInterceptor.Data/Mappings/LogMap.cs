using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleSmtpInterceptor.Data.Entities;

namespace SimpleSmtpInterceptor.Data.Mappings
{
    public class LogMap
        : IEntityTypeConfiguration<Log>
    {
        //Modeled after: https://github.com/nlog/NLog/wiki/Database-target
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            var b = builder;

            b.ToTable("Log", "dbo");

            b.HasKey(x => x.LogId)
                .HasName("PK_dbo.Log_LogId");

            b.Property(x => x.MachineName)
                .HasMaxLength(200)
                .IsUnicode();

            b.Property(x => x.Level)
                .HasMaxLength(5)
                .IsRequired();

            b.Property(x => x.Message)
                .IsUnicode()
                .IsRequired();

            b.Property(x => x.Logger)
                .HasMaxLength(300)
                .IsUnicode();

            b.Property(x => x.Properties)
                .IsUnicode();

            b.Property(x => x.Callsite)
                .HasMaxLength(300)
                .IsUnicode();

            b.Property(x => x.Exception)
                .IsUnicode();

            b.Property(e => e.CreatedOnUtc)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("getutcdate()")
                .IsRequired();
        }
    }
}
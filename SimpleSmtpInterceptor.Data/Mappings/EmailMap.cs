using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleSmtpInterceptor.Data.Models;

namespace SimpleSmtpInterceptor.Data.Mappings
{
    public class EmailMap 
        : IEntityTypeConfiguration<Email>
    {
        public void Configure(EntityTypeBuilder<Email> builder)
        {
            var b = builder;

            b.ToTable("Email", "dbo");

            b.HasKey(x => x.EmailId)
                .HasName("PK_dbo.Email_EmailId");

            b.Property(x => x.From)
                .IsUnicode(false)
                .HasMaxLength(1000)
                .IsRequired();

            b.Property(x => x.To)
                .IsUnicode(false)
                .HasMaxLength(1000)
                .IsRequired();

            b.Property(x => x.Subject)
                .IsUnicode(false)
                .HasMaxLength(78) //RFC 2822
                .IsRequired();

            b.Property(x => x.Message)
                .IsRequired();

            b.Property(e => e.CreatedOnUtc)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("getutcdate()")
                .IsRequired();
        }
    }
}

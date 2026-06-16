using Food.Domain.Models;
using Food.Domain.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Food.Repository.Data.Configurations
{
    public class EmailConfig : IEntityTypeConfiguration<Email>
    {
        public void Configure(EntityTypeBuilder<Email> builder)
        {
            builder.Property(e => e.To)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Body)
                .IsRequired();

            builder.Property(e => e.IsSent)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(1024);

            builder.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

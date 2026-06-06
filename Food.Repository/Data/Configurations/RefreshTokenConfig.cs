using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Food.Repository.Data.Configurations
{
    public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r=>r.Id);
            builder.Property(r => r.Token)
                .IsRequired();
            builder.HasOne(r=>r.AppUser)
                .WithMany(u=>u.RefreshTokens)
                .HasForeignKey(r=>r.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

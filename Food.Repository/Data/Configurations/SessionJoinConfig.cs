using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Food.Repository.Data.Configurations
{
    internal class SessionJoinConfig : IEntityTypeConfiguration<SessionJoin>
    {
        public void Configure(EntityTypeBuilder<SessionJoin> builder)
        {
            builder.HasIndex(sj => new { sj.SessionId, sj.UserId })
                .IsUnique();

            builder.HasOne(sj => sj.Session)
                .WithMany(s => s.SessionJoins)
                .HasForeignKey(sj => sj.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sj => sj.User)
                .WithMany()
                .HasForeignKey(sj => sj.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

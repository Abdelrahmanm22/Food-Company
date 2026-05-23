using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Session;
using Food.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Food.Repository.Data.Configurations
{
    public class SessionConfig : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.Property(s => s.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (SessionStatus)Enum.Parse(typeof(SessionStatus), v))
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.DeliveryCost)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(s => s.Notes)
                .HasMaxLength(1000);

            builder.HasOne(s => s.HostUser)
                .WithMany()
                .HasForeignKey(s => s.HostUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Restaurant)
                .WithMany()
                .HasForeignKey(s => s.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

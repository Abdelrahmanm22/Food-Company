using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Order;
using Food.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Food.Repository.Data.Configurations
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(o => o.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v))
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.DeliveryCost)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(o => o.Session)
                .WithOne(s => s.Order)
                .HasForeignKey<Order>(o => o.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

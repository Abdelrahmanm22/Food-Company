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
    public class OrderDetailConfig : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.Property(od => od.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(od => od.Item)
                .WithMany()
                .HasForeignKey(od => od.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(od => od.User)
                .WithMany()
                .HasForeignKey(od => od.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

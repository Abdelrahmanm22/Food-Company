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
    public class RestaurantConfig : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(r => r.Address)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(r => r.DefaultDeliveryCost)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.HasMany(r => r.Categories)
                .WithOne(c => c.Restaurant)
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

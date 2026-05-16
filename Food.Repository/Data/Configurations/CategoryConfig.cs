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
    public class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);
            builder.HasOne(c => c.Restaurant)
               .WithMany(r => r.Categories)
               .HasForeignKey(c => c.RestaurantId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

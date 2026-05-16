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
    public class ItemConfig : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.Property(I=>I.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(I=>I.Description)
                .IsRequired(false)
                .HasMaxLength(1000);
            builder.Property(I=>I.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(i => i.IsAvailable)
                   .IsRequired()
                   .HasDefaultValue(true);
            builder.Property(i => i.ImageUrl)
                   .HasMaxLength(500);
            builder.HasOne(i => i.Category)
                   .WithMany(c => c.Items)
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<BlogAppUser,IdentityRole<int>,int>
    {
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BlogPost>()
                .HasOne<Category>(s => s.Category)
                .WithMany(g => g.BlogPosts)
                .HasForeignKey(s => s.CategoryId);
            modelBuilder.Entity<BlogPost>()
                .HasOne<BlogAppUser>(s => s.BlogAppUser)
                .WithMany(g => g.BlogPosts)
                .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<BlogComment>()
                .HasOne<BlogPost>(s => s.BlogPost)
                .WithMany(g => g.BlogComments)
                .HasForeignKey(s => s.BlogPostId);
        }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<StoreLike> storeLikes { get; set; }

    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using minimalAPINet7OK.Models;
using System.Collections.Generic;

namespace minimalAPINet7OK.DataContext
{
    public class TodoDb : DbContext
    {
        public TodoDb(DbContextOptions<TodoDb> options)
            : base(options) { }

        public DbSet<Todo> Todos => Set<Todo>();

        public DbSet<User> Users => Set<User>();
        public DbSet<Rol> Roles => Set<Rol>();
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //config many to many
            modelBuilder.Entity<User>()
               .HasMany(e => e.Roles)
               .WithMany(e => e.Users)
               .UsingEntity(
                   "RolUser",
                   l => l.HasOne(typeof(Rol)).WithMany().HasForeignKey("RolesId").HasPrincipalKey(nameof(Rol.Id)).OnDelete(DeleteBehavior.Restrict),
                   r => r.HasOne(typeof(User)).WithMany().HasForeignKey("UsersId").HasPrincipalKey(nameof(User.Id)),
                   j => j.HasKey("RolesId", "UsersId"));

            
            base.OnModelCreating(modelBuilder);
        }
    }
}

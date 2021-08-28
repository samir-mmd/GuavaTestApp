using GuavaUserEngine.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuavaUserEngine.Services
{
    public class UserDbContext : DbContext
    {

        public DbSet<GuavaUser> GuavaUsers { get; set; }

        public UserDbContext()
        {
            Database.EnsureCreated();            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {           
            //optionsBuilder.UseSqlite("Filename=../../../../data/UserStorage.db");
            optionsBuilder.UseSqlite("Filename=UserStorage.db");
        }
    }
}

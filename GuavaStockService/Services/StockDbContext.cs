using GuavaStockService.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuavaStockService.Services
{
    public class StockDbContext : DbContext
    {

        public DbSet<GuavaStock> GuavaStocks { get; set; }

        public StockDbContext()
        {
            Database.EnsureCreated();            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=StockStorage.db");
        }
    }
}

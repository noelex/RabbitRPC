using Microsoft.EntityFrameworkCore;
using RabbitRPC.States.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.EntityFrameworkCore
{
    internal class StateDbContext:DbContext
    {
        public StateDbContext(DbContextOptions<StateDbContext> options):
            base(options)
        {
    
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StateEntry>().HasKey(x => x.Key);
            modelBuilder.Entity<StateEntry>().Property(x => x.Version).IsRowVersion();
            modelBuilder.Entity<StateEntry>().Property(x => x.Value).IsRequired();
        }
    }
}

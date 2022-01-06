using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RabbitRPC.States.Sqlite
{
    internal class DbContextDesignFactory:IDesignTimeDbContextFactory<StateDbContext>
    {
        public StateDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<StateDbContext>();
            options.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "states.db")}");

            return new StateDbContext(options.Options);
        }
    }
}

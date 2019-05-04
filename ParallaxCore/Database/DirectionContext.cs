using Microsoft.EntityFrameworkCore;
using ParallaxCore.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParallaxCore.Database
{
    class DirectionContext : DbContext
    {
        const string MYSQL_CONNECTION_STRING = "server=localhost;UserId=plx_parser;Password=qeoBXYwp;database=parallax;charset=utf8";

        public DbSet<Direction> Directions { get; set; }

        public DbSet<Checksum> Checksums { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(MYSQL_CONNECTION_STRING);
        }
    }
}

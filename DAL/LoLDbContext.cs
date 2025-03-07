﻿using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL
{
    public class LoLDbContext : DbContext
    {

        private string connectionString = File.ReadAllText($"Config{Path.DirectorySeparatorChar}connectionString.txt");
        
        public DbSet<Match> Matches { get; set; }
        public DbSet<ChampionStats> ChampionStats { get; set; }
        public LoLDbContext() : base()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
            base.OnConfiguring(optionsBuilder);

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Match>().HasKey(m => new
            {
                m.MatchID,
                m.Puuid
            });
            
            modelBuilder.Entity<ChampionStats>().HasKey(m => new
            {
                m.Name,
                m.Puuid
            });
        }
    }
}

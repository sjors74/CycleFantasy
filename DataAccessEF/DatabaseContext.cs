﻿using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
//using System.Data.Entity;
using System.Drawing;

namespace Domain.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Competitor> Competitors { get; set;}
        public DbSet<Team> Teams { get; set;}
        public DbSet<CompetitorsInEvent> CompetitorsInEvent { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<ConfigurationItem> ConfigurationItems { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<GameCompetitor> GameCompetitors { get; set; }
        public DbSet<GameCompetitorEvent> GameCompetitorsEvent { get; set; }
        public DbSet<GameCompetitorEventPick> GameCompetitorEventPicks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .HasMany(c => c.Competitors)
                .WithOne(t => t.Team)
                .IsRequired();
            modelBuilder.Entity<Team>()
                .HasOne(c => c.Country);
            modelBuilder.Entity<Competitor>()
                .HasOne(c => c.Country);
            modelBuilder.Entity<Stage>()
                .HasOne(e => e.Event);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Configuration);
            modelBuilder.Entity<Configuration>()
                .HasMany(c => c.ConfigurationItems);
                //.WithOne(c => c.Configuration);
            modelBuilder.Entity<CompetitorsInEvent>()
                .HasOne(c => c.Event);
        }
    }

}

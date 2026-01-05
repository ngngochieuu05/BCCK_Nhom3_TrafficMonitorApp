using System;
using Microsoft.EntityFrameworkCore;
using TrafficMonitorApp.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TrafficMonitorApp.Data
{
    /// <summary>
    /// Entity Framework DbContext for Traffic Monitor application
    /// Manages database connection and entity configurations
    /// </summary>
    public class TrafficDbContext : DbContext
    {
        // DbSets representing database tables
        // EF Core will initialize these during construction
        public DbSet<TrafficSessionDb> TrafficSessions { get; set; } = null!;
        public DbSet<VehicleDetectionDb> VehicleDetections { get; set; } = null!;
        public DbSet<HourlyStatisticsDb> HourlyStatistics { get; set; } = null!;
        public DbSet<UserAccount> Users { get; set; } = null!;

        public TrafficDbContext()
        {
        }

        public TrafficDbContext(DbContextOptions<TrafficDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configure database connection
        /// Sử dụng SQL Server Express từ appsettings.json
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Load connection string from appsettings.json
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("TrafficDb") 
                    ?? @"Server=TEDDY\SQLEXPRESS;Database=QuanLyGiaoThong;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30";
                optionsBuilder.UseSqlServer(connectionString);
                
                // Enable detailed errors in development
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

        /// <summary>
        /// Configure entity relationships and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TrafficSessionDb
            modelBuilder.Entity<TrafficSessionDb>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                
                entity.Property(e => e.StartTime)
                    .IsRequired();
                
                entity.Property(e => e.SourceType)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.SourcePath)
                    .HasMaxLength(500);
                
                entity.Property(e => e.ModelPath)
                    .HasMaxLength(500);

                // One-to-many relationship with VehicleDetections
                entity.HasMany(e => e.Detections)
                    .WithOne(d => d.Session)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete detections when session is deleted

                // Index for faster queries
                entity.HasIndex(e => e.StartTime);
                entity.HasIndex(e => e.EndTime);
            });

            // Configure VehicleDetectionDb
            modelBuilder.Entity<VehicleDetectionDb>(entity =>
            {
                entity.HasKey(e => e.DetectionId);
                
                entity.Property(e => e.DetectedTime)
                    .IsRequired();
                
                entity.Property(e => e.VehicleType)
                    .IsRequired()
                    .HasMaxLength(50);

                // Foreign key relationship
                entity.HasOne(e => e.Session)
                    .WithMany(s => s.Detections)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for common queries
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.DetectedTime);
                entity.HasIndex(e => e.VehicleType);
                entity.HasIndex(e => e.TrackerId);
            });

            // Configure HourlyStatisticsDb
            modelBuilder.Entity<HourlyStatisticsDb>(entity =>
            {
                entity.HasKey(e => e.StatId);
                
                entity.Property(e => e.HourTimestamp)
                    .IsRequired();

                // Index for time-based queries
                entity.HasIndex(e => e.HourTimestamp);
                
                // Unique constraint to prevent duplicate hourly records
                entity.HasIndex(e => e.HourTimestamp)
                    .IsUnique();
            });

            // Configure UserAccount
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(e => e.UserId);
                
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(256);
                
                entity.Property(e => e.FullName)
                    .HasMaxLength(100);
                
                entity.Property(e => e.Email)
                    .HasMaxLength(100);
                
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("User");

                // Unique username constraint
                entity.HasIndex(e => e.Username)
                    .IsUnique();
                
                // Index for common queries
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.IsActive);
            });
        }

        /// <summary>
        /// Ensure database is created and ready
        /// </summary>
        public void EnsureDatabaseCreated()
        {
            try
            {
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if database connection is working
        /// </summary>
        public bool CanConnect()
        {
            try
            {
                return Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }
    }
}
